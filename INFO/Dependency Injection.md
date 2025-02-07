---
created: 2025-02-04-13-38-23+10:00
tags:
---
# MSDN
https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0

---

**Dependency Injection (DI)** — это техника реализации **Inversion of Control (IoC)**, при которой зависимости объекта не создаются им самим, а **"внедряются" извне**. Это делает код более гибким, тестируемым и модульным.

---

### Простая аналогия 🍔
Представьте, что вы заказываете пиццу:
- **Без DI**: Вы сами выращиваете овощи, печете тесто, готовите соус.
- **С DI**: Вам привозят готовые ингредиенты (зависимости), и вы только собираете пиццу.

---

## Как это работает?
### Без DI (Жесткая связь)
```csharp
public class ProductHandler
{
    private readonly JsonProductRepository _repository;

    public ProductHandler()
    {
        // Зависимость создается внутри класса
        _repository = new JsonProductRepository(); 
    }
}
```
**Проблема**: Если вы захотите использовать `SqlProductRepository`, придется менять код `ProductHandler`.

---

### С DI (Гибкая связь)
```csharp
public class ProductHandler
{
    private readonly IProductRepository _repository;

    // Зависимость внедряется через конструктор
    public ProductHandler(IProductRepository repository)
    {
        _repository = repository; // Реализация предоставляется извне
    }
}
```
**Преимущества**:
- Класс не зависит от конкретной реализации `IProductRepository`.
- Легко подменить реализацию (например, для тестов).

---

## 3 основных типа DI
1. **Через конструктор** (наиболее популярный):
   ```csharp
   public class MyService
   {
       private readonly IDependency _dep;
       public MyService(IDependency dep) => _dep = dep;
   }
   ```

2. **Через свойства**:
   ```csharp
   public class MyService
   {
       public IDependency Dep { get; set; }
   }
   ```

3. **Через методы**:
   ```csharp
   public class MyService
   {
       public void Initialize(IDependency dep) { ... }
   }
   ```

---

## Почему DI важен?
4. **Уменьшение связанности (Low Coupling)**:
   - Классы зависят от абстракций (`интерфейсов`), а не конкретных реализаций.

5. **Тестируемость**:
   - Можно передавать mock-объекты в тестах:
   ```csharp
   var mockRepo = new Mock<IProductRepository>();
   var handler = new ProductHandler(mockRepo.Object);
   ```

6. **Гибкость**:
   - Замена реализации требует изменения только конфигурации DI-контейнера.

7. **Управление жизненным циклом**:
   - Контейнер контролирует, как создаются объекты (Singleton, Scoped, Transient).

---

## DI в ASP.NET Core
8. **Регистрация зависимостей** (в `Program.cs`):
```csharp
builder.Services.AddScoped<IProductRepository, JsonProductRepository>();
builder.Services.AddTransient<ProductHandler>();
```

9. **Автоматическое внедрение** в контроллеры:
```csharp
public class ProductsController : ControllerBase
{
    private readonly ProductHandler _handler;

    // Зависимость автоматически внедряется
    public ProductsController(ProductHandler handler) 
    {
        _handler = handler;
    }
}
```

---

## Жизненные циклы зависимостей
| Тип         | Описание                                                                 |
|-------------|-------------------------------------------------------------------------|
| **Transient** | Новый объект при каждом запросе зависимости (`AddTransient`)           |
| **Scoped**    | Один объект на область (например, HTTP-запрос) (`AddScoped`)           |
| **Singleton** | Один объект на все приложение (`AddSingleton`)                         |

Пример:
```csharp
services.AddSingleton<ICacheService, CacheService>(); // Один экземпляр на всё приложение
services.AddScoped<IUserContext, UserContext>(); // Один экземпляр на запрос
services.AddTransient<IEmailService, EmailService>(); // Новый экземпляр каждый раз
```

---

## Распространенные ошибки
10. **Сервис-локатор (Service Locator)**:
   ```csharp
   // Плохо: скрытая зависимость
   var repo = ServiceLocator.Get<IProductRepository>();
   ```
   - Нарушает принцип явных зависимостей.

11. **Цепочки зависимостей**:
   - Избегайте глубоких цепочек вида `A -> B -> C -> D`. Используйте фасады.

12. **Циклические зависимости**:
   - `ClassA` зависит от `ClassB`, который зависит от `ClassA`. Решение: рефакторинг.

---

## Пример: Тестирование с DI
```csharp
// Тест с использованием Moq
[Test]
public void GetProducts_ReturnsFilteredList()
{
    // Arrange
    var mockRepo = new Mock<IProductRepository>();
    mockRepo.Setup(r => r.GetProducts()).Returns(new List<Product> { ... });
    
    var handler = new ProductHandler(mockRepo.Object); // Внедряем mock
    
    // Act
    var result = handler.Get();
    
    // Assert
    Assert.That(result.Count, Is.EqualTo(1));
}
```

---

## Когда использовать DI?
- В проектах средней и большой сложности.
- При частых изменениях требований.
- Для кода, который нужно покрывать unit-тестами.

## Когда НЕ использовать?
- В микроскопических проектах (например, утилитарные скрипты).
- Если внедрение зависимостей усложнит код без видимой пользы.

---

## Главное правило DI:
**"Зависимости должны быть явными"**  
Если класс требует какие-то компоненты для работы — они должны быть явно объявлены (через конструктор, свойства или методы).


