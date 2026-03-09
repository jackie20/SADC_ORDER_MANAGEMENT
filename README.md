# SADC Order Management System

A ASP.NET Core backend for managing **Customers**, **Orders**, **OrderLineItems**, **status transitions**, **idempotent updates**, **RabbitMQ messaging**, and **ZAR FX reporting** for SADC countries.

---

# 1. Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **RabbitMQ**
- **Microsoft Entra JWT**
- **Swagger / OpenAPI**
- **Serilog**
- **OpenTelemetry**
- **In-memory FX cache**
- **Docker** for RabbitMQ bootstrap

---

# 2. Project Name

```text
SADC_Order_Management_System

SADC_Order_Management_System/
│
├── Authorization/
│   └── PolicyNames.cs
│
├── Configurations/
│   ├── EntraOptions.cs
│   ├── FxOptions.cs
│   └── RabbitMqOptions.cs
│
├── Controllers/
│   ├── CustomersController.cs
│   ├── OrdersController.cs
│   └── ReportsController.cs
│
├── DTOs/
│   ├── Requests/
│   └── Responses/
│
├── Helpers/
│   ├── CorrelationHelper.cs
│   ├── CurrencyHelper.cs
│   ├── ETagHelper.cs
│   ├── FxRoundingHelper.cs
│   └── ProblemDetailsHelper.cs
│
├── Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── SeedData.cs
│   ├── Messaging/
│   │   ├── OrderCreatedEvent.cs
│   │   ├── RabbitMqConsumerService.cs
│   │   └── RabbitMqPublisher.cs
│   └── Middleware/
│       ├── CorrelationIdMiddleware.cs
│       ├── ETagMiddleware.cs
│       └── ExceptionHandlingMiddleware.cs
│
├── Models/
│   ├── BaseEntity.cs
│   ├── Customer.cs
│   ├── IdempotencyRecord.cs
│   ├── Order.cs
│   ├── OrderLineItem.cs
│   ├── OrderStatus.cs
│   ├── OutboxMessage.cs
│   ├── ProcessedMessage.cs
│   └── ResponseModel.cs
│
├── Repositories/
│   ├── Implementations/
│   └── Interfaces/
│
├── Services/
│   ├── Implementations/
│   └── Interfaces/
│
├── appsettings.json
├── Program.cs
└── SADC_Order_Management_System.csproj


4. Prerequisites

Install the following before running the solution:

.NET SDK 8

SQL Server or SQL Server Developer Edition

SQL Server Management Studio or Azure Data Studio

Docker Desktop

Git

Optional:

Visual Studio 2022

VS Code

Postman


dotnet new sln -n SADC_Order_Management_System
dotnet new webapi -n SADC_Order_Management_System
dotnet sln add SADC_Order_Management_System/SADC_Order_Management_System.csproj


Install NuGet Packages

Run these commands inside the project folder:

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.Identity.Web
dotnet add package Swashbuckle.AspNetCore
dotnet add package RabbitMQ.Client --version 6.8.1
dotnet add package Microsoft.Extensions.Caching.Memory
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package FluentValidation.AspNetCore
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.Runtime
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol


docker run -d --hostname sadc-rabbit --name sadc-rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management


Create these GitHub secrets before running it:

AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
AZURE_WEBAPP_NAME
