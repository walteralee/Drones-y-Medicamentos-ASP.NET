// =====================================================================
// Acceso a la API REST. Aquí solo hay peticiones HTTP; la interfaz está en app.js
// =====================================================================

// Si la web la sirve la propia API (lo normal) se usa la misma dirección.
// Si se abre aparte (archivo suelto o "python -m http.server 5500") se llama a localhost:5212
const API_ORIGIN =
  location.protocol === "file:" || location.port === "5500"
    ? "http://localhost:5212"
    : "";
const API_BASE = `${API_ORIGIN}/api/drones`;

// Hace la petición y devuelve el JSON. Si la API responde con error, lanza un Error con su mensaje
async function peticion(ruta, opciones = {}) {
  const respuesta = await fetch(`${API_BASE}${ruta}`, opciones);
  const datos = await respuesta.json().catch(() => ({}));

  if (!respuesta.ok) {
    throw new Error(datos.message || `Error ${respuesta.status} en la API`);
  }

  return datos;
}

function conCuerpo(metodo, cuerpo) {
  return {
    method: metodo,
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(cuerpo),
  };
}

const e = encodeURIComponent;

const Api = {
  // ----- Drones -----
  obtenerDrones: () => peticion("/todos"),

  obtenerDronesDisponibles: () => peticion("/disponibles"),

  registrarDron: (serialNumber, model) =>
    peticion("/registrar", conCuerpo("POST", { serialNumber, model })),

  cambiarEstado: (serie, estado) =>
    peticion(`/actualizar_estado_dron?numero_serie=${e(serie)}&estado=${e(estado)}`, { method: "POST" }),

  // proceso: "cargar" (al 100%) o "reducir" (-1%). Devuelve la nueva batería
  actualizarBateria: (serie, proceso) =>
    peticion(`/actualizar_bateria_dron?numero_serie=${e(serie)}&proceso=${e(proceso)}`, { method: "POST" })
      .then((datos) => datos.bateria),

  // ----- Medicamentos -----
  registrarMedicamento: (codigo, nombre, peso) =>
    peticion("/registrar_medicamento", conCuerpo("POST", { codigo, nombre, peso })),

  // estado: "all", "in-use" o "available"
  obtenerMedicamentos: (estado = "all") => peticion(`/medicamentos?estado=${e(estado)}`),

  existeMedicamento: (codigo) => peticion(`/confirmar_existencia_de_medicamento/${e(codigo)}`),

  // ----- Medicamentos cargados en drones -----
  // La API comprueba peso, batería y estado; si algo falla devuelve el motivo
  cargarMedicamento: (serie, codigo) =>
    peticion("/insertar-medicamento-dron", conCuerpo("POST", { numero_serie: serie, codigo })),

  obtenerMedicamentosDeDron: (serie) => peticion(`/mostrar_medicamentos_de_dron/${e(serie)}`),

  vaciarDron: (serie) =>
    peticion(`/eliminar_relaciones_y_medicamentos/${e(serie)}`, { method: "DELETE" }),
};
