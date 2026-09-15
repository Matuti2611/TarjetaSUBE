using TarjetaSUBE;

namespace TarjetaTest;

public class ProveedorDeFechaFija : IProveedorDeFecha
{
    public DateTime FechaFija { get; set; }

    public ProveedorDeFechaFija(DateTime fechaFija)
    {
        FechaFija = fechaFija;
    }

    public DateTime Ahora => FechaFija;
}
