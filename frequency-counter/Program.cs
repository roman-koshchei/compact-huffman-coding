
using System.Text;
using System.Text.Json;

if (args.Length < 1)
{
  Console.WriteLine("Usage: <command>");
  Console.WriteLine("You provided:");
  foreach (var arg in args)
  {
    Console.WriteLine(arg);
  }
  return;
}

string command = args[0];
args = args.Skip(1).ToArray();
if (command == "analyze")
{
  AnalyzeCommand.Run(args);
}
else if (command == "aggregate")
{
  AggregateCommand.Run(args);
}
else if (command == "wiki")
{
  WikiCommand.Run(args);
}
else
{
  Helpers.PrintLn($"Command is not found: '{command}'", ConsoleColor.Red);
}


public static class Helpers
{

  public static void PrintLn(string msg, ConsoleColor color)
  {
    var tmp = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(msg);
    Console.ForegroundColor = tmp;
  }

  private static readonly JsonSerializerOptions jsonSerializerOptions = new()
  {
    WriteIndented = true
  };

  public static void WriteDictToFile(Dictionary<string, long> dict, string jsonFilePath)
  {
    var sortedDictionary = dict
        .OrderByDescending(kvp => kvp.Value)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    string jsonString = JsonSerializer.Serialize(sortedDictionary, jsonSerializerOptions);

    File.WriteAllText(jsonFilePath, jsonString);
  }
}

public static class WikiCommand
{
  public static void Run(string[] args)
  {
    string directory = args.ElementAtOrDefault(0) ?? "../datasets/json/wiki";
    Helpers.PrintLn($"Transforming wiki files from: {directory}", ConsoleColor.Green);

    using var resultFile = File.OpenWrite("../datasets/txt/wiki.txt");

    var jsonFiles = Directory.GetFiles(directory, "*.json");
    foreach (var file in jsonFiles)
    {
      var content = File.ReadAllText(file);
      using var doc = JsonDocument.Parse(content);
      StringBuilder text = new();
      foreach (var element in doc.RootElement.EnumerateArray())
      {
        text.Append(element.GetProperty("text").GetString() ?? "");
      }
      resultFile.Write(Encoding.UTF8.GetBytes(text.ToString()));

      Helpers.PrintLn($"File {file} was processed", ConsoleColor.Green);
    }
  }
}

public static class AggregateCommand
{
  public static void Run(string[] args)
  {
    string directory = args.ElementAtOrDefault(0) ?? "../datasets/results";
    Helpers.PrintLn($"Aggregating files from: {directory}", ConsoleColor.Green);

    Dictionary<string, long> globalDict = [];

    var jsonFiles = Directory.GetFiles(directory, "*.json");
    foreach (var jsonFile in jsonFiles)
    {
      var content = File.ReadAllText(jsonFile);
      var fileDict = JsonSerializer.Deserialize<Dictionary<string, long>>(content)!;
      foreach (var (key, value) in fileDict)
      {
        if (globalDict.TryGetValue(key, out var globalValue))
        {
          globalDict[key] = globalValue + value;
        }
        else
        {
          globalDict.Add(key, value);
        }
      }
    }

    Helpers.WriteDictToFile(globalDict, "../datasets/results/index.json");
  }
}


public static class AnalyzeCommand
{
  public static void Run(string[] args)
  {
    if (args.Length != 1)
    {
      Helpers.PrintLn("File path is required", ConsoleColor.Red);
      return;
    }

    string filePath = args[0];
    Helpers.PrintLn($"Analyzing file: {filePath}", ConsoleColor.Green);


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

      string jsonFilePath = $"../datasets/results/{Path.GetFileNameWithoutExtension(filePath)}.json";
      Helpers.WriteDictToFile(dict, jsonFilePath);
    }
    catch (FileNotFoundException)
    {
      Helpers.PrintLn($"File was not found", ConsoleColor.Red);
    }
    catch (Exception ex)
    {
      Helpers.PrintLn($"Error: {ex.Message}", ConsoleColor.Red);
    }

  }
}