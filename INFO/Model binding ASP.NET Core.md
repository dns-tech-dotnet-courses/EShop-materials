---
created: 2025-01-31-20-56-09+10:00
tags:
---
**Model Binding в C#** — это механизм в ASP.NET Core, который автоматически связывает данные из HTTP-запроса (параметры URL, тело запроса, заголовки и т.д.) с параметрами методов контроллера или свойствами моделей. Это упрощает работу с входящими данными, избавляя разработчика от ручного парсинга.

---

### **Как это работает?**
1. **Источники данных**:
   - **Query String**: Параметры из URL (например, `/products?id=5`).
   - **Route Data**: Значения из маршрута (например, `/products/{id}`).
   - **Form Data**: Данные из HTML-форм (`application/x-www-form-urlencoded` или `multipart/form-data`).
   - **Body**: Тело запроса (например, JSON или XML при использовании `[FromBody]`).
   - **Заголовки (Headers)**: Данные из HTTP-заголовков.

2. **Сопоставление**:
   - Model Binding ищет совпадения между именами параметров метода контроллера и ключами в данных запроса.
   - Например, если метод принимает параметр `int id`, фреймворк попытается найти значение `id` в запросе.

---

### **Пример**
Допустим, есть модель и контроллер:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductController : Controller
{
    // Данные из запроса автоматически свяжутся с объектом Product
    [HttpPost]
    public IActionResult Create(Product product)
    {
        // product.Id, product.Name и product.Price заполнены из запроса
        return Ok();
    }
}
```

- Если отправить POST-запрос с телом JSON:
  ```json
  { "id": 1, "name": "Телефон", "price": 30000 }
  ```
  Model Binding преобразует JSON в объект `Product`.

---

### **Управление Model Binding**
Можно явно указать источник данных с помощью атрибутов:
- `[FromQuery]` — данные из Query String.
- `[FromRoute]` — данные из маршрута.
- `[FromForm]` — данные из формы.
- `[FromBody]` — данные из тела запроса (часто используется с JSON).
- `[FromHeader]` — данные из заголовков.

```csharp
public IActionResult Get([FromQuery] string searchTerm, [FromRoute] int id)
{
    // searchTerm берется из Query String, id — из маршрута
}
```

---

### **Валидация модели**
После привязки данных выполняется валидация:
- Если модель имеет атрибуты валидации (например, `[Required]`), Model Binding проверяет их.
- Результат валидации доступен через `ModelState.IsValid`.

```csharp
public class Product
{
    [Required]
    public string Name { get; set; }
}

[HttpPost]
public IActionResult Create(Product product)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }
    // Логика сохранения
}
```

---

### **Особенности**
- **Сложные объекты**: Model Binding поддерживает вложенные модели и коллекции.
- **Кастомные привязчики**: Можно создать собственный `IModelBinder` для нестандартных сценариев.
- **Конфликты имен**: Если имена параметров не совпадают с ключами в данных, используйте атрибут `[Bind]`.

---

### **Проблемы и решения**
- **Данные не привязываются**: Убедитесь, что имена свойств модели совпадают с ключами в запросе.
- **Неверные типы данных**: Например, строка вместо числа вызовет ошибку.
- **Используйте `[ApiController]`**: В ASP.NET Core этот атрибут автоматически возвращает `400 Bad Request` при ошибках валидации.

---