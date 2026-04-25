# Dashboard Empresarial — Backend

API REST construida con **ASP.NET Core + Entity Framework Core** para soportar la lógica de autenticación, control de acceso, configuración administrativa y consulta de datos operativos y financieros del sistema Dashboard Empresarial.

## Resumen

Este proyecto corresponde a la capa **Backend** del sistema. Fue diseñado para trabajar con **SQL Server** y una base de datos llamada **DashboardDB**.

Su propósito es:

- Exponer endpoints para dashboard financiero y operativo.
- Aplicar autenticación por JWT.
- Restringir información según rol y empresa.
- Gestionar entidades administrativas.
- Registrar ventas operativas.

## Tecnologías principales

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core 10
- SQL Server
- JWT Bearer Authentication
- BCrypt para hash de contraseñas
- Swagger en entorno de desarrollo

## Arquitectura por capas

- `Dashboard.API`: controladores, configuración HTTP, autenticación, CORS y Swagger
- `Dashboard.Data`: entidades y `DashboardDbContext`
- `Dashboard.Infrastructure`: DTOs, interfaces y `AuthService`

## Base de datos utilizada

La solución trabaja con estas tablas principales:

- `empresas`
- `Usuarios`
- `sucursales`
- `cajas`
- `vendedores`
- `clientes`
- `VentaBase`
- `empresas_logs`

`VentaBase` es la tabla central para la información de ventas y alimenta gran parte del dashboard.

## Funcionalidades principales

### 1. Autenticación y sesión

Inicio de sesión por correo y contraseña.

El backend genera JWT con claims como:
- `sub`
- `email`
- `name`
- `role`
- `empresa_id`

### 2. Control de acceso por rol

**Administrador**
- Puede ver información global.
- Puede gestionar cualquier empresa.
- Puede crear empresas.
- Puede crear usuarios operativos.
- Puede actualizar registros administrativos.

**Usuario no admin**
- Solo puede consultar información asociada a su `empresa_id`.
- Solo puede operar sobre sucursales y registros de su empresa.
- Puede registrar ventas desde el módulo operativo.

La lógica de alcance está centralizada en `DashboardControllerBase`, con métodos como:
- `ScopeVentaBase`
- `ScopeSucursales`
- `ScopeCajas`
- `ScopeVendedores`
- `ScopeEmpresas`
- `ScopeEmpresasLogs`
- `ScopeUsuarios`
- `ScopeClientes`

## Endpoints principales

### Autenticación
- `POST /Usuario/login`

### Dashboard
- `GET /DashboardOverview/GetPerformanceOverview`
- `GET /DashboardOverview/GetRevenue`
- `GET /DashboardOverview/GetNewCustomers`
- `GET /DashboardOverview/GetChurnRate`
- `GET /DashboardOverview/GetActiveUsers`
- `GET /DashboardOverview/GetArpc`
- `GET /DashboardOverview/GetCustomerSummary`
- `GET /DashboardOverview/GetRecentDashboardsAndReports`
- `GET /DashboardOverview/GetDataCapabilities`
- `GET /DashboardOverview/GetFinancialSummaryData`

### Configuración
- `GET /Configuracion/GetConfiguracion`
- `GET /Configuracion/BuscarClientes`
- `POST /Configuracion/CrearEmpresa`
- `POST /Configuracion/CrearUsuario`
- `POST /Configuracion/CrearSucursal`
- `POST /Configuracion/CrearVendedor`
- `POST /Configuracion/CrearCaja`
- `POST /Configuracion/RegistrarVenta`
- `PUT /Configuracion/ActualizarEmpresa/{id}`
- `PUT /Configuracion/ActualizarUsuario/{id}`
- `PUT /Configuracion/ActualizarSucursal/{id}`
- `PUT /Configuracion/ActualizarVendedor/{id}`
- `PUT /Configuracion/ActualizarCaja/{id}`
- `PUT /Configuracion/ActualizarCliente/{id}`

### Consultas por entidad
- `GET /Empresa/GetEmpresa`
- `GET /Empresa/GetEmpresaByNombre`
- `GET /Usuario/GetUsuario`
- `GET /Usuario/GetUsuarioByNombre`
- `GET /Sucursal/GetSucursal`
- `GET /Sucursal/GetSucursalByNombre`
- `GET /Caja/GetCaja`
- `GET /Caja/GetCajaByNombre`
- `GET /Cliente/GetCliente`
- `GET /Cliente/GetClienteByNombre`
- `GET /Vendedor/GetVendedor`
- `GET /Vendedor/GetVendedorByNombre`
- `GET /VentaBase/GetVentaBase`
- `GET /VentaBase/GetVentaBaseBySucursal`
- `GET /VentaBase/GetVentaBaseByCliente`
- `GET /VentaBase/GetVentaBaseByVendedor`
- `GET /VentaBase/GetVentaBaseByCaja`
- `GET /EmpresasLog/GetEmpresasLog`
- `GET /EmpresasLog/GetEmpresasLogByEmpresa`

## Configuración local

### Requisitos

- .NET SDK 10
- SQL Server
- Base de datos `DashboardDB` creada y accesible

### Configuración actual en `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "TuConnectionString"
  },
  "Jwt": {
    "Key": "tu_clave_super_secreta_con_bastantes_caracteres",
    "Issuer": "DashboardAPI",
    "Audience": "DashboardFrontend",
    "ExpiresMinutes": 120
  }
}
```

### Ejecución

```bash
cd Dahsboard_Project/Dashboard.API
dotnet restore
dotnet run
```

Swagger queda disponible en desarrollo.

## CORS

El backend tiene configurado CORS para:

```text
http://localhost:tulocalhostOurl
```

Si el frontend se levanta en otro puerto o dominio, debe actualizarse la política `AllowFrontend` en `Program.cs`.

## Seguridad implementada

- JWT Bearer Authentication
- Fallback policy para requerir usuarios autenticados por defecto
- Hash de contraseñas con BCrypt
- Rehash automático si se detecta una contraseña almacenada sin BCrypt
- Restricción de datos por empresa para usuarios no admin

## Consideraciones de negocio observadas

- `Usuarios.estado` determina si un usuario puede iniciar sesión.
- Los estados operativos se manejan con los valores `activo / inactivo`.
- `VentaBase.total` es la fuente principal para ingresos y venta neta.
- Los clientes están asociados a una sucursal (`clientes.sucursal_id`).
- Los IDs se manejan de forma manual desde backend en varios flujos de configuración.

## Posibles mejoras futuras

- Agregar pruebas automáticas para backend.
- Documentar contratos JSON de cada endpoint con ejemplos.
- Separar DTOs de lectura y escritura por módulo.
- Agregar paginación real en endpoints de listados.
- Mover secretos y cadenas de conexión a variables de entorno.
- Agregar auditoría de cambios administrativos.
- Incluir pipeline CI/CD para build y validación automática.

## Estado del proyecto

Proyecto funcional y entregado, con autenticación, control por rol, dashboard financiero, administración operativa y registro de ventas sobre SQL Server.
