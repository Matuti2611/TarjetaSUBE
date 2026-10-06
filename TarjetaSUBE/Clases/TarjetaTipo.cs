namespace TarjetaSUBE;

public class TarjetaTipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal MultiplicadorTarifa { get; set; } = 1.0m;
    public bool EsGratuito { get; set; } = false;

    public TarjetaTipo() { }

    public TarjetaTipo(string nombre, decimal multiplicadorTarifa, bool esGratuito = false)
    {
        Nombre = nombre;
        MultiplicadorTarifa = multiplicadorTarifa;
        EsGratuito = esGratuito || multiplicadorTarifa == 0m;
    }

    public virtual decimal CalcularTarifa(decimal tarifaBasica)
    {
        if (EsGratuito || MultiplicadorTarifa == 0m)
            return 0m;

        return Math.Round(tarifaBasica * MultiplicadorTarifa, 2);
    }
}
