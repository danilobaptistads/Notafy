using OpenCvSharp;

namespace Notafy.Services.ImageHandler;
 public static class ColorConversionHandler
{
    public static Mat ToGray(Mat img)
    {
        var gray = new Mat();
        Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
        return gray;
    }
    public static Mat AdaptiveBinarization(Mat img)
    {
        var bin = new Mat();
        Cv2.AdaptiveThreshold(img, bin, 255,
            AdaptiveThresholdTypes.GaussianC,
            ThresholdTypes.Binary,
            21, 5);
        return bin;
    }
    private static Mat InvertImage(Mat img)
    {
        var inverted = new Mat();
        Cv2.BitwiseNot(img, inverted);
        return inverted;
    }
}