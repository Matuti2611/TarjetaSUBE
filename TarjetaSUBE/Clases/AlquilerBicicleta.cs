namespace TarjetaSUBE;

public class AlquilerBicicleta
{
    public int Id { get; set; }
    public DateTime FechaRetiro { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public decimal MontoAbonado { get; set; }
    public decimal Multa { get; set; }
    public bool MultaCobrada { get; set; }

    public int TarjetaId { get; set; }
    public Tarjeta? Tarjeta { get; set; }

    public int EstacionBicicletasId { get; set; }
    public EstacionBicicletas? Estacion { get; set; }
}
