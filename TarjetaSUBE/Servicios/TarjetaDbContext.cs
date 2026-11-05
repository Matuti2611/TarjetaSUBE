using Microsoft.EntityFrameworkCore;

namespace TarjetaSUBE;

public class TarjetaDbContext : DbContext
{
    public TarjetaDbContext() { }

    public TarjetaDbContext(DbContextOptions<TarjetaDbContext> opciones) : base(opciones) { }

    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
    public DbSet<Colectivo> Colectivos => Set<Colectivo>();
    public DbSet<ColectivoInterurbano> ColectivosInterurbanos => Set<ColectivoInterurbano>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<TarjetaTipo> TarjetaTipos => Set<TarjetaTipo>();
    public DbSet<EstacionBicicletas> EstacionesBicicletas => Set<EstacionBicicletas>();
    public DbSet<AlquilerBicicleta> AlquileresBicicletas => Set<AlquilerBicicleta>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=tarjeta.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColectivoInterurbano>();

        modelBuilder.Entity<AlquilerBicicleta>()
            .Property(a => a.MontoAbonado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AlquilerBicicleta>()
            .Property(a => a.Multa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<AlquilerBicicleta>()
            .HasOne(a => a.Tarjeta)
            .WithMany()
            .HasForeignKey(a => a.TarjetaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AlquilerBicicleta>()
            .HasOne(a => a.Estacion)
            .WithMany()
            .HasForeignKey(a => a.EstacionBicicletasId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tarjeta>()
            .Property(t => t.Saldo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Tarjeta>()
            .Property(t => t.SaldoPendiente)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Tarjeta>()
            .HasOne(t => t.Tipo)
            .WithMany()
            .HasForeignKey(t => t.TarjetaTipoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TarjetaTipo>()
            .Property(t => t.MultiplicadorTarifa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Colectivo>()
            .Property(c => c.Tarifa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .Property(b => b.Tarifa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .Property(b => b.TotalAbonado)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .Property(b => b.SaldoRestante)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .HasOne(b => b.Tarjeta)
            .WithMany()
            .HasForeignKey(b => b.TarjetaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Boleto>()
            .HasOne(b => b.Colectivo)
            .WithMany()
            .HasForeignKey(b => b.ColectivoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
