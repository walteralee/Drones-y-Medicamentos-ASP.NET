# 🚁 Drones y Medicamentos – API REST (ASP.NET)

Proyecto completo **backend + frontend** para la gestión de drones y medicamentos, desarrollado en **ASP.NET 8**, con base de datos **SQLite autocreable** y frontend web en **HTML, CSS y JavaScript**.

El proyecto está preparado para que **cualquier usuario pueda clonarlo y ejecutarlo sin configuración adicional**.

---

> ⚠️ La carpeta `database/` **NO se incluye en el repositorio**.  
> Se genera automáticamente al iniciar el backend.

---

## 🛠️ Requisitos

- .NET SDK 8.0+
- Python 3.x
- Git
- SQLite
- Swagger
- HTML, CSS, JavaScript

---

## 📥 1. Clonar el repositorio

Abre una terminal y ejecuta:

```bash
git clone https://github.com/walteralee/Drones-y-Medicamentos-ASP.NET.git
```
Entra en la carpeta del proyecto:

```bash
cd Drones-y-Medicamentos-ASP.NET
```

⚙️ 2. Ejecutar el backend (API REST)

Desde la raíz del proyecto:

```bash
cd backend/API_REST_Drones_y_Medicamentos
dotnet restore
dotnet run
```
Al iniciarse, la consola mostrará algo como:

```bash
Now listening on: http://localhost:5212
```

🌐 3. Ejecutar el frontend (OBLIGATORIO)

⚠️ NO abrir index.html con doble click
⚠️ NO usar file://

```bash
cd frontend
python -m http.server 5500
```

🌍 5. Abrir la aplicación

En el navegador:

```bash
http://localhost:5500
```
