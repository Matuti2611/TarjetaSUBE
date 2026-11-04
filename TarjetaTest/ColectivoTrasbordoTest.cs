using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class ColectivoTrasbordoTest
{
    private TarjetaDbContext _db = null!;
    private ProveedorDeFechaFija _reloj = null!;
    private Tarjeta _tarjeta = null!;
    private Colectivo _lineaA = null!;
    private Colectivo _lineaB = null!;

    private static readonly DateTime FechaBase = new DateTime(2026, 11, 4, 10, 0, 0);

    [SetUp]
    public void Setup()
    {
        var opciones = new DbContextOptionsBuilder<TarjetaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TarjetaDbContext(opciones);
        Contexto.Db = _db;

        _reloj = new ProveedorDeFechaFija(FechaBase);
        Contexto.Reloj = _reloj;

        _tarjeta = new Tarjeta(10000m);
        _db.Tarjetas.Add(_tarjeta);
        _db.SaveChanges();

        _lineaA = new Colectivo("101");
        _lineaB = new Colectivo("202");
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    [Test]
    public void Trasbordo_EnLineaDistinta_DentroDeUnaHora_EsGratuito()
    {
        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.True);
        Assert.That(boleto.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
        Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_MarcaEsTrasbordo_EnBoleto()
    {
        _lineaA.pagarCon(_tarjeta);

        _reloj.FechaFija = FechaBase.AddMinutes(20);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_SaldoNoDisminuye()
    {
        _lineaA.pagarCon(_tarjeta);
        decimal saldoAntes = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(45);
        _lineaB.pagarCon(_tarjeta);

        Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoAntes));
    }

    [Test]
    public void Trasbordo_NoIncrementaViajesHoy()
    {
        _lineaA.pagarCon(_tarjeta);
        int viajesTrasSegundoViaje = _tarjeta.ViajesHoy;

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        _lineaB.pagarCon(_tarjeta);

        Assert.That(_tarjeta.ViajesHoy, Is.EqualTo(viajesTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_MismaLinea_NoBoleto_NoBoletoGratuito()
    {
        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(20);

        var boleto = _lineaA.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(_tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_MasDeUnaHoraDespues_NoBoletoGratuito()
    {
        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddHours(1).AddSeconds(1);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(_tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_ExactamenteUnaHoraDespues_EsBoletoGratuito()
    {
        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasPrimerViaje = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddHours(1);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.True);
        Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoTrasPrimerViaje));
    }

    [Test]
    public void Trasbordo_EnDomingo_NoBoletoGratuito()
    {
        var sabado = new DateTime(2026, 11, 7, 10, 0, 0);
        _reloj.FechaFija = sabado;

        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        var domingo = new DateTime(2026, 11, 8, 10, 0, 0);
        _reloj.FechaFija = domingo;

        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(_tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_AntesDeHorario_Las630_NoBoletoGratuito()
    {
        var fechaTemprana = new DateTime(2026, 11, 4, 6, 0, 0);
        _reloj.FechaFija = fechaTemprana;

        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = fechaTemprana.AddMinutes(20);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(_tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_ALas22_NoBoletoGratuito()
    {
        var fechaNoche = new DateTime(2026, 11, 4, 21, 50, 0);
        _reloj.FechaFija = fechaNoche;

        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = new DateTime(2026, 11, 4, 22, 5, 0);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(_tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_EnSabado_EsValido()
    {
        var sabado = new DateTime(2026, 11, 7, 10, 0, 0);
        _reloj.FechaFija = sabado;

        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasSegundoViaje = _tarjeta.Saldo;

        _reloj.FechaFija = sabado.AddMinutes(30);
        var boleto = _lineaB.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.True);
        Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoTrasSegundoViaje));
    }

    [Test]
    public void MultiplesTrasbordos_DentroDeUnaHora_TodosGratuitos()
    {
        var lineaC = new Colectivo("303");

        _lineaA.pagarCon(_tarjeta);
        decimal saldoTrasA = _tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(20);
        var boletoB = _lineaB.pagarCon(_tarjeta);

        _reloj.FechaFija = FechaBase.AddMinutes(40);
        var boletoC = lineaC.pagarCon(_tarjeta);

        Assert.That(boletoB!.EsTrasbordo, Is.True);
        Assert.That(boletoC!.EsTrasbordo, Is.True);
        Assert.That(_tarjeta.Saldo, Is.EqualTo(saldoTrasA));
    }

    [Test]
    public void SinBoletoAnterior_NuncaEsTrasbordo()
    {
        var boleto = _lineaA.pagarCon(_tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
    }

    [Test]
    public void Trasbordo_TarjetaNormal_EsValido()
    {
        var tarjetaNormal = new Tarjeta(5000m, new Normal());
        _db.Tarjetas.Add(tarjetaNormal);
        _db.SaveChanges();

        _lineaA.pagarCon(tarjetaNormal);

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = _lineaB.pagarCon(tarjetaNormal);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_TarjetaMedioBoleto_EsValido()
    {
        var tarjetaMedio = new Tarjeta(5000m, new MedioBoletoEstudiantil());
        _db.Tarjetas.Add(tarjetaMedio);
        _db.SaveChanges();

        _lineaA.pagarCon(tarjetaMedio);

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = _lineaB.pagarCon(tarjetaMedio);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_TarjetaBoletoGratuito_EsValido()
    {
        var tarjetaGratuita = new Tarjeta(5000m, new BoletoGratuitoEstudiantil());
        _db.Tarjetas.Add(tarjetaGratuita);
        _db.SaveChanges();

        _lineaA.pagarCon(tarjetaGratuita);

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = _lineaB.pagarCon(tarjetaGratuita);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }
}
