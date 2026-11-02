namespace TarjetaSUBE;

public static class BoletoFrecuente
{
    public static decimal CalcularTarifa(decimal tarifaBase, int viajesPreviosDelMes)
    {
        int numeroDeViajeActual = viajesPreviosDelMes + 1;

        if (numeroDeViajeActual >= 80)
            return Math.Round(tarifaBase * 0.75m, 2);

        if (numeroDeViajeActual >= 30)
            return Math.Round(tarifaBase * 0.80m, 2);

        return tarifaBase;
    }
}
