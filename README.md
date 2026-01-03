# 🚁 Drones y Medicamentos – API REST (ASP.NET)

Proyecto completo **backend + frontend** para la gestión de drones y medicamentos, desarrollado en **ASP.NET 8**, con base de datos **SQLite autocreable** y frontend web en **HTML, CSS y JavaScript**.

El proyecto está preparado para que **cualquier usuario pueda clonarlo y ejecutarlo sin configuración adicional**.

---

## 📁 Estructura del proyecto

Drones-y-Medicamentos-ASP.NET/
├── backend/ → API REST en ASP.NET 8
└── frontend/ → Frontend web (HTML / CSS / JS)


> ⚠️ La carpeta `database/` **NO se incluye en el repositorio**.  
> Se crea automáticamente al ejecutar el backend.

---

## 🛠️ Tecnologías utilizadas

- ASP.NET 8
- C#
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
Requisitos

Tener instalado .NET SDK 8

Puedes comprobarlo con:

```bash
dotnet --version
```

Ejecutar la API

Desde la raíz del proyecto:

```bash
cd backend/API_REST_Drones_y_Medicamentos
dotnet restore
dotnet run
```

✅ Al arrancar:

 - La API se inicia
 - Se crea automáticamente la base de datos SQLite si no existe
 - Se crean las tablas necesarias

La API expone Swagger automáticamente.

Accede desde el navegador a:

```bash
https://localhost:xxxx/swagger
```
