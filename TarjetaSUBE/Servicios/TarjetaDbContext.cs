using Microsoft.EntityFrameworkCore;

namespace TarjetaSUBE;

public class TarjetaDbContext : DbContext
{
    public TarjetaDbContext() { }

    public TarjetaDbContext(DbContextOptions<TarjetaDbContext> opciones) : base(opciones) { }

    public DbSet<Tarjeta> Tarjetas => Set<Tarjeta>();
    public DbSet<Colectivo> Colectivos => Set<Colectivo>();
    public DbSet<Boleto> Boletos => Set<Boleto>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=tarjeta.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tarjeta>()
            .Property(t => t.Saldo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Colectivo>()
            .Property(c => c.Tarifa)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Boleto>()
            .Property(b => b.Tarifa)
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
