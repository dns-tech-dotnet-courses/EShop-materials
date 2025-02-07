---
created: 2025-01-31-15-28-14+10:00
tags:
---
[INFO Слоистая архитектура](INFO/Слоистая%20архитектура.md)  
Переходим в ветку `master`, делаем `pull`.  
Создаем ветку `cw-02` от `master`.  

![](attachments/Pasted%20image%2020250131153218.png)

# EShop.Domain
Добавим решение `EShop.Domain`.  

![](attachments/Pasted%20image%2020250131153326.png)  
![](attachments/Pasted%20image%2020250131153355.png)  
![](attachments/Pasted%20image%2020250131153418.png)

Удалим `Class1.cs` в проекте `EShop.Domain` - он нам не понадобится.  
С помощью Drag & Drop перетаскиваем `Product` в корень `EShop.Domain`.  
Откроем `Product` в сборке `EShop.Presentation` и `Product` в сборке `EShop.Domain`.  
Видим, что при переносе у `Product` в `EShop.Domain` `namespace` остался `EShop.Presentation`.  

![](attachments/Pasted%20image%2020250131154625.png)

Это не правильно - нужно синхронизировать `namespace`.  
Namespace файлов должны соответствовать структуре хранения файла.  
[INFO Namespace](INFO/Namespace.md)  

Установим курсор в первую строку `Product`.  
Нажмем `Alt+Enter` и выполним предложение `IntelliSense`.  
[INFO Контекстная подсказка Visual Studio](INFO/Контекстная%20подсказка%20Visual%20Studio.md)  

![](attachments/Pasted%20image%2020250131154751.png)

После применения предложенных правок видим, что namespace стал корректным.  

![](attachments/Pasted%20image%2020250131154910.png)

Удаляем `Product.cs` из сборки `EShop.Presentation` - он здесь больше не нужен, т.к. теперь он в "правильной" для него сборке `EShop.Domain`.  

Откроем `EShop.Presentation\Controllers\ProductsController.cs`.  
Видим предупреждения статического анализатора - он говорит, что ничего не знает о типе `Product`.  

![](attachments/Pasted%20image%2020250131160144.png)

Сейчас ничего с этим делать не будем, т.к. мы скоро избавимся от кода в этом месте.  

# EShop.Application

В слоистой архитектуре логика работы приложения располагается в Application слое.  
В нашем случае логика приложения - это возврат массива товаров по запросу. Сейчас у нас бедное приложение и границы слоев сложно очертить.  
Пока, что доверимся утверждению, что это слой приложения.  

Добавим в наше решение проект - `EShop.Application`. Также как и `EShop.Domain`, это будет библиотека классов.  
Удалим созданный шаблоном `Class1.cs`.  
Создадим в корне `EShop.Application` класс для обработки запросов связанных с `Product`: `ProductHandler`.  

![](attachments/Pasted%20image%2020250131170915.png)

Обратим внимание, что класс создался с модификатором - `internal`.  
Скопируем в `EShop.Application\ProductHandler.cs` из `EShop.Presentation\Controllers\ProductsController.cs` массив товаров.  

![](attachments/Pasted%20image%2020250131170920.png)

Видим, что статический анализатор так же недоволен типом `Product`.  

![](attachments/Pasted%20image%2020250131170924.png)

Так происходит потому, что сейчас сборки ничего не знают друг о друге и соответственно о типах, которые в них содержатся.  
Чтобы сборки могли видеть типы из других сборок, они должны использовать сборки как зависимости.  
В нашем случае тип `EShop.Application\ProductHandler.cs` из сборки `EShop.Application` пытается использовать тип из `EShop.Domain`. Получается, что `EShop.Application` зависит от `EShop.Domain`.  

Сначала посмотрим, какие зависимости сейчас есть в нашей сборке `EShop.Application`.  
Откроем файл `EShop.Application\EShop.Application.csproj`. Для этого совершим двойной клик по корню `Eshop.Application`.  
Пока видим такую картину:  

![](attachments/Pasted%20image%2020250131171416.png)

Выполним ПКМ по вкладке `Зависимости` у корня `Eshop.Application` и проставим зависимость.  

![](attachments/Pasted%20image%2020250131171619.png)

После этого увидим, что в файле `EShop.Application\EShop.Application.csproj` изменилось содержимое. Появилась ссылка на другой проект.  

![](attachments/Pasted%20image%2020250131171656.png)

Теперь сборка `EShop.Application` может использовать `public` типы из сборки `EShop.Domain`.  

Вернемся в `EShop.Application\ProductHandler.cs`, видим, что анализатор всё еще ругается.  
Но если мы развернем предложение `IntelliSense`, увидим, что список предлагаемых решений изменился.  

![](attachments/Pasted%20image%2020250131171858.png)

