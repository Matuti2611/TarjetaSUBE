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
        Assert.That(tarjeta.ObtenerSaldo(), Is.EqualTo(0m));
        Assert.That(tarjeta.obtenerSaldo(), Is.EqualTo(0m));
    }

    [Test]
    public void ConstructorConParametro_InicializaConSaldoIndicado()
    {
        var tarjeta = new Tarjeta(5000m);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m));
    }

    [Test]
    public void ConstructorConSaldoMenorAlSaldoMinimo_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Tarjeta(-2001m));
    }

    [Test]
    public void ConstructorConSaldoMayorAlLimite_LanzaArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Tarjeta(40001m));
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
    public void Cargar_SuperaLimiteMaximoDe40000_LanzaInvalidOperationException()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(30000m);

        // Intentar cargar 15000 superaría los 40000 (30000 + 15000 = 45000)
        var ex = Assert.Throws<InvalidOperationException>(() => tarjeta.Cargar(15000m));
        Assert.That(ex.Message, Does.Contain("supera el límite"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(30000m));
    }

    [Test]
    public void Cargar_HastaLimiteExactoDe40000_AcreditaExitosamente()
    {
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(20000m);
        tarjeta.Cargar(20000m);

        Assert.That(tarjeta.Saldo, Is.EqualTo(40000m));
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

    [Test]
    public void Cargar_TarjetaEnBaseDeDatos_PersisteCambios()
    {
        var tarjeta = new Tarjeta();
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        tarjeta.Cargar(4000m);

        var tarjetaEnDb = _db.Tarjetas.Find(tarjeta.Id);
        Assert.That(tarjetaEnDb, Is.Not.Null);
        Assert.That(tarjetaEnDb!.Saldo, Is.EqualTo(4000m));
    }
}
