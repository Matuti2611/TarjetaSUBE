using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class TarjetaTest
{
    private TarjetaDbContext _db = null!;

    [SetUp]
    public void Setup()
    {
        var opciones = new DbContextOptionsBuilder<TarjetaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new TarjetaDbContext(opciones);
        Contexto.Db = _db;
        Contexto.Reloj = new ProveedorDeFechaSistema();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void ConstructorPorDefecto_InicializaConSaldoCero()
    {
        var tarjeta = new Tarjeta();
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
        Assert.That(tarjeta.ObtenerSaldo(), Is.EqualTo(0m));
        Assert.That(tarjeta.obtenerSaldo(), Is.EqualTo(0m));
    }

    [Test]
    public void ConstructorConParametro_InicializaConSaldoIndicado()
    {
        var tarjeta = new Tarjeta(5000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void ConstructorConSaldoMenorAlSaldoMinimo_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Tarjeta(-2001m));
    }

    [Test]
    public void ConstructorConSaldoMayorAlLimite_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Tarjeta(56001m));
    }

    [TestCase(2000)]
    [TestCase(3000)]
    [TestCase(4000)]
    [TestCase(5000)]
    [TestCase(8000)]
    [TestCase(10000)]
    [TestCase(15000)]
    [TestCase(20000)]
    [TestCase(25000)]
    [TestCase(30000)]
    public void Cargar_ConTodosLosMontosValidos_AumentaElSaldoCorrectamente(decimal monto)
    {
        var tarjeta = new Tarjeta();
        var resultado = tarjeta.Cargar(monto);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(monto));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
        Assert.That(tarjeta.ObtenerSaldo(), Is.EqualTo(monto));
    }

    [TestCase(2000)]
    [TestCase(3000)]
    [TestCase(4000)]
    [TestCase(5000)]
    [TestCase(8000)]
    [TestCase(10000)]
    [TestCase(15000)]
    [TestCase(20000)]
    [TestCase(25000)]
    [TestCase(30000)]
    public void cargar_Minuscula_ConTodosLosMontosValidos_AumentaElSaldoCorrectamente(decimal monto)
    {
        var tarjeta = new Tarjeta();
        var resultado = tarjeta.cargar(monto);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.obtenerSaldo(), Is.EqualTo(monto));
    }

    [TestCase(100)]
    [TestCase(500)]
    [TestCase(1000)]
    [TestCase(1500)]
    [TestCase(1580)]
    [TestCase(6000)]
    [TestCase(7000)]
    [TestCase(35000)]
    [TestCase(0)]
    [TestCase(-500)]
    public void Cargar_MontoNoPermitido_LanzaArgumentException(decimal montoInvalido)
    {
        var tarjeta = new Tarjeta();

        var ex = Assert.Throws<ArgumentException>(() => tarjeta.Cargar(montoInvalido));
        Assert.That(ex.Message, Does.Contain("Monto no permitido"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void Cargar_SuperaLimiteMaximo_AcreditaHasta56000YDejaElExcedentePendiente()
    {
        var tarjeta = new Tarjeta(50000m);

        var resultado = tarjeta.Cargar(10000m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(4000m));
    }

    [Test]
    public void Cargar_ConSaldoEnElLimite_DejaTodaLaCargaPendiente()
    {
        var tarjeta = new Tarjeta(Tarjeta.LimiteSaldo);

        tarjeta.Cargar(2000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(Tarjeta.LimiteSaldo));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(2000m));
    }

    [Test]
    public void Cargar_HastaLimiteExactoDe56000_AcreditaExitosamente()
    {
        var tarjeta = new Tarjeta(26000m);
        tarjeta.Cargar(30000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void AcreditarCarga_CuandoHayCupo_AcreditaElPendienteHastaElLimite()
    {
        var tarjeta = new Tarjeta(55000m);
        tarjeta.Cargar(2000m);
        tarjeta.DescontarSaldo(1000m);

        var montoAcreditado = tarjeta.AcreditarCarga();

        Assert.That(montoAcreditado, Is.EqualTo(1000m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(0m));
    }

    [Test]
    public void DescontarSaldo_ConSaldoSuficiente_RestaSaldoYDevuelveTrue()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);

        var resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
    }

    [Test]
    public void DescontarSaldo_HastaSaldoNegativoMaximoDe2000_DescuentaYPermiteViaje()
    {
        var tarjeta = new Tarjeta(); // saldo 0
        var resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void DescontarSaldo_SuperaSaldoNegativoPermitido_NoPermiteDescuentoYNoModificaSaldo()
    {
        var tarjeta = new Tarjeta(); // saldo 0
        tarjeta.DescontarSaldo(1580m); // saldo = -1580m

        // Intentar otro viaje de 1580 llevaría el saldo a -3160m < -2000m
        var resultado = tarjeta.DescontarSaldo(1580m);

        Assert.That(resultado, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
    }

    [Test]
    public void Cargar_ConSaldoNegativo_DescuentaCorrectamenteLosViajesConsumidos()
    {
        var tarjeta = new Tarjeta();
        tarjeta.DescontarSaldo(1580m); // Viaje plus consumido, saldo = -1580m

        tarjeta.Cargar(2000m);

        // Al cargar 2000, se salda la deuda de 1580 y quedan 420
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));
    }

    [Test]
    public void DescontarSaldo_MontoNegativo_LanzaArgumentException()
    {
        var tarjeta = new Tarjeta();
        Assert.Throws<ArgumentException>(() => tarjeta.DescontarSaldo(-50m));
    }

    [Test]
    public void DescontarSaldo_Minuscula_FuncionaCorrectamente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        var resultado = tarjeta.descontarSaldo(1580m);

        Assert.That(resultado, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));
    }

}
