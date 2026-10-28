# SGECAR API — Autenticación, roles y control de acceso

API REST (ASP.NET Core .NET 8) para el login, la sesión del usuario, la gestión de roles y usuarios y el control de acceso por permisos.

- **URL base (desarrollo):** `http://localhost:5100` (perfil `http`) o `https://localhost:7045` (perfil `https`)
- **Swagger UI:** `/swagger` (solo en entorno Development)
- **Formato:** JSON en peticiones y respuestas
- **Cadena de conexión:** `ConnectionStrings:GestionEmpresarialConnection` en `appsettings.Development.json`

---

## 1. Cómo autenticarse

1. Llamar a `POST /api/auth/login` con usuario y contraseña.
2. Guardar el `token` de la respuesta.
3. Enviarlo en **todas** las demás peticiones con este encabezado:

   ```
   Authorization: Bearer <token>
   ```

4. Al salir, llamar a `POST /api/auth/logout` para invalidar el token.

**Detalles de la sesión**

- Las sesiones se guardan en memoria del servidor **mientras corre la API**. Si la API se reinicia, todos deben volver a iniciar sesión.
- Una sesión expira tras **30 minutos sin actividad**. Cada petición válida renueva ese tiempo.
- Si el administrador **elimina** a un usuario, lo **desactiva**, le **cambia el rol** o le **cambia la contraseña**, las sesiones abiertas de ese usuario se cierran de inmediato.
- Si el administrador **modifica los permisos de un rol**, o **renombra o elimina un permiso**, los usuarios con sesión abierta en los roles afectados reciben los permisos nuevos de inmediato, sin tener que volver a entrar.

En Swagger: ejecutar el login, copiar el `token`, pulsar **Authorize** y pegarlo (sin la palabra `Bearer`).

---

## 2. Permisos y roles

### Acciones (permisos)

| Acción           | Descripción                   |
|------------------|-------------------------------|
| `AGREGAR`        | Agregar registros             |
| `MODIFICAR`      | Modificar registros           |
| `ELIMINAR`       | Eliminar registros            |
| `CONSULTAR`      | Consultar registros           |
| `CREAR_USUARIOS` | Gestionar usuarios            |
| `CREAR_ROLES`    | Gestionar roles               |

Estos son los **permisos base** (`esSistema: true`). Están como constantes en `SGECAR.Shared.Security.Acciones` (por ejemplo `Acciones.Eliminar`). Como el control de acceso depende de ellos, no se pueden eliminar ni renombrar; solo se puede cambiar su descripción.

Se pueden crear **permisos adicionales** con `POST /api/permisos` (por ejemplo `EXPORTAR_REPORTES`). Un permiso nuevo:

- Se asigna automáticamente al rol **Administrador**.
- Se asigna a otros roles con `PUT /api/roles/{id}`, incluyendo su id en `permisoIds`.
- Se exige en un endpoint con `[RequierePermiso("EXPORTAR_REPORTES")]`.

### Cómo se relacionan usuarios, roles y permisos

```
Usuario ──(rolId)──> Rol ──(RolPermiso)──> Permisos
maria                Auditor               CONSULTAR, AGREGAR
```

Los permisos **no se asignan directamente a un usuario**. Cada usuario tiene un solo rol (`rolId` en `POST /api/usuarios`), y el rol tiene sus permisos (`permisoIds` en `POST /api/roles`).

### Matriz de permisos por rol

| Acción           | Administrador | Supervisor | Ejecutor |
|------------------|:-------------:|:----------:|:--------:|
| `CONSULTAR`      | ✔ | ✔ | ✔ |
| `AGREGAR`        | ✔ | ✘ | ✔ |
| `MODIFICAR`      | ✔ | ✔ | ✘ |
| `ELIMINAR`       | ✔ | ✘ | ✘ |
| `CREAR_USUARIOS` | ✔ | ✘ | ✘ |
| `CREAR_ROLES`    | ✔ | ✘ | ✘ |

La matriz se lee de la tabla `RolPermiso`. Los roles nuevos creados con la API obtienen los permisos que se les asignen.

El rol **Administrador** es del sistema: no se puede modificar ni eliminar.

### Habilitar o deshabilitar botones y menús en el cliente

El cliente debe llamar a `GET /api/auth/permisos` después del login y usar el resultado para habilitar o deshabilitar cada botón u opción de menú. Incluye todos los permisos de la BD, también los adicionales:

