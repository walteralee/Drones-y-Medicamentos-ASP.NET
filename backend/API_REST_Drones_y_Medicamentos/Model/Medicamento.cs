namespace API_REST_Drones_y_Medicamentos.Model
{
    // Medicamento que se puede cargar en un dron. En JSON: codigo, nombre, peso
    public class Medicamento
    {
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        // Peso en gramos
        public double Peso { get; set; }
    }
}
