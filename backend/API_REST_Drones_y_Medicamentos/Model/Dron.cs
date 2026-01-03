namespace API_REST_Drones_y_Medicamentos.Model
{
    public class Drone
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public int WeightLimit { get; set; }
        public int Battery { get; set; }
        public string State { get; set; }
    }
}
