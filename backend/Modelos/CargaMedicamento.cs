using System.Text.Json.Serialization;

namespace DronesYMedicamentos.Api.Modelos;

// Petición para cargar un medicamento en un dron. En JSON: numero_serie, codigo
public class CargaMedicamento
{
    [JsonPropertyName("numero_serie")]
    public string NumeroSerie { get; set; } = string.Empty;

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = string.Empty;
}
