using DronesYMedicamentos.Api.Errores;
using DronesYMedicamentos.Api.Modelos;
using DronesYMedicamentos.Api.Repositorios;

namespace DronesYMedicamentos.Api.Servicios;

// Reglas de los drones: registro, estados, batería y carga de medicamentos
public class DronServicio(IDronRepositorio drones, IMedicamentoRepositorio medicamentos)
{
    public List<Drone> ObtenerTodos() => drones.ObtenerTodos();

    // Drones en IDLE con más batería que el mínimo
    public List<Drone> ObtenerDisponibles() => drones.ObtenerDisponibles(ReglasDron.BateriaMinima);

    // El límite de peso sale del modelo y todo dron nuevo empieza con la batería al 100% y en IDLE
    public void Registrar(string? numeroSerie, string? modelo)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie))
            throw new ErrorDeNegocio("El número de serie es obligatorio.");

        if (modelo == null || !ReglasDron.LimitePorModelo.TryGetValue(modelo, out var limite))
            throw new ErrorDeNegocio("Modelo no válido. Usa: Ligero, Medio, Crucero o Pesado.");

        var dron = new Drone
        {
            SerialNumber = numeroSerie.Trim(),
            Model = modelo,
            WeightLimit = limite,
            Battery = ReglasDron.BateriaMaxima,
            State = ReglasDron.EstadoInicial
        };

        if (!drones.Insertar(dron))
            throw new ErrorDeNegocio("Ya existe un dron con ese número de serie.");
    }

    public void CambiarEstado(string? numeroSerie, string? estado)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie) || string.IsNullOrWhiteSpace(estado))
            throw new ErrorDeNegocio("El número de serie y el estado no pueden ser nulos.");

        if (!ReglasDron.Estados.Contains(estado))
            throw new ErrorDeNegocio($"Estado no válido. Usa: {string.Join(", ", ReglasDron.Estados)}.");

        ObtenerExistente(numeroSerie);
        drones.ActualizarEstado(numeroSerie, estado);
    }

    // "cargar" pone la batería al 100% y "reducir" le quita un 1% (nunca baja de 0). Devuelve la nueva batería
    public int ActualizarBateria(string? numeroSerie, string? proceso)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie))
            throw new ErrorDeNegocio("El número de serie no puede ser nulo.");

        if (proceso != "cargar" && proceso != "reducir")
            throw new ErrorDeNegocio("Proceso no válido. Usa 'cargar' o 'reducir'.");

        var dron = ObtenerExistente(numeroSerie);
        int nuevaBateria = proceso == "cargar" ? ReglasDron.BateriaMaxima : Math.Max(dron.Battery - 1, 0);

        drones.ActualizarBateria(numeroSerie, nuevaBateria);
        return nuevaBateria;
    }

    public int ObtenerBateria(string numeroSerie) => ObtenerExistente(numeroSerie).Battery;

    // Carga un medicamento en un dron si se cumplen todas las reglas: el dron está en IDLE o LOADING,
    // tiene más del 25% de batería, no se pasa de su límite de peso y el medicamento no está en otro dron
    public void CargarMedicamento(string? numeroSerie, string? codigo)
    {
        if (string.IsNullOrWhiteSpace(numeroSerie) || string.IsNullOrWhiteSpace(codigo))
            throw new ErrorDeNegocio("El número de serie del dron y el código del medicamento son obligatorios.");

        var dron = ObtenerExistente(numeroSerie);
        var medicamento = medicamentos.ObtenerPorCodigo(codigo)
            ?? throw new NoEncontrado("El medicamento no existe.");

        if (!ReglasDron.EstadosQuePermitenCargar.Contains(dron.State))
            throw new ErrorDeNegocio("Solo se pueden cargar medicamentos en un dron en IDLE o LOADING.");

        if (dron.Battery <= ReglasDron.BateriaMinima)
            throw new ErrorDeNegocio($"El dron necesita más del {ReglasDron.BateriaMinima}% de batería para cargar.");

        double pesoCargado = medicamentos.ObtenerDeDron(numeroSerie).Sum(m => m.Peso);
        if (pesoCargado + medicamento.Peso > dron.WeightLimit)
            throw new ErrorDeNegocio("No se puede cargar: supera el límite de peso del dron.");

        if (!medicamentos.CargarEnDron(numeroSerie, codigo))
            throw new ErrorDeNegocio("Este medicamento ya está cargado en un dron.");
    }

    public List<Medicamento> ObtenerMedicamentosDe(string numeroSerie) => medicamentos.ObtenerDeDron(numeroSerie);

    // Al terminar una entrega los medicamentos del dron se han entregado: se borran
    public void Vaciar(string numeroSerie) => medicamentos.EliminarLosDeDron(numeroSerie);

    private Drone ObtenerExistente(string numeroSerie)
    {
        return drones.ObtenerPorNumeroSerie(numeroSerie)
            ?? throw new NoEncontrado($"No se encontró un dron con el número de serie '{numeroSerie}'.");
    }
}
