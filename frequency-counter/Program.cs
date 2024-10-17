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
else if (command == "huffman")
{
  HuffmanCommand.Run(args);
}
else if (command == "cleanup")
{
  CleanupCommand.Run(args);
}
else if (command == "compare")
{
  CompareCommand.Run(args);
}
else if (command == "compact-huffman")
{
  CompactHuffmanCommand.Run(args);
}
else
{
  Helpers.PrintLn($"Command is not found: '{command}'", ConsoleColor.Red);
}


public static class Helpers
{
  private const string LEGAL_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,?!:;\"''\'()-[] \t\n\r&@#$%^";

  public static bool StringIsLegal(string str)
  {
    return str.All(LEGAL_CHARACTERS.Contains);
  }

  public static string LegalizeString(string str)
  {
    StringBuilder sb = new();
    foreach (char c in str)
    {
      if (LEGAL_CHARACTERS.Contains(c))
      {
        sb.Append(c);
      }
    }
    return sb.ToString();
  }

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

  public static IEnumerable<string> FileChunks(FileStream stream, int chunkSize)
  {
    byte[] buffer = new byte[chunkSize];
    int bytesRead;

    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
    {
      string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
      yield return chunk;
    }
  }
}



public static class CompareCommand
{
  public static void Run(string[] args)
  {
    string frequenciesFile = args.ElementAtOrDefault(0) ?? "../datasets/results/index.json";
    Helpers.PrintLn($"Using frequencies from: {frequenciesFile}", ConsoleColor.Green);

    var content = File.ReadAllText(frequenciesFile);
    var fileDict = JsonSerializer.Deserialize<Dictionary<string, long>>(content)!;

    var compactHuffmanTree = CompactHuffmanTree.Build(fileDict, 20);
    var huffmanTree = HuffmanTree.Build(fileDict);

    long compactSmaller = 0;
    long compactBigger = 0;
    long difference = 0;



    // using var stream = File.OpenRead("../datasets/txt/tradition.txt");
    // foreach (var chunk in Helpers.FileChunks(stream, 24 * 1024 * 1024))
    // {
    string? chunk;
    var reader = new StreamReader("../datasets/txt/tradition.txt");
    while ((chunk = reader.ReadLine()) != null)
    {
      var legalChunk = Helpers.LegalizeString(chunk);

      var compactEncoded = compactHuffmanTree.Encode(legalChunk);
      var encoded = huffmanTree.Encode(legalChunk);
      if (compactEncoded.Length < encoded.Length)
      {
        compactSmaller += 1;
      }
      else
      {
        compactBigger += 1;
      }
      difference += encoded.Length - compactEncoded.Length;
    }

    if (compactSmaller > compactBigger)
    {

      Helpers.PrintLn("Compact is smaller on average :)", ConsoleColor.Green);
    }
    else
    {
      Helpers.PrintLn("Compact is bigger on average :(", ConsoleColor.Red);
    }

    Helpers.PrintLn($"Compact is smaller {compactSmaller} times", ConsoleColor.White);
    Helpers.PrintLn($"Compact is bigger {compactBigger} times", ConsoleColor.White);
    Helpers.PrintLn($"Difference: {compactSmaller - compactBigger} times", ConsoleColor.White);
    Helpers.PrintLn($"Difference in size: {difference}", ConsoleColor.White);
    Helpers.PrintLn($"Difference in size on average: {difference * 1.0 / (compactSmaller + compactBigger)}", ConsoleColor.White);
  }
}

public static class CleanupCommand
{


  public static void Run(string[] args)
  {
    if (args.Length != 2)
    {
      Helpers.PrintLn("You need to specify: <source-file> <output-file>", ConsoleColor.Red);
      return;
    }


    var sourceFile = args[0];
    var outputFile = args[1];
    Helpers.PrintLn($"Cleaning frequencies from: {sourceFile}", ConsoleColor.Green);

    var content = File.ReadAllText(sourceFile);
    var frequencies = JsonSerializer.Deserialize<Dictionary<string, long>>(content)!;

    var keys = frequencies.Keys.ToList();
    foreach (var key in keys)
    {
      if (Helpers.StringIsLegal(key) is false)
      {
        frequencies.Remove(key);
      }
    }

    Helpers.WriteDictToFile(frequencies, outputFile);
  }
}

