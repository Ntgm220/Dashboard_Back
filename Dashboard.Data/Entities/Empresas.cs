using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class Empresas
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string CodigoEmpresa { get; set; } = null!;

    public DateOnly FSinc { get; set; }

    public virtual ICollection<EmpresasLogs> EmpresasLogs { get; set; } = new List<EmpresasLogs>();

    public virtual ICollection<Sucursales> Sucursales { get; set; } = new List<Sucursales>();

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();

    public virtual ICollection<VentaBase> VentaBase { get; set; } = new List<VentaBase>();
}
