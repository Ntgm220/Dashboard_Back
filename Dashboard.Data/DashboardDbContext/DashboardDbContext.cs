using System;
using System.Collections.Generic;
using Dashboard.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Data.DashboardDbContext;

public partial class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cajas> Cajas { get; set; }

    public virtual DbSet<Clientes> Clientes { get; set; }

    public virtual DbSet<Empresas> Empresas { get; set; }

    public virtual DbSet<EmpresasLogs> EmpresasLogs { get; set; }

    public virtual DbSet<Sucursales> Sucursales { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<Vendedores> Vendedores { get; set; }

    public virtual DbSet<VentaBase> VentaBase { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cajas>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cajas__3213E83F6D270BE6");

            entity.ToTable("cajas");

            entity.HasIndex(e => e.SucursalId, "IX_cajas_sucursal");

            entity.HasIndex(e => new { e.Id, e.SucursalId }, "UQ_cajas_id_sucursal").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo", "DF_cajas_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_cajas_fecha_creacion")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Cajas)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cajas_sucursales");
        });

        modelBuilder.Entity<Clientes>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__clientes__3213E83FEA9E9233");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.SucursalId, "IX_clientes_sucursal");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo", "DF_clientes_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_clientes_fecha_creacion")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_clientes_sucursales");
        });

        modelBuilder.Entity<Empresas>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__empresas__3213E83F672EEFFC");

            entity.ToTable("empresas");

            entity.HasIndex(e => e.CodigoEmpresa, "UQ__empresas__53C9ACE53E43F0F8").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CodigoEmpresa)
                .HasMaxLength(50)
                .HasColumnName("codigo_empresa");
            entity.Property(e => e.FSinc).HasColumnName("f_sinc");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<EmpresasLogs>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__empresas__3213E83F58241544");

            entity.ToTable("empresas_logs");

            entity.HasIndex(e => new { e.EmpresaId, e.Fecha }, "IX_empresas_logs_empresa_fecha");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Empresa).WithMany(p => p.EmpresasLogs)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_empresas_logs_empresas");
        });

        modelBuilder.Entity<Sucursales>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sucursal__3213E83F43C480F8");

            entity.ToTable("sucursales");

            entity.HasIndex(e => e.EmpresaId, "IX_sucursales_empresa");

            entity.HasIndex(e => new { e.Id, e.EmpresaId }, "UQ_sucursales_id_empresa").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo", "DF_sucursales_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_sucursales_fecha_creacion")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Sucursales)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_sucursales_empresas");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83FBDFFC39A");

            entity.HasIndex(e => e.EmpresaId, "IX_Usuarios_empresa");

            entity.HasIndex(e => e.Correo, "UQ__Usuarios__2A586E0B0CB83B5F").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Clave)
                .HasMaxLength(255)
                .HasColumnName("clave");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .HasColumnName("correo");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo", "DF_Usuarios_estado")
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .HasColumnName("rol");

            entity.HasOne(d => d.Empresa).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_empresas");
        });

        modelBuilder.Entity<Vendedores>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vendedor__3213E83F9B66B3EA");

            entity.ToTable("vendedores");

            entity.HasIndex(e => e.SucursalId, "IX_vendedores_sucursal");

            entity.HasIndex(e => new { e.Id, e.SucursalId }, "UQ_vendedores_id_sucursal").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("activo", "DF_vendedores_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaActualizacion).HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(CONVERT([date],getdate()))", "DF_vendedores_fecha_creacion")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.Vendedores)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vendedores_sucursales");
        });

        modelBuilder.Entity<VentaBase>(entity =>
        {
            entity.HasIndex(e => new { e.CajaId, e.Fecha }, "IX_VentaBase_caja_fecha");

            entity.HasIndex(e => new { e.ClienteId, e.Fecha }, "IX_VentaBase_cliente_fecha");

            entity.HasIndex(e => e.Fecha, "IX_VentaBase_fecha");

            entity.HasIndex(e => new { e.SucursalId, e.Fecha }, "IX_VentaBase_sucursal_fecha");

            entity.HasIndex(e => new { e.VendedorId, e.Fecha }, "IX_VentaBase_vendedor_fecha");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Bonos)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("bonos");
            entity.Property(e => e.CajaId).HasColumnName("caja_id");
            entity.Property(e => e.CantidadDevoluciones).HasColumnName("cantidad_devoluciones");
            entity.Property(e => e.CantidadFacturas).HasColumnName("cantidad_facturas");
            entity.Property(e => e.Cheque)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cheque");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Contado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("contado");
            entity.Property(e => e.Credito)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("credito");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.Devolucion)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("devolucion");
            entity.Property(e => e.Efectivo)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("efectivo");
            entity.Property(e => e.EmpresaId).HasColumnName("empresa_id");
            entity.Property(e => e.Factura)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("factura");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Impuesto)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("impuesto");
            entity.Property(e => e.NotaCredito)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("nota_credito");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.SucursalId).HasColumnName("sucursal_id");
            entity.Property(e => e.Tarjeta)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tarjeta");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total");
            entity.Property(e => e.VendedorId).HasColumnName("vendedor_id");

            entity.HasOne(d => d.Caja).WithMany(p => p.VentaBase)
                .HasForeignKey(d => d.CajaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaBase_cajas");

            entity.HasOne(d => d.Cliente).WithMany(p => p.VentaBase)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaBase_clientes");

            entity.HasOne(d => d.Empresa).WithMany(p => p.VentaBase)
                .HasForeignKey(d => d.EmpresaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaBase_empresas");

            entity.HasOne(d => d.Sucursal).WithMany(p => p.VentaBase)
                .HasForeignKey(d => d.SucursalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaBase_sucursales");

            entity.HasOne(d => d.Vendedor).WithMany(p => p.VentaBase)
                .HasForeignKey(d => d.VendedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VentaBase_vendedores");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
