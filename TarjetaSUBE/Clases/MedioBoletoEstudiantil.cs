namespace TarjetaSUBE;

public class MedioBoletoEstudiantil : TarjetaTipo
{
    public const int LimiteViajesMedioBoleto = 2;
    public static readonly TimeSpan IntervaloMinimo = TimeSpan.FromMinutes(5);

    public MedioBoletoEstudiantil() : base("Medio boleto estudiantil", 0.5m, false) { }

    public override bool PuedeViajar(Tarjeta tarjeta, DateTime ahora)
    {
        if (tarjeta == null)
            throw new ArgumentNullException(nameof(tarjeta));

        if (tarjeta.UltimoViaje.HasValue)
        {
            var tiempoTranscurrido = ahora - tarjeta.UltimoViaje.Value;
            if (tiempoTranscurrido < IntervaloMinimo)
            {
                return false;
            }
        }

        return true;
    }

    public override decimal CalcularTarifa(decimal tarifaBasica, int viajesHoy)
    {
        if (viajesHoy < LimiteViajesMedioBoleto)
        {
            return base.CalcularTarifa(tarifaBasica);
        }

        return tarifaBasica;
    }
}