```json
{ "AGREGAR": false, "MODIFICAR": true, "ELIMINAR": false, "CONSULTAR": true, "CREAR_USUARIOS": false, "CREAR_ROLES": false }
```

Aunque el cliente muestre un botón por error, **la API vuelve a validar el permiso en cada endpoint** y responde `403` si no lo tiene.

### Proteger endpoints nuevos (para otros módulos)

Los controladores de otros módulos se protegen con el mismo atributo:

```csharp
[Route("api/productos")]
public class ProductosController : ApiControllerBase
{
    [HttpGet]    [RequierePermiso(Acciones.Consultar)] public IActionResult Listar() { ... }
    [HttpPost]   [RequierePermiso(Acciones.Agregar)]   public IActionResult Crear(...) { ... }
    [HttpPut]    [RequierePermiso(Acciones.Modificar)] public IActionResult Modificar(...) { ... }
    [HttpDelete] [RequierePermiso(Acciones.Eliminar)]  public IActionResult Eliminar(...) { ... }
}
```

- Todo endpoint exige token por defecto. Para uno público se usa `[PermitirAnonimo]`.
- Si se ponen varios `[RequierePermiso]` (en la clase y en el método), se exigen **todos**.
- Dentro de un método se puede inyectar `SesionActual` y consultar `sesion.HasPermission(Acciones.Eliminar)`, `sesion.Usuario`, `sesion.Rol`, etc.

---

## 3. Códigos de respuesta y errores

Todos los errores devuelven un JSON con `mensaje`.

| Código | Cuándo | Cuerpo |
|--------|--------|--------|
| `200` | Operación correcta | Datos o `{ "mensaje": "..." }` |
| `201` | Registro creado | El registro creado; encabezado `Location` con su URL |
| `400` | Datos inválidos (campos vacíos, contraseña débil, eliminarse a sí mismo…) | `{ "mensaje": "..." }` o el formato estándar de validación con `errors` |
| `401` | Sin token, token inválido o sesión expirada; o credenciales incorrectas en el login | `{ "mensaje": "..." }` |
| `403` | **Permisos insuficientes** | `{ "mensaje": "...", "accion": "ELIMINAR", "rol": "Supervisor" }` |
| `404` | El rol o usuario no existe | `{ "mensaje": "..." }` |
| `409` | Conflicto: nombre duplicado, rol o permiso del sistema, rol con usuarios asignados | `{ "mensaje": "..." }` |
| `423` | Cuenta bloqueada por intentos fallidos | `{ "mensaje": "..." }` |

Ejemplo de `403`:

```json
{
  "mensaje": "Permisos insuficientes: el rol \"Supervisor\" no tiene permiso para realizar la acción ELIMINAR.",
  "accion": "ELIMINAR",
  "rol": "Supervisor"
}
```

