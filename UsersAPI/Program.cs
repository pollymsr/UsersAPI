using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Services;
using FiapCloudGames.Infrastructure.Data;
using FiapCloudGames.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;
using FiapCloudGames.API.Middlewares;
using System.Text.Json.Serialization;
using MassTransit;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
        builder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fiap Cloud Games API",
        Version = "v1",
        Description = "API REST para gerenciamento de usuários, autenticação, jogos, biblioteca e promoções da plataforma Fiap Cloud Games."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada. Injete via Secret/Variável de Ambiente.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserDomainService, UserDomainService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"]
            ?? throw new InvalidOperationException("RabbitMQ:Host não configurado.");
        var rabbitUser = builder.Configuration["RabbitMQ:Username"]
            ?? throw new InvalidOperationException("RabbitMQ:Username não configurado.");
        var rabbitPass = builder.Configuration["RabbitMQ:Password"]
            ?? throw new InvalidOperationException("RabbitMQ:Password não configurado.");

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["JWT_SECRET_KEY"]
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT_SECRET_KEY não configurada. Injete via Kubernetes Secret.");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
    var seedAdminPass = builder.Configuration["SEED_ADMIN_PASSWORD"];
    var seedUserPass = builder.Configuration["SEED_USER_PASSWORD"];

    if (!db.Users.Any() && !string.IsNullOrWhiteSpace(seedAdminPass) && !string.IsNullOrWhiteSpace(seedUserPass))
    {
        db.Users.AddRange(
            new FiapCloudGames.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = "Professor Admin",
                Email = "admin@fiap.com.br",
                PasswordHash = userDomainService.HashPassword(seedAdminPass),
                Role = "Admin"
            },
            new FiapCloudGames.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Name = "Aluno Tester",
                Email = "user@fiap.com.br",
                PasswordHash = userDomainService.HashPassword(seedUserPass),
                Role = "User"
            }
        );
        db.SaveChanges();
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();
app.MapControllers();

app.Run();
