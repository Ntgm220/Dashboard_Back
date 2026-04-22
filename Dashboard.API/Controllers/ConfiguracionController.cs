using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ConfiguracionController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public ConfiguracionController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetConfiguracion")]
        public async Task<ActionResult<ConfiguracionResponse>> GetConfiguracion(int? empresaId = null)
        {
            var resolvedEmpresaId = ResolveEmpresaId(empresaId);

            if (!resolvedEmpresaId.HasValue)
            {
                return Forbid();
            }

            var empresa = await ScopeEmpresas(_db.Empresas)
                .AsNoTracking()
                .Where(x => x.Id == resolvedEmpresaId.Value)
                .Select(x => new EmpresaConfigResponse(
                    x.Id,
                    x.Nombre,
                    x.CodigoEmpresa,
                    x.FSinc
                ))
                .FirstOrDefaultAsync();

            if (empresa == null)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var usuarios = await ScopeUsuarios(_db.Usuarios)
                .AsNoTracking()
                .Where(x => x.EmpresaId == resolvedEmpresaId.Value)
                .OrderBy(x => x.Nombre)
                .Select(x => new UsuarioConfigResponse(
                    x.Id,
                    x.EmpresaId,
                    x.Nombre,
                    x.Correo,
                    x.Rol,
                    x.Estado
                ))
                .ToListAsync();

            var sucursales = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .Where(x => x.EmpresaId == resolvedEmpresaId.Value)
                .OrderBy(x => x.Nombre)
                .Select(x => new SucursalConfigResponse(
                    x.Id,
                    x.EmpresaId,
                    x.Nombre,
                    x.Estado,
                    x.FechaCreacion,
                    x.FechaActualizacion
                ))
                .ToListAsync();

            var sucursalIds = sucursales.Select(x => x.Id).ToList();

            var vendedores = await ScopeVendedores(_db.Vendedores)
                .AsNoTracking()
                .Where(x => sucursalIds.Contains(x.SucursalId))
                .OrderBy(x => x.Nombre)
                .Select(x => new VendedorConfigResponse(
                    x.Id,
                    x.SucursalId,
                    x.Nombre,
                    x.Estado,
                    x.FechaCreacion,
                    x.FechaActualizacion
                ))
                .ToListAsync();

            var cajas = await ScopeCajas(_db.Cajas)
                .AsNoTracking()
                .Where(x => sucursalIds.Contains(x.SucursalId))
                .OrderBy(x => x.Nombre)
                .Select(x => new CajaConfigResponse(
                    x.Id,
                    x.SucursalId,
                    x.Nombre,
                    x.Estado,
                    x.FechaCreacion,
                    x.FechaActualizacion
                ))
                .ToListAsync();

            var clientes = await GetClientesQuery(resolvedEmpresaId.Value)
                .OrderBy(x => x.Nombre)
                .Select(x => new ClienteConfigResponse(
                    x.Id,
                    x.SucursalId,
                    x.Nombre,
                    x.Sucursal.Nombre,
                    x.Estado,
                    x.FechaCreacion,
                    x.FechaActualizacion
                ))
                .ToListAsync();

            return Ok(new ConfiguracionResponse(empresa, usuarios, sucursales, vendedores, cajas, clientes));
        }

        [HttpPost("CrearEmpresa")]
        public async Task<ActionResult<EmpresaConfigResponse>> CrearEmpresa([FromBody] CrearEmpresaRequest request)
        {
            if (!IsAdminUser)
            {
                return Forbid();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.CodigoEmpresa))
            {
                return BadRequest("Nombre y codigo de empresa son requeridos.");
            }

            var codigo = request.CodigoEmpresa.Trim();
            var exists = await _db.Empresas.AnyAsync(x => x.CodigoEmpresa == codigo);

            if (exists)
            {
                return Conflict("Ya existe una empresa con ese codigo.");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var empresa = new Empresas
            {
                Id = await GetNextEmpresaId(),
                Nombre = request.Nombre.Trim(),
                CodigoEmpresa = codigo,
                FSinc = request.FSinc ?? today
            };

            _db.Empresas.Add(empresa);
            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetConfiguracion),
                new { empresaId = empresa.Id },
                new EmpresaConfigResponse(empresa.Id, empresa.Nombre, empresa.CodigoEmpresa, empresa.FSinc)
            );
        }

        [HttpPost("CrearUsuario")]
        public async Task<ActionResult<UsuarioConfigResponse>> CrearUsuario([FromBody] CrearUsuarioRequest request)
        {
            if (!IsAdminUser)
            {
                return Forbid();
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Correo) ||
                string.IsNullOrWhiteSpace(request.Clave) ||
                string.IsNullOrWhiteSpace(request.Rol))
            {
                return BadRequest("Nombre, correo, clave y rol son requeridos.");
            }

            var empresaId = ResolveEmpresaId(request.EmpresaId);

            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var requestedRole = request.Rol.Trim();

            if (!IsUsuarioRole(requestedRole))
            {
                return BadRequest("Desde configuracion solo se pueden crear usuarios con rol usuario.");
            }

            var empresaExists = await ScopeEmpresas(_db.Empresas).AnyAsync(x => x.Id == empresaId.Value);

            if (!empresaExists)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var correo = request.Correo.Trim();
            var exists = await _db.Usuarios.AnyAsync(x => x.Correo == correo);

            if (exists)
            {
                return Conflict("Ya existe un usuario con ese correo.");
            }

            var usuario = new Usuarios
            {
                Id = await GetNextUsuarioId(),
                EmpresaId = empresaId.Value,
                Nombre = request.Nombre.Trim(),
                Correo = correo,
                Clave = BCrypt.Net.BCrypt.HashPassword(request.Clave),
                Rol = "usuario",
                Estado = NormalizeEstado(request.Estado)
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            var response = new UsuarioConfigResponse(
                usuario.Id,
                usuario.EmpresaId,
                usuario.Nombre,
                usuario.Correo,
                usuario.Rol,
                usuario.Estado
            );

            return CreatedAtAction(nameof(GetConfiguracion), new { empresaId = usuario.EmpresaId }, response);
        }

        [HttpPut("ActualizarEmpresa/{id:int}")]
        public async Task<ActionResult<EmpresaConfigResponse>> ActualizarEmpresa(int id, [FromBody] ActualizarEmpresaRequest request)
        {
            if (!IsAdminUser)
            {
                return Forbid();
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.CodigoEmpresa))
            {
                return BadRequest("Nombre y codigo de empresa son requeridos.");
            }

            var empresa = await _db.Empresas.FirstOrDefaultAsync(x => x.Id == id);

            if (empresa == null)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var codigo = request.CodigoEmpresa.Trim();
            var exists = await _db.Empresas.AnyAsync(x => x.Id != id && x.CodigoEmpresa == codigo);

            if (exists)
            {
                return Conflict("Ya existe una empresa con ese codigo.");
            }

            empresa.Nombre = request.Nombre.Trim();
            empresa.CodigoEmpresa = codigo;
            empresa.FSinc = request.FSinc ?? empresa.FSinc;

            await _db.SaveChangesAsync();

            return Ok(new EmpresaConfigResponse(empresa.Id, empresa.Nombre, empresa.CodigoEmpresa, empresa.FSinc));
        }

        [HttpPut("ActualizarUsuario/{id:int}")]
        public async Task<ActionResult<UsuarioConfigResponse>> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequest request)
        {
            if (!IsAdminUser)
            {
                return Forbid();
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Correo))
            {
                return BadRequest("Nombre y correo son requeridos.");
            }

            var empresaId = ResolveEmpresaId(request.EmpresaId);

            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var empresaExists = await ScopeEmpresas(_db.Empresas).AnyAsync(x => x.Id == empresaId.Value);

            if (!empresaExists)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var usuario = await ScopeUsuarios(_db.Usuarios).FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
            {
                return NotFound("No se encontro el usuario solicitado.");
            }

            var correo = request.Correo.Trim();
            var exists = await _db.Usuarios.AnyAsync(x => x.Id != id && x.Correo == correo);

            if (exists)
            {
                return Conflict("Ya existe un usuario con ese correo.");
            }

            usuario.EmpresaId = empresaId.Value;
            usuario.Nombre = request.Nombre.Trim();
            usuario.Correo = correo;
            usuario.Estado = NormalizeEstado(request.Estado);

            if (!string.IsNullOrWhiteSpace(request.Clave))
            {
                usuario.Clave = BCrypt.Net.BCrypt.HashPassword(request.Clave);
            }

            if (!IsAdminRole(usuario.Rol))
            {
                usuario.Rol = "usuario";
            }

            await _db.SaveChangesAsync();

            return Ok(new UsuarioConfigResponse(
                usuario.Id,
                usuario.EmpresaId,
                usuario.Nombre,
                usuario.Correo,
                usuario.Rol,
                usuario.Estado
            ));
        }

        [HttpGet("BuscarClientes")]
        public async Task<ActionResult<IEnumerable<ClienteConfigResponse>>> BuscarClientes(string clienteResult, int? empresaId = null)
        {
            if (string.IsNullOrWhiteSpace(clienteResult))
            {
                return BadRequest("El nombre del cliente es requerido.");
            }

            var resolvedEmpresaId = ResolveEmpresaId(empresaId);

            if (!resolvedEmpresaId.HasValue)
            {
                return Forbid();
            }

            var search = clienteResult.Trim();

            var clientes = await GetClientesQuery(resolvedEmpresaId.Value)
                .Where(x => x.Nombre.Contains(search))
                .OrderBy(x => x.Nombre)
                .Take(20)
                .Select(x => new ClienteConfigResponse(
                    x.Id,
                    x.SucursalId,
                    x.Nombre,
                    x.Sucursal.Nombre,
                    x.Estado,
                    x.FechaCreacion,
                    x.FechaActualizacion
                ))
                .ToListAsync();

            return Ok(clientes);
        }

        [HttpPost("RegistrarVenta")]
        public async Task<ActionResult<VentaConfigResponse>> RegistrarVenta([FromBody] RegistrarVentaRequest request)
        {
            if (request == null)
            {
                return BadRequest("Los datos de la venta son requeridos.");
            }

            var empresaId = ResolveEmpresaId(request.EmpresaId);

            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId && x.EmpresaId == empresaId.Value);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para registrar la venta.");
            }

            var cajaExists = await ScopeCajas(_db.Cajas)
                .AnyAsync(x => x.Id == request.CajaId && x.SucursalId == sucursal.Id);

            if (!cajaExists)
            {
                return NotFound("No se encontro una caja permitida para registrar la venta.");
            }

            var vendedorExists = await ScopeVendedores(_db.Vendedores)
                .AnyAsync(x => x.Id == request.VendedorId && x.SucursalId == sucursal.Id);

            if (!vendedorExists)
            {
                return NotFound("No se encontro un vendedor permitido para registrar la venta.");
            }

            var cliente = await ResolveClienteForVenta(request, empresaId.Value);

            if (cliente == null)
            {
                return BadRequest("Debe seleccionar un cliente existente o escribir el nombre de un cliente.");
            }

            var venta = new VentaBase
            {
                Id = await GetNextVentaBaseId(),
                EmpresaId = empresaId.Value,
                SucursalId = sucursal.Id,
                CajaId = request.CajaId,
                VendedorId = request.VendedorId,
                ClienteId = cliente.Id,
                Subtotal = request.Subtotal ?? 0,
                Descuento = request.Descuento ?? 0,
                Impuesto = request.Impuesto ?? 0,
                CantidadFacturas = request.CantidadFacturas ?? 1,
                CantidadDevoluciones = request.CantidadDevoluciones ?? 0,
                Factura = request.Factura ?? request.Total ?? 0,
                Devolucion = request.Devolucion ?? 0,
                Total = request.Total ?? ((request.Factura ?? 0) - (request.Devolucion ?? 0)),
                Efectivo = request.Efectivo ?? 0,
                Tarjeta = request.Tarjeta ?? 0,
                Cheque = request.Cheque ?? 0,
                Bonos = request.Bonos ?? 0,
                NotaCredito = request.NotaCredito ?? 0,
                Credito = request.Credito ?? 0,
                Contado = request.Contado ?? 0,
                Fecha = request.Fecha ?? DateOnly.FromDateTime(DateTime.Today)
            };

            _db.VentaBase.Add(venta);
            await _db.SaveChangesAsync();

            var clienteResponse = new ClienteConfigResponse(
                cliente.Id,
                cliente.SucursalId,
                cliente.Nombre,
                cliente.Sucursal?.Nombre ?? sucursal.Nombre,
                cliente.Estado,
                cliente.FechaCreacion,
                cliente.FechaActualizacion
            );

            var response = new VentaConfigResponse(
                venta.Id,
                venta.EmpresaId,
                venta.SucursalId,
                venta.CajaId,
                venta.VendedorId,
                venta.ClienteId,
                clienteResponse,
                venta.Subtotal,
                venta.Descuento,
                venta.Impuesto,
                venta.CantidadFacturas,
                venta.CantidadDevoluciones,
                venta.Factura,
                venta.Devolucion,
                venta.Total,
                venta.Efectivo,
                venta.Tarjeta,
                venta.Cheque,
                venta.Bonos,
                venta.NotaCredito,
                venta.Credito,
                venta.Contado,
                venta.Fecha
            );

            return CreatedAtAction(nameof(GetConfiguracion), new { empresaId = venta.EmpresaId }, response);
        }

        [HttpPost("CrearSucursal")]
        public async Task<ActionResult<SucursalConfigResponse>> CrearSucursal([FromBody] CrearSucursalRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre de la sucursal es requerido.");
            }

            var empresaId = ResolveEmpresaId(request.EmpresaId);

            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var empresaExists = await ScopeEmpresas(_db.Empresas).AnyAsync(x => x.Id == empresaId.Value);

            if (!empresaExists)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var sucursal = new Sucursales
            {
                Id = await GetNextSucursalId(),
                EmpresaId = empresaId.Value,
                Nombre = request.Nombre.Trim(),
                Estado = NormalizeEstado(request.Estado),
                FechaCreacion = request.FechaCreacion ?? today,
                FechaActualizacion = request.FechaActualizacion ?? today
            };

            _db.Sucursales.Add(sucursal);
            await _db.SaveChangesAsync();

            var response = new SucursalConfigResponse(
                sucursal.Id,
                sucursal.EmpresaId,
                sucursal.Nombre,
                sucursal.Estado,
                sucursal.FechaCreacion,
                sucursal.FechaActualizacion
            );

            return CreatedAtAction(nameof(GetConfiguracion), new { empresaId = sucursal.EmpresaId }, response);
        }

        [HttpPut("ActualizarSucursal/{id:int}")]
        public async Task<ActionResult<SucursalConfigResponse>> ActualizarSucursal(int id, [FromBody] ActualizarSucursalRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre de la sucursal es requerido.");
            }

            var empresaId = ResolveEmpresaId(request.EmpresaId);

            if (!empresaId.HasValue)
            {
                return Forbid();
            }

            var empresaExists = await ScopeEmpresas(_db.Empresas).AnyAsync(x => x.Id == empresaId.Value);

            if (!empresaExists)
            {
                return NotFound("No se encontro la empresa solicitada.");
            }

            var sucursal = await ScopeSucursales(_db.Sucursales).FirstOrDefaultAsync(x => x.Id == id);

            if (sucursal == null)
            {
                return NotFound("No se encontro la sucursal solicitada.");
            }

            sucursal.EmpresaId = empresaId.Value;
            sucursal.Nombre = request.Nombre.Trim();
            sucursal.Estado = NormalizeEstado(request.Estado);
            sucursal.FechaActualizacion = request.FechaActualizacion ?? DateOnly.FromDateTime(DateTime.Today);

            await _db.SaveChangesAsync();

            return Ok(new SucursalConfigResponse(
                sucursal.Id,
                sucursal.EmpresaId,
                sucursal.Nombre,
                sucursal.Estado,
                sucursal.FechaCreacion,
                sucursal.FechaActualizacion
            ));
        }

        [HttpPost("CrearVendedor")]
        public async Task<ActionResult<VendedorConfigResponse>> CrearVendedor([FromBody] CrearVendedorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del vendedor es requerido.");
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para asignar el vendedor.");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var vendedor = new Vendedores
            {
                Id = await GetNextVendedorId(),
                SucursalId = sucursal.Id,
                Nombre = request.Nombre.Trim(),
                Estado = NormalizeEstado(request.Estado),
                FechaCreacion = request.FechaCreacion ?? today,
                FechaActualizacion = request.FechaActualizacion ?? today
            };

            _db.Vendedores.Add(vendedor);
            await _db.SaveChangesAsync();

            var response = new VendedorConfigResponse(
                vendedor.Id,
                vendedor.SucursalId,
                vendedor.Nombre,
                vendedor.Estado,
                vendedor.FechaCreacion,
                vendedor.FechaActualizacion
            );

            return CreatedAtAction(nameof(GetConfiguracion), new { empresaId = sucursal.EmpresaId }, response);
        }

        [HttpPut("ActualizarVendedor/{id:int}")]
        public async Task<ActionResult<VendedorConfigResponse>> ActualizarVendedor(int id, [FromBody] ActualizarVendedorRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del vendedor es requerido.");
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para asignar el vendedor.");
            }

            var vendedor = await ScopeVendedores(_db.Vendedores).FirstOrDefaultAsync(x => x.Id == id);

            if (vendedor == null)
            {
                return NotFound("No se encontro el vendedor solicitado.");
            }

            vendedor.SucursalId = sucursal.Id;
            vendedor.Nombre = request.Nombre.Trim();
            vendedor.Estado = NormalizeEstado(request.Estado);
            vendedor.FechaActualizacion = request.FechaActualizacion ?? DateOnly.FromDateTime(DateTime.Today);

            await _db.SaveChangesAsync();

            return Ok(new VendedorConfigResponse(
                vendedor.Id,
                vendedor.SucursalId,
                vendedor.Nombre,
                vendedor.Estado,
                vendedor.FechaCreacion,
                vendedor.FechaActualizacion
            ));
        }

        [HttpPost("CrearCaja")]
        public async Task<ActionResult<CajaConfigResponse>> CrearCaja([FromBody] CrearCajaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre de la caja es requerido.");
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para asignar la caja.");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var caja = new Cajas
            {
                Id = await GetNextCajaId(),
                SucursalId = sucursal.Id,
                Nombre = request.Nombre.Trim(),
                Estado = NormalizeEstado(request.Estado),
                FechaCreacion = request.FechaCreacion ?? today,
                FechaActualizacion = request.FechaActualizacion ?? today
            };

            _db.Cajas.Add(caja);
            await _db.SaveChangesAsync();

            var response = new CajaConfigResponse(
                caja.Id,
                caja.SucursalId,
                caja.Nombre,
                caja.Estado,
                caja.FechaCreacion,
                caja.FechaActualizacion
            );

            return CreatedAtAction(nameof(GetConfiguracion), new { empresaId = sucursal.EmpresaId }, response);
        }

        [HttpPut("ActualizarCaja/{id:int}")]
        public async Task<ActionResult<CajaConfigResponse>> ActualizarCaja(int id, [FromBody] ActualizarCajaRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre de la caja es requerido.");
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para asignar la caja.");
            }

            var caja = await ScopeCajas(_db.Cajas).FirstOrDefaultAsync(x => x.Id == id);

            if (caja == null)
            {
                return NotFound("No se encontro la caja solicitada.");
            }

            caja.SucursalId = sucursal.Id;
            caja.Nombre = request.Nombre.Trim();
            caja.Estado = NormalizeEstado(request.Estado);
            caja.FechaActualizacion = request.FechaActualizacion ?? DateOnly.FromDateTime(DateTime.Today);

            await _db.SaveChangesAsync();

            return Ok(new CajaConfigResponse(
                caja.Id,
                caja.SucursalId,
                caja.Nombre,
                caja.Estado,
                caja.FechaCreacion,
                caja.FechaActualizacion
            ));
        }

        [HttpPut("ActualizarCliente/{id:int}")]
        public async Task<ActionResult<ClienteConfigResponse>> ActualizarCliente(int id, [FromBody] ActualizarClienteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del cliente es requerido.");
            }

            if (!await CanAccessCliente(id))
            {
                return Forbid();
            }

            var sucursal = await ScopeSucursales(_db.Sucursales)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.SucursalId);

            if (sucursal == null)
            {
                return NotFound("No se encontro una sucursal permitida para asignar el cliente.");
            }

            var cliente = await _db.Clientes.FirstOrDefaultAsync(x => x.Id == id);

            if (cliente == null)
            {
                return NotFound("No se encontro el cliente solicitado.");
            }

            cliente.SucursalId = sucursal.Id;
            cliente.Nombre = request.Nombre.Trim();
            cliente.Estado = NormalizeEstado(request.Estado);
            cliente.FechaActualizacion = request.FechaActualizacion ?? DateOnly.FromDateTime(DateTime.Today);

            await _db.SaveChangesAsync();

            return Ok(new ClienteConfigResponse(
                cliente.Id,
                cliente.SucursalId,
                cliente.Nombre,
                sucursal.Nombre,
                cliente.Estado,
                cliente.FechaCreacion,
                cliente.FechaActualizacion
            ));
        }

        private int? ResolveEmpresaId(int? requestedEmpresaId)
        {
            if (IsAdminUser)
            {
                return requestedEmpresaId ?? CurrentEmpresaId;
            }

            var currentEmpresaId = CurrentEmpresaId;

            if (!currentEmpresaId.HasValue)
            {
                return null;
            }

            if (requestedEmpresaId.HasValue && requestedEmpresaId.Value != currentEmpresaId.Value)
            {
                return null;
            }

            return currentEmpresaId.Value;
        }

        private static bool IsUsuarioRole(string role)
        {
            return string.Equals(role, "usuario", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "user", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdminRole(string role)
        {
            return string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "administrador", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return "activo";
            }

            var normalized = estado.Trim().ToLowerInvariant();
            return normalized == "inactivo" ? "inactivo" : "activo";
        }

        private async Task<int> GetNextEmpresaId()
        {
            return (await _db.Empresas.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextUsuarioId()
        {
            return (await _db.Usuarios.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextClienteId()
        {
            return (await _db.Clientes.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextSucursalId()
        {
            return (await _db.Sucursales.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextVendedorId()
        {
            return (await _db.Vendedores.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextCajaId()
        {
            return (await _db.Cajas.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<int> GetNextVentaBaseId()
        {
            return (await _db.VentaBase.Select(x => (int?)x.Id).MaxAsync() ?? 0) + 1;
        }

        private async Task<Clientes?> ResolveClienteForVenta(RegistrarVentaRequest request, int empresaId)
        {
            if (request.ClienteId.HasValue)
            {
                return await _db.Clientes
                    .Include(x => x.Sucursal)
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ClienteId.Value &&
                        x.SucursalId == request.SucursalId &&
                        x.Sucursal.EmpresaId == empresaId);
            }

            if (string.IsNullOrWhiteSpace(request.ClienteNombre))
            {
                return null;
            }

            var nombre = request.ClienteNombre.Trim();
            var cliente = await _db.Clientes
                .Include(x => x.Sucursal)
                .FirstOrDefaultAsync(x =>
                    x.Nombre == nombre &&
                    x.SucursalId == request.SucursalId &&
                    x.Sucursal.EmpresaId == empresaId);

            if (cliente != null)
            {
                return cliente;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            cliente = new Clientes
            {
                Id = await GetNextClienteId(),
                SucursalId = request.SucursalId,
                Nombre = nombre,
                Estado = "activo",
                FechaCreacion = today,
                FechaActualizacion = today
            };

            _db.Clientes.Add(cliente);
            await _db.SaveChangesAsync();

            return cliente;
        }

        private IQueryable<Clientes> GetClientesQuery(int empresaId)
        {
            var clientes = ScopeClientes(_db.Clientes)
                .Include(x => x.Sucursal)
                .AsNoTracking();

            return IsAdminUser
                ? clientes
                : clientes.Where(x => x.Sucursal.EmpresaId == empresaId);
        }

        private async Task<bool> CanAccessCliente(int clienteId)
        {
            if (IsAdminUser)
            {
                return await _db.Clientes.AnyAsync(x => x.Id == clienteId);
            }

            var empresaId = CurrentEmpresaId;

            if (!empresaId.HasValue)
            {
                return false;
            }

            return await _db.Clientes
                .AsNoTracking()
                .AnyAsync(x => x.Id == clienteId && x.Sucursal.EmpresaId == empresaId.Value);
        }

        public sealed record ConfiguracionResponse(
            EmpresaConfigResponse Empresa,
            IEnumerable<UsuarioConfigResponse> Usuarios,
            IEnumerable<SucursalConfigResponse> Sucursales,
            IEnumerable<VendedorConfigResponse> Vendedores,
            IEnumerable<CajaConfigResponse> Cajas,
            IEnumerable<ClienteConfigResponse> Clientes
        );

        public sealed record CrearEmpresaRequest(string Nombre, string CodigoEmpresa, DateOnly? FSinc);

        public sealed record ActualizarEmpresaRequest(string Nombre, string CodigoEmpresa, DateOnly? FSinc);

        public sealed record CrearUsuarioRequest(
            int? EmpresaId,
            string Nombre,
            string Correo,
            string Clave,
            string Rol,
            string? Estado
        );

        public sealed record ActualizarUsuarioRequest(
            int? EmpresaId,
            string Nombre,
            string Correo,
            string? Clave,
            string? Rol,
            string? Estado
        );

        public sealed record RegistrarVentaRequest(
            int? EmpresaId,
            int SucursalId,
            int CajaId,
            int VendedorId,
            int? ClienteId,
            string? ClienteNombre,
            decimal? Subtotal,
            decimal? Descuento,
            decimal? Impuesto,
            int? CantidadFacturas,
            int? CantidadDevoluciones,
            decimal? Factura,
            decimal? Devolucion,
            decimal? Total,
            decimal? Efectivo,
            decimal? Tarjeta,
            decimal? Cheque,
            decimal? Bonos,
            decimal? NotaCredito,
            decimal? Credito,
            decimal? Contado,
            DateOnly? Fecha
        );

        public sealed record CrearSucursalRequest(
            int? EmpresaId,
            string Nombre,
            string? Estado,
            DateOnly? FechaCreacion,
            DateOnly? FechaActualizacion
        );

        public sealed record ActualizarSucursalRequest(
            int? EmpresaId,
            string Nombre,
            string? Estado,
            DateOnly? FechaActualizacion
        );

        public sealed record CrearVendedorRequest(
            int SucursalId,
            string Nombre,
            string? Estado,
            DateOnly? FechaCreacion,
            DateOnly? FechaActualizacion
        );

        public sealed record ActualizarVendedorRequest(
            int SucursalId,
            string Nombre,
            string? Estado,
            DateOnly? FechaActualizacion
        );

        public sealed record CrearCajaRequest(
            int SucursalId,
            string Nombre,
            string? Estado,
            DateOnly? FechaCreacion,
            DateOnly? FechaActualizacion
        );

        public sealed record ActualizarCajaRequest(
            int SucursalId,
            string Nombre,
            string? Estado,
            DateOnly? FechaActualizacion
        );

        public sealed record ActualizarClienteRequest(
            int SucursalId,
            string Nombre,
            string? Estado,
            DateOnly? FechaActualizacion
        );

        public sealed record EmpresaConfigResponse(int Id, string Nombre, string CodigoEmpresa, DateOnly FSinc);

        public sealed record UsuarioConfigResponse(
            int Id,
            int EmpresaId,
            string Nombre,
            string Correo,
            string Rol,
            string Estado
        );

        public sealed record ClienteConfigResponse(
            int Id,
            int SucursalId,
            string Nombre,
            string SucursalNombre,
            string Estado,
            DateOnly FechaCreacion,
            DateOnly FechaActualizacion
        );

        public sealed record VentaConfigResponse(
            int Id,
            int EmpresaId,
            int SucursalId,
            int CajaId,
            int VendedorId,
            int ClienteId,
            ClienteConfigResponse Cliente,
            decimal Subtotal,
            decimal Descuento,
            decimal Impuesto,
            int CantidadFacturas,
            int CantidadDevoluciones,
            decimal Factura,
            decimal Devolucion,
            decimal Total,
            decimal Efectivo,
            decimal Tarjeta,
            decimal Cheque,
            decimal Bonos,
            decimal NotaCredito,
            decimal Credito,
            decimal Contado,
            DateOnly Fecha
        );

        public sealed record SucursalConfigResponse(
            int Id,
            int EmpresaId,
            string Nombre,
            string Estado,
            DateOnly FechaCreacion,
            DateOnly FechaActualizacion
        );

        public sealed record VendedorConfigResponse(
            int Id,
            int SucursalId,
            string Nombre,
            string Estado,
            DateOnly FechaCreacion,
            DateOnly FechaActualizacion
        );

        public sealed record CajaConfigResponse(
            int Id,
            int SucursalId,
            string Nombre,
            string Estado,
            DateOnly FechaCreacion,
            DateOnly FechaActualizacion
        );
    }
}