Ejemplo de error de validación (`400`):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Nombre": ["El nombre del rol es obligatorio."] }
}
```

---

## 4. Seguridad de contraseñas y bloqueo

- **Hash:** PBKDF2 con HMAC-SHA256, 100 000 iteraciones y un salt aleatorio de 16 bytes por usuario. Se guarda en `Usuarios.ContrasenaHash` con el formato `PBKDF2-SHA256$<iteraciones>$<salt>$<hash>` (118 caracteres).
- **Usuarios del script SQL:** el script `GestionEmpresarial.sql` crea los usuarios con SHA-512 sin salt. La API los acepta y, la primera vez que inician sesión correctamente, convierte su hash al formato con salt.
- **Reglas de contraseña:** mínimo 8 caracteres, con letras y números.
- **Bloqueo:** tras **3 intentos fallidos** seguidos la cuenta se bloquea (`EstadoCuenta = 0`) y el login responde `423`. Un login correcto reinicia el contador. Solo un administrador puede desbloquearla con `POST /api/usuarios/{id}/desbloquear`, o reactivándola con `PUT /api/usuarios/{id}`.
- Con usuario inexistente la respuesta es `401` genérico y no se cuentan intentos.

---

## 5. Endpoints

Resumen:

| Método | Ruta | Permisos requeridos |
|--------|------|---------------------|
| POST | `/api/auth/login` | Público |
| POST | `/api/auth/logout` | Sesión |
| GET | `/api/auth/sesion` | Sesión |
| GET | `/api/auth/permisos` | Sesión |
| GET | `/api/auth/permisos/{accion}` | Sesión |
| GET | `/api/permisos` | `CONSULTAR` |
| GET | `/api/permisos/{id}` | `CONSULTAR` |
| GET | `/api/permisos/rol/{rolId}` | `CONSULTAR` |
| POST | `/api/permisos` | `CREAR_ROLES` + `AGREGAR` |
| PUT | `/api/permisos/{id}` | `CREAR_ROLES` + `MODIFICAR` |
| DELETE | `/api/permisos/{id}` | `CREAR_ROLES` + `ELIMINAR` |
| GET | `/api/roles` | `CREAR_ROLES` + `CONSULTAR` |
| GET | `/api/roles/{id}` | `CREAR_ROLES` + `CONSULTAR` |
| POST | `/api/roles` | `CREAR_ROLES` + `AGREGAR` |
| PUT | `/api/roles/{id}` | `CREAR_ROLES` + `MODIFICAR` |
| DELETE | `/api/roles/{id}` | `CREAR_ROLES` + `ELIMINAR` |
| GET | `/api/usuarios` | `CREAR_USUARIOS` + `CONSULTAR` |
| GET | `/api/usuarios/{id}` | `CREAR_USUARIOS` + `CONSULTAR` |
| POST | `/api/usuarios` | `CREAR_USUARIOS` + `AGREGAR` |
| PUT | `/api/usuarios/{id}` | `CREAR_USUARIOS` + `MODIFICAR` |
| POST | `/api/usuarios/{id}/desbloquear` | `CREAR_USUARIOS` + `MODIFICAR` |
| DELETE | `/api/usuarios/{id}` | `CREAR_USUARIOS` + `ELIMINAR` |
| GET | `/api/clientes` | `CONSULTAR` |
| GET | `/api/clientes/{id}` | `CONSULTAR` |
| POST | `/api/clientes` | `AGREGAR` |
| PUT | `/api/clientes/{id}` | `MODIFICAR` |
| DELETE | `/api/clientes/{id}` | `ELIMINAR` |
| GET | `/api/productos` | `CONSULTAR` |
| GET | `/api/productos/{id}` | `CONSULTAR` |
| GET | `/api/productos/categorias` | `CONSULTAR` |
| POST | `/api/productos` | `AGREGAR` |
| PUT | `/api/productos/{id}` | `MODIFICAR` |
| DELETE | `/api/productos/{id}` | `ELIMINAR` |

Con la matriz actual, la gestión de roles, permisos y usuarios queda solo para el **Administrador**. Clientes y productos usan los permisos genéricos: el Supervisor consulta y modifica, y el Ejecutor consulta y agrega.

### 5.1 Autenticación — `/api/auth`

#### `POST /api/auth/login` — Iniciar sesión

Público. Petición:

```json
{ "usuario": "admin", "contrasena": "Admin123*" }
```

Respuesta `200`:

```json
{
  "token": "82742EACF762AB67372F115033F1417DA5C82BA7C50F5CB15A1D879199C980AB",
  "expiraEn": "2026-09-23T03:40:30.8197016Z",
  "mensaje": "Bienvenido, admin.",
  "sesion": {
    "usuarioId": 1,
    "usuario": "admin",
    "rolId": 1,
    "rol": "Administrador",
    "permisos": ["AGREGAR", "MODIFICAR", "ELIMINAR", "CONSULTAR", "CREAR_USUARIOS", "CREAR_ROLES"],
    "inicioSesion": "2026-09-22T23:10:28.9130889-04:00"
  }
}
```

Errores:

| Código | Mensaje |
|--------|---------|
| `400` | Faltan usuario o contraseña |
| `401` | `Usuario o contraseña incorrectos. Intentos restantes: 2.` |
| `423` | `Contraseña incorrecta. Se alcanzaron 3 intentos fallidos y la cuenta fue bloqueada. Contacte al administrador.` |
| `423` | `La cuenta está bloqueada por exceder el número de intentos permitidos. Contacte al administrador.` |

#### `POST /api/auth/logout` — Cerrar sesión

Invalida el token enviado. Respuesta `200`: `{ "mensaje": "Sesión cerrada." }`

#### `GET /api/auth/sesion` — Sesión actual

Devuelve el usuario y el rol activo (el mismo objeto `sesion` del login).

#### `GET /api/auth/permisos` — Permisos del rol activo

Respuesta `200` (ejemplo para Supervisor):

```json
{ "AGREGAR": false, "MODIFICAR": true, "ELIMINAR": false, "CONSULTAR": true, "CREAR_USUARIOS": false, "CREAR_ROLES": false }
```

#### `GET /api/auth/permisos/{accion}` — HasPermission

Consulta si el rol activo tiene una acción concreta. No distingue mayúsculas.

```
GET /api/auth/permisos/eliminar
```

```json
{ "accion": "ELIMINAR", "permitido": false }
```

### 5.2 Permisos — `/api/permisos`

Objeto `Permiso`:

```json
{ "permisoId": 7, "nombre": "EXPORTAR_REPORTES", "descripcion": "Permite exportar reportes", "esSistema": false }
```

`esSistema = true` indica un permiso base (no se puede eliminar ni renombrar).

#### `GET /api/permisos` — Listar permisos

```json
[
  { "permisoId": 1, "nombre": "AGREGAR", "descripcion": "Permite agregar registros", "esSistema": true },
  { "permisoId": 2, "nombre": "MODIFICAR", "descripcion": "Permite modificar registros", "esSistema": true },
  { "permisoId": 3, "nombre": "ELIMINAR", "descripcion": "Permite eliminar registros", "esSistema": true },
  { "permisoId": 4, "nombre": "CONSULTAR", "descripcion": "Permite consultar registros", "esSistema": true },
  { "permisoId": 5, "nombre": "CREAR_USUARIOS", "descripcion": "Permite crear nuevos usuarios", "esSistema": true },
  { "permisoId": 6, "nombre": "CREAR_ROLES", "descripcion": "Permite crear nuevos roles", "esSistema": true }
]
```

Los `permisoId` son los que se envían en `permisoIds` al crear o modificar un rol.

#### Resto de endpoints

| Endpoint | Petición | Respuesta correcta | Errores |
|----------|----------|--------------------|---------|
| `GET /api/permisos/{id}` | — | `200` `Permiso` | `404` |
| `GET /api/permisos/rol/{rolId}` | — | `200` lista de `Permiso` del rol | — |
| `POST /api/permisos` | `{ "nombre": "EXPORTAR_REPORTES", "descripcion": "Permite exportar reportes" }` | `201` `Permiso` creado | `400` nombre vacío o con formato inválido · `409` nombre duplicado |
| `PUT /api/permisos/{id}` | `{ "nombre": "EXPORTAR_PDF", "descripcion": "Exporta PDF" }` | `200` `{ "mensaje": ... }` | `400` · `404` · `409` nombre duplicado o intento de renombrar un permiso base |
| `DELETE /api/permisos/{id}` | — | `200` `{ "mensaje": "Permiso \"EXPORTAR_PDF\" eliminado correctamente (se quitó de 2 rol(es))." }` | `404` · `409` permiso base |

Reglas:

- **Nombre:** se guarda en mayúsculas. Solo admite letras sin acento, números y guion bajo, y debe empezar con una letra; máximo 100 caracteres. `exportar_reportes` se guarda como `EXPORTAR_REPORTES`.
- **Descripción:** opcional, máximo 200 caracteres.
- **Al crear**, el permiso se asigna automáticamente al rol Administrador.
- **Al eliminar**, el permiso se quita de todos los roles que lo tenían.
- **En un permiso base**, el `PUT` debe mantener el mismo `nombre`; solo cambia la descripción.

### 5.3 Roles — `/api/roles`

Objeto `Rol`:

```json
{
  "rolId": 4,
  "nombre": "Auditor",
  "esSistema": false,
  "cantidadUsuarios": 0,
  "permisos": [ { "permisoId": 4, "nombre": "CONSULTAR", "descripcion": "Permite consultar registros" } ]
}
```

`esSistema = true` indica el rol Administrador (no editable ni eliminable).

| Endpoint | Petición | Respuesta correcta | Errores |
|----------|----------|--------------------|---------|
| `GET /api/roles` | — | `200` lista de `Rol` | — |
| `GET /api/roles/{id}` | — | `200` `Rol` | `404` |
| `POST /api/roles` | `{ "nombre": "Auditor", "permisoIds": [4] }` | `201` `Rol` creado | `400` nombre vacío o de más de 50 caracteres · `409` nombre duplicado |
| `PUT /api/roles/{id}` | `{ "nombre": "Auditor", "permisoIds": [1, 4] }` | `200` `{ "mensaje": ... }` | `400` · `404` · `409` nombre duplicado o rol del sistema |
| `DELETE /api/roles/{id}` | — | `200` `{ "mensaje": ... }` | `404` · `409` rol del sistema o con usuarios asignados |

En `PUT`, `permisoIds` **reemplaza** la lista completa de permisos del rol. Los ids que no existen se ignoran.

### 5.4 Usuarios — `/api/usuarios`

Objeto `Usuario` (nunca incluye la contraseña ni su hash):

```json
{
  "usuarioId": 2,
  "usuario": "supervisor",
  "rolId": 2,
  "rol": "Supervisor",
  "estadoCuenta": false,
  "bloqueado": true,
  "intentosFallidos": 3
}
```

| Endpoint | Petición | Respuesta correcta | Errores |
|----------|----------|--------------------|---------|
| `GET /api/usuarios` | — | `200` lista de `Usuario` | — |
| `GET /api/usuarios/{id}` | — | `200` `Usuario` | `404` |
| `POST /api/usuarios` | `{ "usuario": "maria", "contrasena": "Maria2026x", "rolId": 3 }` | `201` `Usuario` creado | `400` datos o contraseña inválidos, rol inexistente · `409` usuario duplicado |
| `PUT /api/usuarios/{id}` | `{ "rolId": 3, "estadoCuenta": true, "nuevaContrasena": null }` | `200` `{ "mensaje": ... }` | `400` · `404` |
| `POST /api/usuarios/{id}/desbloquear` | — | `200` `{ "mensaje": "Usuario \"supervisor\" desbloqueado." }` | `404` |
| `DELETE /api/usuarios/{id}` | — | `200` `{ "mensaje": ... }` | `400` eliminarse a sí mismo · `404` |

Notas sobre `PUT /api/usuarios/{id}`:

- `nuevaContrasena` vacía o `null` conserva la contraseña actual.
- `estadoCuenta: true` en una cuenta bloqueada la reactiva y reinicia los intentos fallidos.
- Un usuario no puede cambiar su propio rol ni desactivar su propia cuenta (`400`).

### 5.5 Clientes — `/api/clientes`

Objeto `Cliente`:

```json
{
  "clienteId": 1,
  "nombre": "Ana",
  "apellido": "García",
  "telefono": "809-555-0101",
  "correo": "ana.prueba@example.com",
  "direccion": "Santo Domingo"
}
```

`POST` y `PUT` reciben el mismo cuerpo, sin `clienteId`.

| Endpoint | Petición | Respuesta correcta | Errores |
|----------|----------|--------------------|---------|
| `GET /api/clientes?buscar=perez` | `buscar` opcional | `200` lista de `Cliente` | — |
| `GET /api/clientes/{id}` | — | `200` `Cliente` | `404` |
| `POST /api/clientes` | `{ "nombre": "Luis", "apellido": "Mota", "telefono": "809-555-0104", "correo": "luis@example.com", "direccion": "La Vega" }` | `201` `Cliente` creado | `400` datos inválidos · `409` correo duplicado |
| `PUT /api/clientes/{id}` | igual que `POST` | `200` `{ "mensaje": ... }` | `400` · `404` · `409` correo duplicado |
| `DELETE /api/clientes/{id}` | — | `200` `{ "mensaje": ... }` | `404` |

Reglas:

- **Nombre y apellido:** obligatorios, máximo 100 caracteres.
- **Correo:** obligatorio, formato válido, máximo 254 caracteres y único (sin distinguir mayúsculas).
- **Teléfono:** opcional, máximo 20 caracteres; solo números, espacios, guiones, paréntesis y `+`.
- **Dirección:** opcional, máximo 250 caracteres.
- Los campos opcionales vacíos se guardan como `null`, y todos los textos se guardan sin espacios al inicio ni al final.
- **Búsqueda:** `buscar` filtra por nombre completo, teléfono o correo, sin distinguir mayúsculas ni acentos (`perez` encuentra `Pérez`).

### 5.6 Productos — `/api/productos`

Objeto `Producto`:

```json
{
  "productoId": 1,
  "codigo": "PROD-001",
  "nombre": "Teclado USB",
  "precio": 850.00,
  "stock": 15,
  "categoriaId": 1,
  "categoria": "Electrónica"
}
```

`POST` y `PUT` reciben `codigo`, `nombre`, `precio`, `stock` y `categoriaId`.

| Endpoint | Petición | Respuesta correcta | Errores |
|----------|----------|--------------------|---------|
| `GET /api/productos?buscar=mouse&categoriaId=3` | `buscar` y `categoriaId` opcionales | `200` lista de `Producto` | — |
| `GET /api/productos/{id}` | — | `200` `Producto` | `404` |
| `GET /api/productos/categorias` | — | `200` `[ { "categoriaId": 1, "nombre": "Electrónica" }, ... ]` | — |
| `POST /api/productos` | `{ "codigo": "PROD-004", "nombre": "Silla", "precio": 2500.00, "stock": 5, "categoriaId": 2 }` | `201` `Producto` creado | `400` datos inválidos o categoría inexistente · `409` código duplicado |
| `PUT /api/productos/{id}` | igual que `POST` | `200` `{ "mensaje": ... }` | `400` · `404` · `409` código duplicado |
| `DELETE /api/productos/{id}` | — | `200` `{ "mensaje": ... }` | `404` |

Reglas:

- **Código:** obligatorio, máximo 50 caracteres y único (sin distinguir mayúsculas).
- **Nombre:** obligatorio, máximo 150 caracteres.
- **Precio:** mayor o igual a 0, con 2 decimales como máximo.
- **Stock:** entero mayor o igual a 0.
- **Categoría:** debe existir. Las categorías se cargan con el script SQL; no tienen mantenimiento propio.
- **Búsqueda:** `buscar` filtra por código o nombre, sin distinguir mayúsculas ni acentos.

---

## 6. Usuarios de prueba

Creados por `GestionEmpresarial.sql`:

| Usuario | Contraseña | Rol |
|---------|------------|-----|
| `admin` | `Admin123*` | Administrador |
| `supervisor` | `Super123*` | Supervisor |
| `ejecutor` | `Ejecut123*` | Ejecutor |

El archivo `SGECAR.API.http` tiene peticiones de ejemplo listas para ejecutar desde Visual Studio.

---

## 7. Estructura del código

| Capa | Archivos | Responsabilidad |
|------|----------|-----------------|
| API | `Controllers/AuthController.cs`, `RolesController.cs`, `UsuariosController.cs`, `PermisosController.cs`, `ClientesController.cs`, `ProductosController.cs`, `ApiControllerBase.cs` | Endpoints y conversión de resultados a códigos HTTP |
| API | `Security/SesionStore.cs` | Sesiones activas en memoria (token → usuario, rol y permisos) |
| API | `Security/SesionActual.cs` | Sesión de la petición actual y `HasPermission(accion)` |
| API | `Security/Filtros.cs` | `[PermitirAnonimo]`, `[RequierePermiso]`, respuestas `401` y `403` |
| Business | `Services/AuthService.cs` | Login, intentos fallidos y bloqueo (implementa `IAuthService`) |
| Business | `Services/RolService.cs`, `UsuarioService.cs`, `PermisoService.cs`, `ClienteService.cs`, `ProductoService.cs` | Reglas de negocio del CRUD (implementan `IRolService`, `IUsuarioService`, `IPermisoService`, `IClienteService`, `IProductoService`) |
| Business | `Security/PasswordHasher.cs` | Hash y verificación de contraseñas |
| Data | `Repositories/*.cs` | Acceso a datos con Entity Framework Core (implementan `IUsuarioRepository`, `IRoleRepository`, `IPermisoRepository`, `IClienteRepository`, `IProductoRepository`) |
| Shared | `Contracts/Services/I*Service.cs` | Interfaces de los servicios |
| Shared | `Contracts/Repositories/I*Repository.cs` | Interfaces de los repositorios |
| Shared | `Contracts/*.cs` | DTOs y resultados |
| Shared | `Security/Acciones.cs` | Constantes de permisos y roles, `Acciones.EsPermisoBase()` y `RolesSistema.EsRolProtegido()` |

### Inyección de dependencias

Cada clase recibe sus dependencias **por interfaz**, nunca por la clase concreta:

```
Controllers ──> I*Service (Shared) <── *Service (Business) ──> I*Repository (Shared) <── *Repository (Data)
```

El registro está en `Program.cs`:

```csharp
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPermisoService, PermisoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
```

Para un módulo nuevo: crear la interfaz en `SGECAR.Shared/Contracts`, implementarla en Business o Data, registrarla en `Program.cs` e inyectarla por la interfaz.