public static class CompactHuffmanCommand
{

  public static void Run(string[] args)
  {
    string file = args.ElementAtOrDefault(0) ?? "../datasets/results/index.json";
    Helpers.PrintLn($"Using frequencies from: {file}", ConsoleColor.Green);

    var content = File.ReadAllText(file);
    var fileDict = JsonSerializer.Deserialize<Dictionary<string, long>>(content)!;

    var tree = CompactHuffmanTree.Build(fileDict);
    // tree.PrintCodes();

    var encoded = tree.Encode("Made by ROman KOshchei");
    Console.WriteLine("Encoded by Compact Huffman: " + encoded);
  }
}

public static class HuffmanCommand
{
  public static void Run(string[] args)
  {
    string file = args.ElementAtOrDefault(0) ?? "../datasets/results/index.json";
    Helpers.PrintLn($"Using frequencies from: {file}", ConsoleColor.Green);

    var content = File.ReadAllText(file);
    var fileDict = JsonSerializer.Deserialize<Dictionary<string, long>>(content)!;


    var huffmanTree = HuffmanTree.Build(fileDict);
    // huffmanTree.PrintCodes();

    // Encode the input string
    string encodedString = huffmanTree.Encode("Made by ROman KOshchei");
    Console.WriteLine("Encoded: " + encodedString);

    // Decode the encoded string
    string decodedString = huffmanTree.Decode(encodedString);
    Console.WriteLine("Decoded: " + decodedString);
  }
}

public class CompactHuffmanTree
{
  public class Node(string symbols, long frequency)
  {
    public string Symbols { get; set; } = symbols;
    public long Frequency { get; set; } = frequency;
    public Node? Left { get; set; } = null;
    public Node? Right { get; set; } = null;
  }

  private readonly Node root;
  private readonly Dictionary<string, string> codes;

  private CompactHuffmanTree(Node root, Dictionary<string, string> codes)
  {
    this.root = root;
    this.codes = codes;
  }

  public string Encode(string input)
  {
    StringBuilder sb = new();

    for (var i = 0; i < input.Length; i += 1)
    {
      var c = input[i];
      if (codes.ContainsKey(c.ToString()) is false)
      {
        Helpers.PrintLn($"Key symbol: '{JsonSerializer.Serialize(c)}' isn't found in dataset", ConsoleColor.Red);
        continue;
      }

      var cCode = codes[c.ToString()];

      if (i == input.Length - 1)
      {
        sb.Append(codes[c.ToString()]);
      }
      else
      {
        var nextC = input[i + 1];
        var withNextChar = $"{c}{nextC}";

        if (codes.TryGetValue(withNextChar, out var togetherCode))
        {
          var nextCharCode = codes[nextC.ToString()];

          if (cCode.Length + nextCharCode.Length > togetherCode.Length)
          {
            sb.Append(togetherCode);
            i += 1;
          }
          else
          {
            sb.Append(cCode);
          }
        }
        else
        {
          sb.Append(cCode);
        }
      }
    }

    return sb.ToString();
  }

