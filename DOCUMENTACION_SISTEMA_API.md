# Sistema de Reporte y Seguimiento de Incidentes de la Ciudad
## Documento Técnico: Arquitectura, Funcionamiento y Guía de Uso de la API REST (.NET 10)

---

## 1. ¿Qué es una API y por qué se utiliza en este proyecto?

### 1.1. Concepto Fundamental
Una **API** (*Application Programming Interface* o Interfaz de Programación de Aplicaciones) es un conjunto de reglas, protocolos y especificaciones que permiten que dos o más sistemas de software se comuniquen e intercambien datos entre sí de manera estructurada y segura, sin necesidad de conocer los detalles internos de implementación de la otra parte.

En la web moderna, la arquitectura dominante es **REST** (*Representational State Transfer*), la cual se basa en:
- **Recursos identificables por URIs** (por ejemplo `/api/equipment`).
- **Verbos HTTP universales** para indicar la acción a realizar:
  - `GET`: Obtener o consultar información sin alterar el estado.
  - `POST`: Crear un nuevo recurso o disparar una operación que cambia el estado.
  - `PUT`: Modificar o actualizar un recurso existente de forma idempotente.
  - `DELETE`: Eliminar o dar de baja un recurso.
- **Formato estándar JSON** (*JavaScript Object Notation*): Formato liviano, legible tanto por humanos como por computadoras, que sirve como lenguaje común de intercambio de datos.
- **Códigos de estado HTTP**: Señales numéricas estandarizadas (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`, `404 NotFound`, `409 Conflict`, `403 Forbidden`) que comunican el resultado exacto de la operación.

### 1.2. ¿Por qué usamos una API en este caso de negocio?
El **Sistema de Reporte y Seguimiento de Incidentes de la Ciudad** involucra múltiples actores: ciudadanos que reportan desde la calle (luminarias rotas, baches, poda), administradores en oficinas centrales que priorizan recursos, y cuadrillas de empleados que realizan reparaciones en la vía pública.

Utilizar una API REST backend desacoplada brinda ventajas indispensables:
1. **Desacoplamiento Front/Back**: La lógica de negocio (validaciones de disponibilidad de camiones, reglas de asignación y auditoría) reside en el servidor. Si mañana se desea crear una aplicación móvil para las cuadrillas o un portal público de reclamos, ambos consumirán exactamente la misma API sin duplicar una sola línea de lógica.
2. **Seguridad y Control Canónico**: La base de datos nunca se expone directamente a los clientes. La API actúa como una barrera de control que valida permisos, verifica roles y garantiza que las reglas de negocio nunca sean violadas.
3. **Trazabilidad y Auditoría Garantizada**: Al centralizar las operaciones en la API, ningún cambio de estado o asignación de recurso puede ocurrir sin que quede registrado automáticamente quién lo hizo, en qué fecha y a qué hora.

---

## 2. Mapa de Componentes: Qué hace cada cosa y para qué

El proyecto sigue una **arquitectura en capas limpias (Clean/Layered Architecture)**, asegurando separación de responsabilidades y mantenibilidad.

```
Obligatorio.sln
│
├── Obligatorio.Api/                     # Servidor Backend (Web API .NET 10)
│   ├── Models/                           # Entidades de dominio y estados
│   │   ├── Entities/ (Equipo, Reporte, AuditoriaEquipo)
│   │   └── Enums/ (TipoEquipo, EstadoEquipo, EstadoReporte)
│   ├── Data/                             # Persistencia (EF Core + SQLite)
│   │   ├── AppDbContext.cs
│   │   └── Configurations/ (Fluent API por entidad)
│   ├── DTOs/                             # Contratos JSON de entrada y salida
│   ├── Interfaces/                       # Contratos de abstracción (IEquipoRepository, IEquipoService)
│   ├── Repositories/                     # Acceso a base de datos (EquipoRepository)
│   ├── Services/                         # Reglas de negocio (EquipoService)
│   ├── Controllers/                      # Endpoints HTTP (EquipmentController)
│   ├── Auth/                             # Autenticación y roles (DevAuthenticationHandler)
│   └── Program.cs                        # Configuración de servicios, Swagger, CORS y DI
│
└── Obligatorio.Web/                     # Cliente Web Frontend (Blazor Web App)
    ├── Services/                         # Cliente HTTP tipado (EquipmentApiService)
    ├── Models/                           # DTOs y modelos locales de vista
    └── Components/Pages/                 # Vistas interactivas Razor
        ├── ListadoEquipos.razor          # Grilla con filtros, búsqueda y baja lógica
        ├── CrearEquipo.razor             # Formulario de alta con validación
        ├── EditarEquipo.razor            # Edición de recursos
        └── DetalleEquipo.razor           # Ficha, asignación/liberación y auditoría
```

### Detalle por Componente:

| Componente | Archivo principal | ¿Qué hace? | ¿Para qué se utiliza? |
| :--- | :--- | :--- | :--- |
| **Entidad `Equipo`** | `Models/Entities/Equipo.cs` | Modela los vehículos, herramientas y materiales de la ciudad. | Guarda el estado real (`Disponible`, `EnUso`, `EnMantenimiento`), pertenencia a reporte y baja lógica (`Activo = false`). |
| **Entidad `AuditoriaEquipo`** | `Models/Entities/AuditoriaEquipo.cs` | Estructura el historial de trazabilidad. | Cumple el requisito estricto de auditar cada cambio con usuario, timestamp, acción y estado previo/nuevo. |
| **Entidad `Reporte`** | `Models/Entities/Reporte.cs` | Modela el ciclo de vida del incidente urbano. | Permite vincular qué recurso está atendiendo qué reporte y evitar asignaciones a incidentes ya cerrados. |
| **`AppDbContext`** | `Data/AppDbContext.cs` | Contexto de Entity Framework Core 8. | Mapea las entidades a tablas, aplica índices, relaciones y carga datos semilla (*seed data*) iniciales para pruebas. |
| **DTOs** | `DTOs/*.cs` | Objetos de transferencia de datos con DataAnnotations. | Evita sobreexposición de la base de datos (over-posting), define el contrato JSON y valida entradas (`[Required]`, `[MaxLength]`). |
| **`EquipoRepository`** | `Repositories/EquipoRepository.cs` | Implementa `IEquipoRepository`. | Aísla las consultas SQL/EF Core (`AsNoTracking`, filtrado y ordenamiento dinámico), desacoplando la persistencia de la lógica. |
| **`EquipoService`** | `Services/EquipoService.cs` | Implementa `IEquipoService`. | **Corazón del negocio:** valida que un equipo en uso o mantenimiento no se reasigne, gestiona la liberación y genera los registros de auditoría. |
| **`EquipmentController`** | `Controllers/EquipmentController.cs` | Controlador REST `[ApiController]`. | Expone los endpoints HTTP, traduce excepciones a `ProblemDetails` y restringe el acceso al rol `Administrador`. |
| **`DevAuthenticationHandler`** | `Auth/DevAuthenticationHandler.cs` | Manejador de autenticación por encabezado/claims. | Permite evaluar la API de forma ágil desde Swagger o Blazor mediante encabezados `X-User-Role` y `X-User-Id`. |
| **`EquipmentApiService`** | `Obligatorio.Web/Services/` | Servicio HTTP cliente tipado. | Encapsula las llamadas HTTP del frontend Blazor (`GetFromJsonAsync`, `PostAsJsonAsync`) y parsea mensajes de error de la API. |

---

## 3. Cómo Ejecutar y Acceder al Sistema

El sistema utiliza **SQLite embebido** (`city_incidents.db`), por lo que **no se requiere instalar ni configurar SQL Server externamente**. Al arrancar, la base de datos se genera automáticamente con datos de prueba listos para usar.

### Paso 1: Iniciar la API REST (Backend)
Abrir una terminal en la raíz del proyecto y ejecutar:
```powershell
cd Obligatorio.Api
dotnet run --launch-profile http
```
- **URL Base:** `http://localhost:5000`
- **Documentación Swagger UI interactiva:** `http://localhost:5000/` (o `http://localhost:5000/swagger`)

### Paso 2: Iniciar el Cliente Web (Frontend Blazor)
Abrir una segunda terminal y ejecutar:
```powershell
cd Obligatorio.Web
dotnet run --launch-profile http
```
- **URL del Cliente:** `http://localhost:5200`
- La interfaz abrirá el catálogo de recursos con filtros, creación, edición, asignación a incidentes y auditoría en tiempo real.

---

## 4. Catálogo de Endpoints REST (Cómo usar la API)

Todos los endpoints se encuentran bajo el prefijo `/api/equipment` y responden en formato `application/json`.

### 4.1. Listar Equipos con Filtros
- **Método y Ruta:** `GET /api/equipment`
- **Parámetros Opcionales de Consulta (Query Params):**
  - `type`: Filtrar por tipo (`Vehiculo`, `Herramienta`, `Material`).
  - `status`: Filtrar por estado (`Disponible`, `EnUso`, `EnMantenimiento`).
  - `search`: Búsqueda parcial por nombre.
  - `sortBy`: Criterio de orden (`nombre`, `nombre_desc`, `fecha`, `fecha_desc`).
  - `onlyActive`: `true` (por defecto) para omitir bajas lógicas.
- **Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Camioneta Utilitaria 01",
    "type": "Vehiculo",
    "status": "EnUso",
    "isActive": true,
    "currentReportId": 1,
    "currentReportTitle": "Reparación de luminaria en Av. 18 de Julio",
    "createdAt": "2026-01-15T10:00:00Z"
  }
]
```

### 4.2. Obtener Detalle de un Recurso
- **Método y Ruta:** `GET /api/equipment/{id}`
- **Respuesta:**
  - `200 OK`: Devuelve el objeto `EquipoDto`.
  - `404 Not Found`: Si no existe o está inactivo.

### 4.3. Historial de Auditoría de un Recurso
- **Método y Ruta:** `GET /api/equipment/{id}/audit`
- **Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "equipmentId": 1,
    "action": "Asignado",
    "previousStatus": "Disponible",
    "newStatus": "EnUso",
    "reportId": 1,
    "userId": "admin@ciudad.gob",
    "timestamp": "2026-01-15T10:00:00Z",
    "details": "Asignación inicial para reparación de luminaria"
  }
]
```

### 4.4. Crear un Nuevo Recurso (Solo Administrador)
- **Método y Ruta:** `POST /api/equipment`
- **Encabezados:** `Content-Type: application/json`, `X-User-Role: Administrador`
- **Cuerpo (JSON):**
```json
{
  "name": "Hidroelevador Articulado 12m",
  "type": "Vehiculo",
  "status": "Disponible"
}
```
- **Respuesta:** `201 Created` con encabezado `Location: /api/equipment/8` y el recurso creado.

### 4.5. Modificar un Recurso (Solo Administrador)
- **Método y Ruta:** `PUT /api/equipment/{id}`
- **Cuerpo (JSON):**
```json
{
  "name": "Hidroelevador Articulado 12m - Actualizado",
  "type": "Vehiculo",
  "status": "Disponible"
}
```
- **Respuesta:** `200 OK` (o `404 Not Found` / `400 BadRequest`).

### 4.6. Asignar un Recurso a un Reporte (Solo Administrador)
- **Método y Ruta:** `POST /api/equipment/{id}/assign`
- **Cuerpo (JSON):**
```json
{
  "reportId": 2
}
```
- **Respuestas Posibles:**
  - `200 OK`: Recurso asignado satisfactoriamente (pasa a estado `EnUso`).
  - `409 Conflict`: Si el recurso ya estaba `EnUso` o `EnMantenimiento`, o si el reporte indicado ya fue solucionado o cerrado.
  - `404 Not Found`: Si el recurso o el reporte no existen.

### 4.7. Liberar un Recurso Asignado (Solo Administrador)
- **Método y Ruta:** `POST /api/equipment/{id}/release`
- **Respuesta (200 OK):** El recurso vuelve a `Disponible` y `currentReportId` se limpia a `null`.

### 4.8. Baja Lógica del Recurso (Solo Administrador)
- **Método y Ruta:** `DELETE /api/equipment/{id}`
- **Respuesta:** `204 No Content` (el campo `isActive` se establece en `false`, conservando la auditoría y sin borrado físico).

---

## 5. Guía de Pruebas Manuales y Verificación de Reglas de Negocio

Estas pruebas pueden ejecutarse tanto desde la interfaz gráfica de **Swagger UI** (`http://localhost:5000/`) como desde la aplicación **Blazor** (`http://localhost:5200/`).

### Caso 1: Comprobar Filtrado y Ordenamiento
1. En Swagger, invocar `GET /api/equipment?type=Herramienta&status=Disponible`.
2. **Resultado esperado (200 OK):** Se lista únicamente el *"Generador Eléctrico 5kW"* (Id: 5).

---

### Caso 2: Probar Regla de Negocio - Asignar equipo que ya está "EnUso"
1. El equipo `Id = 1` (*"Camioneta Utilitaria 01"*) ya se encuentra `EnUso` (asignado al reporte 1).
2. Intentar ejecutar:
   ```http
   POST /api/equipment/1/assign
   Content-Type: application/json
   
   { "reportId": 2 }
   ```
3. **Resultado esperado (409 Conflict):**
   ```json
   {
     "status": 409,
     "title": "Regla de asignación infringida",
     "detail": "El equipo 'Camioneta Utilitaria 01' ya se encuentra EnUso (asignado al reporte #1) y no puede asignarse a otro reporte."
   }
   ```
   > **Validación:** Se cumple la regla de negocio de no reasignación concurrente.

---

### Caso 3: Probar Regla de Negocio - Asignar equipo "EnMantenimiento"
1. El equipo `Id = 3` (*"Martillo Neumático Industrial"*) se encuentra `EnMantenimiento`.
2. Intentar ejecutar:
   ```http
   POST /api/equipment/3/assign
   Content-Type: application/json
   
   { "reportId": 2 }
   ```
3. **Resultado esperado (409 Conflict):**
   ```json
   {
     "status": 409,
     "title": "Regla de asignación infringida",
     "detail": "El equipo 'Martillo Neumático Industrial' se encuentra EnMantenimiento y no puede ser asignado."
   }
   ```
   > **Validación:** Se previene la asignación de recursos averiados o en reparación.

---

### Caso 4: Asignación Válida y Cambio de Estado
1. El equipo `Id = 2` (*"Camión Grúa Hidráulica 02"*) se encuentra `Disponible`.
2. Ejecutar:
   ```http
   POST /api/equipment/2/assign
   Content-Type: application/json
   
   { "reportId": 2 }
   ```
3. **Resultado esperado (200 OK):**
   - El equipo responde con `status = "EnUso"` y `currentReportId = 2`.
   - Se crea una entrada en auditoría (`GET /api/equipment/2/audit`) con `action: "Asignado"`, `previousStatus: "Disponible"`, `newStatus: "EnUso"`, `reportId: 2` y fecha/hora actual.

---

### Caso 5: Liberación de Recurso
1. Ejecutar:
   ```http
   POST /api/equipment/2/release
   ```
2. **Resultado esperado (200 OK):**
   - El equipo vuelve a `status = "Disponible"` y `currentReportId = null`.
   - En auditoría queda registrado el evento `action: "Liberado"`.

---

### Caso 6: Verificación de Baja Lógica (Sin Borrado Físico)
1. Ejecutar `DELETE /api/equipment/6`.
2. **Resultado esperado:**
   - Respuesta `204 No Content`.
   - Al consultar `GET /api/equipment` (con `onlyActive=true`), el equipo 6 ya no figura en la lista activa.
   - En la base de datos el registro permanece con `Activo = 0` y la auditoría refleja el evento `"BajaLogica"`.

---

### Caso 7: Control de Autorización por Rol
1. En Swagger o en un cliente HTTP (Postman/curl), intentar crear un equipo enviando el encabezado:
   `X-User-Role: Empleado` o `X-User-Role: Ciudadano`
2. **Resultado esperado (403 Forbidden):** La API rechaza la solicitud porque solo el rol `Administrador` posee permisos para crear, modificar, dar de baja o asignar recursos.

