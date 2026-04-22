using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class EmpresasLogs
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public DateOnly Fecha { get; set; }

    public string Tipo { get; set; } = null!;

    public virtual Empresas Empresa { get; set; } = null!;
}
