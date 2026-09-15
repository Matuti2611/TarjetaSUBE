using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class ColectivoTest
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
        Contexto.Reloj = new ProveedorDeFechaFija(new DateTime(2026, 9, 15, 12, 0, 0));
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void ConstructorPorDefecto_AsignaTarifaBasica()
    {
        var colectivo = new Colectivo();
        Assert.That(colectivo.Tarifa, Is.EqualTo(1580m));
        Assert.That(colectivo.Linea, Is.Empty);
    }

    [Test]
    public void ConstructorConLinea_AsignaLineaYTarifaBasica()
    {
        var colectivo = new Colectivo("102 Negra");
        Assert.That(colectivo.Linea, Is.EqualTo("102 Negra"));
        Assert.That(colectivo.Tarifa, Is.EqualTo(1580m));
    }

    [Test]
    public void ConstructorConLineaYTarifa_AsignaValoresCorrectamente()
    {
        var colectivo = new Colectivo("K", 2000m);
        Assert.That(colectivo.Linea, Is.EqualTo("K"));
        Assert.That(colectivo.Tarifa, Is.EqualTo(2000m));
    }

    [Test]
    public void PagarCon_TarjetaConSaldoSuficiente_GeneraBoletoYDescuentaSaldo()
    {
        var colectivo = new Colectivo("115");
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(3000m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(1580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(1420m));
        Assert.That(boleto.Linea, Is.EqualTo("115"));
        Assert.That(boleto.Fecha, Is.EqualTo(new DateTime(2026, 9, 15, 12, 0, 0)));
        Assert.That(tarjeta.Saldo, Is.EqualTo(1420m));
    }

    [Test]
    public void PagarCon_PascalCase_FuncionaExactamenteIgual()
    {
        var colectivo = new Colectivo("103 Roja");
        var tarjeta = new Tarjeta();
        tarjeta.Cargar(2000m);

        var boleto = colectivo.PagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(420m));
    }

    [Test]
    public void PagarCon_TarjetaConSaldoInsuficiente_NoGeneraBoletoYDevuelveNull()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(); // Saldo 0 ("No hay saldo negativo")

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PagarCon_TarjetaNula_LanzaArgumentNullException()
    {
        var colectivo = new Colectivo("143");
        Assert.Throws<ArgumentNullException>(() => colectivo.pagarCon(null!));
    }

    [Test]
    public void PagarCon_PersisteBoletoEnBaseDeDatos()
    {
        var colectivo = new Colectivo("102 Negra");
        _db.Colectivos.Add(colectivo);

        var tarjeta = new Tarjeta();
        tarjeta.Cargar(5000m);
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(_db.Boletos.Find(boleto!.Id), Is.Not.Null);
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }
}
