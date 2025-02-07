---
created: 2025-02-03-14-56-16+10:00
tags:
---
# Переносим данные в JSON файл
Создадим файл `Products.json`  
![](attachments/Pasted%20image%2020250203153108.png)  
В JSON массив наших товаров будет представлен в виде массива объектов:  
```json
[
    {
        "Id": 1,
        "Name": "Iphone",
        "Price": 333
    },
    {
        "Id": 2,
        "Name": "Iphone1",
        "Price": 444
    },
    {
        "Id": 3,
        "Name": "Iphone2",
        "Price": 555
    },
    {
        "Id": 4,
        "Name": "Iphone3",
        "Price": 666
    },
    {
        "Id": 5,
        "Name": "Iphone4",
        "Price": 777
    }
]
```  
[INFO JSON объекты и массивы](INFO/JSON%20объекты%20и%20массивы.md)  
Для того, чтобы файл который не является исходным кодом, при сборке и копировании включался в проект нужно сделать его частью проекта.  
1. ПКМ на файл `Products.json`  
2. Посмотрим содержимое нашего файла  
3. Установим свойства для файла:  
	- Действие при сборке: `Содержимое`  
	- Копировать более позднюю версию  
4. Обратим внимание как настройки повлияли на файл `.csproj` родительского проекта  
![](attachments/Pasted%20image%2020250203160057.png)  
Посмотрим на что влияют выполненные настройки.  
Выполним очистку решения  
![](attachments/Pasted%20image%2020250203154440.png)  
Перейдем в папку решения в проводнике.  
[INFO bin&obj](INFO/bin&obj.md)  
Нас интересует bin исполняемой сборки.  
Как понять какая сборка является исполняемой:  
![](attachments/Pasted%20image%2020250203160346.png)  
1. Можно посмотреть в свойствах решения  
2. Видим что у нас это EShop.Presentation  
3. Также исполняемая сборка выделена жирным в обозревателе решений  
Видим, что после очистки решения папка `\EShop\EShop.Presentation\bin\Debug\net8.0` пустая:  
![](attachments/Pasted%20image%2020250203160152.png)  
Выполним сборку  
![](attachments/Pasted%20image%2020250203154614.png)  
Проверим папку bin - видим, что файл `Products.json` был скопирован при сборке из исходников, причем в исходниках файл располагается в Application, но из-за использования ресурса он был скопирован в папку исполняемой сборки.  
Мы не будем уделять этому много внимания т.к. работа с ресурсами редко пригождается в веб разработке.  
Смысл в том, чтобы не использовать абсолютные пути при использовании ресурсов, а ссылаться в коде на относительные пути, например от корневой папки исполняемого приложения - так код более устойчив.  
![](attachments/Pasted%20image%2020250203160553.png)  
## Убираем товары из памяти  
В `EShop\EShop.Application\ProductHandler.cs` удаляем поле:  
![](attachments/Pasted%20image%2020250203154909.png)  
И добавляем в метод получения код для получения данных из JSON файла.  
```csharp
var assemblyLocation = Assembly.GetExecutingAssembly().Location;
var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)!;
var JsonFilePath = Path.Combine(assemblyDirectory, "Products.json");
var json = File.ReadAllText(JsonFilePath);
var products = JsonSerializer.Deserialize<Product[]>(json);
```  
1. Получаем расположение исполняемой сборки  
2. Получаем путь к исполняемой сборке  
3. Комбинируем путь к файлу из строковых литералов, используем `Path.Combine ` вместо конкатенации, он позволяет избежать проблем в разных операционных системах, связанных с разделителем директорий.  
4. Читаем содержимое файла `Products.json`  
5. Десериализуем содержимое файла в объект рантайма т.е. в массив товаров `Product`  
![](attachments/Pasted%20image%2020250203162524.png)  
[INFO Десериализация JSON](INFO/Десериализация%20JSON.md)  
Сейчас у нас получается упрощенная аналогия персистентного хранилища для данных. Данные отделены от приложения - сейчас с ними могут работать несколько приложений или можно работать с ними напрямую не запуская наше приложение - например, поправить в ручную в VSCode.  
Запустим приложение.  
Дергаем метод API.  
![](attachments/Pasted%20image%2020250203164054.png)  
Откроем файл `"..\EShop\EShop.Presentation\bin\Debug\net8.0\Products.json"` и удалим один элемент  
![](attachments/Pasted%20image%2020250203163612.png)  
Снова вызовем метод  
![](attachments/Pasted%20image%2020250203164210.png)  
Видим что количество товаров также изменилось.  
# EShop.DAL
[INFO Слоистая архитектура](INFO/Слоистая%20архитектура.md)  
Как мы ранее изучили - непосредственная работа с данными должна быть выделена в отдельный слой.  
У нас сейчас работа с данными - это код который получает путь до файла `Products.json` и десериализует его.  
Попробуем вынести этот код в отдельную сборку которая будет скрывать от нас подробности поиска файла и десерилизации, и будет предоставлять в коде приложения уже готовые объекты.  
[INFO DAL (Data Access Layer)](INFO/DAL%20(Data%20Access%20Layer).md)  
Добавим новый проект   
`EShop.DAL`  
Тип проекта - `Библиотека классов`  
![](attachments/Pasted%20image%2020250203151641.png)  
Удалим `Class1.cs`, созданный шаблоном.  
В коре EShop.DAL добавим класс в который перенесем подробности получения `Product`  
Назовем класс `ProductRepository`  
![](attachments/Pasted%20image%2020250203171640.png)  
Заменим ему модификатор доступа на `public`  
Этот класс будет реализовывать паттерн Repository  
 [INFO Паттерн Repository](INFO/Паттерн%20Repository.md)  
 Полностью перенесем метод получения товаров из `ProductHandler`  
 ![](attachments/Pasted%20image%2020250203172103.png)  
 Видим что анализатор ругается. Добавим слою DAL зависимость от слоя Domain. Проигнорируем предложение `InteliSense` и сделаем это вручную  
 ![](attachments/Pasted%20image%2020250203172311.png)  
