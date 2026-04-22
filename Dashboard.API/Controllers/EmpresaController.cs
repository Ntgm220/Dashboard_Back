using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmpresaController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public EmpresaController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetEmpresa")]
        public async Task<ActionResult<IEnumerable<Empresas>>> GetEmpresas()
        {
            try
            {
                var empresas = await ScopeEmpresas(_db.Empresas)
                    .AsNoTracking()
                    .ToListAsync();

                if (empresas == null || !empresas.Any())
                {
                    return NotFound("No se ha encontrado registro de empresas.");
                }

                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las empresas: {ex.Message}");
            }
        }

        [HttpGet("GetEmpresaByNombre")]
        public async Task<ActionResult<IEnumerable<Empresas>>> GetEmpresaByNombre(string empresaResult)
        {
            if (string.IsNullOrWhiteSpace(empresaResult))
            {
                return BadRequest("El nombre de la empresa es requerido.");
            }

            try
            {
                var empresas = await ScopeEmpresas(_db.Empresas)
                    .AsNoTracking()
                    .Where(x => x.Nombre.Contains(empresaResult))
                    .ToListAsync();

                if (!empresas.Any())
                {
                    return NotFound($"No se encontraron empresas con ese nombre {empresaResult}.");
                }

                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener las empresas por nombre: {ex.Message}");
            }
        }
    }
}
