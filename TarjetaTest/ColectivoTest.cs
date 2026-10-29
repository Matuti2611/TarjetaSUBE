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
        Assert.That(boleto.TotalAbonado, Is.EqualTo(1580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(1420m));
        Assert.That(boleto.Linea, Is.EqualTo("115"));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Normal"));
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
    public void PagarCon_TarjetaConSaldoPositivoInsuficiente_PermiteViajePlus()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(1000m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(1580m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-580m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(-580m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PagarCon_TarjetaConSaldoInsuficiente_NoGeneraBoletoYDevuelveNull()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(-1000m); // -1000 - 1580 = -2580 < -2000

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1000m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(0));
    }

    [Test]
    public void PagarCon_TarjetaSinSaldo_EvaluaComoFalse()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(-1000m); // -1000 - 1580 = -2580 < -2000

        bool operacionExitosa = colectivo.pagarCon(tarjeta);

        Assert.That(operacionExitosa, Is.False);
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
        Assert.That(boleto.TarjetaId, Is.EqualTo(tarjeta.Id));
        Assert.That(boleto.ColectivoId, Is.EqualTo(colectivo.Id));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Normal"));
    }

    [Test]
    public void PagarCon_FranquiciaCompleta_SiemprePuedePagarBoleto()
    {
        var colectivo = new Colectivo("102 Negra");
        // Tarjeta con saldo 0 (o negativo) pero FranquiciaCompleta
        var tarjeta = new Tarjeta(0m, new FranquiciaCompleta());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Franquicia completa"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_GeneraBoletoSinDescontarSaldo()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(0m, new BoletoGratuitoEstudiantil());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(0m));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Boleto gratuito estudiantil"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PagarCon_FranquiciaCompletaConSaldoNegativo_GeneraBoletoSinModificarSaldo()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(-1580m, new FranquiciaCompleta());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(0m));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Franquicia completa"));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1580m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PagarCon_MedioBoletoConSaldoInsuficiente_PermiteSaldoNegativo()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(500m, new MedioBoletoEstudiantil());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(790m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(790m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-290m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(-290m));
        Assert.That(boleto.TipoTarjeta, Is.EqualTo("Medio boleto estudiantil"));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PagarCon_MedioBoletoEstudiantil_MontoDelBoletoEsSiempreLaMitadDelNormal()
    {
        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(2000m, new MedioBoletoEstudiantil());

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(colectivo.Tarifa / 2));
        Assert.That(boleto.Tarifa, Is.EqualTo(790m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(790m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(1210m)); // 2000 - 790 = 1210
    }

    [Test]
    public void PagarCon_TarjetaConDeudaPrevia_InformaElTotalIncluyendoLaDeuda()
    {
        var colectivo = new Colectivo("122");
        var tarjeta = new Tarjeta(-200m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(boleto!.Tarifa, Is.EqualTo(1580m));
        Assert.That(boleto.TotalAbonado, Is.EqualTo(1780m));
        Assert.That(boleto.SaldoRestante, Is.EqualTo(-1780m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(-1780m));
    }

    [Test]
    public void PagarCon_MedioBoleto_MenosDeCincoMinutos_NoDejaMarcarYDevuelveNull()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(5000m, new MedioBoletoEstudiantil());

        var primerBoleto = colectivo.pagarCon(tarjeta);
        Assert.That(primerBoleto, Is.Not.Null);

        reloj.FechaFija = reloj.FechaFija.AddMinutes(4).AddSeconds(59);

        var segundoBoleto = colectivo.pagarCon(tarjeta);

        Assert.That(segundoBoleto, Is.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(4210m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(1));
    }

    [Test]
    public void PagarCon_MedioBoleto_CincoMinutosExactos_PermiteMarcarSegundoViaje()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(5000m, new MedioBoletoEstudiantil());

        Assert.That(colectivo.pagarCon(tarjeta), Is.Not.Null);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(5);

        var segundoBoleto = colectivo.pagarCon(tarjeta);

        Assert.That(segundoBoleto, Is.Not.Null);
        Assert.That(segundoBoleto!.Tarifa, Is.EqualTo(790m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(3420m));
        Assert.That(_db.Boletos.Count(), Is.EqualTo(2));
    }

    [Test]
    public void PagarCon_MedioBoleto_MaximoDosViajesPorDiaConMedioBoleto_TerceroEsNormal()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(10000m, new MedioBoletoEstudiantil());

        var boleto1 = colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(10);
        var boleto2 = colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(10);
        var boleto3 = colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(10);
        var boleto4 = colectivo.pagarCon(tarjeta);

        Assert.That(boleto1!.Tarifa, Is.EqualTo(790m));
        Assert.That(boleto2!.Tarifa, Is.EqualTo(790m));
        Assert.That(boleto3!.Tarifa, Is.EqualTo(1580m));
        Assert.That(boleto4!.Tarifa, Is.EqualTo(1580m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(5260m));
    }

    [Test]
    public void PagarCon_MedioBoleto_CambioDeDia_ReiniciaContadorYVuelveACobrarMedioBoleto()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(10000m, new MedioBoletoEstudiantil());

        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(10);
        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(10);
        Assert.That(colectivo.pagarCon(tarjeta)!.Tarifa, Is.EqualTo(1580m));

        reloj.FechaFija = new DateTime(2026, 10, 20, 8, 0, 0);
        var boletoDiaSiguiente = colectivo.pagarCon(tarjeta);

        Assert.That(boletoDiaSiguiente, Is.Not.Null);
        Assert.That(boletoDiaSiguiente!.Tarifa, Is.EqualTo(790m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_MaximoDosViajesGratisPorDia()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(0m, new BoletoGratuitoEstudiantil());

        var boleto1 = colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(2);
        var boleto2 = colectivo.pagarCon(tarjeta);

        Assert.That(boleto1!.Tarifa, Is.EqualTo(0m));
        Assert.That(boleto2!.Tarifa, Is.EqualTo(0m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_TercerViajeEnElMismoDia_CobraPrecioCompleto()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(3000m, new BoletoGratuitoEstudiantil());

        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(2);
        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(2);
        var boleto3 = colectivo.pagarCon(tarjeta);

        Assert.That(boleto3, Is.Not.Null);
        Assert.That(boleto3!.Tarifa, Is.EqualTo(1580m));
        Assert.That(tarjeta.Saldo, Is.EqualTo(1420m));
    }

    [Test]
    public void PagarCon_BoletoGratuitoEstudiantil_CambioDeDia_VuelveATenerViajesGratis()
    {
        var reloj = new ProveedorDeFechaFija(new DateTime(2026, 10, 19, 8, 0, 0));
        Contexto.Reloj = reloj;

        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(5000m, new BoletoGratuitoEstudiantil());

        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(2);
        colectivo.pagarCon(tarjeta);
        reloj.FechaFija = reloj.FechaFija.AddMinutes(2);
        Assert.That(colectivo.pagarCon(tarjeta)!.Tarifa, Is.EqualTo(1580m));

        reloj.FechaFija = new DateTime(2026, 10, 20, 8, 0, 0);
        var boletoNuevoDia = colectivo.pagarCon(tarjeta);

        Assert.That(boletoNuevoDia, Is.Not.Null);
        Assert.That(boletoNuevoDia!.Tarifa, Is.EqualTo(0m));
    }

    [Test]
    public void PagarCon_ConSaldoPendiente_AcreditaElCupoLiberadoLuegoDelViaje()
    {
        var colectivo = new Colectivo("102 Negra");
        var tarjeta = new Tarjeta(50000m);
        tarjeta.Cargar(10000m);

        var boleto = colectivo.pagarCon(tarjeta);

        Assert.That(boleto, Is.Not.Null);
        Assert.That(tarjeta.Saldo, Is.EqualTo(56000m));
        Assert.That(tarjeta.SaldoPendiente, Is.EqualTo(2420m));
        Assert.That(boleto!.SaldoRestante, Is.EqualTo(56000m));
    }
}
