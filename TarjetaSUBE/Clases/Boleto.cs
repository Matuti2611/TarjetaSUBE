namespace TarjetaSUBE;

public class Boleto
{
    public int Id { get; set; }
    public decimal Tarifa { get; set; }
    public DateTime Fecha { get; set; }
    public decimal SaldoRestante { get; set; }
    public string Linea { get; set; } = string.Empty;

    public int TarjetaId { get; set; }
    public Tarjeta? Tarjeta { get; set; }

    public int ColectivoId { get; set; }
    public Colectivo? Colectivo { get; set; }

    public Boleto() { }

    public Boleto(decimal tarifa, DateTime fecha, decimal saldoRestante, string linea)
    {
        Tarifa = tarifa;
        Fecha = fecha;
        SaldoRestante = saldoRestante;
        Linea = linea;
    }

    public override string ToString()
    {
        return $"Boleto | Línea: {Linea} | Tarifa: ${Tarifa} | Saldo restante: ${SaldoRestante} | Fecha: {Fecha:dd/MM/yyyy HH:mm:ss}";
    }
}
