using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class Clientes
{
    public int Id { get; set; }

    public int SucursalId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateOnly FechaCreacion { get; set; }

    public DateOnly FechaActualizacion { get; set; }

    public virtual Sucursales Sucursal { get; set; } = null!;

    public virtual ICollection<VentaBase> VentaBase { get; set; } = new List<VentaBase>();
}
