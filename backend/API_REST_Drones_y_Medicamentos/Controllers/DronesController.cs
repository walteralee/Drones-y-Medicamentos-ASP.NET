using API_REST_Drones_y_Medicamentos.Data;
using API_REST_Drones_y_Medicamentos.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace API_REST_Drones_y_Medicamentos.Controllers
{
    // API de drones y medicamentos. Todas las rutas empiezan por /api/drones
    [ApiController]
    [Route("api/[controller]")]
    public class DronesController : ControllerBase
    {
        // Peso máximo (g) de cada modelo de dron
        private static readonly Dictionary<string, int> LimitePorModelo = new()
        {
            ["Ligero"] = 250,
            ["Medio"] = 500,
            ["Crucero"] = 750,
            ["Pesado"] = 1000
        };

        private static readonly string[] EstadosValidos =
            { "IDLE", "LOADING", "LOADED", "DELIVERING", "DELIVERED", "RETURNING" };

        // Por debajo de esta batería (%) un dron no puede cargar medicamentos
        private const int BateriaMinimaParaCargar = 25;

        private const string ColumnasDron = "id, serial_number, model, weight_limit, battery, state";

        private readonly string _connectionString;

        public DronesController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _connectionString = DatabaseInitializer.ConnectionString(configuration, environment);
        }

        //------------------------------------------------------------------------------------------------
        // DRONES

        // POST: api/drones/registrar
        // El límite de peso sale del modelo y todo dron nuevo empieza con la batería al 100% y en IDLE
        [HttpPost("registrar")]
        public IActionResult RegistrarDrone([FromBody] Drone? drone)
        {
            if (drone == null)
                return BadRequest(new { message = "El dron no puede ser nulo." });

            if (string.IsNullOrWhiteSpace(drone.SerialNumber))
                return BadRequest(new { message = "El número de serie es obligatorio." });

            if (!LimitePorModelo.TryGetValue(drone.Model, out var limite))
                return BadRequest(new { message = "Modelo no válido. Usa: Ligero, Medio, Crucero o Pesado." });

            using var connection = AbrirConexion();
            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO drones (serial_number, model, weight_limit, battery, state)
                VALUES ($serial, $model, $weight, 100, 'IDLE');";

            command.Parameters.AddWithValue("$serial", drone.SerialNumber.Trim());
            command.Parameters.AddWithValue("$model", drone.Model);
            command.Parameters.AddWithValue("$weight", limite);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                return BadRequest(new { message = "Ya existe un dron con ese número de serie." });
            }

            return Ok(new { message = "Dron registrado con éxito." });
        }

        // GET: api/drones/todos
        [HttpGet("todos")]
        public IActionResult ObtenerTodosLosDrones()
        {
            return Ok(LeerDrones($"SELECT {ColumnasDron} FROM drones;"));
        }

        // GET: api/drones/disponibles -> drones en IDLE con más del 25% de batería
        [HttpGet("disponibles")]
        public IActionResult ObtenerDronesDisponibles()
        {
            return Ok(LeerDrones(
                $"SELECT {ColumnasDron} FROM drones WHERE battery > {BateriaMinimaParaCargar} AND state = 'IDLE';"));
        }

        // POST: api/drones/actualizar_estado_dron?numero_serie=X&estado=Y
        [HttpPost("actualizar_estado_dron")]
        public IActionResult ActualizarEstadoDron([FromQuery] string? numero_serie, [FromQuery] string? estado)
        {
            if (string.IsNullOrWhiteSpace(numero_serie) || string.IsNullOrWhiteSpace(estado))
                return BadRequest(new { message = "El número de serie y el estado no pueden ser nulos." });

            if (!EstadosValidos.Contains(estado))
                return BadRequest(new { message = $"Estado no válido. Usa: {string.Join(", ", EstadosValidos)}." });

            using var connection = AbrirConexion();
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE drones SET state = $estado WHERE serial_number = $numero_serie;";
            command.Parameters.AddWithValue("$estado", estado);
            command.Parameters.AddWithValue("$numero_serie", numero_serie);

            if (command.ExecuteNonQuery() == 0)
                return NotFound(new { message = $"No se encontró un dron con el número de serie '{numero_serie}'." });

            return Ok(new { message = $"Estado del dron {numero_serie} actualizado a '{estado}' correctamente." });
        }

        // POST: api/drones/actualizar_bateria_dron?numero_serie=X&proceso=cargar|reducir
        // "cargar" la pone al 100% y "reducir" le quita un 1% (nunca baja de 0)
        [HttpPost("actualizar_bateria_dron")]
        public IActionResult ActualizarBateriaDron([FromQuery] string? numero_serie, [FromQuery] string? proceso)
        {
            if (string.IsNullOrWhiteSpace(numero_serie))
                return BadRequest(new { message = "El número de serie no puede ser nulo." });

            if (proceso != "cargar" && proceso != "reducir")
                return BadRequest(new { message = "Proceso no válido. Usa 'cargar' o 'reducir'." });

            using var connection = AbrirConexion();

            var bateriaActual = LeerBateria(connection, numero_serie);
            if (bateriaActual == null)
                return NotFound(new { message = $"No se encontró un dron con el número de serie '{numero_serie}'." });

            int nuevaBateria = proceso == "cargar" ? 100 : Math.Max(bateriaActual.Value - 1, 0);

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE drones SET battery = $bateria WHERE serial_number = $numero_serie;";
            command.Parameters.AddWithValue("$bateria", nuevaBateria);
            command.Parameters.AddWithValue("$numero_serie", numero_serie);
            command.ExecuteNonQuery();

            return Ok(new { message = "Batería actualizada correctamente.", bateria = nuevaBateria });
        }

        // GET: api/drones/obtener_nivel_bateria_dron/{n_serie}
        [HttpGet("obtener_nivel_bateria_dron/{n_serie}")]
        public IActionResult ObtenerNivelBateriaDron(string n_serie)
        {
            using var connection = AbrirConexion();

            var bateria = LeerBateria(connection, n_serie);
            if (bateria == null)
                return NotFound(new { message = $"No se encontró un dron con el número de serie '{n_serie}'." });

            return Ok(new { bateria });
        }

        //------------------------------------------------------------------------------------------------
        // MEDICAMENTOS

        // POST: api/drones/registrar_medicamento
        [HttpPost("registrar_medicamento")]
        public IActionResult RegistrarMedicamento([FromBody] Medicamento? medicamento)
        {
            if (medicamento == null)
                return BadRequest(new { message = "El medicamento no puede ser nulo." });

            if (string.IsNullOrWhiteSpace(medicamento.Codigo) || string.IsNullOrWhiteSpace(medicamento.Nombre))
                return BadRequest(new { message = "Código y nombre son obligatorios." });

            if (medicamento.Peso <= 0)
                return BadRequest(new { message = "El peso debe ser mayor que 0." });

            using var connection = AbrirConexion();
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO medicamentos (Codigo, Nombre, Peso) VALUES ($codigo, $nombre, $peso);";
            command.Parameters.AddWithValue("$codigo", medicamento.Codigo.Trim());
            command.Parameters.AddWithValue("$nombre", medicamento.Nombre.Trim());
            command.Parameters.AddWithValue("$peso", medicamento.Peso);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                return BadRequest(new { message = "Ya existe un medicamento con ese código." });
            }

            return Ok(new { message = "Medicamento registrado con éxito." });
        }

        // GET: api/drones/medicamentos?estado=all|in-use|available
        // in-use = cargados en algún dron, available = sin cargar
        [HttpGet("medicamentos")]
        public IActionResult ObtenerMedicamentos([FromQuery] string estado = "all")
        {
            string sql = "SELECT m.Codigo, m.Nombre, m.Peso FROM medicamentos m";

            switch (estado.ToLowerInvariant())
            {
                case "all":
                    break;
                case "in-use":
                    sql += " INNER JOIN drones_y_medicamentos dm ON m.Codigo = dm.codigo_medicamento";
                    break;
                case "available":
                    sql += " LEFT JOIN drones_y_medicamentos dm ON m.Codigo = dm.codigo_medicamento"
                         + " WHERE dm.codigo_medicamento IS NULL";
                    break;
                default:
                    return BadRequest(new { message = "Estado no válido. Usa: all, in-use o available." });
            }

            return Ok(LeerMedicamentos(sql + ";"));
        }

        // GET: api/drones/confirmar_existencia_de_medicamento/{codigo} -> true o false
        [HttpGet("confirmar_existencia_de_medicamento/{codigo}")]
        public IActionResult ConfirmarMedicamento(string codigo)
        {
            using var connection = AbrirConexion();
            return Ok(LeerPesoMedicamento(connection, codigo) != null);
        }

        // GET: api/drones/obtener_peso_medicamento/{codigo}
        [HttpGet("obtener_peso_medicamento/{codigo}")]
        public IActionResult ObtenerPesoMedicamento(string codigo)
        {
            using var connection = AbrirConexion();

            var peso = LeerPesoMedicamento(connection, codigo);
            if (peso == null)
                return NotFound(new { message = "El medicamento no existe." });

            return Ok(new { peso });
        }

        //------------------------------------------------------------------------------------------------
        // CARGA DE MEDICAMENTOS EN DRONES

        // POST: api/drones/insertar-medicamento-dron
        // Comprueba todas las reglas antes de cargar: el dron y el medicamento existen, el dron está en
        // IDLE o LOADING, tiene más del 25% de batería y no se pasa de su límite de peso
        [HttpPost("insertar-medicamento-dron")]
        public IActionResult InsertarMedicamentoEnDron([FromBody] RelacionDM? relacion)
        {
            if (relacion == null || string.IsNullOrWhiteSpace(relacion.NumeroSerie) || string.IsNullOrWhiteSpace(relacion.Codigo))
                return BadRequest(new { message = "El número de serie del dron y el código del medicamento son obligatorios." });

            using var connection = AbrirConexion();

            var drones = LeerDrones($"SELECT {ColumnasDron} FROM drones WHERE serial_number = $serie;",
                ("$serie", relacion.NumeroSerie), connection);
            if (drones.Count == 0)
                return NotFound(new { message = "El dron no existe." });
            var dron = drones[0];

            var peso = LeerPesoMedicamento(connection, relacion.Codigo);
            if (peso == null)
                return NotFound(new { message = "El medicamento no existe." });

            if (dron.State != "IDLE" && dron.State != "LOADING")
                return BadRequest(new { message = "Solo se pueden cargar medicamentos en un dron en IDLE o LOADING." });

            if (dron.Battery <= BateriaMinimaParaCargar)
                return BadRequest(new { message = $"El dron necesita más del {BateriaMinimaParaCargar}% de batería para cargar." });

            double pesoCargado = LeerMedicamentos(@"
                SELECT m.Codigo, m.Nombre, m.Peso
                FROM drones_y_medicamentos dm JOIN medicamentos m ON dm.codigo_medicamento = m.Codigo
                WHERE dm.numero_serie_dron = $serie;", ("$serie", relacion.NumeroSerie), connection).Sum(m => m.Peso);

            if (pesoCargado + peso.Value > dron.WeightLimit)
                return BadRequest(new { message = "No se puede cargar: supera el límite de peso del dron." });

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO drones_y_medicamentos (numero_serie_dron, codigo_medicamento)
                VALUES ($serie, $codigo);";
            command.Parameters.AddWithValue("$serie", relacion.NumeroSerie);
            command.Parameters.AddWithValue("$codigo", relacion.Codigo);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
            {
                return BadRequest(new { message = "Este medicamento ya está cargado en un dron." });
            }

            return Ok(new { message = "Relación dron-medicamento insertada con éxito." });
        }

        // GET: api/drones/mostrar_medicamentos_de_dron/{numero_serie}
        [HttpGet("mostrar_medicamentos_de_dron/{numero_serie}")]
        public IActionResult ObtenerMedicamentosDeDron(string numero_serie)
        {
            return Ok(LeerMedicamentos(@"
                SELECT m.Codigo, m.Nombre, m.Peso
                FROM drones_y_medicamentos dm JOIN medicamentos m ON dm.codigo_medicamento = m.Codigo
                WHERE dm.numero_serie_dron = $serie;", ("$serie", numero_serie)));
        }

        // DELETE: api/drones/eliminar_relaciones_y_medicamentos/{numero_serie}
        // Al terminar una entrega, los medicamentos del dron se han entregado: se borran de la base de datos
        [HttpDelete("eliminar_relaciones_y_medicamentos/{numero_serie}")]
        public IActionResult EliminarRelacionesYMedicamentos(string numero_serie)
        {
            using var connection = AbrirConexion();
            using var transaction = connection.BeginTransaction();

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
                    DELETE FROM medicamentos
                    WHERE Codigo IN (SELECT codigo_medicamento FROM drones_y_medicamentos WHERE numero_serie_dron = $serie);
                    DELETE FROM drones_y_medicamentos WHERE numero_serie_dron = $serie;";
                command.Parameters.AddWithValue("$serie", numero_serie);
                command.ExecuteNonQuery();
            }

            transaction.Commit();

            return Ok(new { message = "Todas las relaciones y medicamentos del dron han sido eliminados." });
        }

        //------------------------------------------------------------------------------------------------
        // AYUDAS DE ACCESO A DATOS

        private SqliteConnection AbrirConexion()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private List<Drone> LeerDrones(string sql, (string nombre, object valor)? parametro = null, SqliteConnection? conexion = null)
        {
            var drones = new List<Drone>();
            var connection = conexion ?? AbrirConexion();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                if (parametro != null)
                    command.Parameters.AddWithValue(parametro.Value.nombre, parametro.Value.valor);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    drones.Add(new Drone
                    {
                        Id = reader.GetInt32(0),
                        SerialNumber = reader.GetString(1),
                        Model = reader.GetString(2),
                        WeightLimit = reader.GetInt32(3),
                        Battery = reader.GetInt32(4),
                        State = reader.GetString(5)
                    });
                }
            }
            finally
            {
                if (conexion == null) connection.Dispose();
            }

            return drones;
        }

        private List<Medicamento> LeerMedicamentos(string sql, (string nombre, object valor)? parametro = null, SqliteConnection? conexion = null)
        {
            var medicamentos = new List<Medicamento>();
            var connection = conexion ?? AbrirConexion();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;
                if (parametro != null)
                    command.Parameters.AddWithValue(parametro.Value.nombre, parametro.Value.valor);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    medicamentos.Add(new Medicamento
                    {
                        Codigo = reader.GetString(0),
                        Nombre = reader.GetString(1),
                        Peso = reader.GetDouble(2)
                    });
                }
            }
            finally
            {
                if (conexion == null) connection.Dispose();
            }

            return medicamentos;
        }

        // Batería del dron o null si no existe
        private static int? LeerBateria(SqliteConnection connection, string numeroSerie)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT battery FROM drones WHERE serial_number = $serie;";
            command.Parameters.AddWithValue("$serie", numeroSerie);

            var result = command.ExecuteScalar();
            return result == null ? null : Convert.ToInt32(result);
        }

        // Peso del medicamento o null si no existe
        private static double? LeerPesoMedicamento(SqliteConnection connection, string codigo)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Peso FROM medicamentos WHERE Codigo = $codigo;";
            command.Parameters.AddWithValue("$codigo", codigo);

            var result = command.ExecuteScalar();
            return result == null ? null : Convert.ToDouble(result);
        }
    }
}
