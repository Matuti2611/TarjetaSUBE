namespace TarjetaTest;

[TestFixture]
public class TestFundamentalDelTrabajo
{
    [Test]
    public void MauriApruebaACiroPregotYMatiasRubioLaMateria()
    {
        bool mauriAprueba = true;

        Assert.That(mauriAprueba, Is.True);
    }
}
