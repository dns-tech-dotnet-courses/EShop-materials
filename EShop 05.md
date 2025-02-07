---
created: 2025-02-04-22-07-30+10:00
tags:
---
# Npgsql
Для работы с PostgreSQL из нашего приложения воспользуемся пакетом Npgsql  
В решениях которые содержат несколько проектов, пакеты могут быть установлены только к конкретным сборкам    
Мы хотим установить пакет Npgsql в сборку DAL. Активируем её в обозревателе решений  
И в меню "Проект" откроем менеджер NuGet пакетов  
![](attachments/Pasted%20image%2020250204221624.png)  
Перейдем на вкладку "Обзор"  
Введем "npgsql"  
Выберем нужный нам пакет  
Нажмем установить  
![](attachments/Pasted%20image%2020250204221700.png)  
Подтвердим установку  
![](attachments/Pasted%20image%2020250204221829.png)  
Видим что пакет установился  
![](attachments/Pasted%20image%2020250204222101.png)  
![](attachments/Pasted%20image%2020250205154905.png)  
# Connection string
Для соединения с базой данных добавим в конфигурацию нашего приложения connection string  
[INFO Connections string](INFO/Connections%20string.md)  
В файл `appsetting.json` исполняемой сборки добавим секцию конфигурации
```json
"ConnectionStrings": {
    "PostgresConnection": "Host=localhost;Port=5432;Database=eshop;Username=postgres;Password=password"
}
```
Чтобы сформировать строку с нужными нам параметрами обратимся к файлу `docker-compose.yml` 
![](attachments/Pasted%20image%2020250204222711.png)  
# Наполним таблицу products в бд
Запустим наши контейнеры в докере  
Откроем PowerShell в VisualStudio  
![](attachments/Pasted%20image%2020250205160631.png)  
Введем команду
```shell
docker-compose up -d
```
Откроем pgAdmin 
![](attachments/Pasted%20image%2020250205161352.png)  
Создадим скрипт для вставки данных в таблицу products  
![](attachments/Pasted%20image%2020250205161645.png)  
Подправим шаблон
```sql
INSERT INTO public.products(id, name, price) VALUES 
(1, 'iPhone 13 Pro Max', 999.99),
(2, 'Samsung Galaxy S21', 899.99),
(3, 'Google Pixel 6', 799.99),
(4, 'OnePlus 9 Pro', 869.99),
(5, 'Xiaomi Mi 11', 749.99),
(6, 'iPhone 13', 799.99),
(7, 'Samsung Galaxy A52', 499.99),
(8, 'Huawei P40 Pro', 899.99),
(9, 'Sony Xperia 1 III', 1099.99),
(10, 'ASUS ROG Phone 5', 999.99);
```
Выполним скрипт вставки  
Проверим что данные есть в таблице  
![](attachments/Pasted%20image%2020250205162035.png) 
# DbProductRepository
Создадим класс который позволит нам работать с таблицей товаров в БД  
В сборке `DAL` добавим новый класс `DbProductRepository`. Укажем что `DbProductRepository` имплементирует `IProductRepository`
```csharp
using EShop.Domain;

namespace EShop.DAL
{
    public class DbProductRepository : IProductRepository
    {
        public IEnumerable<Product> Get()
        {
            throw new NotImplementedException();
        }
    }
}
```
Наш класс будет устанавливать соединение с БД и отправлять в нее запросы  
Чтобы установить соединение с БД классу необходимо знать как к ней обратится - для этого и нужна будет [Connections string](INFO/Connections%20string.md)  
Добавим в наш класс поле 
```csharp
private readonly string _connectionString;
```
И создадим конструктор класса в который будем прокидывать новый для нас тип зависимости `IConfiguration`  
[INFO IConfiguration](INFO/IConfiguration.md)  
```csharp
public DbProductRepository(IConfiguration configuration)
{
    
}
```
Видим, что не достает зависимости содержащей описание типа `IConfiguration`  
![](attachments/Pasted%20image%2020250207095231.png)
Примем предложение установить и добавить `using` для `Microsoft.Extensions.Configuration.Abstractions`  
В конструкторе допишем код, который позволит подтянуть строку соединения из конфигурационных файлов приложения  
Добавим null-forgiving оператор "!" в конце строки. Таким образом мы говорим анализатору - спасибо, что предупредил, что здесь может быть NRE, но мы уверены что его здесь не будет - не предупреждай нас в этом месте.
```csharp
_connectionString = configuration.GetConnectionString("PostgresConnection")!;
```
  ![](attachments/Pasted%20image%2020250207095446.png)
