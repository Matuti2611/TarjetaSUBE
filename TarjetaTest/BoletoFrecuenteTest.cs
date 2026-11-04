using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class BoletoFrecuenteTest
{
    [TestCase(0, 1.00)]
    [TestCase(28, 1.00)]
    [TestCase(29, 0.80)]
    [TestCase(78, 0.80)]
    [TestCase(79, 0.75)]
    public void CalcularTarifa_AplicaElDescuentoCorrecto(
        int viajesPrevios,
        decimal porcentajeEsperado)
    {
        var tarifa = BoletoFrecuente.CalcularTarifa(
            Colectivo.TarifaBasica,
            viajesPrevios);
        var tarifaEsperada = Math.Round(
            Colectivo.TarifaBasica * porcentajeEsperado,
            2);

        Assert.That(tarifa, Is.EqualTo(tarifaEsperada));
    }
}



