using Microsoft.Data.Sqlite;

namespace DronesYMedicamentos.Api.Datos;

// Crea la carpeta, el archivo .db y las tablas si todavía no existen
public static class InicializadorBaseDatos
{
    public static void Crear(ConexionSqlite conexionSqlite)
    {
        var archivo = new SqliteConnectionStringBuilder(conexionSqlite.CadenaConexion).DataSource;
        var carpeta = Path.GetDirectoryName(archivo);

        if (!string.IsNullOrEmpty(carpeta))
        {
            Directory.CreateDirectory(carpeta);
        }

        using var conexion = conexionSqlite.Abrir();
        using var comando = conexion.CreateCommand();
        comando.CommandText = @"
CREATE TABLE IF NOT EXISTS drones (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    serial_number TEXT NOT NULL UNIQUE,
    model TEXT NOT NULL CHECK (model IN ('Ligero', 'Medio', 'Crucero', 'Pesado')),
    weight_limit INTEGER NOT NULL CHECK (weight_limit BETWEEN 0 AND 1000),
    battery INTEGER NOT NULL CHECK (battery BETWEEN 0 AND 100),
    state TEXT NOT NULL DEFAULT 'IDLE'
        CHECK (state IN ('IDLE','LOADING','LOADED','DELIVERING','DELIVERED','RETURNING'))
);

CREATE TABLE IF NOT EXISTS medicamentos (
    Codigo TEXT NOT NULL UNIQUE,
    Nombre TEXT NOT NULL,
    Peso REAL NOT NULL,
    PRIMARY KEY (Codigo)
);

CREATE TABLE IF NOT EXISTS drones_y_medicamentos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    numero_serie_dron TEXT NOT NULL,
    codigo_medicamento TEXT NOT NULL UNIQUE,

    FOREIGN KEY (numero_serie_dron) REFERENCES drones(serial_number)
        ON DELETE CASCADE
        ON UPDATE CASCADE,

    FOREIGN KEY (codigo_medicamento) REFERENCES medicamentos(Codigo)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);";
        comando.ExecuteNonQuery();
    }
}
