using DronesYMedicamentos.Api.Modelos;

namespace DronesYMedicamentos.Api.Repositorios;

// Acceso a las tablas medicamentos y drones_y_medicamentos (qué medicamentos lleva cada dron)
public interface IMedicamentoRepositorio
{
    List<Medicamento> ObtenerTodos();

    // Medicamentos cargados en algún dron
    List<Medicamento> ObtenerEnUso();

    // Medicamentos sin cargar
    List<Medicamento> ObtenerDisponibles();

    Medicamento? ObtenerPorCodigo(string codigo);

    List<Medicamento> ObtenerDeDron(string numeroSerie);

    // Devuelve false si ya existe un medicamento con ese código
    bool Insertar(Medicamento medicamento);

    // Devuelve false si el medicamento ya está cargado en un dron
    bool CargarEnDron(string numeroSerie, string codigo);

    // Borra los medicamentos que lleva el dron (ya se han entregado) y sus relaciones
    void EliminarLosDeDron(string numeroSerie);
}
