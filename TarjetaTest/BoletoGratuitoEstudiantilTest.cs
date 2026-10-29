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

    [Test]
    public void CalcularTarifa_PrimerosDosViajes_SonGratuitos()
    {
        var tipo = new BoletoGratuitoEstudiantil();

        var tarifaViaje1 = tipo.CalcularTarifa(1580m, viajesHoy: 0);
        var tarifaViaje2 = tipo.CalcularTarifa(1580m, viajesHoy: 1);

        Assert.That(tarifaViaje1, Is.EqualTo(0m));
        Assert.That(tarifaViaje2, Is.EqualTo(0m));
    }

    [Test]
    public void CalcularTarifa_TercerViajeOMas_CobraTarifaNormal()
    {
        var tipo = new BoletoGratuitoEstudiantil();

        var tarifaViaje3 = tipo.CalcularTarifa(1580m, viajesHoy: 2);
        var tarifaViaje4 = tipo.CalcularTarifa(1580m, viajesHoy: 3);

        Assert.That(tarifaViaje3, Is.EqualTo(1580m));
        Assert.That(tarifaViaje4, Is.EqualTo(1580m));
    }
}
