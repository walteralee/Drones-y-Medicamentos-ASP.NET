let drones = [];
const droneList = document.getElementById("drone-list");
const registerBtn = document.getElementById("register-btn");
const registerMedicamentBtn = document.getElementById(
  "register-medicament-btn"
);
const cargasBateriaActivas = {};

// URL base de tu API
const API_BASE = "http://localhost:5212/api/drones";

// ✅ Cambiar de sección
function showSection(section) {
  // Ocultar todas las secciones
  document.querySelectorAll(".page-section").forEach((sec) => {
    sec.classList.add("hidden");
  });

  // Mostrar la sección seleccionada
  const activeSection = document.getElementById(section + "-section");
  activeSection.classList.remove("hidden");

  // Resetear filtros SOLO de esta sección
  resetFiltrosDeSeccion(activeSection);

  // Cargar datos por defecto
  if (section === "list") {
    cargarTodos(); // filtro ALL por defecto
  }

  if (section === "medicaments") {
    cargarMedicamentos("all"); // filtro ALL por defecto
  }
}

function resetFiltrosDeSeccion(section) {
  const groups = section.querySelectorAll(".filter-buttons");

  groups.forEach((group) => {
    const buttons = group.querySelectorAll(".filter-btn");
    buttons.forEach((b) => b.classList.remove("active"));
    if (buttons.length > 0) {
      buttons[0].classList.add("active");
    }
  });
}

// Bloquear caracteres no numéricos en el input de peso

const pesoInput = document.getElementById("med-weight");

pesoInput.addEventListener("input", function () {
  // 1️⃣ Eliminar todo lo que no sean números
  this.value = this.value.replace(/[^0-9]/g, "");

  if (this.value === "") return;

  let valor = parseInt(this.value, 10);

  // 2️⃣ Mínimo 1
  if (valor < 1) {
    this.value = 1;
  }

  // 3️⃣ Máximo 1000
  if (valor > 1000) {
    this.value = 1000;
  }
});

//----------------------------------------------------------- REGISTRAR DRON ------------------------------------------------------------------------------

// ✅ Registrar dron
registerBtn.addEventListener("click", async () => {
  const serial = document.getElementById("serial").value.trim();
  const model = document.getElementById("model").value;

  if (!serial) {
    alert("⚠️ El número de serie es obligatorio");
    return;
  }

  // 🔹 Asignar peso según el modelo
  let weightLimit = 0;
  if (model === "Ligero") {
    weightLimit = 250;
  } else if (model === "Medio") {
    weightLimit = 500;
  } else if (model === "Crucero") {
    weightLimit = 750;
  } else if (model === "Pesado") {
    weightLimit = 1000;
  } else {
    alert("⚠️ Modelo de dron no válido");
    return;
  }

  const nuevoDrone = {
    serialNumber: serial,
    model: model,
    weightLimit: weightLimit,
    battery: 100,
    state: "IDLE",
  };

  try {
    const response = await fetch(`${API_BASE}/registrar`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(nuevoDrone),
    });

    if (response.ok) {
      alert("✅ Dron registrado con éxito");
      // 🔹 Resetear campos
      document.getElementById("serial").value = "";
      document.getElementById("model").selectedIndex = 0; // vuelve al primer modelo
      await cargarTodos();
    } else {
      const error = await response.json();
      alert("❌ Error: " + error.message);
    }
  } catch (err) {
    alert("⚠️ Error de conexión con la API");
  }
});

//----------------------------------------------------------- REGISTRAR MEDICAMENTO ----------------------------------------------------------------

