namespace TarjetaSUBE;

public class Colectivo
{
    public const decimal TarifaBasica = 1580m;
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

    // 1. Calculamos si es trasbordo ANTES de validar las reglas de la tarjeta
    bool esTrasbordo = tarjeta.EsTrasbordoValido(Linea, ahora); 

    // 2. Le pasamos 'esTrasbordo' a PuedeViajar para que pueda exceptuar la regla de los 5 min
    if (!tarjeta.PuedeViajar(ahora, esTrasbordo))
    {
        return null;
    }

    decimal tarifaACobrar = 0m;

    // 3. Si NO es trasbordo, calculamos la tarifa normal (incluyendo frecuente)
    if (!esTrasbordo) 
    {
        tarifaACobrar = tarjeta.CalcularTarifa(Tarifa, ahora);

        if (tarjeta.Tipo.AplicarBoletoFrecuente)
        {
            int viajesPreviosDelMes = Contexto.Db.Boletos.Count(b =>
                b.TarjetaId == tarjeta.Id &&
                b.Fecha.Year == ahora.Year &&
                b.Fecha.Month == ahora.Month);

            tarifaACobrar = BoletoFrecuente.CalcularTarifa(tarifaACobrar, viajesPreviosDelMes);
        }
    }

    if (tarifaACobrar > 0m && !tarjeta.DescontarSaldo(tarifaACobrar))
    {
        return null;
    }

    // 4. Registramos el viaje pasándole la línea y si fue trasbordo
    tarjeta.RegistrarViaje(ahora, Linea, esTrasbordo);
    tarjeta.AcreditarCarga();

    var boleto = new Boleto
    {
        Tarifa = tarifaACobrar,
        TotalAbonado = CalcularTotalAbonado(tarifaACobrar, saldoAntesDelPago),
        Fecha = ahora,
        SaldoRestante = tarjeta.Saldo,
        Linea = Linea,
        TipoTarjeta = tarjeta.Tipo?.Nombre ?? string.Empty,
        // Asumiendo que agregaste EsTrasbordo en Boleto.cs para los tests
        // EsTrasbordo = esTrasbordo, 
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

    public bool EsTrasbordo(Tarjeta tarjeta, DateTime ahora)
    {
        if (!tarjeta.UltimoBoletoFecha.HasValue)
            return false;

        // Verificar que sea lunes a sábado de 7:00 a 22:00
        DayOfWeek dia = ahora.DayOfWeek;
        if (dia == DayOfWeek.Sunday)
            return false;

        int hora = ahora.Hour;
        if (hora < 7 || hora >= 22)
            return false;

        // Verificar que esté dentro de la ventana de 1 hora
        TimeSpan tiempoTranscurrido = ahora - tarjeta.UltimoBoletoFecha.Value;
        if (tiempoTranscurrido > VentanaTrasbordo || tiempoTranscurrido < TimeSpan.Zero)
            return false;

        // Verificar que sea una línea distinta
        if (string.Equals(Linea, tarjeta.UltimoBoletoLinea, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    private static decimal CalcularTotalAbonado(decimal tarifa, decimal saldoAntesDelPago)
    {
        if (tarifa == 0m || saldoAntesDelPago >= 0m)
            return tarifa;

        return tarifa + Math.Abs(saldoAntesDelPago);
    }
}
