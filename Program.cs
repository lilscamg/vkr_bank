using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using vkr_bank.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using vkr_bank.Helpers;

var builder = WebApplication.CreateBuilder(args);
// var jwtKey = builder.Configuration["JWTKey"];

// добавление сервисов
builder.Services.AddControllersWithViews();  // добавляем сервисы MVC
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IKeyService, KeyService>();


var app = builder.Build();

// подключение middleware
app.UseHttpsRedirection();  // поддержка HTTPS
app.UseStaticFiles();  // устанавливает путь к статическим файлам к wwwroot через ~/
app.UseRouting();  // маршруты 
app.UseAuthentication(); // добавление аутентификации
app.UseAuthorization();  // добавление авторизации
app.MapControllerRoute(name: "default", pattern: "{controller=Main}/{action=Main}/{id?}");  // устанавливаем сопоставление маршрутов с контроллерами
app.Run();