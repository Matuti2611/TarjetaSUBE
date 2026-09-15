namespace TarjetaSUBE;

public static class Contexto
{
    public static TarjetaDbContext Db { get; set; } = new TarjetaDbContext();
    public static IProveedorDeFecha Reloj { get; set; } = new ProveedorDeFechaSistema();
}
