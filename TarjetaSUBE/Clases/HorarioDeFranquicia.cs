namespace TarjetaSUBE;

public static class HorarioDeFranquicia
{
    public static bool EstaHabilitado(DateTime fechaHora)
    {
        bool esDiaHabil = fechaHora.DayOfWeek >= DayOfWeek.Monday &&
                          fechaHora.DayOfWeek <= DayOfWeek.Friday;

        TimeSpan hora = fechaHora.TimeOfDay;
        bool estaEnHorario = hora >= TimeSpan.FromHours(6) &&
                              hora <= TimeSpan.FromHours(22);

        return esDiaHabil && estaEnHorario;
    }
}
