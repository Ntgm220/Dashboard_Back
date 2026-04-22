using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class VentaBase
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public int SucursalId { get; set; }

    public int CajaId { get; set; }

    public int VendedorId { get; set; }

    public int ClienteId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public int CantidadFacturas { get; set; }

    public int CantidadDevoluciones { get; set; }

    public decimal Factura { get; set; }

    public decimal Devolucion { get; set; }

    public decimal Total { get; set; }

    public decimal Efectivo { get; set; }

    public decimal Tarjeta { get; set; }

    public decimal Cheque { get; set; }

    public decimal Bonos { get; set; }

    public decimal NotaCredito { get; set; }

    public decimal Credito { get; set; }

    public decimal Contado { get; set; }

    public DateOnly Fecha { get; set; }

    public virtual Cajas Caja { get; set; } = null!;

    public virtual Clientes Cliente { get; set; } = null!;

    public virtual Empresas Empresa { get; set; } = null!;

    public virtual Sucursales Sucursal { get; set; } = null!;

    public virtual Vendedores Vendedor { get; set; } = null!;
}
