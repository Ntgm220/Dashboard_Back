using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VendedorController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public VendedorController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetVendedor")]
        public async Task<ActionResult<IEnumerable<Vendedores>>> GetVendedores()
        {
            try
            {
                var vendedores = await ScopeVendedores(_db.Vendedores)
                    .Include(x => x.Sucursal)
                    .AsNoTracking()
                    .ToListAsync();

                if (vendedores == null || !vendedores.Any())
                {
                    return NotFound("No se ha encontrado registro de vendedores.");
                }

                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los vendedores: {ex.Message}");
            }
        }

        [HttpGet("GetVendedorByNombre")]
        public async Task<ActionResult<IEnumerable<Vendedores>>> GetVendedorByNombre(string vendedorResult)
        {
            if (string.IsNullOrWhiteSpace(vendedorResult))
            {
                return BadRequest("El nombre del vendedor es requerido.");
            }

            try
            {
                var vendedores = await ScopeVendedores(_db.Vendedores)
                    .Include(x => x.Sucursal)
                    .AsNoTracking()
                    .Where(x => x.Nombre.Contains(vendedorResult))
                    .ToListAsync();

                if (!vendedores.Any())
                {
                    return NotFound($"No se encontraron vendedores con ese nombre {vendedorResult}.");
                }

                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los vendedores por nombre: {ex.Message}");
            }
        }
    }
}
