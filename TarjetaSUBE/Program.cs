using TarjetaSUBE;

Console.WriteLine("=== Sistema SUBE Rosario - Iteración 1 ===");

// Asegurar creación de base de datos SQLite local
Contexto.Db.Database.EnsureCreated();

var colectivo = new Colectivo("102 Negra");
Contexto.Db.Colectivos.Add(colectivo);
Contexto.Db.SaveChanges();

var tarjeta = new Tarjeta();
Contexto.Db.Tarjetas.Add(tarjeta);
Contexto.Db.SaveChanges();

Console.WriteLine($"Tarjeta creada. Saldo inicial: ${tarjeta.Saldo}");

// Carga de saldo válida
tarjeta.Cargar(2000m);
Console.WriteLine($"Carga realizada: $2000. Saldo actual: ${tarjeta.Saldo}");

// Pago de pasaje
var boleto = colectivo.pagarCon(tarjeta);
if (boleto is not null)
{
    Console.WriteLine("Viaje pagado con éxito:");
    Console.WriteLine(boleto);
}
else
{
    Console.WriteLine("No se pudo pagar el pasaje (saldo insuficiente).");
}

Contexto.Db.Dispose();
