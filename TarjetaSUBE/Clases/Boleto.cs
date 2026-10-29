namespace TarjetaSUBE;

public class Boleto
{
    public int Id { get; set; }
    public decimal Tarifa { get; set; }
    public decimal TotalAbonado { get; set; }
    public DateTime Fecha { get; set; }
    public decimal SaldoRestante { get; set; }
    public string Linea { get; set; } = string.Empty;
    public string TipoTarjeta { get; set; } = string.Empty;

    public int TarjetaId { get; set; }
    public Tarjeta? Tarjeta { get; set; }

    public int ColectivoId { get; set; }
    public Colectivo? Colectivo { get; set; }

    public Boleto() { }

    public Boleto(
        decimal tarifa,
        DateTime fecha,
        decimal saldoRestante,
        string linea,
        string tipoTarjeta = "",
        decimal? totalAbonado = null,
        int tarjetaId = 0)
    {
        Tarifa = tarifa;
        TotalAbonado = totalAbonado ?? tarifa;
        Fecha = fecha;
        SaldoRestante = saldoRestante;
        Linea = linea;
        TipoTarjeta = tipoTarjeta;
        TarjetaId = tarjetaId;
    }

    public override string ToString()
    {
        return $"Boleto | Tarjeta: {TarjetaId} | Tipo: {TipoTarjeta} | Línea: {Linea} | Tarifa: ${Tarifa} | Total abonado: ${TotalAbonado} | Saldo restante: ${SaldoRestante} | Fecha: {Fecha:dd/MM/yyyy HH:mm:ss}";
    }

    public static implicit operator bool(Boleto? b) => b is not null;
}
