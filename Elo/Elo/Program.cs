using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Elo.Infrastructure.Data;
using Elo.Infrastructure.Repositories;
using Elo.Infrastructure.Services;
using Elo.Infrastructure.Middleware;
using Elo.Presentation.Configuration;
using Elo.Application.Behaviors;
using Elo.Application.Mappers;
using Elo.Application.UseCases.Clientes;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Services;
using FluentValidation;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost",
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "https://localhost:7271")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Domain Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IFornecedorCategoriaService, FornecedorCategoriaService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IEmpresaContextService, EmpresaContextService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAfiliadoService, AfiliadoService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IHistoriaService, HistoriaService>();
builder.Services.AddScoped<IHistoriaTipoService, HistoriaTipoService>();
builder.Services.AddScoped<IHistoriaStatusService, HistoriaStatusService>();
builder.Services.AddScoped<ITicketTipoService, TicketTipoService>();
builder.Services.AddScoped<IContaReceberService, ContaReceberService>();
builder.Services.AddScoped<IContaPagarService, ContaPagarService>();
builder.Services.AddScoped<IAssinaturaService, AssinaturaService>();
builder.Services.AddScoped<IEmpresaFormaPagamentoService, EmpresaFormaPagamentoService>();

// Application Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IClienteMapper, ClienteMapper>();
builder.Services.AddScoped<IUserMapper, UserMapper>();
builder.Services.AddScoped<IFornecedorMapper, FornecedorMapper>();
builder.Services.AddScoped<IProdutoMapper, ProdutoMapper>();
builder.Services.AddScoped<IAfiliadoMapper, AfiliadoMapper>();

// MediatR
var applicationAssembly = typeof(CreateCliente.Command).Assembly;
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(applicationAssembly));

// Behaviors
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(applicationAssembly);

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// Global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure database is created and initialized
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    await DbInitializer.InitializeAsync(context);
}

app.Run();
