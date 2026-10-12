using DronesYMedicamentos.Api.Modelos;
using DronesYMedicamentos.Api.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace DronesYMedicamentos.Api.Controladores;

// Endpoints HTTP de la API. Todas las rutas empiezan por /api/drones.
// Aquí no hay reglas: se recibe la petición, se llama al servicio y se responde.
// Los errores (400 y 404) los convierte en JSON el ManejadorErrores
[ApiController]
[Route("api/[controller]")]
public class DronesController(DronServicio dronServicio, MedicamentoServicio medicamentoServicio) : ControllerBase
{
    //------------------------------------------------------------------------------------------------
    // DRONES

    // POST: api/drones/registrar  { serialNumber, model }
    [HttpPost("registrar")]
    public IActionResult RegistrarDrone([FromBody] Drone? drone)
    {
        dronServicio.Registrar(drone?.SerialNumber, drone?.Model);
        return Ok(new { message = "Dron registrado con éxito." });
    }

    // GET: api/drones/todos
    [HttpGet("todos")]
    public IActionResult ObtenerTodosLosDrones() => Ok(dronServicio.ObtenerTodos());

    // GET: api/drones/disponibles
    [HttpGet("disponibles")]
    public IActionResult ObtenerDronesDisponibles() => Ok(dronServicio.ObtenerDisponibles());

    // POST: api/drones/actualizar_estado_dron?numero_serie=X&estado=Y
    [HttpPost("actualizar_estado_dron")]
    public IActionResult ActualizarEstadoDron([FromQuery] string? numero_serie, [FromQuery] string? estado)
    {
        dronServicio.CambiarEstado(numero_serie, estado);
        return Ok(new { message = $"Estado del dron {numero_serie} actualizado a '{estado}' correctamente." });
    }

    // POST: api/drones/actualizar_bateria_dron?numero_serie=X&proceso=cargar|reducir
    [HttpPost("actualizar_bateria_dron")]
    public IActionResult ActualizarBateriaDron([FromQuery] string? numero_serie, [FromQuery] string? proceso)
    {
        int bateria = dronServicio.ActualizarBateria(numero_serie, proceso);
        return Ok(new { message = "Batería actualizada correctamente.", bateria });
    }

    // GET: api/drones/obtener_nivel_bateria_dron/{n_serie}
    [HttpGet("obtener_nivel_bateria_dron/{n_serie}")]
    public IActionResult ObtenerNivelBateriaDron(string n_serie) => Ok(new { bateria = dronServicio.ObtenerBateria(n_serie) });

    //------------------------------------------------------------------------------------------------
    // MEDICAMENTOS

    // POST: api/drones/registrar_medicamento  { codigo, nombre, peso }
    [HttpPost("registrar_medicamento")]
    public IActionResult RegistrarMedicamento([FromBody] Medicamento? medicamento)
    {
        medicamentoServicio.Registrar(medicamento);
        return Ok(new { message = "Medicamento registrado con éxito." });
    }

    // GET: api/drones/medicamentos?estado=all|in-use|available
    [HttpGet("medicamentos")]
    public IActionResult ObtenerMedicamentos([FromQuery] string? estado) => Ok(medicamentoServicio.Obtener(estado));

    // GET: api/drones/confirmar_existencia_de_medicamento/{codigo}  -> true o false
    [HttpGet("confirmar_existencia_de_medicamento/{codigo}")]
    public IActionResult ConfirmarMedicamento(string codigo) => Ok(medicamentoServicio.Existe(codigo));

    // GET: api/drones/obtener_peso_medicamento/{codigo}
    [HttpGet("obtener_peso_medicamento/{codigo}")]
    public IActionResult ObtenerPesoMedicamento(string codigo) => Ok(new { peso = medicamentoServicio.ObtenerPeso(codigo) });

    //------------------------------------------------------------------------------------------------
    // CARGA DE MEDICAMENTOS EN DRONES

    // POST: api/drones/insertar-medicamento-dron  { numero_serie, codigo }
    [HttpPost("insertar-medicamento-dron")]
    public IActionResult InsertarMedicamentoEnDron([FromBody] CargaMedicamento? carga)
    {
        dronServicio.CargarMedicamento(carga?.NumeroSerie, carga?.Codigo);
        return Ok(new { message = "Relación dron-medicamento insertada con éxito." });
    }

    // GET: api/drones/mostrar_medicamentos_de_dron/{numero_serie}
    [HttpGet("mostrar_medicamentos_de_dron/{numero_serie}")]
    public IActionResult ObtenerMedicamentosDeDron(string numero_serie) => Ok(dronServicio.ObtenerMedicamentosDe(numero_serie));

    // DELETE: api/drones/eliminar_relaciones_y_medicamentos/{numero_serie}
    [HttpDelete("eliminar_relaciones_y_medicamentos/{numero_serie}")]
    public IActionResult EliminarRelacionesYMedicamentos(string numero_serie)
    {
        dronServicio.Vaciar(numero_serie);
        return Ok(new { message = "Todas las relaciones y medicamentos del dron han sido eliminados." });
    }
}
