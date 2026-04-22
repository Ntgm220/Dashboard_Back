using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SucursalController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public SucursalController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetSucursal")]
        public async Task<ActionResult<IEnumerable<Sucursales>>> GetSucursales()
        {
            try
            {
                var sucursales = await ScopeSucursales(_db.Sucursales)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .ToListAsync();

                if (sucursales == null || !sucursales.Any())
                {
                    return NotFound("No se ha encontrado registro de sucursales.");
                }

                return Ok(sucursales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las sucursales: {ex.Message}");
            }
        }

        [HttpGet("GetSucursalByNombre")]
        public async Task<ActionResult<IEnumerable<Sucursales>>> GetSucursalByNombre(string sucursalResult)
        {
            if (string.IsNullOrWhiteSpace(sucursalResult))
            {
                return BadRequest("El nombre de la sucursal es requerido.");
            }

            try
            {
                var sucursales = await ScopeSucursales(_db.Sucursales)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .Where(x => x.Nombre.Contains(sucursalResult))
                    .ToListAsync();

                if (!sucursales.Any())
                {
                    return NotFound($"No se encontraron sucursales con ese nombre {sucursalResult}.");
                }

                return Ok(sucursales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las sucursales por nombre: {ex.Message}");
            }
        }
    }
}