## Замена зависимости в IoC
Перeйдем в `Program`  
Заменим реализацию для `IProductRepository`  на `DbProductRepository`  
![](attachments/Pasted%20image%2020250205165446.png)  
## Удаление старого репозитория
Удалим класс `JsonProductRepository` из `EShop.DAL` и файл `Products.json` из `EShop.Application` 
Убедимся что после удаления `Products.json` в файле `.csproj` не осталось мусора.
Откроем файл проекта  
Кликнем по красной стрелке и посмотрим локальные изменения кода  
Видим, что при удалении файла в обозревателе решений `.csproj` был корректно подчищен   
![](attachments/Pasted%20image%2020250205170014.png)  
Вернемся в `DbProductRepository`, поставим брикпоинт внутри конструктора и запустим приложение  
Запустим любой из наших методов в Swagger и вернемся в IDE  
Воспользуемся "Отладка"/"Быстрая проверка" и рассмотрим переменную `configuration`  
![](attachments/Pasted%20image%2020250205164831.png)    
Видим что в переменной существует 119 секций с разными настройками, нас интересует `ConnectionString` видим, что есть значение которое мы добавили в `appsettings.json`.
Обратим внимание на свойство `Provider` - `JsonConfigurationProvider`  
[INFO JsonConfigurationProvider](INFO/JsonConfigurationProvider.md)  
Отпустим отладчик  
Увидим что возникла ошибка  
![](attachments/Pasted%20image%2020250205165213.png)  
Это корректно т.к. у нас отсутствует реализация метода `Get` в новом репозитории  
Напишем код  
```csharp
var products = new List<Product>();
var connection = new NpgsqlConnection(_connectionString);
connection.Open();
var command = new NpgsqlCommand("SELECT id, name, price::numeric FROM products", connection);
var reader = command.ExecuteReader();
```
Разберем каждую строку кода:
```csharp
var products = new List<Product>();
```
Создается новый пустой список объектов типа Product, куда будут складываться все продукты, полученные из базы данных.
```csharp
var connection = new NpgsqlConnection(_connectionString);
```
Создается новый объект подключения к PostgreSQL базе данных. 
```csharp
connection.Open();
```
Открывается соединение с базой данных.
```csharp
var command = new NpgsqlCommand("SELECT id, name, price::numeric FROM products", connection);
```
Создается SQL-команда, которая будет выполняться в базе данных:
- `SELECT id, name, price::numeric FROM products` - это SQL-запрос
- price::numeric преобразует поле price из типа money в numeric
- connection - указывает, через какое соединение выполнять запрос
```csharp
var reader = command.ExecuteReader();
```
Выполняется команда и создается объект reader, который позволяет построчно читать результаты запроса. `ExecuteReader()` используется для запросов, которые возвращают набор данных (как SELECT).
Перейдем в pgAdmin и проверим запрос который мы используем в коде    
![](attachments/Pasted%20image%2020250205171943.png)  
Запрос корректный  
## NpgsqlDataReader
Посмотрим какой тип вернёт нам запрос в БД  
![](attachments/Pasted%20image%2020250207102247.png)Пока не вполне понятно, что это  
Добавим в метод код-заглушку    
```csharp
return null;
```
И установим в этой строке брикпоинт  
Запустим приложение, выполним в `Swagger` любой из наших методов и перейдем в IDE  
Раскроем панель переменных и найдем там `reader`, всё еще сложно понять что содержит эта переменная которая должна нам возвращать какой-то результат запроса в БД  
Развернем "Представление результатов". Видим в нем 10 членов - похоже на 10 товаров которые мы добавили в таблицу  
Пойдем в глубь пока не дойдем до члена \_values  
![](attachments/Pasted%20image%2020250205173143.png)  
Видим, что это значения колонок из нашей таблицы  
Справка говорит, что  
`NpgsqlDataReader представляет собой однонаправленный поток записей из базы данных PostgreSQL. Он позволяет последовательно читать результаты SQL-запроса строка за строкой`  
Попробуем как-то наглядно "увидеть" что же это такое  
Добавим такой код  
```csharp
while (await reader.ReadAsync())
{
    for (int i = 0; i < reader.FieldCount; i++)
    {
        Console.WriteLine($"{reader.GetName(i)}: {reader[i]}");
    }
}
```
Этот код будет при проходе по каждой строке из потока записей БД, выводить имя и значение каждой колонки  
Поставим брикпоинт в возврате метода  
Запустим приложение, вызовем в Swagger любой из методов  
Откроем консоль которая запускается при отладке приложения  
![](attachments/Pasted%20image%2020250207102542.png)  
Посмотрим на результаты консольного логирования  
Это должно позволить лучше понять что возвращается из БД  
## Маппинг 
Продолжим доработки  
Заменим отладочный код таким кодом  
```csharp
while (reader.Read())
{
    var product = new Product
    {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Price = reader.GetDecimal(2)
    };
    products.Add(product);
}
```
Давайте разберем этот фрагмент кода построчно:
```csharp
while (reader.Read())
```
Это цикл, который читает каждую строку результата запроса из базы данных. `reader.Read()` перемещает курсор на следующую строку и возвращает `true`, если строка существует, и `false`, когда все строки прочитаны.

