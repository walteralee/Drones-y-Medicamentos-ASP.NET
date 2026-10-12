namespace DronesYMedicamentos.Api.Errores;

// El dron o el medicamento pedido no existe. Se responde con 404 Not Found
public class NoEncontrado(string mensaje) : Exception(mensaje);
