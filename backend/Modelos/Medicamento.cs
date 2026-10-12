namespace DronesYMedicamentos.Api.Modelos;

// Medicamento que se puede cargar en un dron. En JSON: codigo, nombre, peso
public class Medicamento
{
    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    // Peso en gramos
    public double Peso { get; set; }
}
