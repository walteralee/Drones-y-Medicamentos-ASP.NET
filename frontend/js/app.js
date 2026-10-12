// =====================================================================
// Interfaz de la web: secciones, formularios, listados y entregas.
// Las peticiones a la API están en api.js (objeto Api)
// =====================================================================

// Tiempo que el dron pasa en cada estado durante una entrega
const MS_POR_ESTADO_ENTREGA = 3000;

// Cada cuánto pierden batería todos los drones (1% por vez)
const MS_DESCARGA_BATERIA = 60000;

const CICLO_ENTREGA = ["LOADED", "DELIVERING", "DELIVERED", "RETURNING", "IDLE"];

// Drones que se muestran ahora mismo en el listado
let drones = [];

const droneList = document.getElementById("drone-list");
const medicamentList = document.getElementById("medicament-list");

// ---------------------------------------------------------------------
// UTILIDADES
// ---------------------------------------------------------------------

// Escapa el texto que viene del usuario antes de meterlo en el HTML (evita inyectar código)
function escapeHtml(texto) {
  const div = document.createElement("div");
  div.textContent = String(texto ?? "");
  return div.innerHTML;
}

const esperar = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

// ---------------------------------------------------------------------
// NAVEGACIÓN ENTRE SECCIONES
// ---------------------------------------------------------------------

// Se llama desde los botones del menú (onclick en index.html)
function showSection(section) {
  document.querySelectorAll(".page-section").forEach((sec) => sec.classList.add("hidden"));

  const activeSection = document.getElementById(section + "-section");
  activeSection.classList.remove("hidden");

  // Al entrar en una sección se vuelve al primer filtro ("Todos")
  activeSection.querySelectorAll(".filter-buttons").forEach((grupo) => {
    const botones = grupo.querySelectorAll(".filter-btn");
    botones.forEach((b) => b.classList.remove("active"));
    botones[0]?.classList.add("active");
  });

  if (section === "list") cargarTodos();
  if (section === "medicaments") cargarMedicamentos("all");
}

// Marca como activo el botón pulsado dentro de su grupo de filtros
function activarFiltro(btn) {
  btn
    .closest(".filter-buttons")
    .querySelectorAll(".filter-btn")
    .forEach((b) => b.classList.remove("active"));
  btn.classList.add("active");
}

// ---------------------------------------------------------------------
// REGISTRAR DRON
// ---------------------------------------------------------------------

document.getElementById("register-btn").addEventListener("click", async () => {
  const serial = document.getElementById("serial").value.trim();
  const model = document.getElementById("model").value;

  if (!serial) {
    alert("⚠️ El número de serie es obligatorio");
    return;
  }

  try {
    // El límite de peso, la batería y el estado inicial los pone la API según el modelo
    await Api.registrarDron(serial, model);
    alert("✅ Dron registrado con éxito");
    document.getElementById("serial").value = "";
    document.getElementById("model").selectedIndex = 0;
  } catch (error) {
    alert("❌ " + error.message);
  }
});

// ---------------------------------------------------------------------
// REGISTRAR MEDICAMENTO
// ---------------------------------------------------------------------

// El peso solo admite números enteros entre 1 y 1000
document.getElementById("med-weight").addEventListener("input", function () {
  this.value = this.value.replace(/[^0-9]/g, "");
  if (this.value === "") return;

  const valor = parseInt(this.value, 10);
  if (valor < 1) this.value = 1;
  if (valor > 1000) this.value = 1000;
});

document.getElementById("register-medicament-btn").addEventListener("click", async () => {
  const codigo = document.getElementById("med-code").value.trim();
  const nombre = document.getElementById("med-name").value.trim();
  const peso = parseInt(document.getElementById("med-weight").value.trim(), 10);

  if (!codigo || !nombre) {
    alert("⚠️ Código y nombre son obligatorios");
    return;
  }

  if (isNaN(peso) || peso < 1 || peso > 1000) {
    alert("⚠️ El peso debe ser un número entre 1 y 1000 gramos");
    return;
  }

  try {
    await Api.registrarMedicamento(codigo, nombre, peso);
    alert("✅ Medicamento registrado con éxito");
    document.getElementById("med-code").value = "";
    document.getElementById("med-name").value = "";
    document.getElementById("med-weight").value = "";
  } catch (error) {
    alert("❌ " + error.message);
  }
});

