using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClienteController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public ClienteController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetCliente")]
        public async Task<ActionResult<IEnumerable<Clientes>>> GetClientes()
        {
            try
            {
                var clientes = await GetClientesQuery()
                    .ToListAsync();

                if (clientes == null || !clientes.Any())
                {
                    return NotFound("No se ha encontrado registro de clientes.");
                }

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los clientes: {ex.Message}");
            }
        }

        [HttpGet("GetClienteByNombre")]
        public async Task<ActionResult<IEnumerable<Clientes>>> GetClienteByNombre(string clienteResult)
        {
            if (string.IsNullOrWhiteSpace(clienteResult))
            {
                return BadRequest("El nombre del cliente es requerido.");
            }

            try
            {
                var clientes = await GetClientesQuery()
                    .Where(x => x.Nombre.Contains(clienteResult))
                    .ToListAsync();

                if (!clientes.Any())
                {
                    return NotFound($"No se encontraron clientes con ese nombre {clienteResult}.");
                }

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los clientes por nombre: {ex.Message}");
            }
        }

        private IQueryable<Clientes> GetClientesQuery()
        {
            return ScopeClientes(_db.Clientes)
                .Include(x => x.Sucursal)
                .AsNoTracking();
        }
    }
}
