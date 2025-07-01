//Importaciones para usar EF 
using Microsoft.EntityFrameworkCore;
using PaymentManager.Models;


var builder = WebApplication.CreateBuilder(args);


// Configurar Entity Framework con SQL Server, la cadena de conexion la agregue en appsettings.json, (DefaultConnection)
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Soporte para controllers y vistas
builder.Services.AddControllersWithViews();

var app = builder.Build();





// Ajustes de la plantilla para construir la app y configurar solicitudes HTTP (Desactive el HTTPS al crear el proyecto)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();


app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
