namespace DronesYMedicamentos.Api.Errores;

// Petición que no cumple una regla de la aplicación. Se responde con 400 Bad Request
public class ErrorDeNegocio(string mensaje) : Exception(mensaje);
