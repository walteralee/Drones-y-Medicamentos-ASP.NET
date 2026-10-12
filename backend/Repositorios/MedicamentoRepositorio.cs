using DronesYMedicamentos.Api.Datos;
using DronesYMedicamentos.Api.Modelos;
using Microsoft.Data.Sqlite;

namespace DronesYMedicamentos.Api.Repositorios;

public class MedicamentoRepositorio(ConexionSqlite conexionSqlite) : IMedicamentoRepositorio
{
    private const string Select = "SELECT m.Codigo, m.Nombre, m.Peso FROM medicamentos m";

    // Código de error de SQLite cuando se incumple una restricción (UNIQUE, CHECK, FOREIGN KEY...)
    private const int SqliteConstraint = 19;

    public List<Medicamento> ObtenerTodos()
    {
        return Consultar($"{Select};");
    }

    public List<Medicamento> ObtenerEnUso()
    {
        return Consultar($"{Select} INNER JOIN drones_y_medicamentos dm ON m.Codigo = dm.codigo_medicamento;");
    }

    public List<Medicamento> ObtenerDisponibles()
    {
        return Consultar($"{Select} LEFT JOIN drones_y_medicamentos dm ON m.Codigo = dm.codigo_medicamento "
                       + "WHERE dm.codigo_medicamento IS NULL;");
    }

    public Medicamento? ObtenerPorCodigo(string codigo)
    {
        return Consultar($"{Select} WHERE m.Codigo = $codigo;", ("$codigo", codigo)).FirstOrDefault();
    }

    public List<Medicamento> ObtenerDeDron(string numeroSerie)
    {
        return Consultar($"{Select} INNER JOIN drones_y_medicamentos dm ON m.Codigo = dm.codigo_medicamento "
                       + "WHERE dm.numero_serie_dron = $serie;", ("$serie", numeroSerie));
    }

    public bool Insertar(Medicamento medicamento)
    {
        return InsertarSinDuplicados(
            "INSERT INTO medicamentos (Codigo, Nombre, Peso) VALUES ($codigo, $nombre, $peso);",
            ("$codigo", medicamento.Codigo), ("$nombre", medicamento.Nombre), ("$peso", medicamento.Peso));
    }

    public bool CargarEnDron(string numeroSerie, string codigo)
    {
        return InsertarSinDuplicados(
            "INSERT INTO drones_y_medicamentos (numero_serie_dron, codigo_medicamento) VALUES ($serie, $codigo);",
            ("$serie", numeroSerie), ("$codigo", codigo));
    }

    public void EliminarLosDeDron(string numeroSerie)
    {
        using var conexion = conexionSqlite.Abrir();
        using var transaccion = conexion.BeginTransaction();
        using var comando = conexion.CreateCommand();
        comando.Transaction = transaccion;
        comando.CommandText = @"
            DELETE FROM medicamentos
            WHERE Codigo IN (SELECT codigo_medicamento FROM drones_y_medicamentos WHERE numero_serie_dron = $serie);
            DELETE FROM drones_y_medicamentos WHERE numero_serie_dron = $serie;";
        comando.Parameters.AddWithValue("$serie", numeroSerie);
        comando.ExecuteNonQuery();
        transaccion.Commit();
    }

    private bool InsertarSinDuplicados(string sql, params (string nombre, object valor)[] parametros)
    {
        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

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

    private List<Medicamento> Consultar(string sql, params (string nombre, object valor)[] parametros)
    {
        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = sql;
        foreach (var (nombre, valor) in parametros)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

        var medicamentos = new List<Medicamento>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            medicamentos.Add(new Medicamento
            {
                Codigo = lector.GetString(0),
                Nombre = lector.GetString(1),
                Peso = lector.GetDouble(2)
            });
        }

        return medicamentos;
    }
}
