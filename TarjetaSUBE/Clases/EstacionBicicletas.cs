namespace TarjetaSUBE;

public class EstacionBicicletas
{
    public const decimal TarifaDiaria = 2418.75m;
    public const decimal MultaPorDemora = 1250m;
    public static readonly TimeSpan TiempoMaximoDeUso = TimeSpan.FromHours(1);

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public bool RetirarBicicleta(Tarjeta tarjeta)
    {
        if (tarjeta == null)
            throw new ArgumentNullException(nameof(tarjeta));

        if (Contexto.Db.AlquileresBicicletas.Any(a =>
                a.TarjetaId == tarjeta.Id &&
                a.FechaDevolucion == null))
        {
            return false;
        }

        DateTime ahora = Contexto.Reloj.Ahora;
        DateTime inicioDelDia = ahora.Date;
        DateTime inicioDelDiaSiguiente = inicioDelDia.AddDays(1);

        bool yaPagoLaTarifaDelDia = Contexto.Db.AlquileresBicicletas.Any(a =>
            a.TarjetaId == tarjeta.Id &&
            a.FechaRetiro >= inicioDelDia &&
            a.FechaRetiro < inicioDelDiaSiguiente);

        var multasPendientes = Contexto.Db.AlquileresBicicletas
            .Where(a =>
                a.TarjetaId == tarjeta.Id &&
                a.Multa > 0m &&
                !a.MultaCobrada)
            .ToList();

        decimal tarifaDelServicio = yaPagoLaTarifaDelDia ? 0m : TarifaDiaria;
        decimal montoACobrar = tarifaDelServicio + multasPendientes.Sum(a => a.Multa);

        if (!tarjeta.DescontarSaldo(montoACobrar))
            return false;

        foreach (var alquilerConMulta in multasPendientes)
            alquilerConMulta.MultaCobrada = true;

        var alquiler = new AlquilerBicicleta
        {
            FechaRetiro = ahora,
            MontoAbonado = montoACobrar,
            Tarjeta = tarjeta,
            TarjetaId = tarjeta.Id,
            Estacion = this,
            EstacionBicicletasId = Id
        };

        Contexto.Db.AlquileresBicicletas.Add(alquiler);
        Contexto.Db.SaveChanges();
        tarjeta.AcreditarCarga();

        return true;
    }

    public bool DevolverBicicleta(Tarjeta tarjeta)
    {
        if (tarjeta == null)
            throw new ArgumentNullException(nameof(tarjeta));

        var alquiler = Contexto.Db.AlquileresBicicletas.FirstOrDefault(a =>
            a.TarjetaId == tarjeta.Id &&
            a.FechaDevolucion == null);

        if (alquiler == null)
            return false;

        DateTime ahora = Contexto.Reloj.Ahora;
        alquiler.FechaDevolucion = ahora;

        if (ahora - alquiler.FechaRetiro > TiempoMaximoDeUso)
            alquiler.Multa = MultaPorDemora;

        Contexto.Db.SaveChanges();
        return true;
    }
}
