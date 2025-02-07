---
created: 2025-02-03-16-27-05+10:00
tags:
---
Сериализация в JSON в .NET — это процесс преобразования объектов .NET (например, классов, структур, коллекций) в строку формата JSON. Это позволяет сохранить данные в текстовом виде, передать их через сеть или записать в файл.

Десериализация — это обратный процесс, при котором строка в формате JSON преобразуется обратно в объект .NET.

В .NET для работы с JSON обычно используется библиотека `System.Text.Json` (встроена с .NET Core 3.0 и выше) или популярная сторонняя библиотека `Newtonsoft.Json`. Примеры:

- **Сериализация с помощью `System.Text.Json`:**
  ```csharp
  var obj = new { Name = "Alice", Age = 30 };
  string json = JsonSerializer.Serialize(obj);
  Console.WriteLine(json); // {"Name":"Alice","Age":30}
  ```

- **Десериализация с помощью `System.Text.Json`:**
  ```csharp
  string json = "{\"Name\":\"Alice\",\"Age\":30}";
  var obj = JsonSerializer.Deserialize<YourClass>(json);
  Console.WriteLine(obj.Name); // Alice
  ```

Сериализация/десериализация часто используется при работе с API, базами данных или сохранением пользовательских настроек.