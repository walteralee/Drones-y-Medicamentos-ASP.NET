using System.Diagnostics;
using DronesYMedicamentos.Api.Datos;
using DronesYMedicamentos.Api.Errores;
using DronesYMedicamentos.Api.Repositorios;
using DronesYMedicamentos.Api.Servicios;
using Microsoft.Extensions.FileProviders;

// En la versión publicada (.exe o Docker) la web va en wwwroot junto al programa: entonces la carpeta
// de trabajo es la del programa, aunque se abra desde un acceso directo o desde otra carpeta
var esVersionPublicada = File.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html"));

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = esVersionPublicada ? AppContext.BaseDirectory : null
});

// Capas: Controladores -> Servicios -> Repositorios -> Datos (SQLite)
builder.Services.AddSingleton<ConexionSqlite>();
builder.Services.AddScoped<IDronRepositorio, DronRepositorio>();
builder.Services.AddScoped<IMedicamentoRepositorio, MedicamentoRepositorio>();
builder.Services.AddScoped<DronServicio>();
builder.Services.AddScoped<MedicamentoServicio>();

// Errores: los esperados (400, 404) los resuelve el filtro y los inesperados (500) el manejador global
builder.Services.AddControllers(opciones => opciones.Filters.Add<FiltroErroresDeNegocio>());
builder.Services.AddExceptionHandler<ManejadorErrores>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Permite llamar a la API desde otra dirección (por ejemplo, el frontend servido aparte en el puerto 5500)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Crea la base de datos y las tablas si no existen
InicializadorBaseDatos.Crear(app.Services.GetRequiredService<ConexionSqlite>());

app.UseExceptionHandler();

// Swagger siempre disponible en /swagger para probar la API
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("PermitirTodo");

// La web la sirve la propia API: en la versión publicada está en wwwroot y,
// al ejecutar desde el código, se usa la carpeta frontend de la raíz del repositorio
var carpetaWeb = esVersionPublicada
    ? Path.Combine(app.Environment.ContentRootPath, "wwwroot")
    : Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "frontend"));

if (Directory.Exists(carpetaWeb))
{
    var archivosWeb = new PhysicalFileProvider(carpetaWeb);
    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = archivosWeb });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = archivosWeb });
}

app.MapControllers();

// Al abrir el programa con doble clic se abre el navegador solo (no en Docker ni en los tests)
if (app.Configuration.GetValue<bool>("AbrirNavegador")
    && Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault() ?? "http://localhost:5212";
        Console.WriteLine();
        Console.WriteLine($"  Drones y Medicamentos funcionando en {url}");
        Console.WriteLine("  Cierra esta ventana para apagarlo.");
        Console.WriteLine();
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Si no se puede abrir el navegador, el usuario puede entrar a mano en la dirección de arriba
        }
    });
}

app.Run();

// Necesario para los tests de integración (WebApplicationFactory<Program>)
public partial class Program
{
}
