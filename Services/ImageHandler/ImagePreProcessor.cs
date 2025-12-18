
using OpenCvSharp;

namespace Notafy.Services;

public static class ImagePreProcessor
{
    public static  byte[] Process(string imagePath)
    {
        var originalImage = Cv2.ImRead(imagePath);
        if (originalImage.Empty())
        {
            throw new Exception("Não foi possivel carregar a imagem");
        } 

        var manipulatedImage = originalImage.CvtColor(ColorConversionCodes.BGR2GRAY);
        //manipulatedImage = manipulatedImage.GaussianBlur(new Size(5,5), 0);
        //manipulatedImage = manipulatedImage.BilateralFilter(9, 75, 75);
         var clash = Cv2.CreateCLAHE(2.0, new Size(8, 8));
        // clash.Apply(manipulatedImage,manipulatedImage);

        // manipulatedImage = manipulatedImage.AdaptiveThreshold(
        //                                     255,
        //                                     AdaptiveThresholdTypes.GaussianC, 
        //                                     ThresholdTypes.Binary, 
        //                                     21, 
        //                                     5);
        // var manipulatedImage= new Mat();
        // Cv2.CvtColor(originalImage, manipulatedImage, ColorConversionCodes.BGR2GRAY);
        
        // manipulatedImage = manipulatedImage.GaussianBlur(new Size(5,5), 0);

        Cv2.NamedWindow("Imagem Processada", WindowFlags.FreeRatio);
        Cv2.ImShow("Imagem Processada", manipulatedImage); 
    
        Cv2.WaitKey(0); 
        Cv2.DestroyAllWindows();

        Cv2.ImEncode(".png", manipulatedImage, out byte[] imageBytes);

        return imageBytes;
 
    
    }

    


    


}
