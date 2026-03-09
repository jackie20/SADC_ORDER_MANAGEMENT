using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SADC_Order_Management_System.Authorization;
using SADC_Order_Management_System.Configurations;
using SADC_Order_Management_System.Infrastructure.Data;
using SADC_Order_Management_System.Infrastructure.Middleware;
using SADC_Order_Management_System.Infrastructure.Messaging;
using SADC_Order_Management_System.Repositories.Implementations;
using SADC_Order_Management_System.Repositories.Interfaces;
using SADC_Order_Management_System.Services.Implementations;
using SADC_Order_Management_System.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/api-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<FxOptions>(builder.Configuration.GetSection(FxOptions.SectionName));
builder.Services.Configure<EntraOptions>(builder.Configuration.GetSection(EntraOptions.SectionName));

builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection(EntraOptions.SectionName));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.OrdersRead, policy =>
        policy.RequireClaim("roles", PolicyNames.OrdersRead, PolicyNames.OrdersWrite, PolicyNames.OrdersAdmin));

    options.AddPolicy(PolicyNames.OrdersWrite, policy =>
        policy.RequireClaim("roles", PolicyNames.OrdersWrite, PolicyNames.OrdersAdmin));

    options.AddPolicy(PolicyNames.OrdersAdmin, policy =>
        policy.RequireClaim("roles", PolicyNames.OrdersAdmin));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));



builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SADC_Order_Management_System"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation());

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<IProcessedMessageRepository, ProcessedMessageRepository>();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFxService, FxService>();
builder.Services.AddScoped<IOutboxService, OutboxService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ETagMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readiness");
app.MapControllers();

app.Run();