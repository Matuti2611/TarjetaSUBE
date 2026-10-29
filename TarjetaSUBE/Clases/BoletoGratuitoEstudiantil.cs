namespace TarjetaSUBE;

public class BoletoGratuitoEstudiantil : TarjetaTipo
{
    public const int LimiteViajesGratuitos = 2;

    public BoletoGratuitoEstudiantil() : base("Boleto gratuito estudiantil", 0.0m, true) { }

    public override decimal CalcularTarifa(decimal tarifaBasica, int viajesHoy)
    {
        if (viajesHoy < LimiteViajesGratuitos)
        {
            return 0m;
        }

        return tarifaBasica;
    }
}
