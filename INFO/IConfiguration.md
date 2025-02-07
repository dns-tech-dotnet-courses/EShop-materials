---
created: 2025-02-05-16-34-22+10:00
tags:
---
IConfiguration - это интерфейс в .NET, который предоставляет доступ к конфигурационным настройкам приложения. Это ключевой компонент системы конфигурации .NET Core, который позволяет получать настройки из различных источников.

IConfiguration может читать конфигурационные данные из:
- JSON файлов (например, appsettings.json)
- Переменных окружения
- Командной строки
- XML файлов
- INI файлов
- Памяти приложения
- Пользовательских источников конфигурации

Пример использования в Program.cs для веб-приложения:

```csharp
var builder = WebApplication.CreateBuilder(args);
// Configuration уже доступен через builder.Configuration

// Можно добавить дополнительные источники конфигурации
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables();
```

Пример appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "CustomSettings": {
    "ApiKey": "my-secret-key",
    "BaseUrl": "https://api.example.com"
  }
}
```

Использование в классе:
```csharp
public class MyService
{
    private readonly string _apiKey;
    private readonly string _connectionString;

    public MyService(IConfiguration configuration)
    {
        _apiKey = configuration["CustomSettings:ApiKey"];
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
}
```

Регистрация сервиса в DI-контейнере:
```csharp
builder.Services.AddTransient<MyService>();
```

IConfiguration поддерживает:
- Иерархическую структуру настроек (через `:` в путях)
- Привязку к классам конфигурации (configuration binding)
- Перезагрузку конфигурации во время выполнения (если источник поддерживает)
- Шифрование конфиденциальных данных

Пример использования с классами конфигурации:
```csharp
public class CustomSettings
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
}

// Регистрация
builder.Services.Configure<CustomSettings>(
    builder.Configuration.GetSection("CustomSettings"));

// Использование
public class MyService
{
    private readonly CustomSettings _settings;

    public MyService(IOptions<CustomSettings> options)
    {
        _settings = options.Value;
    }
}
```

IConfiguration является важной частью современных .NET приложений и предоставляет гибкий способ управления конфигурацией, что особенно важно при работе с микросервисами и контейнеризированными приложениями.