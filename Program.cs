using ExternalIdDemo.Api.Authorization;
using ExternalIdDemo.Api.Data;
using ExternalIdDemo.Api.Interfaces;
using ExternalIdDemo.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "MfaRequired",
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.AddRequirements(
                new MfaRequirement());
        });
});


// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// Data Protection
builder.Services.AddDataProtection();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<IMfaService, MfaService>();

builder.Services.AddSingleton<IMfaEnrollmentStore, MfaEnrollmentStore>();

// TOTP services
builder.Services.AddScoped<ITotpSecretProtector, TotpSecretProtector>();

builder.Services.AddScoped<ITotpService,  TotpService>();

builder.Services.AddSingleton<IMfaSessionStore, MfaSessionStore>();

builder.Services.AddSingleton<IAuthorizationHandler, MfaAuthorizationHandler>();
// Controllers
builder.Services.AddControllers();

// ---------------------------------------------------------
// SWAGGER + JWT SUPPORT
// ---------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Microsoft Entra External Id API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(

                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:3002"
               
            )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();