Так происходит из-за того, что ссылки в зависимостях недостаточно. Ссылка в зависимостях позволяет задействовать типы и `namespace` из зависимости. Чтобы мы могли в классе `EShop.Application\ProductHandler` сослаться на тип из `EShop.Domain`, необходимо в целевом файле указать `namespace` типа, в котором объявлен класс `Product.cs`.  

Можем написать код руками или доверить вставку `namespace` `IntelliSense`. Если `IntelliSense` видит в зависимостях текущей сборки тип с нужным именем - он сам предложит добавить нужный `using`. В больших проектах, у которых много сборок в зависимостях, следует быть осторожным, т.к. если в разных зависимостях есть типы с одинаковыми именами - может подтянуться не тот `using`. Надо выбирать тот, который вам нужен.  

1. Примем предложение `IntelliSense` по добавлению `using`.  
2. Подчистим другие неиспользуемые `using` (они прописаны в `global using` и не требуют указания в каждом месте использования).  
3. Видим, что предупреждения о неизвестном типе `Product.cs` пропали.  

![](attachments/Pasted%20image%2020250131172325.png)

Перенесем метод для получения списка товаров из `EShop.Presentation\Controllers\ProductsController.cs` в `EShop.Application\ProductHandler.cs`.  

Идея в том, чтобы в Presentation слое использовать типы из Application слоя. Например, класс контроллера предоставляет эндпоинты API для клиентов приложения, но сам контроллер не занимается логикой обработки - он формирует входные точки и их маршрутизацию. Обработка происходит в слое Application.  

Чтобы привести наше приложение в такой вид, сделаем следующее.  
Presentation слой в лице `EShop.Presentation\Controllers\ProductsController.cs` собирается использовать Application слой в лице `EShop.Application\ProductHandler.cs`, значит добавим зависимость сборке `EShop.Presentation` от `EShop.Application`.  

Добавим в `EShop.Presentation\Controllers\ProductsController.cs` нужный `using`.  
Попросим в методе контроллера обработчик совершить работу для контроллера. Для этого создадим экземпляр обработчика в коде контроллера, написав код `var handler = new ProductHandler()`. Увидим, что анализатор на что-то ругается. В этот раз проблема в том, что у обработчика по умолчанию установился модификатор доступа `internal` - это значит, что он доступен только в пределах сборки, в которой он объявлен.  

![](attachments/Pasted%20image%2020250131174305.png)

Заменим `internal` на `public`. И увидим, что теперь мы можем использовать `ProductHandler` в коде контроллера.  

![](attachments/Pasted%20image%2020250131174708.png)

# DTO
Сейчас метод контроллера возвращает `Product`.  Который является доменной сущностью.
Обычно внешние слои, такие как `Presentation`, не отдают клиентам доменные сущности, а возвращают специальные модели данных, такие как DTO, например.  
[INFO DTO](INFO/DTO.md)  

Мы хотим сделать также и возвращать в контроллере DTO.  
Какой слой должен конвертировать доменную сущность в возвращаемый DTO? Подходы могут разниться.  
Сделаем так, что `application` слой будет возвращать доменную сущность, а `presentation` слой будет заниматься конвертацией из доменной сущности в DTO.  
[INFO Где конвертировать доменные сущности](INFO/Где%20конвертировать%20доменные%20сущности.md)  

Создадим новый класс `ProductDto` в корне `EShop.Presentation`.  
Заменим тип `class` на `record`.  
[INFO Record](INFO/Record.md)  

Добавим свойства `ProductDto`:  
```csharp
public record ProductDto
{
    public decimal Price { get; set; }
    public string Name { get; set; }
}
```

Обратим внимание, что мы решили скрыть от клиентов свойство `Id`, которое присутствует в `Product`.  

Вернемся в контроллер и заменим возвращаемый методом тип с `Product` на `ProductDto`.  
Теперь нужно добавить конвертацию из `Product` в `ProductDto`.  
Напишем код получения списка `Product` из обработчика и конвертации списка доменных сущностей в DTO.  

```csharp
[HttpGet]
public IEnumerable<ProductDto> Get()
{
    var productHandler = new ProductHandler();
    var products = productHandler.Get();

    var listOfDto = new List<ProductDto>();
    foreach (var product in products)
        listOfDto.Add(new ProductDto { Name = product.Name, Price = product.Price });

    return listOfDto;
}
```

Проверим, что всё работает.  

![](attachments/Pasted%20image%2020250131182419.png)

Видим, что `Id` нет в возвращаемом контроллером списке `Product`.  
Мы разделили логику по слоям.  

# Транзитивные зависимости

Обратим внимание, что в сборке `EShop.Application` мы используем тип `Product` из `EShop.Domain`.  

![](attachments/Pasted%20image%2020250131203913.png)

При этом `EShop.Presentation` не ссылается прямо на `EShop.Domain`. Мы не добавляли ссылку на него через диспетчер ссылок.  
Ссылка есть только на `EShop.Application`.  

