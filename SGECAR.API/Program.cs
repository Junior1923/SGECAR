using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SGECAR.API.Security;
using SGECAR.Business.Services;
using SGECAR.Data.Context;
using SGECAR.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// TODO ENDPOINT REQUIERE SESIÓN SALVO LOS MARCADOS CON [PermitirAnonimo]
builder.Services.AddControllers(options => options.Filters.Add(new AutenticacionFilter()));

builder.Services.AddDbContext<GestionEmpresarialContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GestionEmpresarialConnection")
    )
);

// SESIÓN (SesionStore GUARDA LAS SESIONES MIENTRAS CORRE LA API; SesionActual ES LA DE CADA PETICIÓN)
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<SesionStore>();
builder.Services.AddScoped<SesionActual>();

// REPOSITORIOS
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<PermisoRepository>();

// SERVICIOS DE NEGOCIO
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<RolService>();
builder.Services.AddScoped<PermisoService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SGECAR API", Version = "v1" });

    // PERMITE PEGAR EL TOKEN DEL LOGIN EN EL BOTÓN "Authorize" DE SWAGGER
    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Token devuelto por POST /api/auth/login",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", esquema);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { esquema, Array.Empty<string>() } });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
