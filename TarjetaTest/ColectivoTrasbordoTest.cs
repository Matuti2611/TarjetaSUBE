using Microsoft.EntityFrameworkCore;
using TarjetaSUBE;

namespace TarjetaTest;

[TestFixture]
public class ColectivoTrasbordoTest
{
    private TarjetaDbContext _db = null!;
    private ProveedorDeFechaFija _reloj = null!;

    // Martes 2026-11-03 10:00 → día hábil, dentro del horario permitido
    private static readonly DateTime FechaBase = new DateTime(2026, 11, 4, 10, 0, 0); // martes

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
    }

    [TearDown]
    public void TearDown() => _db.Dispose();

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Tarjeta CrearTarjetaConSaldo(decimal saldo = 10000m)
    {
        var tarjeta = new Tarjeta(saldo);
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();
        return tarjeta;
    }

    // ─── Escenario feliz: trasbordo válido ────────────────────────────────────

    [Test]
    public void Trasbordo_EnLineaDistinta_DentroDeUnaHora_EsGratuito()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        // Primer boleto en línea A
        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        // 30 minutos después → trasbordo en línea B
        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.True);
        Assert.That(boleto.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
        // El saldo no debe haber disminuido
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_MarcaEsTrasbordo_EnBoleto()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        _reloj.FechaFija = FechaBase.AddMinutes(20);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_SaldoNoDisminuye()
    {
        var tarjeta = CrearTarjetaConSaldo(5000m);
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoAntes = tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(45);
        lineaB.pagarCon(tarjeta);

        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoAntes));
    }

    [Test]
    public void Trasbordo_NoIncrementaViajesHoy()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        int viajesTrasSegundoViaje = tarjeta.ViajesHoy;

        _reloj.FechaFija = FechaBase.AddMinutes(30);
        lineaB.pagarCon(tarjeta);

        Assert.That(tarjeta.ViajesHoy, Is.EqualTo(viajesTrasSegundoViaje));
    }

    // ─── Trasbordo NO válido: misma línea ────────────────────────────────────

    [Test]
    public void Trasbordo_MismaLinea_NoBoleto_NoBoletoGratuito()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA1 = new Colectivo("101");
        var lineaA2 = new Colectivo("101");

        lineaA1.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddMinutes(20);
        var boleto = lineaA2.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje)); // se cobró
    }

    // ─── Trasbordo NO válido: fuera de la ventana de 1 hora ──────────────────

    [Test]
    public void Trasbordo_MasDeUnaHoraDespues_NoBoletoGratuito()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddHours(1).AddSeconds(1); // 1 hora + 1 segundo
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_ExactamenteUnaHoraDespues_NoBoletoGratuito()
    {
        // Exactamente 1 hora = NO dentro de la ventana (vence a la hora exacta)
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = FechaBase.AddHours(1); // exactamente 1h
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    // ─── Trasbordo NO válido: domingo ─────────────────────────────────────────

    [Test]
    public void Trasbordo_EnDomingo_NoBoletoGratuito()
    {
        // Usamos un domingo para el trasbordo
        // Primer boleto el sábado
        var sabado = new DateTime(2026, 11, 7, 10, 0, 0); // sábado
        _reloj.FechaFija = sabado;

        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        // Intentar trasbordo el domingo (dentro de 1h)
        var domingo = new DateTime(2026, 11, 8, 10, 0, 0); // domingo
        _reloj.FechaFija = domingo;
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    // ─── Trasbordo NO válido: fuera del horario permitido ─────────────────────

    [Test]
    public void Trasbordo_AntesDeHorario_Las630_NoBoletoGratuito()
    {
        var fechaTemprana = new DateTime(2026, 11, 4, 6, 0, 0); // 6:00
        _reloj.FechaFija = fechaTemprana;

        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = fechaTemprana.AddMinutes(20); // 6:20, aún fuera de rango
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    [Test]
    public void Trasbordo_ALas22_NoBoletoGratuito()
    {
        var fechaNoche = new DateTime(2026, 11, 4, 21, 50, 0); // 21:50
        _reloj.FechaFija = fechaNoche;

        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = new DateTime(2026, 11, 4, 22, 5, 0); // 22:05, fuera de rango
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
        Assert.That(tarjeta.Saldo, Is.LessThan(saldoTrasSegundoViaje));
    }

    // ─── Trasbordo válido: sábado ─────────────────────────────────────────────

    [Test]
    public void Trasbordo_EnSabado_EsValido()
    {
        var sabado = new DateTime(2026, 11, 7, 10, 0, 0); // sábado
        _reloj.FechaFija = sabado;

        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        decimal saldoTrasSegundoViaje = tarjeta.Saldo;

        _reloj.FechaFija = sabado.AddMinutes(30);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.True);
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoTrasSegundoViaje));
    }

    // ─── Múltiples trasbordos dentro de la ventana ───────────────────────────

    [Test]
    public void MultiplesTrasbordos_DentroDeUnaHora_TodosGratuitos()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");
        var lineaC = new Colectivo("303");

        // Primer boleto pagado (línea A)
        lineaA.pagarCon(tarjeta);
        decimal saldoTrasA = tarjeta.Saldo;

        // Segundo viaje: trasbordo a línea B (20 min)
        _reloj.FechaFija = FechaBase.AddMinutes(20);
        var boletoB = lineaB.pagarCon(tarjeta);

        // Tercer viaje: trasbordo a línea C (40 min desde el inicio)
        _reloj.FechaFija = FechaBase.AddMinutes(40);
        var boletoC = lineaC.pagarCon(tarjeta);

        Assert.That(boletoB!.EsTrasbordo, Is.True, "El segundo viaje debería ser trasbordo");
        Assert.That(boletoC!.EsTrasbordo, Is.True, "El tercer viaje debería ser trasbordo");
        Assert.That(tarjeta.Saldo, Is.EqualTo(saldoTrasA)); // solo se cobró una vez
    }

    // ─── Sin boleto previo: NO es trasbordo ───────────────────────────────────

    [Test]
    public void SinBoletoAnterior_NuncaEsTrasbordo()
    {
        var tarjeta = CrearTarjetaConSaldo();
        var lineaA = new Colectivo("101");

        var boleto = lineaA.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.EsTrasbordo, Is.False);
    }

    // ─── Todas las tarjetas pueden hacer trasbordo ───────────────────────────

    [Test]
    public void Trasbordo_TarjetaNormal_EsValido()
    {
        var tarjeta = new Tarjeta(5000m, new Normal());
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_TarjetaMedioBoleto_EsValido()
    {
        var tarjeta = new Tarjeta(5000m, new MedioBoleto());
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }

    [Test]
    public void Trasbordo_TarjetaBoletoGratuito_EsValido()
    {
        var tarjeta = new Tarjeta(5000m, new BoletoGratuito());
        _db.Tarjetas.Add(tarjeta);
        _db.SaveChanges();

        var lineaA = new Colectivo("101");
        var lineaB = new Colectivo("202");

        lineaA.pagarCon(tarjeta);
        _reloj.FechaFija = FechaBase.AddMinutes(30);
        var boleto = lineaB.pagarCon(tarjeta);

        Assert.That(boleto!.EsTrasbordo, Is.True);
    }
}