// ---------------------------------------------------------------------
// LISTADO DE DRONES
// ---------------------------------------------------------------------

document.getElementById("filter-btn-all").addEventListener("click", (evento) => {
  activarFiltro(evento.currentTarget);
  cargarTodos();
});

document.getElementById("filter-btn-disponibles").addEventListener("click", (evento) => {
  activarFiltro(evento.currentTarget);
  cargarDisponibles();
});

async function cargarTodos() {
  try {
    drones = await Api.obtenerDrones();
    renderDrones();
  } catch (error) {
    console.error("Error al cargar drones:", error);
  }
}

async function cargarDisponibles() {
  try {
    drones = await Api.obtenerDronesDisponibles();
    renderDrones();
  } catch (error) {
    alert("❌ Error al cargar drones disponibles.");
  }
}

function renderDrones() {
  droneList.innerHTML = "";

  if (drones.length === 0) {
    droneList.innerHTML = "<p>No hay drones registrados.</p>";
    return;
  }

  drones.forEach((drone) => droneList.appendChild(crearTarjetaDron(drone)));
}

function crearTarjetaDron(drone) {
  const card = document.createElement("div");
  card.classList.add("drone-card");

  const puedeCargar = drone.battery > 25;
  const puedeRecargar = drone.battery < 100 && (drone.state === "IDLE" || drone.state === "LOADING");
  const puedeSalir = drone.state === "LOADING";

  card.innerHTML = `
    <img src="imagenes/dron.jpg" alt="Dron">
    <div class="drone-info">
      <p><strong>Número de serie:</strong> ${escapeHtml(drone.serialNumber)}</p>
      <p><strong>Modelo:</strong> ${escapeHtml(drone.model)}</p>
      <p><strong>Límite peso:</strong> ${drone.weightLimit} gr</p>
      <p><strong>Batería:</strong> ${drone.battery}%</p>
      <p><strong>Estado:</strong> ${escapeHtml(drone.state)}</p>

      <button class="load-btn" ${puedeCargar ? "" : "disabled"}>Cargar</button>
      <button class="show-medicaments">Mostrar medicamentos</button>
      <button class="low-battery" ${puedeRecargar ? "" : "disabled"}>Cargar batería</button>
      <button class="start" ${puedeSalir ? "" : "disabled"}>Comenzar</button>

      <div class="medicament-list hidden">
        <p><em>No hay medicamentos cargados.</em></p>
      </div>
    </div>
  `;

  card.querySelector(".load-btn").addEventListener("click", () => mostrarFormularioCarga(card, drone));
  card.querySelector(".show-medicaments").addEventListener("click", () => mostrarMedicamentosDeDron(card, drone.serialNumber));
  card.querySelector(".low-battery").addEventListener("click", () => recargarBateria(drone.serialNumber));
  card.querySelector(".start").addEventListener("click", async (evento) => {
    evento.currentTarget.disabled = true;
    await realizarEntrega(drone);
  });

  return card;
}

// ---------------------------------------------------------------------
// CARGAR MEDICAMENTOS EN UN DRON
// ---------------------------------------------------------------------

function mostrarFormularioCarga(card, drone) {
  // Evita que se abra dos veces
  if (card.querySelector("#load-form")) return;

  const form = document.createElement("div");
  form.id = "load-form";
  form.innerHTML = `
    <h4>Cargar Medicamentos</h4>
    <input type="text" placeholder="Código" class="med-code">
    <button class="add-med-btn">Añadir</button>
    <button class="cancel-btn">Cancelar</button>
  `;

  form.querySelector(".cancel-btn").addEventListener("click", () => form.remove());

  form.querySelector(".add-med-btn").addEventListener("click", async () => {
    const codigo = form.querySelector(".med-code").value.trim();

    if (!codigo) {
      alert("⚠️ Completa el campo de código");
      return;
    }

    form.remove();

    try {
      if (!(await Api.existeMedicamento(codigo))) {
        alert("❌ El medicamento no existe en la base de datos");
        return;
      }

      // La API comprueba el límite de peso, la batería y el estado del dron
      await Api.cargarMedicamento(drone.serialNumber, codigo);
      await Api.cambiarEstado(drone.serialNumber, "LOADING");
      alert(`✅ Medicamento añadido correctamente:\nCódigo: ${codigo}\nDron: ${drone.serialNumber}`);
      await cargarTodos();
    } catch (error) {
      alert("❌ " + error.message);
    }
  });

  card.appendChild(form);
}

