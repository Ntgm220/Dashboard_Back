using System;
using System.Collections.Generic;

namespace Dashboard.Data.Entities;

public partial class Usuarios
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public string Correo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual Empresas Empresa { get; set; } = null!;
}