registerMedicamentBtn.addEventListener("click", async () => {
  const codigo = document.getElementById("med-code").value.trim();
  const nombre = document.getElementById("med-name").value.trim();
  const pesoStr = document.getElementById("med-weight").value.trim();
  const peso = parseInt(pesoStr, 10);

  // 🔒 Validaciones frontend (coinciden con el backend)
  if (!codigo || !nombre) {
    alert("⚠️ Código y nombre son obligatorios");
    return;
  }

  if (isNaN(peso) || peso < 1 || peso > 1000) {
    alert("⚠️ El peso debe ser un número entre 1 y 1000 gramos");
    return;
  }

  // 📦 Objeto EXACTO que espera el backend
  const nuevoMedicamento = {
    codigo: codigo,
    nombre: nombre,
    peso: peso,
  };

  try {
    const response = await fetch(`${API_BASE}/registrar_medicamento`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(nuevoMedicamento),
    });

    const result = await response.json();

    if (response.ok) {
      alert("✅ Medicamento registrado con éxito");

      // 🧹 Limpiar formulario
      document.getElementById("med-code").value = "";
      document.getElementById("med-name").value = "";
      document.getElementById("med-weight").value = "";

      // (opcional) refrescar listado si ya lo tienes
      // cargarMedicamentos();
    } else {
      alert("❌ Error: " + result.message);
    }
  } catch (error) {
    console.error(error);
    alert("⚠️ Error de conexión con la API");
  }
});

//----------------------------------------------------------- FILTRADO ------------------------------------------------------------------------------

// ✅ Botones de filtro
document
  .getElementById("filter-btn-all")
  .addEventListener("click", cargarTodos);
document
  .getElementById("filter-btn-disponibles")
  .addEventListener("click", cargarDisponibles);

// ✅ Obtener todos los drones
async function cargarTodos() {
  try {
    const response = await fetch(`${API_BASE}/todos`);
    drones = await response.json();
    renderDrones();
  } catch (err) {
    console.error("Error al cargar drones:", err);
  }
}

// ✅ Obtener drones disponibles
async function cargarDisponibles() {
  try {
    const response = await fetch(`${API_BASE}/disponibles`);
    drones = await response.json();

    if (response.ok) {
      renderDrones();
    } else {
      alert("Error al cargar drones disponibles.");
    }
  } catch (err) {
    console.error("Error al cargar drones disponibles:", err);
  }
}

// ✅ Renderizado de lista
function renderDrones() {
  droneList.innerHTML = "";

  if (!drones || drones.length === 0) {
    droneList.innerHTML = "<p>No hay drones registrados.</p>";
    return;
  }

  drones.forEach((drone) => {
    const card = document.createElement("div");
    card.classList.add("drone-card");

    card.innerHTML = `
      <img src="imagenes/dron.jpg" alt="Dron">
      <div class="drone-info">
        <p><strong>Número de serie:</strong> ${drone.serialNumber}</p>
        <p><strong>Modelo:</strong> ${drone.model}</p>
        <p><strong>Límite peso:</strong> ${drone.weightLimit} gr</p>
        <p><strong>Batería:</strong> ${drone.battery}%</p>
        <p><strong>Estado:</strong> ${drone.state}</p>

        <button class="load-btn" ${
          drone.battery <= 25 ? "disabled" : ""
        }>Cargar</button>

        <button class="show-medicaments">Mostrar medicamentos</button>

        <button class="low-battery" ${
          drone.battery === 100 ||
          (drone.state !== "IDLE" && drone.state !== "LOADING")
            ? "disabled"
            : ""
        }>Cargar batería
        </button>


        <button class="start" ${
          drone.state !== "LOADING" ? "disabled" : ""
        }>Comenzar</button>

        <div class="medicament-list hidden">
          <p><em>No hay medicamentos cargados.</em></p>
        </div>
      </div>
    `;

    const loadBtn = card.querySelector(".load-btn");
    const showMedBtn = card.querySelector(".show-medicaments");
    const startBtn = card.querySelector(".start");
    const chargeBtn = card.querySelector(".low-battery");

    // Mostrar formulario de carga
    loadBtn.addEventListener("click", () => showLoadForm(card, drone));

    // Mostrar medicamentos del dron
    showMedBtn.addEventListener("click", () => {
      cargarMedicamentosParaDrone(card, drone.serialNumber);
    });

    // Cargar batería (SOLO en IDLE)
    chargeBtn.addEventListener("click", () => {
      if (drone.state !== "IDLE" && drone.state !== "LOADING") return;
      cargar_dron(drone.serialNumber);
    });

    // Proceso de entrega
    startBtn.addEventListener("click", async () => {
      drone.state = "LOADED";
      await modificarEstadoDron(drone.serialNumber, "LOADED");
      renderDrones();
      procesoDeEntrega(drone);
      vaciarDron(drone);
      renderDrones();
    });

    droneList.appendChild(card);
  });
}

