using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class BoletoGratuitoEstudiantilTest
{
    [Test]
    public void Constructor_PropiedadesPorDefecto_SonCorrectas()
    {
        var tipo = new BoletoGratuitoEstudiantil();

        Assert.That(tipo.Nombre, Is.EqualTo("Boleto gratuito estudiantil"));
        Assert.That(tipo.MultiplicadorTarifa, Is.EqualTo(0.0m));
        Assert.That(tipo.EsGratuito, Is.True);
    }

    [Test]
    public void CalcularTarifa_DevuelveCero()
    {
        var tipo = new BoletoGratuitoEstudiantil();
        var tarifa = tipo.CalcularTarifa(1580m);

        Assert.That(tarifa, Is.EqualTo(0m));
    }
}
