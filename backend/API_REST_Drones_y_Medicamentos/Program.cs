using API_REST_Drones_y_Medicamentos.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();  
    });
});


var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("Sqlite");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string 'Sqlite' no encontrada");
}

DatabaseInitializer.Initialize(connectionString);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}   

app.UseHttpsRedirection();

app.UseCors("PermitirTodo");

app.UseAuthorization(); 

app.MapControllers();

app.Run();
