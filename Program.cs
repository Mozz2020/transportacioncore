using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TransportationCore.CustomError;
using TransportationCore.Data;
using TransportationCore.Middleware;
using TransportationCore.Controllers;
using Audit.Core;
using TransportationCore.Data.Models;
using TransportationCore.Data.Interfaces;
using TransportationCore.Data.Utilidades;

var builder = WebApplication.CreateBuilder(args);


JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer("name=ConnectionStrings:DefaultConnection"));

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddControllers().AddJsonOptions(x =>
//x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ValidateModelAttribute());
})
   .AddJsonOptions(x =>
   {
       x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
   });


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ApiSGE+", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[]{ }
        }

    });
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UsuarioPolicy", policy => policy.RequireClaim("Permission", "Acciones Usuario"));
    options.AddPolicy("AsignarTCPolicy", policy => policy.RequireClaim("Permission", "Asignar TC"));
    options.AddPolicy("AdministradorPolicy", policy => policy.RequireClaim("Permission", "Administrador"));
    options.AddPolicy("AsignarCuentaPolicy", policy => policy.RequireClaim("Permission", "Asignar Cuenta"));
    options.AddPolicy("EditarClientePolicy", policy => policy.RequireClaim("Permission", "Editar Clientes"));
    options.AddPolicy("AsignarVehiculoPolicy", policy => policy.RequireClaim("Permission", "Asignar Vehiculo"));
    options.AddPolicy("EditarContactosPolicy", policy => policy.RequireClaim("Permission", "Editar Contactos"));
    options.AddPolicy("EditarOperadoresPolicy", policy => policy.RequireClaim("Permission", "Editar Operadores"));
    options.AddPolicy("EditarDirectoresPolicy", policy => policy.RequireClaim("Permission", "Editar Directores"));
    options.AddPolicy("ReportesGeneralesPolicy", policy => policy.RequireClaim("Permission", "Reportes Generales"));
    options.AddPolicy("CheckinCheckoutPolicy", policy => policy.RequireClaim("Permission", "Check In / Chek Out"));
    options.AddPolicy("EditarPlanificacionPolicy", policy => policy.RequireClaim("Permission", "Editar Planificacion"));
    options.AddPolicy("EditarCoordinadoresPolicy", policy => policy.RequireClaim("Permission", "Editar Coordinadores"));
    options.AddPolicy("ReportesAdministrativosPolicy", policy => policy.RequireClaim("Permission", "Reportes Administrativos"));
    options.AddPolicy("EditarCatalogosGeneralesPolicy", policy => policy.RequireClaim("Permission", "Editar Catalogos Generales"));

});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:3001/sted-planning")  
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<ISoftDeleteService, SoftDeleteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(x => x
           .AllowAnyMethod()
           .AllowAnyHeader()
           .SetIsOriginAllowed(origin => true)
           .AllowCredentials());
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
};

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors(x => x
       .AllowAnyMethod()
       .AllowAnyHeader()
       .SetIsOriginAllowed(origin => true)
       .AllowCredentials());

//app.UseCors();

app.Run();
