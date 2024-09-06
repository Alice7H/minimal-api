using minimal_api.Dominio.DTOs;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Interfaces;
using minimal_api.Infraestrutura.Db;

namespace minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _context;
        public AdministradorServico(DbContexto contexto)
        {
            _context = contexto;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {
            return _context.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        }
        public Administrador? BuscaPorId(int id)
        {
            return _context.Administradores.Where(a => a.Id == id).FirstOrDefault();
        }

        public List<Administrador> Todos(int? pagina = 1)
        {
            var query = _context.Administradores.AsQueryable();
            int itensPorPagina = 10;
            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }

        public Administrador Incluir(Administrador administrador)
        {
            _context.Administradores.Add(administrador);
            _context.SaveChanges();
            return administrador;
        }
    }
}