```csharp
var product = new Product
{
    Id = reader.GetInt32(0),
    Name = reader.GetString(1),
    Price = reader.GetDecimal(2)
};
```
Здесь для каждой строки создается новый объект класса Product и заполняется данными:
- `reader.GetInt32(0)` читает первую колонку (индекс 0) как целое число (ID продукта)
- `reader.GetString(1)` читает вторую колонку (индекс 1) как строку (название продукта)
- `reader.GetDecimal(2)` читает третью колонку (индекс 2) как десятичное число (цена продукта)

```csharp
products.Add(product);
```
Созданный объект продукта добавляется в список products, который был создан в начале метода.
## Результат
Результирующий код репозитория  
```csharp
using EShop.Domain;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EShop.DAL
{
    public class DbProductRepository : IProductRepository
    {
        private readonly string _connectionString;
        public DbProductRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PostgresConnection");
        }
        public async Task<IEnumerable<Product>> Get()
        {
            var products = new List<Product>();
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            var command = new NpgsqlCommand("SELECT id, name, price::numeric FROM products", connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var product = new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2)
                };
                products.Add(product);
            }
            return products;
        }
    }
}
```
Запустим приложение  
Вызовем любой метод в Swagger  
![](attachments/Pasted%20image%2020250205175716.png)  
Убедимся что все работает  
# unmanaged & IDisposable
[INFO Unmanaged, IDisposable, using](INFO/Unmanaged,%20IDisposable,%20using.md)  
Неуправляемый код - это код, который выполняется вне среды выполнения .NET (CLR) и напрямую взаимодействует с операционной системой. Это могут быть:
1. Файловые дескрипторы
2. Сетевые соединения
3. Дескрипторы окон
4. Указатели на неуправляемую память
5. Соединения с базами данных
6. COM-объекты  
Для работы с  неуправляемыми ресурсами существует интерфейс `IDisposable`. Когда класс реализует `IDisposable`, он сигнализирует о том, что использует неуправляемые ресурсы, которые нужно явно освобождать.
У нас в классе сейчас есть несколько объектов которые реализуют `IDisposable`  
Используем "Перейти к определению" для `NpgsqlConnection`  
Увидим, что в иерархии есть `IDisposable`  
![](attachments/Pasted%20image%2020250207104508.png)  
Конструкция `using` в C# предоставляет удобный способ работы с неуправляемыми ресурсами.
Когда вы используете `using` вокруг объекта, который реализует интерфейс `IDisposable`, он гарантирует, что метод `Dispose` будет вызван автоматически, даже если произойдет ошибка во время выполнения кода внутри блока `using`. Это позволяет избежать утечек памяти и обеспечивает корректное освобождение ресурсов.
Добавим `using` в наш код  
Установим курсор на строке  
```csharp
var connection = new NpgsqlConnection(_connectionString);
```
Вызовем `InetliSense` и примем предложение 
![](attachments/Pasted%20image%2020250207104556.png)    
Дальше переходим к строке где используется `NpgsqlCommand`  
Также вызываем `InteliSense` и принимаем правки  
Тоже самое повторяем в следующей строке
В результате получим такой код   
![](attachments/Pasted%20image%2020250207104633.png) 