using System.Text.Json.Serialization;

namespace API_REST_Drones_y_Medicamentos.Model
{
    // Petición para cargar un medicamento en un dron. En JSON: numero_serie, codigo
    public class RelacionDM
    {
        [JsonPropertyName("numero_serie")]
        public string NumeroSerie { get; set; } = string.Empty;

        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;
    }
}
