
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Sesión: aquí se guardan el token y los datos del usuario logueado
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Cliente HTTP que apunta a la API
builder.Services.AddHttpClient("SGECAR.API", cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["ApiUrl"]!);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();