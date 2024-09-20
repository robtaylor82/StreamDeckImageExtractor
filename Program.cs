
using System.Text.Json;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            args = args.Append("c:\\Temp\\A2A Comanche").ToArray();
        }

        if (args.Length != 1)
        {
            Console.WriteLine("Usage: Program <Directory Path>");
            return;
        }

        string dirPath = args[0];

        if (!Directory.Exists(dirPath))
        {
            Console.WriteLine($"Directory '{dirPath}' does not exist.");
            return;
        }

        // Gather all .json files from directory and subdirectories
        string[] filePaths = Directory.GetFiles(dirPath, "*.json", SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            IterateJsonFile(filePath, dirPath);
        }
    }

    public static void IterateJsonFile(string jsonFilePath, string dirPath)
    {
        string jsonString = File.ReadAllText(jsonFilePath);
        using JsonDocument doc = JsonDocument.Parse(jsonString);
        JsonElement element = doc.RootElement;
        int counter = 0;

        ProcessElement(element, dirPath, ref counter);
    }

    public static void ProcessElement(JsonElement element, string dirPath, ref int counter)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                string? encodedImage = element.GetString();

                if(string.IsNullOrWhiteSpace(encodedImage))
                {
                    return;
                }

                Console.WriteLine($"Processing element {encodedImage}");

                try
                {
                    byte[] imageBytes = Convert.FromBase64String(encodedImage.Trim().Replace("data:image/png;base64,", ""));

                    if (imageBytes.Length == 0)
                    {
                        return;
                    }

                    string newFileName = $"{dirPath}\\output\\image{counter}.png";
                    File.WriteAllBytes(newFileName, imageBytes);
                    Console.WriteLine($"Written: {newFileName}");
                }
                catch (FormatException)
                {
                    // Suppress format exceptions, move on to the next element
                    Console.WriteLine("Invalid Base64 string");
                }
                finally
                {
                    counter++;
                }

                break;

            case JsonValueKind.Object:

                foreach (JsonProperty property in element.EnumerateObject())
                {
                    ProcessElement(property.Value, dirPath, ref counter);
                }
                break;

            case JsonValueKind.Array:

                foreach (JsonElement newElement in element.EnumerateArray())
                {
                    ProcessElement(newElement, dirPath, ref counter);
                }
                break;
        }
    }
}