![](attachments/Pasted%20image%2020250131203604.png)

Такое поведение обеспечено механизмом "Транзитивных зависимостей".  
Если проект `EShop.Application` ссылается на `EShop.Domain`, а `EShop.Presentation` ссылается на `EShop.Application`, то `EShop.Presentation` автоматически получает доступ к типам из `EShop.Domain` через `EShop.Application` (если они используются в публичном API `EShop.Application`).  

Как это можно отследить в IDE:  

![](attachments/Pasted%20image%2020250131204127.png)

Разворачиваем "Зависимости" в проектах и видим:  
- `EShop.Application` зависит от `EShop.Domain` - это прямая зависимость.  
- `EShop.Domain` не зависит от других сборок.  
- `EShop.Presentation` зависит от сборки `EShop.Application`, которая зависит от `EShop.Domain`.  

В 3 видим, что `EShop.Presentation` может "добраться" до сборки `EShop.Domain` через `EShop.Application`.  

# Работа с параметрами HTTP-запроса

Представим, что мы хотим добавить фильтрацию по цене - чтобы нам отображались только товары с ценой меньше либо равной значению параметра.  
Хотим добавить сортировку - чтобы товары отображались по возрастанию или по убыванию цены. Значение параметра сортировки будет принимать строковые значения - "asc" отображать товары по возрастанию цены, "desc" отображать товары по убыванию цены.  

URL обращения на эндпоинт с использованием параметров будет выглядеть следующим образом:  
```
GET /api/product?priceFilter=<value>&priceSortOrder=<value>.
```

Для сопоставления будем использовать механизм Model Binding из ASP.NET Core.  
[INFO Model binding](INFO/Model%20binding%20ASP.NET%20Core.md)  

Добавляем в параметры метода следующий код:  
```csharp
[FromQuery] decimal? priceFilter, [FromQuery] string? priceSortOrder
```

`[FromQuery] decimal? priceFilter` - дает указание искать в строке запроса параметр с типом число и названием `priceFilter`.  

Видим в Swagger, что у нас появилась возможность использовать параметры для нашего запроса GET.  

![](attachments/Pasted%20image%2020250131205038.png)

Дописываем обработку фильтра и сортировку:  
```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IEnumerable<ProductDto> Get([FromQuery] decimal? priceFilter, [FromQuery] string? priceSortOrder)   
    {
        var catalogHandler = new CatalogHandler();
        var products = catalogHandler.GetItems();

        if (priceFilter is not null)
            products = products.Where(p => p.Price <= priceFilter);

        if (!string.IsNullOrEmpty(priceSortOrder))
        {
            products = priceSortOrder.ToLower() == "desc"
                ? products.OrderByDescending(p => p.Price)
                : products.OrderBy(p => p.Price);
        }

        var listOfDto = new List<ProductDto>();
        foreach (var product in products)
            listOfDto.Add(new ProductDto { Name = product.Name, Price = product.Price });

        return listOfDto;
    }
}
```

# Члены класса контроллера

https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-8.0  
https://learn.microsoft.com/ru-ru/dotnet/api/microsoft.aspnetcore.http.httprequest?view=aspnetcore-8.0  

Поставим брейкпоинт в коде контроллера.  
Запустим приложение в режиме отладки и выполним метод в Swagger.  

![](attachments/Pasted%20image%2020250131211247.png)

Обратимся к локальным видимым переменным.  
`this` - это текущий экземпляр класса, в котором мы находимся. Мы остановили код в методе класса `ProductsController`, т.е. в члене класса контроллера, и сейчас `this` отображает доступный нам контекст этого экземпляра.  

Для того чтобы посмотреть свойства доступных переменных, воспользуемся инструментом "Быстрая проверка".  

![](attachments/Pasted%20image%2020250131213155.png)

В открывшемся окне вводим `this` в поле `Выражение` и нажимаем кнопку `Пересчитать`.  

![](attachments/Pasted%20image%2020250131213345.png)  
![](attachments/Pasted%20image%2020250131213459.png)

Видим члены текущего экземпляра, в котором мы стоим в точке останова.  
Это свойства, которые класс `ProductsController`, созданный нами, получил из-за наследования от класса `ControllerBase`.  

Сейчас нас интересует член `Request` и его члены: `Query` и `QueryString`.  
- `Request` - представляет входящую сторону отдельного HTTP-запроса.  
- `Query` - возвращает коллекцию значений запроса, проанализированную из `Request.QueryString`.  
- `QueryString` - Возвращает или задает необработанную строку запроса, используемую для создания коллекции запросов в `Request.Query`.  

Мы видим, что у нас эти свойства сейчас пустые. Это верно, т.к. мы не указали никаких параметров в запросе в Swagger.  
Отпустим отладку. Заполним параметр `priceFilter` в Swagger и выполним запрос снова.  
Видим, что свойство заполнено.  
![](attachments/Pasted%20image%2020250131214359.png)