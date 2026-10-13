using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class MedioBoletoEstudiantilTest
{
    [Test]
    public void Constructor_PropiedadesPorDefecto_SonCorrectas()
    {
        var tipo = new MedioBoletoEstudiantil();

        Assert.That(tipo.Nombre, Is.EqualTo("Medio boleto estudiantil"));
        Assert.That(tipo.MultiplicadorTarifa, Is.EqualTo(0.5m));
        Assert.That(tipo.EsGratuito, Is.False);
    }

    [Test]
    public void CalcularTarifa_DevuelveLaMitadDeLaTarifa()
    {
        var tipo = new MedioBoletoEstudiantil();
        var tarifa = tipo.CalcularTarifa(1580m);

        Assert.That(tarifa, Is.EqualTo(790m));
    }
}