Как мы изучили ранее - ссылки не достаточно и нужно добавить `using`  
В этот раз воспользуемся предложением `InteliSense` и позволим ему сделать работу за нас  
Теперь у нас есть отдельный класс который куда-то ходит за данными и возвращает нам объекты.   
 ![](attachments/Pasted%20image%2020250203172437.png)  
 Вернёмся к `EShop.Application\ProductHandler.cs` - удалим в нем код, который мы вынесли в DAL. И вызовем наш `EShop.DAL\ProductRepository.cs`.
 Мы не можем создать экземпляр `ProductRepository` в `ProductHandler`, т.к. у `Application` отсутствует зависимость от `DAL` - добавим её.
 ![](attachments/Pasted%20image%2020250203172858.png)  
 Теперь слой `Application` зависит от `Domain` и `DAL` - и это не очень хорошо. Но сейчас оставим так, совсем скоро мы решим эту проблему - когда посмотрим почему это не удобно.  
 Перепишем код `ProductHandler`  
```csharp
namespace EShop.Application
{
    public class ProductHandler
    {
        public IEnumerable<Product> Get()
        {
            var productsRepository = new ProductRepository();
            var products = productsRepository.Get();

            return products;
        }
    }
}
```  
Проверим, что всё работает.  
Если оставить приведённый код в его текущем виде, он легко может привести к ряду проблем, особенно по мере роста проекта. Вот подробные примеры проблем с использованием конкретного кода и описание их последствий.  

## Жесткая связь с ProductRepository  
### 1. **Трудности с тестированием**  
Проблема этого заключается в жесткой зависимости `ProductHandler` от конкретного класса `ProductRepository`. Будет сложно или даже невозможно протестировать его изолированно, так как он всегда создаёт экземпляр `ProductRepository` внутри себя.  
Мы не сможем заменить реальную реализацию `ProductRepository`, например на, мок-объект (mock) для проверки поведения `ProductHandler` в изоляции.  
`ProductHandler` невозможно протестировать без фактического подключения базы данных. Это нарушает принцип unit-тестирования.  
**Пример неудачного теста:**  
```csharp
[TestClass]
public class ProductHandlerTests
{
    [TestMethod]
    public void Get_ShouldReturnProducts()
    {
        // Arrange
        var handler = new ProductHandler();
        
        // Act
        var products = handler.Get();  // Этот вызов потребует настоящей реализации базы данных!

        // Assert
        Assert.IsNotNull(products);
        // Проблема: тест зависит от базы данных, а не только от логики Get().
    }
}
```  
**Почему это проблема?**  
- Если база данных недоступна во время тестов, тесты будут падать, даже если сам `ProductHandler` работает корректно.  
- Настройка тестовой среды становится сложной, так как потребуется создавать реальные данные в базе данных.  
- Процесс тестирования становится медленнее из-за обращения к базе данных.  
---

