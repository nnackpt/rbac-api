using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RBACapi.Data;
using RBACapi.Services;
using RBACapi.Services.Interfaces;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.License.SetNonCommercialOrganization("<My Noncommercial organization>");

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//     {
//         policy.WithOrigins(allowedOrigins!)
//         .AllowCredentials()
//         .AllowAnyHeader()
//         .AllowAnyMethod();
//     });
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://10.83.49.10:3000")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbContextFactory<ApplicationDbContext>>(provider =>
{
    var options = provider.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
    return new PooledDbContextFactory<ApplicationDbContext>(options);
});

builder.Services.AddScoped<IApplicationsService, ApplicationsService>();
builder.Services.AddScoped<IAppFunctionsService, AppFunctionsService>();
builder.Services.AddScoped<IAppRolesService, AppRolesService>();
builder.Services.AddScoped<IRbacService, RbacService>();
builder.Services.AddScoped<IUsersAuthorizeService, UsersAuthorizeService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IAuthUsersService, AuthUsersService>();
builder.Services.AddScoped<IUserReviewFormService, UserReviewFormService>();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});
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

app.UseHttpsRedirection();

// #region start ปรับเพิ่มการเรียกใช้ react
// app.UseStaticFiles();
// app.UseRouting();
// app.MapFallbackToFile("index.html");
// #endregion

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
