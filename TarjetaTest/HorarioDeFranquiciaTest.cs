using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class HorarioDeFranquiciaTest
{
    [TestCase(2026, 11, 2, 6, 0, 0, true)]
    [TestCase(2026, 11, 6, 22, 0, 0, true)]
    [TestCase(2026, 11, 2, 5, 59, 59, false)]
    [TestCase(2026, 11, 2, 22, 0, 1, false)]
    [TestCase(2026, 11, 7, 12, 0, 0, false)]
    [TestCase(2026, 11, 8, 12, 0, 0, false)]
    public void EstaHabilitado_ValidaElDiaYLaFranjaHoraria(
        int anio,
        int mes,
        int dia,
        int hora,
        int minuto,
        int segundo,
        bool resultadoEsperado)
    {
        var fechaHora = new DateTime(anio, mes, dia, hora, minuto, segundo);

        bool resultado = HorarioDeFranquicia.EstaHabilitado(fechaHora);

        Assert.That(resultado, Is.EqualTo(resultadoEsperado));
    }
}
