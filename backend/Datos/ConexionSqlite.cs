using Microsoft.Data.Sqlite;

namespace DronesYMedicamentos.Api.Datos;

// Abre conexiones a la base de datos SQLite.
// La ruta del .db de appsettings se resuelve respecto a la carpeta de la aplicación
// y se activan las claves foráneas (en SQLite vienen desactivadas)
public class ConexionSqlite
{
    public string CadenaConexion { get; }

    public ConexionSqlite(IConfiguration configuration, IHostEnvironment environment)
    {
        var cadena = configuration.GetConnectionString("Sqlite");

        if (string.IsNullOrWhiteSpace(cadena))
        {
            throw new InvalidOperationException("Falta la cadena de conexión 'Sqlite' en appsettings.json");
        }

        var builder = new SqliteConnectionStringBuilder(cadena) { ForeignKeys = true };

        if (builder.DataSource != ":memory:" && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(environment.ContentRootPath, builder.DataSource));
        }

        CadenaConexion = builder.ToString();
    }

    public SqliteConnection Abrir()
    {
        var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();
        return conexion;
    }
}
