using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DronesYMedicamentos.Api.Errores;

// Convierte los errores esperados de los servicios en respuestas JSON { message }:
// ErrorDeNegocio -> 400 y NoEncontrado -> 404.
// Se resuelven aquí, dentro de MVC, para que no lleguen al ManejadorErrores
// y la consola no los muestre como fallos del servidor (son respuestas normales de la API)
public class FiltroErroresDeNegocio : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        int? codigo = context.Exception switch
        {
            ErrorDeNegocio => StatusCodes.Status400BadRequest,
            NoEncontrado => StatusCodes.Status404NotFound,
            _ => null
        };

        if (codigo == null)
        {
            return; // Error inesperado: lo trata el ManejadorErrores
        }

        context.Result = new ObjectResult(new { message = context.Exception.Message }) { StatusCode = codigo };
        context.ExceptionHandled = true;
    }
}
