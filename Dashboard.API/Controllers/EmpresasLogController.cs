using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmpresasLogController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public EmpresasLogController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetEmpresasLog")]
        public async Task<ActionResult<IEnumerable<EmpresasLogs>>> GetEmpresasLogs()
        {
            try
            {
                var logs = await ScopeEmpresasLogs(_db.EmpresasLogs)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .ToListAsync();

                if (logs == null || !logs.Any())
                {
                    return NotFound("No se ha encontrado registro de logs de empresas.");
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los logs de las empresas: {ex.Message}");
            }
        }

        [HttpGet("GetEmpresasLogByEmpresa")]
        public async Task<ActionResult<IEnumerable<EmpresasLogs>>> GetEmpresasLogByEmpresa(string empresaResult)
        {
            if (string.IsNullOrWhiteSpace(empresaResult))
            {
                return BadRequest("El nombre de la empresa es requerido.");
            }

            try
            {
                var logs = await ScopeEmpresasLogs(_db.EmpresasLogs)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .Where(x => x.Empresa.Nombre.Contains(empresaResult))
                    .ToListAsync();

                if (!logs.Any())
                {
                    return NotFound($"No se encontraron logs para la empresa {empresaResult}.");
                }

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los logs por empresa: {ex.Message}");
            }
        }
    }
}
