using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CajaController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public CajaController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetCaja")]
        public async Task<ActionResult<IEnumerable<Cajas>>> GetCajas()
        {
            try
            {
                var cajas = await ScopeCajas(_db.Cajas)
                    .Include(x => x.Sucursal)
                    .AsNoTracking()
                    .ToListAsync();

                if (cajas == null || !cajas.Any())
                {
                    return NotFound("No se ha encontrado registro de cajas.");
                }

                return Ok(cajas);
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las cajas: {ex.Message}");
            }
        }

        [HttpGet("GetCajaByNombre")]
        public async Task<ActionResult<IEnumerable<Cajas>>> GetCajaByNombre(string cajaResult)
        {
            if (string.IsNullOrWhiteSpace(cajaResult))
            {
                return BadRequest("El nombre de la caja es requerido.");
            }

            try
            {
                var cajas = await ScopeCajas(_db.Cajas)
                    .Include(x => x.Sucursal)
                    .AsNoTracking()
                    .Where(x => x.Nombre.Contains(cajaResult))
                    .ToListAsync();

                if (!cajas.Any())
                {
                    return NotFound($"No se encontraron cajas con ese nombre {cajaResult}.");
                }

                return Ok(cajas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las cajas por nombre: {ex.Message}");
            }
        }
    }
}
