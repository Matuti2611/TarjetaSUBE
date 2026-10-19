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

        DateTime ahora = Contexto.Reloj.Ahora;

        if (!tarjeta.PuedeViajar(ahora))
        {
            return null;
        }

        decimal tarifaACobrar = tarjeta.CalcularTarifa(Tarifa, ahora);

        if (tarifaACobrar > 0m && !tarjeta.DescontarSaldo(tarifaACobrar))
        {
            return null;
        }

        tarjeta.RegistrarViaje(ahora);

        var boleto = new Boleto
        {
            Tarifa = tarifaACobrar,
            Fecha = ahora,
            SaldoRestante = tarjeta.Saldo,
            Linea = Linea,
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
}