  public static CompactHuffmanTree Build(Dictionary<string, long> frequencies, int doubleCharCount = 20)
  {
    frequencies = frequencies
      .OrderByDescending(kvp => kvp.Value)
      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    var doubleCount = 0;
    var keys = frequencies.Keys.ToList();
    foreach (var key in keys)
    {
      if (key.Length != 2) continue;

      if (doubleCount >= doubleCharCount) { frequencies.Remove(key); }
      else { doubleCount += 1; }
    }

    var nodes = frequencies.Select(f => new Node(f.Key, f.Value)).ToList();
    while (nodes.Count > 1)
    {
      nodes = [.. nodes.OrderBy(n => n.Frequency)];
      var left = nodes[0];
      var right = nodes[1];

      var parent = new Node("\0", left.Frequency + right.Frequency)
      {
        Left = left,
        Right = right
      };

      nodes.RemoveRange(0, 2);
      nodes.Add(parent);
    }
    var root = nodes[0]!;

    var codes = new Dictionary<string, string>();
    GenerateCodes(codes, root, "");

    return new CompactHuffmanTree(root, codes);
  }

  private static void GenerateCodes(
    Dictionary<string, string> codes, Node? node, string code
  )
  {
    if (node == null) return;

    if (node.Left == null && node.Right == null) // Leaf node
    {
      codes[node.Symbols] = code;
    }

    GenerateCodes(codes, node.Left, code + "0");
    GenerateCodes(codes, node.Right, code + "1");
  }

  public void PrintCodes()
  {
    Console.WriteLine("Compact Huffman Codes:");

    var sortedCodes = codes
       .OrderByDescending(kvp => kvp.Value.Length)
       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    foreach (var kvp in sortedCodes)
    {
      Console.WriteLine($"Character: '{kvp.Key}' Code: {kvp.Value}");
    }
  }
}


public class HuffmanTree
{

  public class Node(char symbol, long frequency)
  {
    public char Symbol { get; set; } = symbol;
    public long Frequency { get; set; } = frequency;
    public Node? Left { get; set; } = null;
    public Node? Right { get; set; } = null;
  }

  private readonly Node root;
  private readonly Dictionary<char, string> codes;

  private HuffmanTree(Node root, Dictionary<char, string> codes)
  {
    this.root = root;
    this.codes = codes;
  }

  public void PrintCodes()
  {
    Console.WriteLine("Huffman Codes:");

    var sortedCodes = codes
       .OrderByDescending(kvp => kvp.Value.Length)
       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    foreach (var kvp in sortedCodes)
    {
      Console.WriteLine($"Character: '{kvp.Key}' Code: {kvp.Value}");
    }
  }


  public static HuffmanTree Build(Dictionary<string, long> mixedFrequencies)
  {
    Dictionary<char, long> charFrequencies = new(mixedFrequencies.Count);
    foreach (var (key, value) in mixedFrequencies)
    {
      if (key.Length == 1)
      {
        charFrequencies.Add(key[0], value);
      }
    }

    var nodes = charFrequencies.Select(x => new Node(x.Key, x.Value)).ToList();
    while (nodes.Count > 1)
    {
      nodes = [.. nodes.OrderBy(n => n.Frequency)];
      var left = nodes[0];
      var right = nodes[1];

      var parent = new Node('\0', left.Frequency + right.Frequency)
      {
        Left = left,
        Right = right
      };

      nodes.RemoveRange(0, 2);
      nodes.Add(parent);
    }
    var root = nodes[0]!;

    var codes = new Dictionary<char, string>();
    GenerateCodes(codes, root, "");

    return new HuffmanTree(root, codes);
  }

  private static void GenerateCodes(Dictionary<char, string> codes, Node? node, string code)
  {
    if (node == null) return;

    if (node.Left == null && node.Right == null)
    {
      codes[node.Symbol] = code;
    }

    GenerateCodes(codes, node.Left, code + "0");
    GenerateCodes(codes, node.Right, code + "1");
  }

  public string Encode(string input)
  {
    return string.Concat(input.Select(c => codes[c]));
  }

  public string Decode(string encoded)
  {
    var result = "";
    var currentNode = root;
    foreach (var bit in encoded)
    {
      currentNode = bit == '0' ? currentNode.Left : currentNode.Right;

      if (currentNode.Left == null && currentNode.Right == null)
      {
        result += currentNode.Symbol;
        currentNode = root;
      }
    }

    return result;
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