### 2. **Сложности с изменением логики репозитория**  
Если потребуется заменить `ProductRepository` на другую реализацию (например, для работы с другой базой данных, с кешем или вообще с REST API), изменение потребует редактирования класса `ProductHandler`. А это нарушает принцип **"открытости/закрытости" (O из SOLID)**.  
Чтобы изменить логику работы с репозиторием, нужно будет:  
1. Изменить тело метода `Get()`.  
2. Добавить дополнительные проверки, условия или замены.  
Допустим, мы решаем хранить часть данных в кэше:  
```csharp
public IEnumerable<Product> Get()
{
    var productsRepository = new ProductRepository();

    // Если понадобится использовать кеш
    if (UseCache)
    {
        // Это приведет к необходимости добавить логику кеша прямо в метод
        if (Cache.Exists("Products"))
        {
            return Cache.Get<IEnumerable<Product>>("Products");
        }
    }

    var products = productsRepository.Get();

    // Кешируем полученные данные для повторного использования
    Cache.Set("Products", products);

    return products;
}
```  
**Почему это проблема?**  
- Класс `ProductHandler` начинает содержать ответственность за работу с кэшем, что нарушает принцип **единственной ответственности (S)**. Теперь это не просто обработчик бизнес-логики, а ещё и менеджер кеша.  
- Код становится больше, сложнее, менее читаемым и менее поддерживаемым.  

---
### 3. **Сложности при добавлении новых репозиториев**  
Если через месяц нам потребуется добавить альтернативный репозиторий, например, `ExternalApiProductRepository` для данных, которые поступают от внешнего API, то придется переписать метод `Get()`.  
```csharp
public IEnumerable<Product> Get(string source)
{
    if (source == "Database")
    {
        var productsRepository = new ProductRepository();
        return productsRepository.Get();
    }
    else if (source == "ExternalApi")
    {
        var apiRepository = new ExternalApiProductRepository();
        return apiRepository.Fetch();
    }

    throw new ArgumentException("Invalid source");
}
```  
**Почему это плохо?**  
- Логика выбора репозитория привязывается к `ProductHandler`, вместо того чтобы делегироваться. Это нарушает принцип **единственной ответственности (S)**.
- Подобная реализация может легко масштабироваться до трудноуправляемого и запутанного кода, если понадобится добавить третье, четвёртое или пятое условие.

---
### 4. **Проблемы с расширяемостью**  
Если бизнес-логика изменится, и нужно будет добавить фильтрацию или преобразование возвращаемых продуктов, то текущий дизайн вынудит вносить правки прямо в класс `ProductHandler`.  
Пример плохо масштабируемого кода:  
```csharp
public IEnumerable<Product> Get(bool includeInactive)
{
    var productsRepository = new ProductRepository();
    var products = productsRepository.Get();

    // Бизнес-логика реализации фильтрации
    if (!includeInactive)
    {
        products = products.Where(p => p.IsActive).ToList();
    }

    return products;
}
```  
**Проблема:**  
- Возникает "шумящий" код, где бизнес-логика (например, фильтрация по `IsActive`) находится вперемешку с вызовом репозитория.  
- При необходимости добавлять новые фильтры (по категориям, по дате и т. д.) код будет становиться всё более сложным.  

---
### 5. **Тесная связанность компонентов**  
Когда `ProductHandler` сам создаёт экземпляр `ProductRepository`, это приводит к жесткой связанности этих классов. В конечном итоге любое изменение в репозитории может затронуть обработчик.  
Например, если вы решите изменить зависимость `ProductRepository` так, чтобы она принимала настройки из конфигурации (например, строку подключения), то это приведёт к необходимости изменения в коде `ProductHandler`.  
**Что это означает?**  
- Подобное изменение нарушает принцип **разделения ответственности (SRP)**.  
- Проект становится сложнее сопровождать, так как каждый класс знает слишком много о других.  

---
### 6. **Потенциальные проблемы при использовании Dependency Injection**  
Многие современные фреймворки (например, ASP.NET Core) предполагают использование внедрения зависимостей (Dependency Injection, DI). Однако текущий код не позволяет внедрить репозиторий как зависимость, так как `ProductRepository` создаётся внутри метода `Get()`.  
**Почему это важно?**  
- Если проект будет использовать DI-фреймворк, текущий подход сделает его интеграцию проблематичной.  
- Вам придётся вносить серьёзные изменения в код позднее, что увеличит стоимость разработки.  

