using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class EstacionBicicletasTest
{
    private TarjetaDbContext _db = null!;
    private ProveedorDeFechaFija _reloj = null!;
    private EstacionBicicletas _estacion = null!;

    [SetUp]
    public void SetUp()
    {
        var opciones = new DbContextOptionsBuilder<TarjetaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new TarjetaDbContext(opciones);
        Contexto.Db = _db;
        _reloj = new ProveedorDeFechaFija(new DateTime(2026, 11, 4, 10, 0, 0));
        Contexto.Reloj = _reloj;

        _estacion = new EstacionBicicletas { Nombre = "Parque Independencia" };
        _db.EstacionesBicicletas.Add(_estacion);
        _db.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
    }

    private Tarjeta CrearTarjeta(decimal saldo, TarjetaTipo? tipo = null)
    {
        var tarjeta = new Tarjeta(saldo, tipo);
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();
        return tarjeta;
    }

    private void RegistrarMultaPendiente(Tarjeta tarjeta, DateTime fecha, decimal multa)
    {
        _db.AlquileresBicicletas.Add(new AlquilerBicicleta
        {
            FechaRetiro = fecha,
            FechaDevolucion = fecha.AddHours(2),
            Multa = multa,
            TarjetaId = tarjeta.Id,
            EstacionBicicletasId = _estacion.Id
        });
        _db.SaveChanges();
    }

    [Test]
    public void RetirarBicicleta_ConSaldoSuficiente_CobraTarifaDiariaYRegistraAlquiler()
    {
        var tarjeta = CrearTarjeta(5000m);

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirar, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - EstacionBicicletas.TarifaDiaria));
        var alquiler = _db.AlquileresBicicletas.Single();
        Assert.That(alquiler.MontoAbonado, Is.EqualTo(EstacionBicicletas.TarifaDiaria));
        Assert.That(alquiler.FechaRetiro, Is.EqualTo(_reloj.Ahora));
        Assert.That(alquiler.FechaDevolucion, Is.Null);
    }

    [Test]
    public void DevolverBicicleta_ExactamenteUnaHoraDespues_NoGeneraMulta()
    {
        var tarjeta = CrearTarjeta(5000m);
        _estacion.RetirarBicicleta(tarjeta);
        _reloj.FechaFija = _reloj.FechaFija.AddHours(1);

        bool pudoDevolver = _estacion.DevolverBicicleta(tarjeta);

        Assert.That(pudoDevolver, Is.True);
        Assert.That(_db.AlquileresBicicletas.Single().Multa, Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBicicleta_DosVecesElMismoDia_CobraTarifaUnaSolaVez()
    {
        var tarjeta = CrearTarjeta(5000m);
        Assert.That(_estacion.RetirarBicicleta(tarjeta), Is.True);
        Assert.That(_estacion.DevolverBicicleta(tarjeta), Is.True);
        decimal saldoDespuesDelPrimerAlquiler = tarjeta.Saldo;

        _reloj.FechaFija = _reloj.FechaFija.AddMinutes(30);
        bool pudoRetirarPorSegundaVez = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirarPorSegundaVez, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoDespuesDelPrimerAlquiler));
        Assert.That(
            _db.AlquileresBicicletas.OrderBy(a => a.Id).Last().MontoAbonado,
            Is.EqualTo(0m));
    }

    [Test]
    public void RetirarBicicleta_SaldoInsuficienteSinMultas_NoRetiraNiDescuentaSaldo()
    {
        var tarjeta = CrearTarjeta(400m);

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirar, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(400m));
        Assert.That(_db.AlquileresBicicletas, Is.Empty);
    }

    [Test]
    public void RetirarBicicleta_SaldoInsuficienteConMultaPendiente_NoRetiraNiCobraLaMulta()
    {
        var tarjeta = CrearTarjeta(1500m);
        RegistrarMultaPendiente(tarjeta, _reloj.Ahora.AddDays(-1), 1250m);

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirar, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(1500m));
        Assert.That(_db.AlquileresBicicletas.Single().MultaCobrada, Is.False);
    }

    [Test]
    public void RetirarBicicleta_ConVariasMultasPendientes_CobraTarifaYMultas()
    {
        var tarjeta = CrearTarjeta(10000m);
        RegistrarMultaPendiente(tarjeta, _reloj.Ahora.AddDays(-2), 1250m);
        RegistrarMultaPendiente(tarjeta, _reloj.Ahora.AddDays(-1), 1250m);

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        decimal montoEsperado = EstacionBicicletas.TarifaDiaria + 2500m;
        Assert.That(pudoRetirar, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(10000m - montoEsperado));
        Assert.That(_db.AlquileresBicicletas.Count(a => a.MultaCobrada), Is.EqualTo(2));
        Assert.That(
            _db.AlquileresBicicletas.OrderBy(a => a.Id).Last().MontoAbonado,
            Is.EqualTo(montoEsperado));
    }

    [Test]
    public void DevolverBicicleta_MasDeUnaHoraDespues_GeneraMultaPendiente()
    {
        var tarjeta = CrearTarjeta(5000m);
        _estacion.RetirarBicicleta(tarjeta);
        _reloj.FechaFija = _reloj.FechaFija.AddHours(1).AddSeconds(1);

        bool pudoDevolver = _estacion.DevolverBicicleta(tarjeta);

        Assert.That(pudoDevolver, Is.True);
        var alquiler = _db.AlquileresBicicletas.Single();
        Assert.That(alquiler.Multa, Is.EqualTo(EstacionBicicletas.MultaPorDemora));
        Assert.That(alquiler.MultaCobrada, Is.False);
    }

    [Test]
    public void RetirarBicicleta_ConSaldoPlusDisponible_RespetaElLimiteDeTarjeta()
    {
        var tarjeta = CrearTarjeta(500m);

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirar, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(500m - EstacionBicicletas.TarifaDiaria));
    }

    [Test]
    public void RetirarBicicleta_BoletoGratuito_CobraLaTarifaCompleta()
    {
        var tarjeta = CrearTarjeta(5000m, new BoletoGratuitoEstudiantil());

        bool pudoRetirar = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirar, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(5000m - EstacionBicicletas.TarifaDiaria));
    }

    [Test]
    public void RetirarBicicleta_ConOtraBicicletaEnUso_NoPermiteUnSegundoAlquiler()
    {
        var tarjeta = CrearTarjeta(5000m);
        Assert.That(_estacion.RetirarBicicleta(tarjeta), Is.True);
        decimal saldoDespuesDelPrimerAlquiler = tarjeta.Saldo;

        bool pudoRetirarOtraBicicleta = _estacion.RetirarBicicleta(tarjeta);

        Assert.That(pudoRetirarOtraBicicleta, Is.False);
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoDespuesDelPrimerAlquiler));
    }

    [Test]
    public void DevolverBicicleta_SinAlquilerActivo_DevuelveFalse()
    {
        var tarjeta = CrearTarjeta(5000m);

        bool pudoDevolver = _estacion.DevolverBicicleta(tarjeta);

        Assert.That(pudoDevolver, Is.False);
    }

    [Test]
    public void RetirarBicicleta_TarjetaNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _estacion.RetirarBicicleta(null!));
    }

    [Test]
    public void DevolverBicicleta_TarjetaNula_LanzaArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _estacion.DevolverBicicleta(null!));
    }
}
