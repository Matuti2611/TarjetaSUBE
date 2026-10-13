using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class TarjetaTipoTest
{
    [Test]
    public void PermiteCrearNuevoTipoDeTarjeta_SinCrearNuevaClase()
    {
        var tarifaSocial = new TarjetaTipo("Tarifa Social", 0.45m);

        Assert.That(tarifaSocial.Nombre, Is.EqualTo("Tarifa Social"));
        Assert.That(tarifaSocial.MultiplicadorTarifa, Is.EqualTo(0.45m));
        Assert.That(tarifaSocial.CalcularTarifa(1580m), Is.EqualTo(711m));
    }

    [Test]
    public void ConstructorPorDefecto_InicializaCorrectamente()
    {
        var tipo = new TarjetaTipo();

        Assert.That(tipo.Id, Is.EqualTo(0));
        Assert.That(tipo.Nombre, Is.Empty);
        Assert.That(tipo.MultiplicadorTarifa, Is.EqualTo(1.0m));
        Assert.That(tipo.EsGratuito, Is.False);
    }
}
