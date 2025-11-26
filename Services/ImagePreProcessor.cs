using OpenCvSharp;

namespace Notafy.Services;

public static class ImagePreProcessor
{
    public static void Process(string imagePath)
    {
        
        var originalImage = Cv2.ImRead(imagePath);
        if (originalImage.Empty())
        {
            Console.WriteLine("Erro: imagem não encontrada.");
            return;

        }

        var grayScaleImage = ApplyGreyScale(originalImage);
        var binarizedImage = ApplyBinarizationOtsu(grayScaleImage);
        var cropdImage = CropImage(binarizedImage,grayScaleImage);
        var equalizedImage = EqualizeContrast(cropdImage);
        var adptativBinarizedImage = AdaptativBinarization(equalizedImage);

    }

    private static Mat ApplyGreyScale(Mat originalImage)
    {
        var grayScaleImage = new Mat();
        Cv2.CvtColor(originalImage, grayScaleImage, ColorConversionCodes.BGR2GRAY);

        return grayScaleImage;
    }
    private static Mat ApplyBinarizationOtsu(Mat grayScaleImage)
    {
        var blackWhiteImage = new Mat();
        Cv2.Threshold(grayScaleImage, blackWhiteImage, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
        return blackWhiteImage;
    }
    private static Mat CropImage(Mat binarizedImage, Mat grayScaleImage)
    {

        Point[][] contours ;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(binarizedImage, out contours , out hierarchy,
        RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        if (contours.Length == 0)
        {
            return grayScaleImage;
        }
       
        var largestEdge = contours.OrderByDescending(c => Cv2.ContourArea(c)).First();
        var boundingRectangle  = Cv2.BoundingRect(largestEdge);

        Mat croppedImage = new Mat(grayScaleImage, boundingRectangle );

        return croppedImage;

    }
    private static Mat EqualizeContrast(Mat image)
        { 
        var clahe = Cv2.CreateCLAHE(clipLimit: 3.0, tileGridSize: new Size(8, 8));
        Mat equalizedImage = new Mat();
        clahe.Apply(image, equalizedImage);

        return equalizedImage;

        
        }
    private static Mat AdaptativBinarization(Mat image)
    {
        Mat binarizedImage = new Mat();
        Cv2.AdaptiveThreshold(image, binarizedImage, 255, AdaptiveThresholdTypes.GaussianC,
        ThresholdTypes.Binary, 35, 10
        );

        return binarizedImage;
        
    }

}