//------------------------------------------------------- LISTAR MEDICAMENTOS ----------------------------------------------------------------------

const medicamentList = document.getElementById("medicament-list");
const API_MEDICAMENTOS = "http://localhost:5212/api/Drones/medicamentos";

// Se ejecuta al pulsar el botón del menú
async function cargarMedicamentos(estado = "all") {
  try {
    const url =
      estado === "all"
        ? API_MEDICAMENTOS
        : `${API_MEDICAMENTOS}?estado=${estado}`;

    const response = await fetch(url);

    if (!response.ok) {
      throw new Error("Error al obtener medicamentos");
    }

    const medicamentos = await response.json();
    renderMedicamentos(medicamentos);
  } catch (error) {
    console.error(error);
    medicamentList.innerHTML = "<p>❌ Error al cargar los medicamentos.</p>";
  }
}

// Renderizado en el DOM
function renderMedicamentos(medicamentos) {
  medicamentList.innerHTML = "";

  if (!medicamentos || medicamentos.length === 0) {
    medicamentList.innerHTML =
      "<p><em>No hay medicamentos registrados.</em></p>";
    return;
  }

  medicamentos.forEach((med) => {
    const card = document.createElement("div");
    card.classList.add("medicament-card");

    card.innerHTML = `
      <img src="imagenes/medicamento.png" alt="Medicamento">
      <div class="medicament-info">
        <p><strong>Código:</strong> ${med.codigo}</p>
        <p><strong>Nombre:</strong> ${med.nombre}</p>
        <p><strong>Peso:</strong> ${med.peso} g</p>
      </div>
    `;

    medicamentList.appendChild(card);
  });
}

document.querySelectorAll(".filter-btn").forEach((btn) => {
  btn.addEventListener("click", () => {
    // Quitar active a todos
    document
      .querySelectorAll(".filter-btn")
      .forEach((b) => b.classList.remove("active"));

    // Activar el actual
    btn.classList.add("active");

    // Cargar según filtro
    const estado = btn.dataset.filter;
    cargarMedicamentos(estado);
  });
});

//----------------------------------------------------------- CARGAR DRON ------------------------------------------------------------------------------

// 🔹 MOSTRAR FORMULARIO DE CARGA DE MEDICAMENTOS
function showLoadForm(card, drone) {
  // Evita que se duplique el formulario si ya existe
  if (card.querySelector("#load-form")) return;

  const form = document.createElement("div");
  form.id = "load-form";

  form.innerHTML = `
    <h4>Cargar Medicamentos</h4>
    <input type="text" placeholder="Código" class="med-code">
    <button id="add-med-btn">Añadir</button>
    <button id="cancel-btn">Cancelar</button>
  `;

  // Botón cancelar → elimina el formulario
  form.querySelector("#cancel-btn").addEventListener("click", () => {
    form.remove();
  });

  // Botón añadir medicamento (aquí luego puedes conectar a tu API de medicamentos)
  form.querySelector("#add-med-btn").addEventListener("click", async () => {
    const codigo = form.querySelector(".med-code").value.trim();
    const numero_serie_dron = drone.serialNumber;

    if (!codigo) {
      alert("⚠️ Completa el campo de código");
      return;
    }

    const existe = await confirmarMedicamento(codigo);

    if (existe) {
      const limite_peso = await comprobarLimitePeso(card, codigo);

      if (limite_peso) {
        const ok = await insertarDronMedicamento(numero_serie_dron, codigo);
        if (ok) {
          await cargarMedicamentosParaDrone(card, numero_serie_dron);
          drone.state = "LOADING";
          await modificarEstadoDron(drone.serialNumber, "LOADING");
          renderDrones();
        }
      } else {
        alert(
          "No se puede insertar el medicamento porque supera el limite de peso."
        );
      }
    } else {
      alert("❌ El medicamento no existe en la base de datos");
    }

    form.remove(); // cierra el formulario tras añadir
  });

  card.appendChild(form);
}

// 🔹 Función para confirmar existencia de medicamento
async function confirmarMedicamento(codigo) {
  try {
    const response = await fetch(
      `http://localhost:5212/api/drones/confirmar_existencia_de_medicamento/${codigo}`
    );

    if (!response.ok) {
      throw new Error("Error en la API");
    }

    const existe = await response.json(); // devuelve true o false
    return existe;
  } catch (error) {
    console.error("⚠️ Error al consultar el medicamento:", error);
    return false;
  }
}

