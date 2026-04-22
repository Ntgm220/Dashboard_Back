using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class VentaBaseController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public VentaBaseController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetVentaBase")]
        public async Task<ActionResult<IEnumerable<VentaBase>>> GetVentaBase(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var ventas = await GetVentaBaseQuery(ToDateOnly(from), ToDateOnly(to)).ToListAsync();

                if (!ventas.Any())
                {
                    return NotFound("No se ha encontrado registro de ventas.");
                }

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas: {ex.Message}");
            }
        }

        [HttpGet("GetVentaBaseBySucursal")]
        public async Task<ActionResult<IEnumerable<VentaBase>>> GetVentaBaseBySucursal(string sucursalResult, DateTime? from = null, DateTime? to = null)
        {
            if (string.IsNullOrWhiteSpace(sucursalResult))
            {
                return BadRequest("El nombre de la sucursal es requerido.");
            }

            try
            {
                var ventas = await GetVentaBaseQuery(ToDateOnly(from), ToDateOnly(to))
                    .Where(x => x.Sucursal.Nombre.Contains(sucursalResult.Trim()))
                    .ToListAsync();

                if (!ventas.Any())
                {
                    return NotFound($"No se encontraron ventas para la sucursal {sucursalResult}.");
                }

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas por sucursal: {ex.Message}");
            }
        }

        [HttpGet("GetVentaBaseByCliente")]
        public async Task<ActionResult<IEnumerable<VentaBase>>> GetVentaBaseByCliente(string clienteResult, DateTime? from = null, DateTime? to = null)
        {
            if (string.IsNullOrWhiteSpace(clienteResult))
            {
                return BadRequest("El nombre del cliente es requerido.");
            }

            try
            {
                var ventas = await GetVentaBaseQuery(ToDateOnly(from), ToDateOnly(to))
                    .Where(x => x.Cliente.Nombre.Contains(clienteResult.Trim()))
                    .ToListAsync();

                if (!ventas.Any())
                {
                    return NotFound($"No se encontraron ventas para el cliente {clienteResult}.");
                }

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas por cliente: {ex.Message}");
            }
        }

        [HttpGet("GetVentaBaseByVendedor")]
        public async Task<ActionResult<IEnumerable<VentaBase>>> GetVentaBaseByVendedor(string vendedorResult, DateTime? from = null, DateTime? to = null)
        {
            if (string.IsNullOrWhiteSpace(vendedorResult))
            {
                return BadRequest("El nombre del vendedor es requerido.");
            }

            try
            {
                var ventas = await GetVentaBaseQuery(ToDateOnly(from), ToDateOnly(to))
                    .Where(x => x.Vendedor.Nombre.Contains(vendedorResult.Trim()))
                    .ToListAsync();

                if (!ventas.Any())
                {
                    return NotFound($"No se encontraron ventas para el vendedor {vendedorResult}.");
                }

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas por vendedor: {ex.Message}");
            }
        }

        [HttpGet("GetVentaBaseByCaja")]
        public async Task<ActionResult<IEnumerable<VentaBase>>> GetVentaBaseByCaja(string cajaResult, DateTime? from = null, DateTime? to = null)
        {
            if (string.IsNullOrWhiteSpace(cajaResult))
            {
                return BadRequest("El nombre de la caja es requerido.");
            }

            try
            {
                var ventas = await GetVentaBaseQuery(ToDateOnly(from), ToDateOnly(to))
                    .Where(x => x.Caja.Nombre.Contains(cajaResult.Trim()))
                    .ToListAsync();

                if (!ventas.Any())
                {
                    return NotFound($"No se encontraron ventas para la caja {cajaResult}.");
                }

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las ventas por caja: {ex.Message}");
            }
        }

        private IQueryable<VentaBase> GetVentaBaseQuery(DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            return ScopeVentaBase(_db.VentaBase)
                .Include(x => x.Empresa)
                .Include(x => x.Sucursal)
                .Include(x => x.Caja)
                .Include(x => x.Vendedor)
                .Include(x => x.Cliente)
                .AsNoTracking()
                .Where(x =>
                    (!fromDate.HasValue || x.Fecha >= fromDate.Value) &&
                    (!toDate.HasValue || x.Fecha <= toDate.Value));
        }

        private static DateOnly? ToDateOnly(DateTime? value)
        {
            return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }
    }
}
