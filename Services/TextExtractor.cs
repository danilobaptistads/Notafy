using Tesseract;
namespace Notafy.Services;

class TextExtractor
{
    public string Extract(string imgSrc)
    {
        using var engine = new TesseractEngine("./tessdata", "por", EngineMode.Default);
    
        using var img = Pix.LoadFromFile(imgSrc);
        using var page = engine.Process(img);

        return  page.GetText();
    }
}