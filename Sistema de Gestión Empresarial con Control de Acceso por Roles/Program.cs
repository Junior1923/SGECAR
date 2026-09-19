
using Microsoft.EntityFrameworkCore;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Data;
using Sistema_de_Gestión_Empresarial_con_Control_de_Acceso_por_Roles.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<GestionEmpresarialContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("GestionEmpresarialConnection")
    )
);
builder.Services.AddScoped<UsuarioRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
