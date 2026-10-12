using Microsoft.AspNetCore.Diagnostics;

namespace DronesYMedicamentos.Api.Errores;

// Convierte las excepciones en respuestas JSON { message } con su código HTTP,
// así los servicios solo lanzan errores y los controladores no repiten try/catch
public class ManejadorErrores(ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (codigo, mensaje) = exception switch
        {
            ErrorDeNegocio e => (StatusCodes.Status400BadRequest, e.Message),
            NoEncontrado e => (StatusCodes.Status404NotFound, e.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error inesperado en el servidor.")
        };

        if (codigo == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado");
        }

        context.Response.StatusCode = codigo;
        await context.Response.WriteAsJsonAsync(new { message = mensaje }, cancellationToken);
        return true;
    }
}
