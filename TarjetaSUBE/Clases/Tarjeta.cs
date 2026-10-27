namespace TarjetaSUBE;

public class Tarjeta
{
    public const decimal LimiteSaldo = 40000m;
    public const decimal SaldoMinimo = -2000m;

    public static readonly decimal[] CargasAceptadas =
    {
        2000m, 3000m, 4000m, 5000m, 8000m, 10000m, 15000m, 20000m, 25000m, 30000m
    };

    public int Id { get; set; }
    public decimal Saldo { get; set; } = 0m;
    public int? TarjetaTipoId { get; set; }
    public TarjetaTipo Tipo { get; set; } = new Normal();
    public DateTime? UltimoViaje { get; set; }
    public int ViajesHoy { get; set; }

    public Tarjeta()
    {
        Tipo = new Normal();
    }

    public Tarjeta(TarjetaTipo tipo)
    {
        Tipo = tipo ?? new Normal();
    }

    public Tarjeta(decimal saldoInicial, TarjetaTipo? tipo = null)
    {
        if (saldoInicial < SaldoMinimo)
            throw new ArgumentException($"El saldo inicial no puede ser menor a ${SaldoMinimo}.");

        if (saldoInicial > LimiteSaldo)
            throw new ArgumentException($"El saldo inicial no puede superar el límite de ${LimiteSaldo}.");

        Saldo = saldoInicial;
        Tipo = tipo ?? new Normal();
    }

    public int ObtenerViajesHoy(DateTime ahora)
    {
        if (!UltimoViaje.HasValue || UltimoViaje.Value.Date != ahora.Date)
        {
            return 0;
        }

        return ViajesHoy;
    }

    public void RegistrarViaje(DateTime ahora)
    {
        if (!UltimoViaje.HasValue || UltimoViaje.Value.Date != ahora.Date)
        {
            ViajesHoy = 1;
        }
        else
        {
            ViajesHoy++;
        }

        UltimoViaje = ahora;

        if (Contexto.Db.ChangeTracker.Entries<Tarjeta>().Any(e => e.Entity == this))
        {
            Contexto.Db.SaveChanges();
        }
    }

    public bool PuedeViajar(DateTime? fecha = null)
    {
        var ahora = fecha ?? Contexto.Reloj.Ahora;
        return Tipo?.PuedeViajar(this, ahora) ?? true;
    }

    public decimal CalcularTarifa(decimal tarifaBasica)
    {
        return CalcularTarifa(tarifaBasica, Contexto.Reloj.Ahora);
    }

    public decimal CalcularTarifa(decimal tarifaBasica, DateTime ahora)
    {
        int viajesHoy = ObtenerViajesHoy(ahora);
        return Tipo?.CalcularTarifa(tarifaBasica, viajesHoy) ?? tarifaBasica;
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

        if (Saldo - monto < SaldoMinimo)
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
