using Dashboard.Data.Entities;
using Dashboard.Infrastructure.DTOs;
using Dashboard.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsuarioController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;
        private readonly IAuthService _authService;

        public UsuarioController(DashboardDbContext db, IAuthService authService)
        {
            _db = db;
            _authService = authService;
        }

        [HttpGet("GetUsuario")]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarios()
        {
            try
            {
                var users = await ScopeUsuarios(_db.Usuarios)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .ToListAsync();

                if (users == null || !users.Any())
                {
                    return NotFound("No se ha encontrado registro de usuarios.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los usuarios: {ex.Message}");
            }
        }

        [HttpGet("GetUsuarioByNombre")]
        public async Task<ActionResult<IEnumerable<Usuarios>>> GetUsuarioByNombre(string usuarioResult)
        {
            if (string.IsNullOrWhiteSpace(usuarioResult))
            {
                return BadRequest("El nombre del usuario es requerido.");
            }

            try
            {
                var users = await ScopeUsuarios(_db.Usuarios)
                    .Include(x => x.Empresa)
                    .AsNoTracking()
                    .Where(x => x.Nombre.Contains(usuarioResult))
                    .ToListAsync();

                if (!users.Any())
                {
                    return NotFound($"No se encontraron usuarios con ese nombre {usuarioResult}.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al obtener los usuarios por nombre: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Solicitud invalida" });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email y contraseña son requeridos" });
            }

            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Credenciales invalidas" });
            }

            return Ok(result);
        }
    }
}
