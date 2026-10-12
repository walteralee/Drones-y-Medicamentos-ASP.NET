using System.Net;
using System.Net.Http.Json;
using API_REST_Drones_y_Medicamentos.Model;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API_REST_Drones_y_Medicamentos.Tests;

// Tests de integración: arrancan la API de verdad con una base de datos SQLite temporal y vacía
public class DronesApiTests : IDisposable
{
    private const string Api = "/api/drones";

    private readonly string _rutaBd;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DronesApiTests()
    {
        _rutaBd = Path.Combine(Path.GetTempPath(), $"drones_test_{Guid.NewGuid():N}.db");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Sqlite", $"Data Source={_rutaBd};Pooling=False");
            builder.UseSetting("AbrirNavegador", "false");
        });

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        File.Delete(_rutaBd);
    }

    //--------------------------------------------------------------------------------------------
    // DRONES

    [Fact]
    public async Task Registrar_dron_le_pone_el_limite_de_su_modelo_bateria_100_e_IDLE()
    {
        await RegistrarDron("DR-1", "Crucero");

        var drones = await _client.GetFromJsonAsync<List<Drone>>($"{Api}/todos");

        var dron = Assert.Single(drones!);
        Assert.Equal("DR-1", dron.SerialNumber);
        Assert.Equal(750, dron.WeightLimit);
        Assert.Equal(100, dron.Battery);
        Assert.Equal("IDLE", dron.State);
    }

    [Fact]
    public async Task No_se_puede_registrar_dos_drones_con_el_mismo_numero_de_serie()
    {
        await RegistrarDron("DR-1", "Ligero");

        var respuesta = await _client.PostAsJsonAsync($"{Api}/registrar", new { serialNumber = "DR-1", model = "Medio" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_se_puede_registrar_un_dron_con_modelo_no_valido()
    {
        var respuesta = await _client.PostAsJsonAsync($"{Api}/registrar", new { serialNumber = "DR-1", model = "Gigante" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Disponibles_solo_devuelve_drones_en_IDLE_con_mas_del_25_por_ciento()
    {
        await RegistrarDron("LIBRE", "Ligero");
        await RegistrarDron("OCUPADO", "Ligero");
        await _client.PostAsync($"{Api}/actualizar_estado_dron?numero_serie=OCUPADO&estado=DELIVERING", null);

        var disponibles = await _client.GetFromJsonAsync<List<Drone>>($"{Api}/disponibles");

        var dron = Assert.Single(disponibles!);
        Assert.Equal("LIBRE", dron.SerialNumber);
    }

    [Fact]
    public async Task La_bateria_baja_de_1_en_1_y_al_cargar_vuelve_a_100()
    {
        await RegistrarDron("DR-1", "Ligero");

        await _client.PostAsync($"{Api}/actualizar_bateria_dron?numero_serie=DR-1&proceso=reducir", null);
        await _client.PostAsync($"{Api}/actualizar_bateria_dron?numero_serie=DR-1&proceso=reducir", null);
        Assert.Equal(98, await Bateria("DR-1"));

        await _client.PostAsync($"{Api}/actualizar_bateria_dron?numero_serie=DR-1&proceso=cargar", null);
        Assert.Equal(100, await Bateria("DR-1"));
    }

    [Fact]
    public async Task No_se_acepta_un_estado_de_dron_inventado()
    {
        await RegistrarDron("DR-1", "Ligero");

        var respuesta = await _client.PostAsync($"{Api}/actualizar_estado_dron?numero_serie=DR-1&estado=VOLANDO", null);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    //--------------------------------------------------------------------------------------------
    // MEDICAMENTOS

    [Fact]
    public async Task Registrar_medicamento_valida_los_campos_y_el_codigo_repetido()
    {
        await RegistrarMedicamento("MED-1", "Paracetamol", 100);

        var sinNombre = await _client.PostAsJsonAsync($"{Api}/registrar_medicamento", new { codigo = "MED-2", nombre = "", peso = 10 });
        var pesoCero = await _client.PostAsJsonAsync($"{Api}/registrar_medicamento", new { codigo = "MED-3", nombre = "X", peso = 0 });
        var repetido = await _client.PostAsJsonAsync($"{Api}/registrar_medicamento", new { codigo = "MED-1", nombre = "Otro", peso = 10 });

        Assert.Equal(HttpStatusCode.BadRequest, sinNombre.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, pesoCero.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, repetido.StatusCode);
    }

    [Fact]
    public async Task Filtra_medicamentos_en_uso_y_disponibles()
    {
        await RegistrarDron("DR-1", "Pesado");
        await RegistrarMedicamento("CARGADO", "Ibuprofeno", 100);
        await RegistrarMedicamento("LIBRE", "Amoxicilina", 100);
        await CargarEnDron("DR-1", "CARGADO");

        var enUso = await _client.GetFromJsonAsync<List<Medicamento>>($"{Api}/medicamentos?estado=in-use");
        var disponibles = await _client.GetFromJsonAsync<List<Medicamento>>($"{Api}/medicamentos?estado=available");
        var todos = await _client.GetFromJsonAsync<List<Medicamento>>($"{Api}/medicamentos");

        Assert.Equal("CARGADO", Assert.Single(enUso!).Codigo);
        Assert.Equal("LIBRE", Assert.Single(disponibles!).Codigo);
        Assert.Equal(2, todos!.Count);
    }

    //--------------------------------------------------------------------------------------------
    // CARGA DE MEDICAMENTOS EN DRONES

    [Fact]
    public async Task No_deja_cargar_mas_peso_del_limite_del_dron()
    {
        await RegistrarDron("DR-1", "Ligero"); // 250 g
        await RegistrarMedicamento("A", "A", 200);
        await RegistrarMedicamento("B", "B", 100);

        var primera = await CargarEnDron("DR-1", "A");
        var segunda = await CargarEnDron("DR-1", "B"); // 200 + 100 > 250

        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task Un_medicamento_no_puede_estar_en_dos_drones()
    {
        await RegistrarDron("DR-1", "Pesado");
        await RegistrarDron("DR-2", "Pesado");
        await RegistrarMedicamento("A", "A", 10);

        await CargarEnDron("DR-1", "A");
        var otraVez = await CargarEnDron("DR-2", "A");

        Assert.Equal(HttpStatusCode.BadRequest, otraVez.StatusCode);
    }

    [Fact]
    public async Task No_deja_cargar_un_dron_con_25_por_ciento_de_bateria_o_menos()
    {
        await RegistrarDron("DR-1", "Pesado");
        await RegistrarMedicamento("A", "A", 10);
        for (int i = 0; i < 75; i++)
        {
            await _client.PostAsync($"{Api}/actualizar_bateria_dron?numero_serie=DR-1&proceso=reducir", null);
        }

        var respuesta = await CargarEnDron("DR-1", "A");

        Assert.Equal(25, await Bateria("DR-1"));
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task No_deja_cargar_un_dron_que_esta_repartiendo()
    {
        await RegistrarDron("DR-1", "Pesado");
        await RegistrarMedicamento("A", "A", 10);
        await _client.PostAsync($"{Api}/actualizar_estado_dron?numero_serie=DR-1&estado=DELIVERING", null);

        var respuesta = await CargarEnDron("DR-1", "A");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Al_vaciar_el_dron_se_borran_sus_medicamentos_y_no_los_de_otros()
    {
        await RegistrarDron("DR-1", "Pesado");
        await RegistrarDron("DR-2", "Pesado");
        await RegistrarMedicamento("DE-DR1", "A", 10);
        await RegistrarMedicamento("DE-DR2", "B", 10);
        await CargarEnDron("DR-1", "DE-DR1");
        await CargarEnDron("DR-2", "DE-DR2");

        await _client.DeleteAsync($"{Api}/eliminar_relaciones_y_medicamentos/DR-1");

        var delDron1 = await _client.GetFromJsonAsync<List<Medicamento>>($"{Api}/mostrar_medicamentos_de_dron/DR-1");
        var todos = await _client.GetFromJsonAsync<List<Medicamento>>($"{Api}/medicamentos");

        Assert.Empty(delDron1!);
        Assert.Equal("DE-DR2", Assert.Single(todos!).Codigo);
    }

    //--------------------------------------------------------------------------------------------
    // WEB

    [Fact]
    public async Task La_API_sirve_la_web_y_swagger()
    {
        var web = await _client.GetAsync("/");
        var swagger = await _client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, web.StatusCode);
        Assert.Contains("Gestión de Drones", await web.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, swagger.StatusCode);
    }

    //--------------------------------------------------------------------------------------------
    // AYUDAS

    private async Task RegistrarDron(string serie, string modelo)
    {
        var respuesta = await _client.PostAsJsonAsync($"{Api}/registrar", new { serialNumber = serie, model = modelo });
        respuesta.EnsureSuccessStatusCode();
    }

    private async Task RegistrarMedicamento(string codigo, string nombre, double peso)
    {
        var respuesta = await _client.PostAsJsonAsync($"{Api}/registrar_medicamento", new { codigo, nombre, peso });
        respuesta.EnsureSuccessStatusCode();
    }

    private Task<HttpResponseMessage> CargarEnDron(string serie, string codigo)
    {
        return _client.PostAsJsonAsync($"{Api}/insertar-medicamento-dron", new { numero_serie = serie, codigo });
    }

    private async Task<int> Bateria(string serie)
    {
        var respuesta = await _client.GetFromJsonAsync<Dictionary<string, int>>($"{Api}/obtener_nivel_bateria_dron/{serie}");
        return respuesta!["bateria"];
    }
}
