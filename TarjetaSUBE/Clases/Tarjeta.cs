namespace TarjetaSUBE;

public class Tarjeta
{
    public const decimal LimiteSaldo = 40000m;

    public static readonly decimal[] CargasAceptadas =
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };

    public int Id { get; set; }
    public decimal Saldo { get; set; } = 0m;

    public Tarjeta() { }

    public Tarjeta(decimal saldoInicial)
    {
        if (saldoInicial < 0)
            throw new ArgumentException("El saldo inicial no puede ser negativo.");

        if (saldoInicial > LimiteSaldo)
            throw new ArgumentException($"El saldo inicial no puede superar el límite de ${LimiteSaldo}.");

        Saldo = saldoInicial;
    }

    public bool Cargar(decimal monto)
    {
        if (!CargasAceptadas.Contains(monto))
        {
            throw new ArgumentException($"Monto no permitido: ${monto}. Las cargas aceptadas son: {string.Join(", ", CargasAceptadas)}.");
        }

        if (Saldo + monto > LimiteSaldo)
        {
            throw new InvalidOperationException($"La carga de ${monto} supera el límite máximo permitido de ${LimiteSaldo}. Saldo actual: ${Saldo}.");
        }

        Saldo += monto;

        if (Contexto.Db.ChangeTracker.Entries<Tarjeta>().Any(e => e.Entity == this))
        {
            Contexto.Db.SaveChanges();
        }

        return true;
    }

    public bool cargar(decimal monto) => Cargar(monto);

    public bool DescontarSaldo(decimal monto)
    {
        if (monto < 0)
        {
            throw new ArgumentException("El monto a descontar no puede ser negativo.");
        }

        if (Saldo < monto)
        {
            return false;
        }

        Saldo -= monto;

        if (Contexto.Db.ChangeTracker.Entries<Tarjeta>().Any(e => e.Entity == this))
        {
            Contexto.Db.SaveChanges();
        }

        return true;
    }

    public bool descontarSaldo(decimal monto) => DescontarSaldo(monto);

    public decimal ObtenerSaldo() => Saldo;
    public decimal obtenerSaldo() => Saldo;
}
