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

    [Test]
    public void PuedeViajar_SinViajesPrevios_DevuelveTrue()
    {
        var tipo = new MedioBoletoEstudiantil();
        var tarjeta = new Tarjeta(tipo);

        Assert.That(tipo.PuedeViajar(tarjeta, DateTime.Now), Is.True);
    }

    [Test]
    public void PuedeViajar_MenorACincoMinutos_DevuelveFalse()
    {
        var tipo = new MedioBoletoEstudiantil();
        var tarjeta = new Tarjeta(tipo);
        var ahora = new DateTime(2026, 10, 19, 10, 0, 0);

        tarjeta.UltimoViaje = ahora;

        var intento = ahora.AddMinutes(4).AddSeconds(59);
        Assert.That(tipo.PuedeViajar(tarjeta, intento), Is.False);
    }

    [Test]
    public void PuedeViajar_CincoMinutosOMas_DevuelveTrue()
    {
        var tipo = new MedioBoletoEstudiantil();
        var tarjeta = new Tarjeta(tipo);
        var ahora = new DateTime(2026, 10, 19, 10, 0, 0);

        tarjeta.UltimoViaje = ahora;

        var cincoMinutos = ahora.AddMinutes(5);
        Assert.That(tipo.PuedeViajar(tarjeta, cincoMinutos), Is.True);

        var diezMinutos = ahora.AddMinutes(10);
        Assert.That(tipo.PuedeViajar(tarjeta, diezMinutos), Is.True);
    }

    [Test]
    public void PuedeViajar_TarjetaNula_LanzaArgumentNullException()
    {
        var tipo = new MedioBoletoEstudiantil();
        Assert.Throws<ArgumentNullException>(() => tipo.PuedeViajar(null!, DateTime.Now));
    }

    [Test]
    public void CalcularTarifa_PrimerosDosViajes_AplicaCincuentaPorCiento()
    {
        var tipo = new MedioBoletoEstudiantil();

        var tarifaViaje1 = tipo.CalcularTarifa(1580m, viajesHoy: 0);
        var tarifaViaje2 = tipo.CalcularTarifa(1580m, viajesHoy: 1);

        Assert.That(tarifaViaje1, Is.EqualTo(790m));
        Assert.That(tarifaViaje2, Is.EqualTo(790m));
    }

    [Test]
    public void CalcularTarifa_TercerViajeOMas_AplicaTarifaNormal()
    {
        var tipo = new MedioBoletoEstudiantil();

        var tarifaViaje3 = tipo.CalcularTarifa(1580m, viajesHoy: 2);
        var tarifaViaje4 = tipo.CalcularTarifa(1580m, viajesHoy: 3);

        Assert.That(tarifaViaje3, Is.EqualTo(1580m));
        Assert.That(tarifaViaje4, Is.EqualTo(1580m));
    }
}
