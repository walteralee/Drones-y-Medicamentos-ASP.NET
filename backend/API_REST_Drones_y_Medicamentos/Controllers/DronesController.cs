using API_REST_Drones_y_Medicamentos.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;


namespace TuProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DronesController : ControllerBase
    {
        private readonly string _connectionString;

        public DronesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Sqlite");

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new Exception("Connection string 'Sqlite' no encontrada");
            }
        }

        // ✅ POST: api/drones/registrar
        [HttpPost("registrar")]
        public IActionResult RegistrarDrone([FromBody] Drone drone)
        {
            if (drone == null)
                return BadRequest(new { message = "El dron no puede ser nulo." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO drones (serial_number, model, weight_limit, battery, state) 
                    VALUES ($serial, $model, $weight, $battery, $state);
                ";

                command.Parameters.AddWithValue("$serial", drone.SerialNumber);
                command.Parameters.AddWithValue("$model", drone.Model);
                command.Parameters.AddWithValue("$weight", drone.WeightLimit);
                command.Parameters.AddWithValue("$battery", drone.Battery);
                command.Parameters.AddWithValue("$state", drone.State);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    return BadRequest(new { message = $"Error al insertar el dron: {ex.Message}" });
                }
            }

            return Ok(new { message = "Dron registrado con éxito." });
        }

        // ✅ GET: api/drones/todos
        [HttpGet("todos")]
        public IActionResult ObtenerTodosLosDrones()
        {
            var drones = new List<Drone>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, serial_number, model, weight_limit, battery, state FROM drones;";

                using (var reader = command.ExecuteReader())
                {
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
            }

            return Ok(drones);
        }

        // ✅ GET: api/drones/disponibles
        [HttpGet("disponibles")]
        public IActionResult ObtenerDronesDisponibles()
        {
            var drones = new List<Drone>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, serial_number, model, weight_limit, battery, state FROM drones WHERE battery > 25 AND state = 'IDLE';";

                using (var reader = command.ExecuteReader())
                {
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
            }

            return Ok(drones);
        }

        [HttpPost("insertar_medicamento")]
        public IActionResult InsertarMedicamento([FromBody] Medicamento medicamento)
        {
            if (medicamento == null)
            {
                return BadRequest(new { message = "El medicamento no puede ser nulo." });
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO medicamentos (Codigo, Nombre, Peso) VALUES ($codigo, $nombre, $peso);";

                command.Parameters.AddWithValue("$codigo", medicamento.codigo);
                command.Parameters.AddWithValue("$nombre", medicamento.nombre);
                command.Parameters.AddWithValue("$peso", medicamento.peso);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    return BadRequest(new { message = $"Error al insertar el medicamento: {ex.Message}" });
                }
            }

            return Ok(new { message = "💊 Medicamento registrado con éxito." });
        }

        [HttpGet("confirmar_existencia_de_medicamento/{codigo}")]
        public IActionResult ConfirmarMedicamento(string codigo)
        {
            bool existe = false;

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM medicamentos WHERE Codigo = $codigo LIMIT 1;";
                command.Parameters.AddWithValue("$codigo", codigo);

                var result = command.ExecuteScalar();
                existe = (result != null);
            }

            return Ok(existe);
        }


        [HttpPost("insertar-medicamento-dron")]
        public IActionResult InsertarMedicamentoEnDron([FromBody] RelacionDM relacion)
        {
            if (relacion == null)
            {
                return BadRequest(new { message = "La relación no puede ser nula." });
            }

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO drones_y_medicamentos (numero_serie_dron, codigo_medicamento) 
            VALUES ($nserie, $codigo);
        ";

                command.Parameters.AddWithValue("$nserie", relacion.numero_serie);
                command.Parameters.AddWithValue("$codigo", relacion.codigo);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    return BadRequest(new { message = $"Error al insertar la relación: {ex.Message}" });
                }
            }

            return Ok(new { message = "✅ Relación dron-medicamento insertada con éxito." });
        }

        [HttpGet("mostrar_medicamentos_de_dron/{numero_serie}")]
        public IActionResult ObtenerMedicamentosDeDron(string numero_serie)
        {
            if (string.IsNullOrWhiteSpace(numero_serie))
                return BadRequest(new { message = "El número de serie no puede ser nulo." });

            var medicamentos = new List<Medicamento>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT m.""Codigo"", m.""Nombre"", m.""Peso""
            FROM drones_y_medicamentos dm
            JOIN medicamentos m ON dm.codigo_medicamento = m.""Codigo""
            WHERE dm.numero_serie_dron = $numeroSerie;
            ";

                command.Parameters.AddWithValue("$numeroSerie", numero_serie);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        medicamentos.Add(new Medicamento
                        {
                            codigo = reader.GetString(0),
                            nombre = reader.GetString(1),
                            peso = reader.GetDouble(2)
                        });
                    }
                }
            }

            // Devuelve lista (vacía si no hay ninguno) — evita NotFound salvo que quieras ese comportamiento
            return Ok(medicamentos);
        }

        [HttpPost("actualizar_estado_dron")]
        public IActionResult ActualizarEstadoDron([FromQuery] string numero_serie, [FromQuery] string estado)
        {
            if (string.IsNullOrWhiteSpace(numero_serie) || string.IsNullOrWhiteSpace(estado))
                return BadRequest(new { message = "⚠️ El número de serie y el estado no pueden ser nulos." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            UPDATE drones
            SET state = $estado
            WHERE serial_number = $numero_serie;
        ";

                command.Parameters.AddWithValue("$estado", estado);
                command.Parameters.AddWithValue("$numero_serie", numero_serie);

                try
                {
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        return NotFound(new { message = $"⚠️ No se encontró un dron con el número de serie '{numero_serie}'." });

                    return Ok(new { message = $"✅ Estado del dron {numero_serie} actualizado a '{estado}' correctamente." });
                }
                catch (SqliteException ex)
                {
                    return BadRequest(new { message = $"❌ Error al actualizar el estado del dron: {ex.Message}" });
                }
            }
        }

        [HttpDelete("eliminar_relaciones_y_medicamentos/{numero_serie}")]
        public IActionResult EliminarRelacionesYMedicamentos(string numero_serie)
        {
            if (string.IsNullOrWhiteSpace(numero_serie))
                return BadRequest(new { message = "El número de serie no puede ser nulo." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Obtener todos los códigos de medicamentos asociados a este dron
                        var getMedsCmd = connection.CreateCommand();
                        getMedsCmd.CommandText = @"
                    SELECT codigo_medicamento 
                    FROM drones_y_medicamentos
                    WHERE numero_serie_dron = $nserie;
                ";
                        getMedsCmd.Parameters.AddWithValue("$nserie", numero_serie);

                        var medicamentosAsociados = new List<string>();
                        using (var reader = getMedsCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                medicamentosAsociados.Add(reader.GetString(0));
                            }
                        }

                        // 2️⃣ Eliminar relaciones del dron en drones_y_medicamentos
                        var delRelationsCmd = connection.CreateCommand();
                        delRelationsCmd.CommandText = @"
                    DELETE FROM drones_y_medicamentos
                    WHERE numero_serie_dron = $nserie;
                ";
                        delRelationsCmd.Parameters.AddWithValue("$nserie", numero_serie);
                        delRelationsCmd.ExecuteNonQuery();

                        // 3️⃣ Eliminar los medicamentos asociados
                        if (medicamentosAsociados.Count > 0)
                        {
                            var delMedsCmd = connection.CreateCommand();
                            delMedsCmd.CommandText = @"
                        DELETE FROM medicamentos
                        WHERE Codigo IN (" + string.Join(",", medicamentosAsociados.Select((_, i) => "$cod" + i)) + ")";

                            for (int i = 0; i < medicamentosAsociados.Count; i++)
                            {
                                delMedsCmd.Parameters.AddWithValue("$cod" + i, medicamentosAsociados[i]);
                            }

                            delMedsCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (SqliteException ex)
                    {
                        transaction.Rollback();
                        return BadRequest(new { message = $"Error al eliminar relaciones y medicamentos: {ex.Message}" });
                    }
                }
            }

            return Ok(new { message = "✅ Todas las relaciones y medicamentos del dron han sido eliminados." });
        }


        [HttpGet("obtener_peso_medicamento/{codigo}")]
        public IActionResult ObtenerPesoMedicamento(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return BadRequest(new { message = "El código del medicamento no puede ser nulo." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT ""Peso""
            FROM medicamentos
            WHERE ""Codigo"" = $codigo;
        ";
                command.Parameters.AddWithValue("$codigo", codigo);

                var result = command.ExecuteScalar();

                if (result == null)
                    return NotFound(new { message = "El medicamento no existe." });

                double peso = Convert.ToDouble(result);

                return Ok(new { peso = peso });
            }
        }

        [HttpPost("actualizar_bateria_dron")]
        public IActionResult ActualizarBateriaDron([FromQuery] string numero_serie,[FromQuery] string proceso)
        {
            if (string.IsNullOrWhiteSpace(numero_serie))
                return BadRequest(new { message = "⚠️ El número de serie no puede ser nulo." });

            if (string.IsNullOrWhiteSpace(proceso))
                return BadRequest(new { message = "⚠️ El proceso no puede ser nulo." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // 1️⃣ Obtener batería actual
                var selectCmd = connection.CreateCommand();
                selectCmd.CommandText =
                    @"SELECT battery FROM drones WHERE serial_number = $numero_serie;";
                selectCmd.Parameters.AddWithValue("$numero_serie", numero_serie);

                var result = selectCmd.ExecuteScalar();

                if (result == null)
                    return NotFound(new
                    {
                        message = $"⚠️ No se encontró un dron con el número de serie '{numero_serie}'."
                    });

                int bateriaActual = Convert.ToInt32(result);
                int nuevaBateria;

                // 2️⃣ Lógica EXACTA según proceso
                if (proceso == "cargar")
                {
                    nuevaBateria = 100;
                }
                else if (proceso == "reducir")
                {
                    nuevaBateria = bateriaActual > 0 ? bateriaActual - 1 : 0;
                }
                else
                {
                    return BadRequest(new
                    {
                        message = "⚠️ Proceso no válido. Usa 'cargar' o 'reducir'."
                    });
                }

                // 3️⃣ Actualizar batería en BD
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
            UPDATE drones
            SET battery = $nuevaBateria
            WHERE serial_number = $numero_serie;
        ";

                updateCmd.Parameters.AddWithValue("$nuevaBateria", nuevaBateria);
                updateCmd.Parameters.AddWithValue("$numero_serie", numero_serie);

                try
                {
                    updateCmd.ExecuteNonQuery();

                    return Ok(new
                    {
                        message = "✅ Batería actualizada correctamente.",
                        bateria = nuevaBateria
                    });
                }
                catch (SqliteException ex)
                {
                    return BadRequest(new
                    {
                        message = $"❌ Error al actualizar la batería: {ex.Message}"
                    });
                }
            }
        }



        [HttpGet("obtener_nivel_bateria_dron/{n_serie}")]
        public IActionResult ObtenerNivelBateriaDron(string n_serie)
        {
            if (string.IsNullOrWhiteSpace(n_serie))
                return BadRequest(new { message = "⚠️ El número de serie no puede ser nulo." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            SELECT battery
            FROM drones
            WHERE serial_number = $n_serie;
            ";
                command.Parameters.AddWithValue("$n_serie", n_serie);

                var result = command.ExecuteScalar();

                if (result == null)
                    return NotFound(new { message = $"❌ No se encontró un dron con el número de serie '{n_serie}'." });

                int nivelBateria = Convert.ToInt32(result);
                return Ok(new { bateria = nivelBateria });
            }
        }

        // ✅ POST: api/drones/registrar_medicamento
        [HttpPost("registrar_medicamento")]
        public IActionResult RegistrarMedicamento([FromBody] Medicamento medicamento)
        {
            // 🔒 Validación básica
            if (medicamento == null)
                return BadRequest(new { message = "El medicamento no puede ser nulo." });

            if (string.IsNullOrWhiteSpace(medicamento.codigo) ||
                string.IsNullOrWhiteSpace(medicamento.nombre))
                return BadRequest(new { message = "Código y nombre son obligatorios." });

            if (medicamento.peso <= 0)
                return BadRequest(new { message = "El peso debe ser mayor que 0." });

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
            INSERT INTO medicamentos (Codigo, Nombre, Peso)
            VALUES ($codigo, $nombre, $peso);
            ";

                command.Parameters.AddWithValue("$codigo", medicamento.codigo);
                command.Parameters.AddWithValue("$nombre", medicamento.nombre);
                command.Parameters.AddWithValue("$peso", medicamento.peso);

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException ex)
                {
                    // 🔁 Código duplicado (PRIMARY KEY / UNIQUE)
                    if (ex.SqliteErrorCode == 19) // SQLITE_CONSTRAINT
                    {
                        return BadRequest(new
                        {
                            message = "❌ Ya existe un medicamento con ese código."
                        });
                    }

                    return BadRequest(new
                    {
                        message = $"❌ Error al insertar el medicamento: {ex.Message}"
                    });
                }
            }

            return Ok(new
            {
                message = "💊 Medicamento registrado con éxito."
            });
        }


        // ✅ GET: api/drones/medicamentos?estado=all|in-use|available
        [HttpGet("medicamentos")]
        public IActionResult ObtenerMedicamentos([FromQuery] string estado = "all")
        {
            var medicamentos = new List<Medicamento>();

            // 🔹 Normalizamos el estado
            estado = estado.ToLower();

            // 🔹 SQL base
            string sql = @"
                        SELECT m.Codigo, m.Nombre, m.Peso
                        FROM medicamentos m
                        ";

            // 🔹 Aplicar filtros según estado
            if (estado == "in-use")
            {
                sql += @"
                       INNER JOIN drones_y_medicamentos dm
                       ON m.Codigo = dm.codigo_medicamento
                       ";
            }
            else if (estado == "available")
            {
                sql += @"
                       LEFT JOIN drones_y_medicamentos dm
                       ON m.Codigo = dm.codigo_medicamento
                       WHERE dm.codigo_medicamento IS NULL
                       ";
            }
            else if (estado != "all")
            {
                // 🔒 Estado inválido
                return BadRequest(new
                {
                    message = "Estado no válido. Usa: all, in-use o available."
                });
            }

            // 🔹 Acceso a BD (UNA sola vez)
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            medicamentos.Add(new Medicamento
                            {
                                codigo = reader.GetString(0),
                                nombre = reader.GetString(1),
                                peso = reader.GetDouble(2)
                            });
                        }
                    }
                }
            }

            return Ok(medicamentos);
        }


    }
}
