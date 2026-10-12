namespace API_REST_Drones_y_Medicamentos.Model
{
    // Dron de reparto. En JSON: id, serialNumber, model, weightLimit, battery, state
    public class Drone
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; } = string.Empty;

        // Ligero, Medio, Crucero o Pesado
        public string Model { get; set; } = string.Empty;

        // Peso máximo que puede llevar, en gramos
        public int WeightLimit { get; set; }

        // Porcentaje de batería (0-100)
        public int Battery { get; set; }

        // IDLE, LOADING, LOADED, DELIVERING, DELIVERED o RETURNING
        public string State { get; set; } = "IDLE";
    }
}
