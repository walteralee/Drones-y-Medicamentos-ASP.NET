using DronesYMedicamentos.Api.Modelos;

namespace DronesYMedicamentos.Api.Repositorios;

// Acceso a la tabla drones
public interface IDronRepositorio
{
    List<Drone> ObtenerTodos();

    List<Drone> ObtenerDisponibles(int bateriaMinima);

    Drone? ObtenerPorNumeroSerie(string numeroSerie);

    // Devuelve false si ya existe un dron con ese número de serie
    bool Insertar(Drone dron);

    void ActualizarEstado(string numeroSerie, string estado);

    void ActualizarBateria(string numeroSerie, int bateria);
}
