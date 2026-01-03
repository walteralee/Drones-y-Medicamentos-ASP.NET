using Microsoft.Data.Sqlite;
using System.IO;

namespace API_REST_Drones_y_Medicamentos.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
            var directory = Path.GetDirectoryName(dataSource);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
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
);
";
            command.ExecuteNonQuery();
        }
    }
}
