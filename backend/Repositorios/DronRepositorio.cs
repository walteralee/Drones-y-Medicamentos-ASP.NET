using DronesYMedicamentos.Api.Datos;
using DronesYMedicamentos.Api.Modelos;
using Microsoft.Data.Sqlite;

namespace DronesYMedicamentos.Api.Repositorios;

public class DronRepositorio(ConexionSqlite conexionSqlite) : IDronRepositorio
{
    private const string Columnas = "id, serial_number, model, weight_limit, battery, state";

    // Código de error de SQLite cuando se incumple una restricción (UNIQUE, CHECK, FOREIGN KEY...)
    private const int SqliteConstraint = 19;

    public List<Drone> ObtenerTodos()
    {
        return Consultar($"SELECT {Columnas} FROM drones;");
    }

    public List<Drone> ObtenerDisponibles(int bateriaMinima)
    {
        return Consultar($"SELECT {Columnas} FROM drones WHERE battery > $bateria AND state = 'IDLE';",
            ("$bateria", bateriaMinima));
    }

    public Drone? ObtenerPorNumeroSerie(string numeroSerie)
    {
        return Consultar($"SELECT {Columnas} FROM drones WHERE serial_number = $serie;", ("$serie", numeroSerie))
            .FirstOrDefault();
    }

    public bool Insertar(Drone dron)
    {
        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"
            INSERT INTO drones (serial_number, model, weight_limit, battery, state)
            VALUES ($serie, $modelo, $limite, $bateria, $estado);";
        comando.Parameters.AddWithValue("$serie", dron.SerialNumber);
        comando.Parameters.AddWithValue("$modelo", dron.Model);
        comando.Parameters.AddWithValue("$limite", dron.WeightLimit);
        comando.Parameters.AddWithValue("$bateria", dron.Battery);
        comando.Parameters.AddWithValue("$estado", dron.State);

        try
        {
            comando.ExecuteNonQuery();
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteConstraint)
        {
            return false;
        }
    }

    public void ActualizarEstado(string numeroSerie, string estado)
    {
        Actualizar("UPDATE drones SET state = $valor WHERE serial_number = $serie;", numeroSerie, estado);
    }

    public void ActualizarBateria(string numeroSerie, int bateria)
    {
        Actualizar("UPDATE drones SET battery = $valor WHERE serial_number = $serie;", numeroSerie, bateria);
    }

    private void Actualizar(string sql, string numeroSerie, object valor)
    {
        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        comando.Parameters.AddWithValue("$serie", numeroSerie);
        comando.Parameters.AddWithValue("$valor", valor);
        comando.ExecuteNonQuery();
    }

    private List<Drone> Consultar(string sql, params (string nombre, object valor)[] parametros)
    {
        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

        var drones = new List<Drone>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            drones.Add(new Drone
            {
                Id = lector.GetInt32(0),
                SerialNumber = lector.GetString(1),
                Model = lector.GetString(2),
                WeightLimit = lector.GetInt32(3),
                Battery = lector.GetInt32(4),
                State = lector.GetString(5)
            });
        }

        return drones;
    }
}
