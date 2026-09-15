using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class BoletoTest
{
    [Test]
    public void ConstructorPorDefecto_InicializaCorrectamente()
    {
        var boleto = new Boleto();
        Assert.That(boleto.Id, Is.EqualTo(0));
        Assert.That(boleto.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.Linea, Is.Empty);
    }

    [Test]
    public void ConstructorConParametros_AsignaPropiedades()
    {
        var fecha = new DateTime(2026, 9, 15, 14, 30, 0);
        var boleto = new Boleto(1580m, fecha, 2420m, "102 Negra");

        Assert.That(boleto.Tarifa, Is.EqualTo(1580m));
        Assert.That(boleto.Fecha, Is.EqualTo(fecha));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(2420m));
        Assert.That(boleto.Linea, Is.EqualTo("102 Negra"));
    }

    [Test]
    public void ToString_ContieneInformacionClaveDelBoleto()
    {
        var fecha = new DateTime(2026, 9, 15, 14, 30, 0);
        var boleto = new Boleto(1580m, fecha, 2420m, "102 Negra");

        var texto = boleto.ToString();

        Assert.That(texto, Does.Contain("102 Negra"));
        Assert.That(texto, Does.Contain("1580"));
        Assert.That(texto, Does.Contain("2420"));
        Assert.That(texto, Does.Contain("15/09/2026"));
    }
}
