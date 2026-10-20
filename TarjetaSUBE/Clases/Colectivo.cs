namespace TarjetaSUBE;

public class Colectivo
{
    public const decimal TarifaBasica = 1580m;

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
        decimal tarifaACobrar = tarjeta.CalcularTarifa(Tarifa);

        if (tarifaACobrar > 0m && !tarjeta.DescontarSaldo(tarifaACobrar))
        {
            return null;
        }

        var boleto = new Boleto
        {
            Tarifa = tarifaACobrar,
            TotalAbonado = CalcularTotalAbonado(tarifaACobrar, saldoAntesDelPago),
            Fecha = Contexto.Reloj.Ahora,
            SaldoRestante = tarjeta.Saldo,
            Linea = Linea,
            TipoTarjeta = tarjeta.Tipo?.Nombre ?? string.Empty,
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

    private static decimal CalcularTotalAbonado(decimal tarifa, decimal saldoAntesDelPago)
    {
        if (tarifa == 0m || saldoAntesDelPago >= 0m)
            return tarifa;

        return tarifa + Math.Abs(saldoAntesDelPago);
    }
}