---
## Ослабляем связанность с ProductRepository  
 Чтобы ослабить связь `ProductHandler` c `ProductRepository`.  
 Для начала определим контракт для `ProductRepository`  
 Создадим файл `IProductRepository` в сборке `EShop.Domain`  
 ![](attachments/Pasted%20image%2020250203180440.png)  
 Заменим тип `class` на `interface` и подправим модификатор доступа на `public`  
 ![](attachments/Pasted%20image%2020250203180636.png)  
 ![](attachments/Pasted%20image%2020250203180642.png)  
 Нам нужно сейчас только одно поведение репозитория - опишем его в контракте.  
 ![](attachments/Pasted%20image%2020250203180831.png)  
Мы описали контракт - `IProductRepository`. Класс, который будет имплементировать (реализовывать) этот интерфейс - обязан содержать метод с сигнатурой  `public IEnumerable<Product> Get();`  
Объявим, что `ProductRepository` обязуется выполнять контракт `IProductRepository`. Если в описание `IProductRepository` добавится метод, `ProductRepository` - будет обязан его реализовывать.  
Добавим в описание `ProductRepository` код  
![](attachments/Pasted%20image%2020250203181405.png)  
Теперь нам нужно сделать, так чтобы `ProductHandler` не использовал код `new ProductRepository();`  
Сделать это можно несколькими способами, мы же будем прокидывать его через конструктор.   
Добавим в `EShop.Application\ProductHandler.cs` - поле  
```csharp
private readonly IProductRepository _productRepository;
```  
Добавим параметризованный конструктор класса `ProductHandler`  
```csharp
public ProductHandler(IProductRepository productRepository)
{
    _productRepository = productRepository;
}
```  
В коде `ProductHandler` метода `Get()` заменим жесткое связывание на использование поля этого класса:  
```csharp
public IEnumerable<Product> Get()
{
    var products = _productRepository.Get();
    return products;
}
```  
![](attachments/Pasted%20image%2020250203225331.png)  
Поле `_productRepository` класса `ProductHandler` в данном случае является его зависимостью. Обычно такие поля так и называют зависимость класса n.  
В такой схеме мы предполагаем, что  при инстанциировании `ProductHandler` - вызывающий код будет иметь экземпляр реализации `IProductRepository` который и будет использоваться при вызове конструктора.  
### Создание экземпляров `ProductHandler`  
Теперь нужно найти места использования `ProductHandler` и поправить создание его экземпляров. Т.к. проект у нас совсем маленький - у нас такое место только одно, это класс `ProductsController`.  
Перепишем создание экземпляра хэндлера с помощью нового конструктора.  
Напишем такой код в контроллере:  
```csharp
var productRepository = new ProductRepository();
var productHandler = new ProductHandler(productRepository);
```  
![](attachments/Pasted%20image%2020250203223339.png)  
Остановимся и попробуем осмыслить - какую выгоду мы получили от проделанных действий?  
Теперь мы можем использовать разные реализации `IProductRepository` при создании экземпляров `ProductHandler`. Из-за того, что реализацию зависимости `_productRepository` выбирает вызывающий код - т.е. тот который инстанцирует `ProductHandler` через `new() ` - мы можем использовать разные инфраструктурные решения не трогая код самого `ProductHandler`.  
На самом деле, можно и нужно избавиться от жесткой связки и в контроллере. И мы этим займемся, но позже - сейчас попробуем рассмотреть возможности которые нам открывает использование интерфейсов.  
## Разные реализации `IProductRepository`  
Решим такую задачу. API должен иметь еще один отдельный метод с отдельным эндпоинтом GET api/products/GetById, с одним параметром типа int - id товара. При этом товары хранятся не в JSON файле, а в памяти как было у нас вначале.  
Для этого нам понадобится:  
- сделать другую реализацию `IProductRepository`  
- сделать отдельный роут для метода  
- код в обработчике  
### Класс InMemoryProductRepository  
В сборке DAL создадим новый класс `InMemoryProductRepository`  
Укажем что он реализует интерфейс `IProductRepository`  
```csharp
public class InMemoryProductRepository: IProductRepository
{
    
}
```  
Увидим, что анализатор ругается, что наш новый класс не выполняет контракт - в нём нет метода `Get()`.  
Примем предложение `InteliSense` добавить код за нас.  
На самом деле реальной реализации за нас `InteliSense` не сделал - он ведь не может знать, что мы хотим реализовать, он просто подставил плейсхолдер, чтобы контракт формально выполнялся, в реальности при вызове этого метода будет возникать ошибка. Сейчас нас это устраивает  
![](attachments/Pasted%20image%2020250203232002.png)  
### `ProductHandler`  
Добавим новый метод в обработчик:  
```cs
public IEnumerable<Product> GetById(int id)
{
    var listOfProducts = new List<Product>();
    var products = _productRepository.Get();
    foreach (var product in products) {
        if (product.Id == id)
        {
            listOfProducts.Add(product);
            return listOfProducts;
        }
    }
    return listOfProducts;
}
```  
![](attachments/Pasted%20image%2020250203233208.png)  
### Метод GetById  
В `ProductsController` создадим новый метод  
```csharp
 [HttpGet]
 public IEnumerable<ProductDto> GetById([FromQuery] int id)
 {
     var productRepository = new InMemoryProductRepository();
     var handler = new ProductHandler(productRepository);
     var products = handler.GetById(id);
     
     var listOfDto = new List<ProductDto>();
     foreach (var product in products)
         listOfDto.Add(new ProductDto { Name = product.Name, Price = product.Price });

     return listOfDto;
 }  
```
И добавим аттрибуты методам контроллера, чтобы могли быть размечены новые роуты т.к. у нас появляется новый GET метод  
![](attachments/Pasted%20image%2020250203233850.png)  
Добавим брикпоинт в `InMemoryProductRepository`  
Перейдем в Swagger и вызовем новый метод  
![](attachments/Pasted%20image%2020250204092253.png)  
Попали в брикпоинт с вызовом исключения  
![](attachments/Pasted%20image%2020250204092317.png)  
Отпустим отладку и посмотрим как исключение будет отражено в Swagger  
![](attachments/Pasted%20image%2020250204092335.png)  
Избавимся от исключения в `InMemoryProductRepository` и добавим логику  
Добавим массив продуктов в эту реализацию репозитория, как у нас было в начале  
```csharp
private Product[] products = [
    new Product { Id = 1, Name = "Iphone", Price = 333 },
    new Product { Id = 2, Name = "Iphone1", Price = 444 },
    new Product { Id = 3, Name = "Iphone2", Price = 555 },
    new Product { Id = 4, Name = "Iphone3", Price = 666 },
    new Product { Id = 5, Name = "Iphone4", Price = 777 }
];
```  
![](attachments/Pasted%20image%2020250204093733.png)  
Запустим приложение и вызовем новый метод с параметром  
![](attachments/Pasted%20image%2020250204093834.png)  
Сейчас у нас есть две реализации `IProductRepository`:  
1. `InMemoryProductRepository` - который хранит данные в оперативной памяти и данные существуют только пока существует экземпляр класса  
2. `ProductRepository` - который хранит данные на жестком диске в файле JSON.  
Название `ProductRepository` сейчас не отражает действительность - изменим ему имя на `JSONProductRepository`. Рассмотрим на этом примере средства рефакторинга, которые нам предоставляет IDE.  
Перейдем в файл `EShop.DAL\ProductRepository.cs` выделим декларацию класса и выберем в контекстном окне "Переименовать" (Ctrl + R, Ctrl + R, два раза подряд надо нажать эту комбинацию)  
![](attachments/Pasted%20image%2020250204095748.png)  
отобразится меню рефакторинга  
![](attachments/Pasted%20image%2020250204095800.png)  
Добавим Json в наименование, и обратим внимание на 3 - до рефакторинга, имя файла - старое  
![](attachments/Pasted%20image%2020250204095922.png)  
нажмем ENTER, после этого имя в ссылках в коде было изменено, и имя файла также было изменено.  
![](attachments/Pasted%20image%2020250204100048.png)Запустим, проверим в Swagger, что всё работает.  
Удалим `InMemoryProductRepository` - он нам больше не понадобится.  
Заменим в методе контроллера `GetById() ` использование репо на `JsonProductRepository`.  
![](attachments/Pasted%20image%2020250204101105.png)  
Проверим в Swagger что код работает  