async function mostrarMedicamentosDeDron(card, numeroSerie) {
  const lista = card.querySelector(".medicament-list");

  try {
    const meds = await Api.obtenerMedicamentosDeDron(numeroSerie);

    lista.innerHTML =
      meds.length === 0
        ? "<p><em>No hay medicamentos cargados.</em></p>"
        : meds
            .map(
              (m) => `
          <div class="med-item">
            <strong>${escapeHtml(m.codigo)}</strong> — ${escapeHtml(m.nombre)} — ${m.peso} g
          </div>`
            )
            .join("");
  } catch (error) {
    lista.innerHTML = "<p>Error al cargar medicamentos.</p>";
  }

  lista.classList.remove("hidden");
}

// ---------------------------------------------------------------------
// ENTREGAS Y BATERÍA
// ---------------------------------------------------------------------

// El dron pasa por todos los estados del ciclo (gastando un 1% de batería en cada uno)
// y al volver se vacía, porque sus medicamentos ya se han entregado
async function realizarEntrega(drone) {
  try {
    for (const estado of CICLO_ENTREGA) {
      await Api.cambiarEstado(drone.serialNumber, estado);
      await Api.actualizarBateria(drone.serialNumber, "reducir");
      await cargarTodos();
      await esperar(MS_POR_ESTADO_ENTREGA);
    }

    await Api.vaciarDron(drone.serialNumber);
    await cargarTodos();
  } catch (error) {
    alert("❌ Error durante la entrega: " + error.message);
  }
}

async function recargarBateria(numeroSerie) {
  try {
    await Api.actualizarBateria(numeroSerie, "cargar");
    await cargarTodos();
  } catch (error) {
    alert("❌ " + error.message);
  }
}

// Todos los drones pierden un 1% de batería cada minuto mientras la web está abierta
function iniciarDescargaDeBateria() {
  setInterval(async () => {
    for (const dron of drones) {
      try {
        dron.battery = await Api.actualizarBateria(dron.serialNumber, "reducir");
      } catch (error) {
        console.error("Error actualizando batería:", error);
      }
    }
    renderDrones();
  }, MS_DESCARGA_BATERIA);
}

// ---------------------------------------------------------------------
// LISTADO DE MEDICAMENTOS
// ---------------------------------------------------------------------

document.querySelectorAll("#medicaments-section .filter-btn").forEach((btn) => {
  btn.addEventListener("click", () => {
    activarFiltro(btn);
    cargarMedicamentos(btn.dataset.filter);
  });
});

async function cargarMedicamentos(estado = "all") {
  try {
    renderMedicamentos(await Api.obtenerMedicamentos(estado));
  } catch (error) {
    medicamentList.innerHTML = "<p>❌ Error al cargar los medicamentos.</p>";
  }
}

function renderMedicamentos(medicamentos) {
  medicamentList.innerHTML = "";

  if (medicamentos.length === 0) {
    medicamentList.innerHTML = "<p><em>No hay medicamentos registrados.</em></p>";
    return;
  }

  medicamentos.forEach((med) => {
    const card = document.createElement("div");
    card.classList.add("medicament-card");
    card.innerHTML = `
      <img src="imagenes/medicamento.png" alt="Medicamento">
      <div class="medicament-info">
        <p><strong>Código:</strong> ${escapeHtml(med.codigo)}</p>
        <p><strong>Nombre:</strong> ${escapeHtml(med.nombre)}</p>
        <p><strong>Peso:</strong> ${med.peso} g</p>
      </div>
    `;
    medicamentList.appendChild(card);
  });
}

// ---------------------------------------------------------------------
// INICIO
// ---------------------------------------------------------------------

cargarTodos();
iniciarDescargaDeBateria();
