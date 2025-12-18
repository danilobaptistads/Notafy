using OpenCvSharp;
using Tesseract;
namespace Notafy.Services;

class TextExtractor :IDisposable
{
    private readonly TesseractEngine engine;
    public TextExtractor()
    {
        engine = new TesseractEngine("./tessdata", "por", EngineMode.Default);
    }

    public string Extract(byte[] imgSrc)
    {
        using var img = Pix.LoadFromMemory(imgSrc);
        using var page = engine.Process(img);

        return page.GetText();
    }

    public void Dispose()
    {
        engine?.Dispose();
    }
}