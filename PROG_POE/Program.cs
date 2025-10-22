using Microsoft.EntityFrameworkCore;
using PROG_POE.Data;
using PROG_POE.Services;


var builder = WebApplication.CreateBuilder(args);

// Add MVC support
builder.Services.AddControllersWithViews();

// Configure EF Core with InMemory Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CMCS_DB"));

// Add dependency injection for your services
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PROG_POE.Data.AppDbContext>();
    DbSeeder.Seed(db);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
