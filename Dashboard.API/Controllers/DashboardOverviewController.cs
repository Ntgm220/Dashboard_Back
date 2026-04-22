using Dashboard.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashboardOverviewController : DashboardControllerBase
    {
        private readonly DashboardDbContext _db;

        public DashboardOverviewController(DashboardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetPerformanceOverview")]
        public async Task<ActionResult<PerformanceOverviewResponse>> GetPerformanceOverview(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            var metrics = new List<DashboardMetricResponse>
            {
                await GetRevenueMetric(period.From, period.To),
                await GetNewCustomersMetric(period.From, period.To),
                UnsupportedMetric("websiteSessions", "Website Sessions", "No existe tabla o integracion de analitica web para sesiones."),
                UnsupportedMetric("pageViews", "Page Views", "No existe tabla o integracion de analitica web para vistas de pagina."),
                await GetChurnRateMetric(period.From, period.To),
                await GetActiveUsersMetric(),
                await GetArpcMetric(period.From, period.To),
                UnsupportedMetric("bounceRate", "Bounce Rate", "No existe tabla o integracion de analitica web para rebote.")
            };

            return Ok(new PerformanceOverviewResponse(period.From, period.To, metrics));
        }

        [HttpGet("GetRevenue")]
        public async Task<ActionResult<DashboardMetricResponse>> GetRevenue(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            return Ok(await GetRevenueMetric(period.From, period.To));
        }

        [HttpGet("GetNewCustomers")]
        public async Task<ActionResult<DashboardMetricResponse>> GetNewCustomers(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            return Ok(await GetNewCustomersMetric(period.From, period.To));
        }

        [HttpGet("GetChurnRate")]
        public async Task<ActionResult<DashboardMetricResponse>> GetChurnRate(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            return Ok(await GetChurnRateMetric(period.From, period.To));
        }

        [HttpGet("GetActiveUsers")]
        public async Task<ActionResult<DashboardMetricResponse>> GetActiveUsers()
        {
            return Ok(await GetActiveUsersMetric());
        }

        [HttpGet("GetArpc")]
        public async Task<ActionResult<DashboardMetricResponse>> GetArpc(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            return Ok(await GetArpcMetric(period.From, period.To));
        }

        [HttpGet("GetCustomerSummary")]
        public async Task<ActionResult<CustomerSummaryResponse>> GetCustomerSummary(DateTime? from = null, DateTime? to = null)
        {
            var period = ResolvePeriod(from, to);
            if (period.From > period.To)
            {
                return BadRequest("La fecha inicial no puede ser mayor que la fecha final.");
            }

            var clientes = GetClientesQuery();
            var totalCustomers = await clientes.CountAsync();
            var activeCustomers = await clientes.CountAsync(x => x.Estado == "activo");
            var inactiveCustomers = totalCustomers - activeCustomers;
            var newCustomers = await GetNewCustomersCount(period.From, period.To);
            var churnRate = await GetChurnRateValue(period.From, period.To);

            return Ok(new CustomerSummaryResponse(
                totalCustomers,
                activeCustomers,
                inactiveCustomers,
                newCustomers,
                churnRate,
                "La disponibilidad se gestiona con clientes.estado. Clientes nuevos usan fecha_creacion y churn usa estado/fecha_actualizacion."
            ));
        }

        [HttpGet("GetRecentDashboardsAndReports")]
        public ActionResult<UnsupportedFeatureResponse> GetRecentDashboardsAndReports()
        {
            return Ok(new UnsupportedFeatureResponse(
                "recentDashboardsAndReports",
                false,
                "La base de datos no tiene tablas para dashboards, reportes o historial de visualizaciones."
            ));
        }

        [HttpGet("GetDataCapabilities")]
        public ActionResult<IEnumerable<DataCapabilityResponse>> GetDataCapabilities()
        {
            var capabilities = new List<DataCapabilityResponse>
            {
                new("revenue", true, "Calculado con VentaBase.total."),
                new("activeUsers", true, "Calculado con Usuarios.estado."),
                new("arpc", true, "Calculado con VentaBase.total dividido entre clientes distintos con ventas."),
                new("customerTotals", true, "Calculado con clientes.estado."),
                new("newCustomers", true, "Calculado con clientes.fecha_creacion."),
                new("churnRate", true, "Estimado con clientes.estado y clientes.fecha_actualizacion."),
                new("financialSummary", true, "Calculado con VentaBase y sus relaciones a clientes, vendedores, sucursales y cajas."),
                new("websiteSessions", false, "Falta tabla o integracion de analitica web."),
                new("pageViews", false, "Falta tabla o integracion de analitica web."),
                new("bounceRate", false, "Falta tabla o integracion de analitica web."),
                new("recentDashboardsAndReports", false, "Falta tabla de reportes/dashboards e historial de visualizacion.")
            };

            return Ok(capabilities);
        }

        [HttpGet("GetFinancialSummaryData")]
        public async Task<ActionResult<FinancialSummaryDataResponse>> GetFinancialSummaryData()
        {
            var clientes = await GetFinancialSaleItems("Cliente").ToListAsync();
            var vendedores = await GetFinancialSaleItems("Vendedor").ToListAsync();
            var sucursales = await GetFinancialSaleItems("Sucursal").ToListAsync();
            var tipoPago = await GetPaymentItems().ToListAsync();

            return Ok(new FinancialSummaryDataResponse(clientes, vendedores, sucursales, tipoPago));
        }

        private async Task<DashboardMetricResponse> GetRevenueMetric(DateOnly from, DateOnly to)
        {
            var currentRevenue = await GetRevenueTotal(from, to);
            var previousPeriod = ResolvePreviousPeriod(from, to);
            var previousRevenue = await GetRevenueTotal(previousPeriod.From, previousPeriod.To);

            return new DashboardMetricResponse(
                "revenue",
                "Revenue",
                true,
                currentRevenue,
                "currency",
                CalculateChangePercentage(currentRevenue, previousRevenue),
                "VentaBase.total",
                null
            );
        }

        private async Task<DashboardMetricResponse> GetNewCustomersMetric(DateOnly from, DateOnly to)
        {
            var currentNewCustomers = await GetNewCustomersCount(from, to);
            var previousPeriod = ResolvePreviousPeriod(from, to);
            var previousNewCustomers = await GetNewCustomersCount(previousPeriod.From, previousPeriod.To);

            return new DashboardMetricResponse(
                "newCustomers",
                "New Customers",
                true,
                currentNewCustomers,
                "count",
                CalculateChangePercentage(currentNewCustomers, previousNewCustomers),
                "clientes.fecha_creacion",
                null
            );
        }

        private async Task<DashboardMetricResponse> GetChurnRateMetric(DateOnly from, DateOnly to)
        {
            var currentChurnRate = await GetChurnRateValue(from, to);
            var previousPeriod = ResolvePreviousPeriod(from, to);
            var previousChurnRate = await GetChurnRateValue(previousPeriod.From, previousPeriod.To);

            return new DashboardMetricResponse(
                "churnRate",
                "Churn Rate",
                true,
                currentChurnRate,
                "percent",
                CalculateChangePercentage(currentChurnRate, previousChurnRate),
                "clientes.estado, clientes.fecha_actualizacion",
                "Estimado como clientes desactivados en el periodo dividido entre clientes existentes al final del periodo."
            );
        }

        private async Task<DashboardMetricResponse> GetActiveUsersMetric()
        {
            var activeUsers = await ScopeUsuarios(_db.Usuarios)
                .AsNoTracking()
                .CountAsync(x => x.Estado == "activo");

            return new DashboardMetricResponse(
                "activeUsers",
                "Active Users",
                true,
                activeUsers,
                "count",
                null,
                "Usuarios.estado",
                "No hay fecha historica de actividad; se muestra el total actual de usuarios activos."
            );
        }

        private async Task<DashboardMetricResponse> GetArpcMetric(DateOnly from, DateOnly to)
        {
            var currentArpc = await GetArpcValue(from, to);
            var previousPeriod = ResolvePreviousPeriod(from, to);
            var previousArpc = await GetArpcValue(previousPeriod.From, previousPeriod.To);

            return new DashboardMetricResponse(
                "arpc",
                "ARPC",
                true,
                currentArpc,
                "currency",
                CalculateChangePercentage(currentArpc, previousArpc),
                "VentaBase.total / clientes distintos",
                null
            );
        }

        private async Task<decimal> GetRevenueTotal(DateOnly from, DateOnly to)
        {
            return await ScopeVentaBase(_db.VentaBase)
                .AsNoTracking()
                .Where(x => x.Fecha >= from && x.Fecha <= to)
                .SumAsync(x => (decimal?)x.Total) ?? 0;
        }

        private IQueryable<FinancialSaleItemResponse> GetFinancialSaleItems(string tipoResumen)
        {
            return ScopeVentaBase(_db.VentaBase)
                .Include(x => x.Cliente)
                .Include(x => x.Vendedor)
                .Include(x => x.Sucursal)
                .AsNoTracking()
                .Select(x => new FinancialSaleItemResponse(
                    x.Id,
                    x.EmpresaId,
                    x.SucursalId,
                    x.CajaId,
                    x.VendedorId,
                    x.ClienteId,
                    tipoResumen,
                    tipoResumen == "Cliente" ? x.ClienteId : tipoResumen == "Vendedor" ? x.VendedorId : x.SucursalId,
                    tipoResumen == "Cliente" ? x.Cliente.Nombre : tipoResumen == "Vendedor" ? x.Vendedor.Nombre : x.Sucursal.Nombre,
                    x.CantidadFacturas,
                    x.CantidadDevoluciones,
                    x.Factura,
                    x.Devolucion,
                    x.Total,
                    x.Fecha,
                    x.Sucursal.Nombre
                ));
        }

        private IQueryable<PaymentSaleItemResponse> GetPaymentItems()
        {
            return ScopeVentaBase(_db.VentaBase)
                .Include(x => x.Sucursal)
                .AsNoTracking()
                .Select(x => new PaymentSaleItemResponse(
                    x.Id,
                    x.SucursalId,
                    x.Subtotal,
                    x.Descuento,
                    x.Impuesto,
                    x.CantidadFacturas,
                    x.Total,
                    x.Efectivo,
                    x.Tarjeta,
                    x.Cheque,
                    x.Bonos,
                    x.NotaCredito,
                    x.Credito,
                    x.Contado,
                    x.Fecha,
                    new NamedEntityResponse(x.Sucursal.Id, x.Sucursal.Nombre)
                ));
        }

        private async Task<int> GetNewCustomersCount(DateOnly from, DateOnly to)
        {
            return await GetClientesQuery()
                .CountAsync(x => x.FechaCreacion >= from && x.FechaCreacion <= to);
        }

        private async Task<decimal> GetChurnRateValue(DateOnly from, DateOnly to)
        {
            var totalCustomersAtPeriodEnd = await GetClientesQuery()
                .CountAsync(x => x.FechaCreacion <= to);

            if (totalCustomersAtPeriodEnd == 0)
            {
                return 0;
            }

            var inactiveCustomersInPeriod = await GetClientesQuery()
                .CountAsync(x =>
                    x.Estado == "inactivo" &&
                    x.FechaActualizacion >= from &&
                    x.FechaActualizacion <= to);

            return decimal.Round(((decimal)inactiveCustomersInPeriod / totalCustomersAtPeriodEnd) * 100, 2);
        }

        private async Task<decimal> GetArpcValue(DateOnly from, DateOnly to)
        {
            var ventasPorCliente = ScopeVentaBase(_db.VentaBase)
                .AsNoTracking()
                .Where(x => x.Fecha >= from && x.Fecha <= to);

            var totalRevenue = await ventasPorCliente.SumAsync(x => (decimal?)x.Total) ?? 0;
            var distinctCustomers = await ventasPorCliente
                .Select(x => x.ClienteId)
                .Distinct()
                .CountAsync();

            if (distinctCustomers == 0)
            {
                return 0;
            }

            return decimal.Round(totalRevenue / distinctCustomers, 2);
        }

        private static DashboardMetricResponse UnsupportedMetric(string key, string title, string message)
        {
            return new DashboardMetricResponse(key, title, false, null, null, null, null, message);
        }

        private IQueryable<Clientes> GetClientesQuery()
        {
            return ScopeClientes(_db.Clientes).AsNoTracking();
        }

        private static DateRange ResolvePeriod(DateTime? from, DateTime? to)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var defaultFrom = new DateOnly(today.Year, today.Month, 1);

            return new DateRange(
                from.HasValue ? DateOnly.FromDateTime(from.Value) : defaultFrom,
                to.HasValue ? DateOnly.FromDateTime(to.Value) : today
            );
        }

        private static DateRange ResolvePreviousPeriod(DateOnly from, DateOnly to)
        {
            var days = to.DayNumber - from.DayNumber + 1;
            var previousTo = from.AddDays(-1);
            var previousFrom = previousTo.AddDays(-days + 1);

            return new DateRange(previousFrom, previousTo);
        }

        private static decimal? CalculateChangePercentage(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0)
            {
                return currentValue == 0 ? 0 : null;
            }

            return decimal.Round(((currentValue - previousValue) / previousValue) * 100, 2);
        }

        public sealed record PerformanceOverviewResponse(
            DateOnly From,
            DateOnly To,
            IEnumerable<DashboardMetricResponse> Metrics
        );

        public sealed record DashboardMetricResponse(
            string Key,
            string Title,
            bool IsAvailable,
            decimal? Value,
            string? Unit,
            decimal? ChangePercentage,
            string? Source,
            string? Message
        );

        public sealed record CustomerSummaryResponse(
            int TotalCustomers,
            int ActiveCustomers,
            int InactiveCustomers,
            int NewCustomers,
            decimal ChurnRate,
            string Message
        );

        public sealed record UnsupportedFeatureResponse(
            string Key,
            bool IsAvailable,
            string Message
        );

        public sealed record DataCapabilityResponse(
            string Key,
            bool IsAvailable,
            string SourceOrReason
        );

        public sealed record FinancialSummaryDataResponse(
            IEnumerable<FinancialSaleItemResponse> Clientes,
            IEnumerable<FinancialSaleItemResponse> Vendedores,
            IEnumerable<FinancialSaleItemResponse> Sucursales,
            IEnumerable<PaymentSaleItemResponse> TipoPago
        );

        public sealed record FinancialSaleItemResponse(
            int Id,
            int EmpresaId,
            int SucursalId,
            int CajaId,
            int VendedorId,
            int ClienteId,
            string TipoResumen,
            int ReferenciaId,
            string Nombre,
            int CantidadFacturas,
            int CantidadDevoluciones,
            decimal Factura,
            decimal Devolucion,
            decimal Total,
            DateOnly Fecha,
            string SucursalNombre
        );

        public sealed record PaymentSaleItemResponse(
            int Id,
            int SucursalId,
            decimal Subtotal,
            decimal Descuento,
            decimal Impuesto,
            int Cantidad,
            decimal Total,
            decimal Efectivo,
            decimal Tarjeta,
            decimal Cheque,
            decimal Bonos,
            decimal NotaCredito,
            decimal Credito,
            decimal Contado,
            DateOnly Fecha,
            NamedEntityResponse Sucursal
        );

        public sealed record NamedEntityResponse(int Id, string Nombre);

        private readonly record struct DateRange(DateOnly From, DateOnly To);
    }
}
