namespace DronesYMedicamentos.Api.Modelos;

// Valores fijos del dominio: modelos, estados y límites de los drones
public static class ReglasDron
{
    // Peso máximo (g) de cada modelo
    public static readonly IReadOnlyDictionary<string, int> LimitePorModelo = new Dictionary<string, int>
    {
        ["Ligero"] = 250,
        ["Medio"] = 500,
        ["Crucero"] = 750,
        ["Pesado"] = 1000
    };

    // Estados por los que pasa un dron. El ciclo de entrega es LOADED → DELIVERING → DELIVERED → RETURNING → IDLE
    public static readonly string[] Estados = { "IDLE", "LOADING", "LOADED", "DELIVERING", "DELIVERED", "RETURNING" };

    public const string EstadoInicial = "IDLE";

    // Solo se pueden cargar medicamentos en estos estados
    public static readonly string[] EstadosQuePermitenCargar = { "IDLE", "LOADING" };

    // Con esta batería (%) o menos un dron no puede cargar ni aparece como disponible
    public const int BateriaMinima = 25;

    public const int BateriaMaxima = 100;
}
