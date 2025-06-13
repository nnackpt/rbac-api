using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Services;
using RBACapi.Services.Interfaces;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://10.83.51.52:3000")
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING")));

builder.Services.AddScoped<IApplicationsService, ApplicationsService>();
builder.Services.AddScoped<RBACapi.Services.AppFunctionsService>();
builder.Services.AddScoped<IAppRolesService, AppRolesService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
