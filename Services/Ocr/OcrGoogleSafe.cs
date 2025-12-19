using System.Text;
using System.Text.Json;

public class OcrGoogleSafe
{
    private const int MAX_CALLS_PER_MONTH = 900;
    private const int MAX_IMAGE_SIZE_MB = 4;
    private static readonly string CounterFile = "ocr_counter.json";
    private  readonly string ApiKey;
    private static readonly bool OcrEnabled = Environment.GetEnvironmentVariable("OCR_ENABLED") != "false";

    public OcrGoogleSafe(IConfiguration Keyconfig)
    {
        ApiKey = Keyconfig["ExternalKeys:GOOGLE_VISION_API_KEY"];
    }
    public string ReadText(string imagePath)
    {
        if (!OcrEnabled)
            throw new Exception("OCR desativado por segurança (OCR_ENABLED=false)");

        if (string.IsNullOrEmpty(ApiKey))
            throw new Exception("API Key não configurada");

        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Imagem não encontrada");

        ValidateImageSize(imagePath);
        ValidateMonthlyLimit();

        string base64Image = Convert.ToBase64String(File.ReadAllBytes(imagePath));

        var requestBody = new
        {
            requests = new[]
            {
                new
                {
                    image = new { content = base64Image },
                    features = new[]
                    {
                        new { type = "TEXT_DETECTION" }
                    }
                }
            }
        };

        string json = JsonSerializer.Serialize(requestBody);

        using var client = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = client.PostAsync(
            $"https://vision.googleapis.com/v1/images:annotate?key={ApiKey}",
            content
        ).Result;

        if (!response.IsSuccessStatusCode)
            throw new Exception("Falha na chamada OCR");

        IncrementCounter();

        var resultJson = response.Content.ReadAsStringAsync().Result;
        using var doc = JsonDocument.Parse(resultJson);

        return doc.RootElement
            .GetProperty("responses")[0]
            .GetProperty("fullTextAnnotation")
            .GetProperty("text")
            .GetString();
    }

    private static void ValidateImageSize(string path)
    {
        var sizeMb = new FileInfo(path).Length / (1024.0 * 1024.0);
        if (sizeMb > MAX_IMAGE_SIZE_MB)
            throw new Exception("Imagem grande demais. OCR bloqueado por segurança.");
    }

    private static void ValidateMonthlyLimit()
    {
        var counter = LoadCounter();
        if (counter.Month != DateTime.UtcNow.Month)
        {
            counter.Month = DateTime.UtcNow.Month;
            counter.Count = 0;
            SaveCounter(counter);
        }

        if (counter.Count >= MAX_CALLS_PER_MONTH)
            throw new Exception("Limite mensal de OCR atingido");
    }

    private static void IncrementCounter()
    {
        var counter = LoadCounter();
        counter.Count++;
        SaveCounter(counter);
    }

    private static Counter LoadCounter()
    {
        if (!File.Exists(CounterFile))
            return new Counter { Month = DateTime.UtcNow.Month, Count = 0 };

        return JsonSerializer.Deserialize<Counter>(
            File.ReadAllText(CounterFile)
        );
    }

    private static void SaveCounter(Counter counter)
    {
        File.WriteAllText(
            CounterFile,
            JsonSerializer.Serialize(counter)
        );
    }

    
}
