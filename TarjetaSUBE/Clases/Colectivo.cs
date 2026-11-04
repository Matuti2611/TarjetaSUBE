namespace TarjetaSUBE;

public class Colectivo
{
    public const decimal TarifaBasica = 2150m;
    public static readonly TimeSpan VentanaTrasbordo = TimeSpan.FromHours(1);

    public int Id { get; set; }
    public string Linea { get; set; } = string.Empty;
    public decimal Tarifa { get; set; } = TarifaBasica;

    public Colectivo()
    {
        Tarifa = TarifaBasica;
    }

    public Colectivo(string linea)
    {
        Linea = linea;
        Tarifa = TarifaBasica;
    }

    public Colectivo(string linea, decimal tarifa)
    {
        Linea = linea;
        Tarifa = tarifa;
    }

    public Boleto? pagarCon(Tarjeta tarjeta)
    {
        if (tarjeta == null)
            throw new ArgumentNullException(nameof(tarjeta));

        decimal saldoAntesDelPago = tarjeta.Saldo;
        DateTime ahora = Contexto.Reloj.Ahora;
        bool esTrasbordo = EsTrasbordo(tarjeta, ahora);

        if (!tarjeta.PuedeViajar(ahora, esTrasbordo))
            return null;

        decimal tarifaACobrar = esTrasbordo
            ? 0m
            : tarjeta.CalcularTarifa(Tarifa, ahora);

        if (!esTrasbordo && tarjeta.Tipo.AplicarBoletoFrecuente)
        {
            int viajesPreviosDelMes = Contexto.Db.Boletos.Count(b =>
                b.TarjetaId == tarjeta.Id &&
                b.Fecha.Year == ahora.Year &&
                b.Fecha.Month == ahora.Month);

            tarifaACobrar = BoletoFrecuente.CalcularTarifa(
                tarifaACobrar,
                viajesPreviosDelMes);
        }

        if (tarifaACobrar > 0m && !tarjeta.DescontarSaldo(tarifaACobrar))
            return null;

        if (!esTrasbordo)
            tarjeta.RegistrarViaje(ahora);

        tarjeta.RegistrarBoleto(ahora, Linea, esTrasbordo);
        tarjeta.AcreditarCarga();

        var boleto = new Boleto
        {
            Tarifa = tarifaACobrar,
            TotalAbonado = CalcularTotalAbonado(tarifaACobrar, saldoAntesDelPago),
            Fecha = ahora,
            SaldoRestante = tarjeta.Saldo,
            Linea = Linea,
            TipoTarjeta = tarjeta.Tipo?.Nombre ?? string.Empty,
            EsTrasbordo = esTrasbordo,
            Tarjeta = tarjeta,
            TarjetaId = tarjeta.Id,
            Colectivo = this,
            ColectivoId = Id
        };

        Contexto.Db.Boletos.Add(boleto);
        Contexto.Db.SaveChanges();

        return boleto;
    }

    public Boleto? PagarCon(Tarjeta tarjeta) => pagarCon(tarjeta);

    private bool EsTrasbordo(Tarjeta tarjeta, DateTime ahora)
    {
        if (!tarjeta.InicioVentanaTrasbordo.HasValue)
            return false;

        if (ahora.DayOfWeek == DayOfWeek.Sunday)
            return false;

        if (ahora.Hour < 7 || ahora.Hour >= 22)
            return false;

        TimeSpan tiempoTranscurrido = ahora - tarjeta.InicioVentanaTrasbordo.Value;
        if (tiempoTranscurrido > VentanaTrasbordo || tiempoTranscurrido < TimeSpan.Zero)
            return false;

        return !string.Equals(
            Linea,
            tarjeta.LineaAnterior,
            StringComparison.OrdinalIgnoreCase);
    }

    private static decimal CalcularTotalAbonado(decimal tarifa, decimal saldoAntesDelPago)
    {
        if (tarifa == 0m || saldoAntesDelPago >= 0m)
            return tarifa;

        return tarifa + Math.Abs(saldoAntesDelPago);
    }
}
