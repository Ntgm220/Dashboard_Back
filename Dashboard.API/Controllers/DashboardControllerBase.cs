using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.API.Controllers
{
    public abstract class DashboardControllerBase : ControllerBase
    {
        protected bool IsAdminUser =>
            User.IsInRole("admin") ||
            User.IsInRole("Admin") ||
            User.IsInRole("ADMIN") ||
            User.IsInRole("administrador") ||
            User.IsInRole("Administrador");

        protected int? CurrentEmpresaId
        {
            get
            {
                var value = User.FindFirst("empresa_id")?.Value;
                return int.TryParse(value, out var empresaId) ? empresaId : null;
            }
        }

        protected IQueryable<VentaBase> ScopeVentaBase(IQueryable<VentaBase> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Sucursales> ScopeSucursales(IQueryable<Sucursales> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Cajas> ScopeCajas(IQueryable<Cajas> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.Sucursal.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Vendedores> ScopeVendedores(IQueryable<Vendedores> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.Sucursal.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Empresas> ScopeEmpresas(IQueryable<Empresas> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.Id == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<EmpresasLogs> ScopeEmpresasLogs(IQueryable<EmpresasLogs> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Usuarios> ScopeUsuarios(IQueryable<Usuarios> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }

        protected IQueryable<Clientes> ScopeClientes(IQueryable<Clientes> query)
        {
            if (IsAdminUser)
            {
                return query;
            }

            var empresaId = CurrentEmpresaId;
            return empresaId.HasValue
                ? query.Where(x => x.Sucursal.EmpresaId == empresaId.Value)
                : query.Where(x => false);
        }
    }
}
