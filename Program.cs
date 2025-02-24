using BlockchainTest.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký BlockchainService là Scoped (thay vì Singleton)
builder.Services.AddScoped<BlockchainService>();

// Đăng ký BlockChainContext với SQLite
builder.Services.AddDbContext<BlockChainContext>(options =>
    options.UseSqlite("Data Source=blockchain.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Sửa từ MapStaticAssets() và WithStaticAssets() để sử dụng middleware chuẩn của ASP.NET Core
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();