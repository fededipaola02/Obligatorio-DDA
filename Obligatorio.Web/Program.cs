using Obligatorio.Web.Components;
using Obligatorio.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Registro de componentes Razor interactivos del lado del servidor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Registro de HttpClient tipado para EquipmentApiService
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5000/";
builder.Services.AddHttpClient<ServicioEquipos>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("X-User-Role", "Administrador");
    client.DefaultRequestHeaders.Add("X-User-Id", "admin@ciudad.gob");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