async function comprobarLimitePeso(card, codigo) {
  try {
    // 1️⃣ Obtener el límite de peso del dron desde el card
    const total = parseFloat(
      card
        .querySelector(".drone-info p:nth-child(3)")
        .textContent.match(/\d+/)[0] // extrae el número de "Límite peso: xxx gr"
    );

    // 2️⃣ Sumar todos los pesos actuales de los medicamentos del dron
    const medicamentItems = card.querySelectorAll(".med-item");
    let cantidad_actual = 0;

    medicamentItems.forEach((item) => {
      const match = item.textContent.match(/(\d+)\s*g/);
      if (match) cantidad_actual += parseFloat(match[1]);
    });

    // 3️⃣ Obtener el peso del medicamento a agregar desde la API
    const response = await fetch(
      `${API_BASE}/obtener_peso_medicamento/${encodeURIComponent(codigo)}`
    );
    if (!response.ok) {
      console.error(
        "❌ Error al obtener el peso del medicamento:",
        await response.text()
      );
      return false;
    }

    const data = await response.json();
    const peso_medicamento = parseFloat(data.peso);

    // 4️⃣ Calcular si el dron puede soportar el nuevo medicamento
    const resultado = total - (cantidad_actual + peso_medicamento);

    console.log(
      `🧮 Peso total: ${total} | Cargado: ${cantidad_actual} | Nuevo: ${peso_medicamento} | Resultado: ${resultado}`
    );

    return resultado >= 0; // true si puede cargarlo, false si excede el límite
  } catch (error) {
    console.error("⚠️ Error en comprobarLimitePeso:", error);
    return false;
  }
}

// 🔹 Insertar relación Dron - Medicamento
async function insertarDronMedicamento(nserie_dron, codigo_medicamento) {
  try {
    const response = await fetch(
      "http://localhost:5212/api/drones/insertar-medicamento-dron",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          numero_serie: nserie_dron,
          codigo: codigo_medicamento,
        }),
      }
    );

    if (response.ok) {
      const result = await response.json();
      alert(
        `✅ Medicamento añadido correctamente:\nCódigo: ${codigo_medicamento}\nDron: ${nserie_dron}`
      );
      return true;
    } else {
      const error = await response.json();
      if (error.message?.includes("UNIQUE")) {
        alert("⚠️ Este medicamento ya está asignado a un dron.");
      } else {
        alert(
          "❌ Error al insertar medicamento. Verifica si existe o ya está insertado."
        );
      }
      return false;
    }
  } catch (error) {
    console.error("⚠️ Error al insertar relación dron-medicamento:", error);
    alert("⚠️ Error de conexión con la API.");
    return false;
  }
}

//----------------------------------------------------------- MOSTRAR MEDICAMENTOS --------------------------------------------------------------

async function cargarMedicamentosParaDrone(card, numeroSerie) {
  const medicamentList = card.querySelector(".medicament-list");
  try {
    const res = await fetch(
      `${API_BASE}/mostrar_medicamentos_de_dron/${encodeURIComponent(
        numeroSerie
      )}`
    );
    if (!res.ok) {
      console.error("Error al obtener medicamentos:", await res.text());
      medicamentList.innerHTML = "<p>Error al cargar medicamentos.</p>";
      medicamentList.classList.remove("hidden");
      return;
    }

    const meds = await res.json();
    if (!meds || meds.length === 0) {
      medicamentList.innerHTML =
        "<p><em>No hay medicamentos cargados.</em></p>";
    } else {
      medicamentList.innerHTML = meds
        .map(
          (m) => `
        <div class="med-item">
          <strong>${m.codigo}</strong> — ${m.nombre} — ${m.peso} g
        </div>
      `
        )
        .join("");
    }
    medicamentList.classList.remove("hidden");
  } catch (err) {
    console.error("Error de conexión:", err);
    medicamentList.innerHTML = "<p>Error de conexión.</p>";
    medicamentList.classList.remove("hidden");
  }
}

//-------------------------------------------------- ACTUALIZAR ESTADO DRON --------------------------------------------------------------------

