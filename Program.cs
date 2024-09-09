using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Enums;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Servicos;
using minimal_api.Infraestrutura.Db;
using MinimalApi.Dominio.ModelViews;

#region Builder
var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration.GetSection("Jwt").ToString();

// Add services
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

if (string.IsNullOrEmpty(key)) key = "123456";
builder.Services
.AddAuthentication(options =>
{
  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
  options.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
    ValidateIssuer = false,
    ValidateAudience = false
  };
});
builder.Services.AddAuthorization();

builder.Services.AddDbContext<DbContexto>(options =>
{
  options.UseMySql(
    builder.Configuration.GetConnectionString("MySql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
  );
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    In = ParameterLocation.Header,
    Description = " Insira o token JWT:",
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    BearerFormat = "JWT",
    Scheme = "bearer",
  });
  options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type = ReferenceType.SecurityScheme,
          Id="Bearer"
        }
      },
      new string[]{}
    }
  });
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home()))
.AllowAnonymous()
.WithTags("Home");
#endregion

#region Administradores
string GerarTokenJwt(Administrador administrador)
{
  if (string.IsNullOrEmpty(key)) return string.Empty;

  var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
  var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new List<Claim>()
  {
    new Claim("Email", administrador.Email),
    new Claim("Perfil", administrador.Perfil),
    new Claim(ClaimTypes.Role, administrador.Perfil),
  };

  var token = new JwtSecurityToken(
    claims: claims,
    expires: DateTime.Now.AddDays(1),
    signingCredentials: credentials
  );

  return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.Login(loginDTO);
  if (administrador != null)
  {
    string token = GerarTokenJwt(administrador);
    return Results.Ok(new AdministradorLogado
    {
      Email = administrador.Email,
      Perfil = administrador.Perfil,
      Token = token
    });
  }
  return Results.Unauthorized();
})
.AllowAnonymous()
.WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
  var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };
  if (string.IsNullOrEmpty(administradorDTO.Email))
    validacao.Mensagens.Add("O email não pode ser um campo vazio");

  if (string.IsNullOrEmpty(administradorDTO.Senha))
    validacao.Mensagens.Add("A senha não pode ser um campo vazio");

  if (administradorDTO.Perfil == null)
    validacao.Mensagens.Add("O perfil não pode ser um campo vazio");

  if (validacao.Mensagens.Count() > 0)
    return Results.BadRequest(validacao);

  var administrador = new Administrador
  {
    Email = administradorDTO.Email,
    Senha = administradorDTO.Senha,
    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
  };
  administradorServico.Incluir(administrador);

  var adm = new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  };
  return Results.Created($"/administradores/{administrador.Id}", adm);

})
.RequireAuthorization()
.WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
  var adms = new List<AdministradorModelView>();
  var administradores = administradorServico.Todos(pagina);
  foreach (var adm in administradores)
  {
    adms.Add(new AdministradorModelView
    {
      Id = adm.Id,
      Email = adm.Email,
      Perfil = adm.Perfil
    });
  }
  return Results.Ok(adms);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
  var administrador = administradorServico.BuscaPorId(id);
  if (administrador == null) return Results.NotFound();

  var adm = new AdministradorModelView
  {
    Id = administrador.Id,
    Email = administrador.Email,
    Perfil = administrador.Perfil
  };

  return Results.Ok(adm);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Administradores");
#endregion

#region Veiculos
ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
  var validacao = new ErrosDeValidacao { Mensagens = new List<string>() };

  if (string.IsNullOrEmpty(veiculoDTO.Nome))
    validacao.Mensagens.Add("O nome não pode ser um campo vazio");

  if (string.IsNullOrEmpty(veiculoDTO.Marca))
    validacao.Mensagens.Add("A marca não pode ser um campo vazio");

  if (veiculoDTO.Ano < 1886)
    validacao.Mensagens.Add("O ano deve ser a partir que 1886");
  return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count() > 0)
    return Results.BadRequest(validacao);

  var veiculo = new Veiculo
  {
    Nome = veiculoDTO.Nome,
    Marca = veiculoDTO.Marca,
    Ano = veiculoDTO.Ano,
  };

  veiculoServico.Incluir(veiculo);
  return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
  var veiculos = veiculoServico.Todos(pagina);
  return Results.Ok(veiculos);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  if (veiculo == null) return Results.NotFound();

  return Results.Ok(veiculo);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" })
.WithTags("Veiculos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  if (veiculo == null) return Results.NotFound();

  var validacao = validaDTO(veiculoDTO);
  if (validacao.Mensagens.Count() > 0)
    return Results.BadRequest(validacao);

  veiculo.Nome = veiculoDTO.Nome;
  veiculo.Marca = veiculoDTO.Marca;
  veiculo.Ano = veiculoDTO.Ano;

  veiculoServico.Atualizar(veiculo);
  return Results.Ok(veiculo);
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
  var veiculo = veiculoServico.BuscaPorId(id);
  if (veiculo == null) return Results.NotFound();

  veiculoServico.Apagar(veiculo);
  return Results.NoContent();
})
.RequireAuthorization()
.RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
.WithTags("Veiculos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion