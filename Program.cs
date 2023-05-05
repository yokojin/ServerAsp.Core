using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectApp.Data;
using ServerApp.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


using static ServerApp.Controllers.RegController;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddAuthentication("Bearer").AddJwtBearer(options =>
{ options.TokenValidationParameters = new TokenValidationParameters

{
    /*Параметры*/
    // указывает, будет ли валидироваться издатель при валидации токена
    ValidateIssuer = true,
    // строка, представляющая издателя
    ValidIssuer = AuthOptions.ISSUER,
    // будет ли валидироваться потребитель токена
    ValidateAudience = true,
    // установка потребителя токена
    ValidAudience = AuthOptions.AUDIENCE,
    // будет ли валидироваться время существования
    ValidateLifetime = true,
    // установка ключа безопасности
    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
    // валидация ключа безопасности
    ValidateIssuerSigningKey = true,
};});
//сервис jwt токенов

builder.Services.AddTransient<AuthenticationHandler>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<UsersDataContext>(options => options.UseNpgsql
(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<IdentityOptions>(options => { 
});

//для работы таймера
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
builder.Services.AddCors();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   // app.UseSwagger();
  //  app.UseSwaggerUI();
}

app.UseRouting(); //Использовать маршурты

Console.WriteLine("Start my Server");
//Политика CROSS
app.UseCors(builder =>
{
    builder.AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
    .AllowAnyMethod()
    .WithOrigins("http://localhost:8080", "http://localhost:8080/SendMessage");
    

});

app.UseHttpsRedirection();
app.UseAuthentication(); //добавление middleware аунтификации
app.UseAuthorization(); 
app.UseDefaultFiles();
app.UseStaticFiles(); // Использование статичных файлов
app.MapControllers();
//app.MapControllerRoute(name: "default", pattern: "{controller=TestController}/{action=Index}/{id?}");
app.MapPost("/login", async (User user, AuthenticationHandler authenticationHandler) => await authenticationHandler.AuthenticateAsync(user));


// Создать экземпляр класса LoginService и передать ему данные пользователя
//LoginService loginService = new LoginService();
//loginService.Login(user);

// Вернуть ответ

app.MapControllerRoute(name: "default", pattern: "{controller=RegController}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=ProverkaController}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=NikkiDoController}/{action=FirstFixData}/{id?}");
app.MapControllerRoute(name: "default", pattern: "{controller=NikkiDoController}/{action=CheckData}/{id?}");

app.Use(async (context, next) => {
    context.Response.Headers.Add("X-MyHeader", "Hello World!");
    await next.Invoke();
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TimerF>("/SendMessage");
    endpoints.MapControllerRoute(name: "default", pattern: "{controller=NikkiDoController}/{action=FirstFixData}/{id?}");
   
    // endpoints.Map("/login", async context => await context.Response.WriteAsync("Hello METANIT.COM! use endpoints"));
});

app.Run();



