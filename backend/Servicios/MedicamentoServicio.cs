using DronesYMedicamentos.Api.Errores;
using DronesYMedicamentos.Api.Modelos;
using DronesYMedicamentos.Api.Repositorios;

namespace DronesYMedicamentos.Api.Servicios;

// Reglas de los medicamentos: registro, consulta y filtros
public class MedicamentoServicio(IMedicamentoRepositorio medicamentos)
{
    public void Registrar(Medicamento? medicamento)
    {
        if (medicamento == null)
            throw new ErrorDeNegocio("El medicamento no puede ser nulo.");

        if (string.IsNullOrWhiteSpace(medicamento.Codigo) || string.IsNullOrWhiteSpace(medicamento.Nombre))
            throw new ErrorDeNegocio("Código y nombre son obligatorios.");

        if (medicamento.Peso <= 0)
            throw new ErrorDeNegocio("El peso debe ser mayor que 0.");

        medicamento.Codigo = medicamento.Codigo.Trim();
        medicamento.Nombre = medicamento.Nombre.Trim();

        if (!medicamentos.Insertar(medicamento))
            throw new ErrorDeNegocio("Ya existe un medicamento con ese código.");
    }

    // estado: all (todos), in-use (cargados en un dron) o available (sin cargar)
    public List<Medicamento> Obtener(string? estado)
    {
        return (estado ?? "all").ToLowerInvariant() switch
        {
            "all" => medicamentos.ObtenerTodos(),
            "in-use" => medicamentos.ObtenerEnUso(),
            "available" => medicamentos.ObtenerDisponibles(),
            _ => throw new ErrorDeNegocio("Estado no válido. Usa: all, in-use o available.")
        };
    }

    public bool Existe(string codigo) => medicamentos.ObtenerPorCodigo(codigo) != null;

    public double ObtenerPeso(string codigo)
    {
        var medicamento = medicamentos.ObtenerPorCodigo(codigo)
            ?? throw new NoEncontrado("El medicamento no existe.");

        return medicamento.Peso;
    }
}
