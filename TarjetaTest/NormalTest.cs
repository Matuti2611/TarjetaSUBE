using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class NormalTest
{
    [Test]
    public void Constructor_PropiedadesPorDefecto_SonCorrectas()
    {
        var tipo = new Normal();

        Assert.That(tipo.Nombre, Is.EqualTo("Normal"));
        Assert.That(tipo.MultiplicadorTarifa, Is.EqualTo(1.0m));
        Assert.That(tipo.EsGratuito, Is.False);
    }

    [Test]
    public void CalcularTarifa_DevuelveTarifaCompleta()
    {
        var tipo = new Normal();
        var tarifa = tipo.CalcularTarifa(Colectivo.TarifaBasica);

        Assert.That(tarifa, Is.EqualTo(2150m));
    }
}
