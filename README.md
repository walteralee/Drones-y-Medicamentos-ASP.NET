<h1 align="center">🚁 Drones y Medicamentos</h1>

<p align="center">
  <img src="imagenes/badges/dotnet.svg" alt=".NET 8" />
  <img src="imagenes/badges/csharp.svg" alt="C# API REST" />
  <img src="imagenes/badges/sqlite.svg" alt="SQLite" />
  <img src="imagenes/badges/docker.svg" alt="Docker" />
  <img src="imagenes/badges/tests.svg" alt="14 tests" />
</p>

**Drones y Medicamentos** es una aplicación web para gestionar una flota de drones que reparten medicamentos. Puedes registrar drones y medicamentos, cargar cada dron sin pasarte de su límite de peso, controlar su batería y lanzar la entrega: el dron pasa por todos los estados (cargado, repartiendo, entregado, volviendo) y al regresar queda vacío y listo para otro viaje. Por dentro es una **API REST en ASP.NET 8 (C#)** con base de datos **SQLite** y una web en **HTML, CSS y JavaScript**.

<p align="center">
  <img src="imagenes/capturas/listado_drones.png" width="800" alt="Listado de drones" />
</p>

---

## Instalación

### Windows: sin instalar nada

<p align="center">
  <a href="https://github.com/walteralee/Drones-y-Medicamentos-ASP.NET/releases/latest/download/DronesYMedicamentos-Windows.zip">
    <img src="imagenes/badges/descargar_windows.svg" alt="Descargar para Windows" height="50" />
  </a>
</p>

1. **Descarga** el ZIP con el botón de arriba.
2. **Descomprímelo** (clic derecho > *Extraer todo*).
3. **Haz doble clic** en `DronesYMedicamentos.exe`. Se abre el navegador con la aplicación.

> No necesitas instalar .NET ni nada más: todo va dentro. Para cerrarla, cierra la ventana negra.
> Si Windows muestra *"Windows protegió su PC"*, pulsa **Más información > Ejecutar de todas formas** (sale porque el programa no está firmado).
> Para desinstalarlo, borra la carpeta.

### Mac y Linux (o Windows con Docker)

Con [Docker Desktop](https://www.docker.com/products/docker-desktop/) abierto, ejecuta en una terminal:

```bash
git clone https://github.com/walteralee/Drones-y-Medicamentos-ASP.NET.git
```

```bash
cd Drones-y-Medicamentos-ASP.NET
```

```bash
docker compose up -d
```

Abre **http://localhost:5212** en el navegador. Para apagarla: `docker compose down`.

> Los datos se guardan en un volumen de Docker y se mantienen aunque la apagues.

---

## Funcionalidades

- **Registrar drones** de 4 modelos, cada uno con su límite de peso: Ligero (250 g), Medio (500 g), Crucero (750 g) y Pesado (1000 g).
- **Registrar medicamentos** con código, nombre y peso (de 1 a 1000 g).
- **Cargar medicamentos en un dron.** La aplicación no te deja:
  - pasarte del límite de peso del dron,
  - cargar un dron con un 25 % de batería o menos,
  - cargar un dron que está repartiendo,
  - poner el mismo medicamento en dos drones.
- **Entregas:** al pulsar *Comenzar*, el dron pasa por `LOADED → DELIVERING → DELIVERED → RETURNING → IDLE`, gasta batería en cada paso y al volver se vacía.
- **Batería:** baja con el uso y con el tiempo; se recarga al 100 % con un botón.
- **Filtros:** drones disponibles (en IDLE y con más del 25 % de batería) y medicamentos en uso o libres.
- **Swagger** en `/swagger` para probar la API desde el navegador.

<p align="center">
  <img src="imagenes/capturas/registrar_dron.png" width="400" alt="Registrar dron" />
  &nbsp;
  <img src="imagenes/capturas/medicamentos.png" width="400" alt="Listado de medicamentos" />
</p>

## Tecnologías

| | |
|---|---|
| Backend | C# · ASP.NET 8 (API REST con controladores) |
| Base de datos | SQLite (se crea sola al arrancar) con Microsoft.Data.Sqlite |
| Frontend | HTML, CSS, JavaScript y Bootstrap 5 |
| Documentación de la API | Swagger (Swashbuckle) |
| Tests | xUnit + WebApplicationFactory (14 tests de integración) |
| Despliegue | Ejecutable de Windows autocontenido y Docker |

## API

Todas las rutas empiezan por `/api/drones`. Puedes probarlas en **http://localhost:5212/swagger**.

| Método | Ruta | Qué hace |
|---|---|---|
| POST | `/registrar` | Registra un dron (`serialNumber`, `model`) |
| GET | `/todos` | Lista todos los drones |
| GET | `/disponibles` | Drones en IDLE con más del 25 % de batería |
| POST | `/actualizar_estado_dron?numero_serie=&estado=` | Cambia el estado de un dron |
| POST | `/actualizar_bateria_dron?numero_serie=&proceso=cargar\|reducir` | Recarga al 100 % o baja un 1 % |
| GET | `/obtener_nivel_bateria_dron/{n_serie}` | Batería de un dron |
| POST | `/registrar_medicamento` | Registra un medicamento (`codigo`, `nombre`, `peso`) |
| GET | `/medicamentos?estado=all\|in-use\|available` | Lista medicamentos con filtro |
| GET | `/confirmar_existencia_de_medicamento/{codigo}` | `true` o `false` |
| GET | `/obtener_peso_medicamento/{codigo}` | Peso de un medicamento |
| POST | `/insertar-medicamento-dron` | Carga un medicamento en un dron (`numero_serie`, `codigo`) |
| GET | `/mostrar_medicamentos_de_dron/{numero_serie}` | Medicamentos que lleva un dron |
| DELETE | `/eliminar_relaciones_y_medicamentos/{numero_serie}` | Vacía el dron al terminar la entrega |

<p align="center">
  <img src="imagenes/capturas/swagger.png" width="800" alt="Swagger de la API" />
</p>

## Ejecutar desde el código

Solo si quieres ver o modificar el código. Necesitas el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) (o superior).

```bash
cd backend/API_REST_Drones_y_Medicamentos
```

```bash
dotnet run
```

Se abre el navegador en **http://localhost:5212** con la web y la API juntas. También puedes abrir `backend/API_REST_Drones_y_Medicamentos.sln` con Visual Studio y pulsar ▶.

**Tests** (desde la carpeta `backend`):

```bash
dotnet test
```

**Generar el ZIP de Windows** (desde la raíz, queda en `dist/`):

```bash
powershell -ExecutionPolicy Bypass -File scripts/publicar-windows.ps1
```

## Estructura

```text
backend/
├── API_REST_Drones_y_Medicamentos/
│   ├── Controllers/DronesController.cs   endpoints y reglas (peso, batería, estado)
│   ├── Data/DatabaseInitializer.cs       crea la base de datos y las tablas
│   ├── Model/                            Drone, Medicamento y RelacionDM
│   └── Program.cs                        arranque: API, web, Swagger y navegador
└── API_REST_Drones_y_Medicamentos.Tests/ tests de integración de la API
frontend/                                 web: index.html, styles.css, script.js
scripts/publicar-windows.ps1              genera el ZIP de Windows
Dockerfile · docker-compose.yml           versión con Docker
```

La base de datos se crea sola al arrancar en `database/drones_y_medicamentos.db` y no se sube al repositorio.

## Autor

**Walter Alejandro Cutiño Ledo** · [github.com/walteralee](https://github.com/walteralee)

---

## English

**Drones y Medicamentos** is a web app to manage a fleet of delivery drones that carry medicines: register drones and medicines, load each drone without exceeding its weight limit, track its battery and run the delivery cycle (`LOADED → DELIVERING → DELIVERED → RETURNING → IDLE`). Built with an **ASP.NET 8 REST API (C#)**, **SQLite** and an **HTML/CSS/JavaScript** frontend, with 14 integration tests.

**Install:**

- **Windows (nothing to install):** download **[DronesYMedicamentos-Windows.zip](https://github.com/walteralee/Drones-y-Medicamentos-ASP.NET/releases/latest/download/DronesYMedicamentos-Windows.zip)**, unzip it and double-click `DronesYMedicamentos.exe`. The browser opens by itself.
- **Mac / Linux (Docker):** `git clone` the repo, then run `docker compose up -d` and open http://localhost:5212.

**From source:** `cd backend/API_REST_Drones_y_Medicamentos` and `dotnet run` (.NET 8 SDK). API docs at `/swagger`.
**Author:** Walter Alejandro Cutiño Ledo · [github.com/walteralee](https://github.com/walteralee)