async function modificarEstadoDron(nserie_dron, estado) {
  try {
    const response = await fetch(
      `http://localhost:5212/api/drones/actualizar_estado_dron?numero_serie=${encodeURIComponent(
        nserie_dron
      )}&estado=${encodeURIComponent(estado)}`,
      { method: "POST" }
    );

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      alert(
        `❌ Error al actualizar estado del dron: ${
          error.message || "Error desconocido"
        }`
      );
      return false;
    }

    const result = await response.json();
    console.log("✅ Estado actualizado:", result.message);
    return true;
  } catch (error) {
    console.error("⚠️ Error de conexión:", error);
    alert(
      "⚠️ Error al actualizar estado del dron (no hay conexión con el servidor)"
    );
    return false;
  }
}

async function procesoDeEntrega(dron, intervalo = 3000) {
  // intervalo en ms, por ejemplo 3s
  const estadosDron = [
    "LOADED",
    "DELIVERING",
    "DELIVERED",
    "RETURNING",
    "IDLE",
  ];

  for (const estado of estadosDron) {
    // Cambiar el estado del dron en frontend
    dron.state = estado;

    // Actualizar el estado en backend
    await modificarEstadoDron(dron.serialNumber, estado);

    // Renderizar cambios en la UI
    renderDrones();

    //Reducri la bateria del dron en 1%
    await actualizar_bateria_dron(dron.serialNumber, "reducir");

    cargarTodos();

    // Esperar intervalo antes de pasar al siguiente estado
    await new Promise((resolve) => setTimeout(resolve, intervalo));
  }

  console.log(
    `✅ El dron ${dron.serialNumber} ha completado el ciclo de entrega.`
  );
}

async function vaciarDron(drone) {
  try {
    const response = await fetch(
      `http://localhost:5212/api/drones/eliminar_relaciones_y_medicamentos/${encodeURIComponent(
        drone.serialNumber
      )}`,
      { method: "DELETE" }
    );

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      alert("❌ Error al vaciar el dron");
      return false;
    }

    const result = await response.json().catch(() => ({}));
    console.log("✅ Dron vaciado con éxito:", result.message);
    return true;
  } catch (error) {
    console.error("⚠️ Error de conexión:", error);
    alert("⚠️ Error al vaciar el dron (no hay conexión con el servidor)");
    return false;
  }
}

//-------------------------------------------------- ACTUALIZAR BATERIA DRON --------------------------------------------------------------------

async function actualizar_bateria_dron(numero_serie, proceso) {
  const dron = drones.find((d) => d.serialNumber === numero_serie);
  if (!dron) return;

  // Llamada ÚNICA al backend (él decide qué hacer)
  const response = await fetch(
    `${API_BASE}/actualizar_bateria_dron?numero_serie=${encodeURIComponent(
      numero_serie
    )}&proceso=${encodeURIComponent(proceso)}`,
    { method: "POST" }
  );

  if (!response.ok) {
    console.error("❌ Error actualizando batería");
    return;
  }

  const data = await response.json();

  // Sincronizar frontend con backend
  dron.battery = data.bateria;
}

function reducir_bateria_todos_los_drones(intervalo = 60000) {
  setInterval(async () => {
    for (const dron of drones) {
      // 🔻 Una reducción por dron, sin importar el estado
      await actualizar_bateria_dron(dron.serialNumber, "reducir");
    }

    // 🔄 Refrescar UI tras sincronizar con BD
    renderDrones();
  }, intervalo);
}

function cargar_dron(numero_serie_dron) {
  const dron = drones.find((d) => d.serialNumber === numero_serie_dron);
  if (!dron) return;

  // ❌ Solo se puede cargar en IDLE
  if (dron.state !== "IDLE" && dron.state !== "LOADING") return;

  // 🔋 Disparo al backend (carga a 100)
  actualizar_bateria_dron(numero_serie_dron, "cargar")
    .then(() => {
      // 🔄 Refrescar UI cuando el backend termine
      renderDrones();
    })
    .catch((err) => {
      console.error("❌ Error cargando dron:", err);
    });
}

//----------------------------------------------------------- MAIN ------------------------------------------------------------------------------

// ✅ Cargar todos al inicio
cargarTodos();

// 🔋 Iniciar consumo global de batería
reducir_bateria_todos_los_drones();

