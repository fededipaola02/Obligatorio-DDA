using System.Text.Json;
using System.Text.Json.Serialization;
using Obligatorio.Api.Auth;
using Obligatorio.Api.Data;
using Obligatorio.Api.Interfaces;
using Obligatorio.Api.Repositories;
using Obligatorio.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Base de Datos SQLite (con opción a SQL Server)
var proveedor = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=city_incidents.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (proveedor.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("SqlServerConnection") ?? cadenaConexion;
        options.UseSqlServer(sqlServerConnection);
    }
    else
    {
        options.UseSqlite(cadenaConexion);
    }
});

// 2. Inyección de Dependencias
builder.Services.AddScoped<IEquipoRepository, EquipoRepository>();
builder.Services.AddScoped<IEquipoService, EquipoService>();

// 3. Autenticación y Autorización por Rol (solo Administrador puede crear, modificar, dar de baja y asignar)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = DevAuthenticationHandler.SchemeName;
    options.DefaultChallengeScheme = DevAuthenticationHandler.SchemeName;
})
.AddScheme<AuthenticationSchemeOptions, DevAuthenticationHandler>(DevAuthenticationHandler.SchemeName, null);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiereAdministrador", policy => policy.RequireRole("Administrador"));
});

// 4. Controladores y Serialización JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums como texto ("Vehiculo", "Disponible", etc.)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 5. Configuración de CORS para el cliente Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirClienteBlazor", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. Swagger / OpenAPI en Español
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Equipos y Recursos - Sistema de Incidentes",
        Version = "v1",
        Description = "API REST en .NET 10 con CRUD completo de equipos y recursos de la ciudad, reglas de negocio y auditoría obligatoria."
    });

    c.AddSecurityDefinition("EncabezadoRol", new OpenApiSecurityScheme
    {
        Name = "X-Rol-Usuario",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Simulación de rol (ej: 'Administrador', 'Empleado'). Por defecto es 'Administrador'."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "EncabezadoRol"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 7. Inicialización automática de la base de datos y Seed Data
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        logger.LogInformation("Base de datos ({Proveedor}) inicializada con datos semilla.", proveedor);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error al inicializar la base de datos.");
    }
}

// 8. Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Equipos v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("PermitirClienteBlazor");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
