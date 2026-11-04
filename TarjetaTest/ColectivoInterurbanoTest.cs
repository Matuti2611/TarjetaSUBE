using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class ColectivoInterurbanoTest
{
    private TarjetaDbContext _db = null!;

    [SetUp]
    public void SetUp()
    {
        var opciones = new DbContextOptionsBuilder<TarjetaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new TarjetaDbContext(opciones);
        Contexto.Db = _db;
        Contexto.Reloj = new ProveedorDeFechaFija(
            new DateTime(2026, 11, 2, 10, 0, 0));
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void ConstructorPorDefecto_AsignaTarifaInterurbana()
    {
        var colectivo = new ColectivoInterurbano();

        Assert.That(colectivo.Tarifa, Is.EqualTo(ColectivoInterurbano.TarifaInterurbana));
        Assert.That(colectivo.Linea, Is.Empty);
    }

    [Test]
    public void ConstructorConLinea_AsignaLineaYTarifaInterurbana()
    {
        var colectivo = new ColectivoInterurbano("35/9");

        Assert.That(colectivo.Linea, Is.EqualTo("35/9"));
        Assert.That(colectivo.Tarifa, Is.EqualTo(ColectivoInterurbano.TarifaInterurbana));
    }

    [Test]
    public void PagarCon_Normal_CobraTarifaInterurbana()
    {
        var colectivo = new ColectivoInterurbano("35/9");
        var tarjeta = new Tarjeta(5000m, new Normal());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(3000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(2000m));
        Assert.That(boleto.Linea, Is.EqualTo("35/9"));
    }

    [TestCase(typeof(MedioBoletoEstudiantil), 1500)]
    [TestCase(typeof(BoletoGratuitoEstudiantil), 0)]
    [TestCase(typeof(FranquiciaCompleta), 0)]
    public void PagarCon_FranquiciaEnHorario_AplicaLaTarifaCorrespondiente(
        Type tipoDeFranquicia,
        decimal tarifaEsperada)
    {
        var tipo = (TarjetaTipo)Activator.CreateInstance(tipoDeFranquicia)!;
        var tarjeta = new Tarjeta(5000m, tipo);

        var boleto = new ColectivoInterurbano("35/9").pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(tarifaEsperada));
    }

    [TestCase(typeof(MedioBoletoEstudiantil))]
    [TestCase(typeof(BoletoGratuitoEstudiantil))]
    [TestCase(typeof(FranquiciaCompleta))]
    public void PagarCon_FranquiciaFueraDeHorario_NoGeneraBoleto(
        Type tipoDeFranquicia)
    {
        Contexto.Reloj = new ProveedorDeFechaFija(
            new DateTime(2026, 11, 8, 10, 0, 0));
        var tipo = (TarjetaTipo)Activator.CreateInstance(tipoDeFranquicia)!;
        var tarjeta = new Tarjeta(5000m, tipo);
        decimal saldoAntes = tarjeta.Saldo;

        var boleto = new ColectivoInterurbano("35/9").pagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntes));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PagarCon_NormalCon29ViajesPrevios_AplicaBoletoFrecuente()
    {
        var ahora = Contexto.Reloj.Ahora;
        var tarjeta = new Tarjeta(50000m, new Normal());
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        for (int i = 0; i < 29; i++)
        {
            _db.Boletos.Add(new Boleto
            {
                TarjetaId = tarjeta.Id,
                Fecha = ahora,
                Tarifa = ColectivoInterurbano.TarifaInterurbana,
                TotalAbonado = ColectivoInterurbano.TarifaInterurbana,
                SaldoRestante = tarjeta.Saldo,
                Linea = "35/9"
            });
        }

        _db.SaveChanges();

        var boleto = new ColectivoInterurbano("35/9").pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(
            boleto!.Tarifa,
            Is.EqualTo(ColectivoInterurbano.TarifaInterurbana * 0.80m));
    }

    [Test]
    public void PagarCon_SaldoQueAlcanzaElLimiteNegativo_PermiteElViaje()
    {
        var tarjeta = new Tarjeta(1000m, new Normal());

        var boleto = new ColectivoInterurbano("35/9").pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.SaldoMinimo));
    }

    [Test]
    public void GuardarInterurbano_PermiteRecuperarloDesdeElContexto()
    {
        var colectivo = new ColectivoInterurbano("35/9");
        _db.ColectivosInterurbanos.Add(colectivo);
        _db.SaveChanges();

        var colectivoGuardado = _db.ColectivosInterurbanos.Single();

        Assert.That(colectivoGuardado.Linea, Is.EqualTo("35/9"));
        Assert.That(
            colectivoGuardado.Tarifa,
            Is.EqualTo(ColectivoInterurbano.TarifaInterurbana));
    }
}
