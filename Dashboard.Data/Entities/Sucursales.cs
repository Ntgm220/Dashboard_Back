using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class Sucursales
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateOnly FechaCreacion { get; set; }

    public DateOnly FechaActualizacion { get; set; }

    public virtual ICollection<Cajas> Cajas { get; set; } = new List<Cajas>();

    public virtual ICollection<Clientes> Clientes { get; set; } = new List<Clientes>();

    public virtual Empresas Empresa { get; set; } = null!;

    public virtual ICollection<Vendedores> Vendedores { get; set; } = new List<Vendedores>();

    public virtual ICollection<VentaBase> VentaBase { get; set; } = new List<VentaBase>();
}
