using Microsoft.AspNetCore.Diagnostics;

namespace DronesYMedicamentos.Api.Errores;

// Último recurso para errores inesperados (fallos de verdad): responde 500 con un JSON { message }
// sin enseñar detalles internos. ASP.NET ya registra estos errores en la consola.
// Los errores esperados (400 y 404) no llegan aquí: los resuelve FiltroErroresDeNegocio
public class ManejadorErrores : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "Error inesperado en el servidor." }, cancellationToken);
        return true;
    }
}
