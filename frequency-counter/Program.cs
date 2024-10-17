
using System.Text;
using System.Text.Json;

if (args.Length < 1)
{
  Console.WriteLine("Usage: <executable> <file-path>");
  Console.WriteLine("You provided:");
  foreach (var arg in args)
  {
    Console.WriteLine(arg);
  }
  return;
}

string filePath = args[0];
PrintLn($"Analyzing file: {filePath}", ConsoleColor.Yellow);

try
{
  using var stream = File.OpenRead(filePath);

  Dictionary<string, long> dict = [];

  byte[] buffer = new byte[64 * 1024 * 1024];
  int bytesRead = 0;

  while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
  {
    string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    if (bytesRead == buffer.Length && char.IsWhiteSpace(chunk[^1]) is false)
    {
      int lastSpaceIndex = chunk.LastIndexOf(' ');
      if (lastSpaceIndex != -1)
      {
        stream.Seek(chunk.Length - lastSpaceIndex, SeekOrigin.Current);
      }
    }

    foreach (var character in chunk)
    {
      var characterStr = character.ToString();
      if (dict.TryGetValue(characterStr, out long value))
      {
        dict[characterStr] = value + 1;
      }
      else
      {
        dict.Add(characterStr, 1);
      }
    }

    for (int i = 0; i < chunk.Length - 2; i += 1)
    {
      var current = chunk[i];
      var next = chunk[i + 1];
      var pair = $"{current}{next}";

      if (dict.TryGetValue(pair, out long value))
      {
        dict[pair] = value + 1;
      }
      else
      {
        dict.Add(pair, 1);
      }
    }
  }

  dict.Remove("  ");
  dict.Remove("\n");
  dict.Remove("\r");
  dict.Remove("\r\n");

  string jsonFilePath = $"./results/{Path.GetFileNameWithoutExtension(filePath)}.json";

  var sortedDictionary = dict
    .OrderByDescending(kvp => kvp.Value)
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

  var options = new JsonSerializerOptions { WriteIndented = true };
  string jsonString = JsonSerializer.Serialize(sortedDictionary, options);

  File.WriteAllText(jsonFilePath, jsonString);
}
catch (FileNotFoundException)
{
  PrintLn($"File was not found", ConsoleColor.Red);
}
catch (Exception ex)
{
  PrintLn($"Error: {ex.Message}", ConsoleColor.Red);
}

static void PrintLn(string msg, ConsoleColor color)
{
  var tmp = Console.ForegroundColor;
  Console.ForegroundColor = color;
  Console.WriteLine(msg);
  Console.ForegroundColor = tmp;
}
