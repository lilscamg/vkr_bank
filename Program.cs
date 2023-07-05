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

// ���������� ��������
builder.Services.AddControllersWithViews();  // ��������� ������� MVC
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IKeyService, KeyService>();


var app = builder.Build();

// ����������� middleware
app.UseHttpsRedirection();  // ��������� HTTPS
app.UseStaticFiles();  // ������������� ���� � ����������� ������ � wwwroot ����� ~/
app.UseRouting();  // �������� 
app.UseAuthentication(); // ���������� ��������������
app.UseAuthorization();  // ���������� �����������
app.MapControllerRoute(name: "default", pattern: "{controller=Main}/{action=Main}/{id?}");  // ������������� ������������� ��������� � �������������
app.Run();