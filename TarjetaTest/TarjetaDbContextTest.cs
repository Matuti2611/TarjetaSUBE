using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class TarjetaDbContextTest
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
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    [Test]
    public void DbSets_PermitenAgregarYConsultarEntidades()
    {
        var tarjeta = new Tarjeta { Saldo = 5000m };
        var colectivo = new Colectivo("142 Negra");

        _db.Tarjetas.Add(tarjeta);
        _db.Colectivos.Add(colectivo);
        _db.SaveChanges();

        var boleto = new Boleto
        {
            Tarifa = 1580m,
            SaldoRestante = 3420m,
            Fecha = DateTime.Now,
            Linea = colectivo.Linea,
            TarjetaId = tarjeta.Id,
            ColectivoId = colectivo.Id
        };

        _db.Boletos.Add(boleto);
        _db.SaveChanges();

        Assert.That(_db.Tarjetas.Count(), Is.EqualTo(1));
        Assert.That(_db.Colectivos.Count(), Is.EqualTo(1));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));

        var boletoEnDb = _db.Boletos
            .Include(b => b.Tarjeta)
            .Include(b => b.Colectivo)
            .FirstOrDefault(b => b.Id == boleto.Id);

        Assert.That(boletoEnDb, Is.Not.Null);
        Assert.That(boletoEnDb!.Tarjeta, Is.Not.Null);
        Assert.That(boletoEnDb.Tarjeta!.Saldo, Is.EqualTo(5000m));
        Assert.That(boletoEnDb.Colectivo, Is.Not.Null);
        Assert.That(boletoEnDb.Colectivo!.Linea, Is.EqualTo("142 Negra"));
    }
}
