using OpenCvSharp;

namespace Notafy.Services.ImageHandler;
 public static class ImageFilters
{
    public static Mat DenoiseMedian(Mat img)
    {
        var denoised = new Mat();
        int kernelSize = 3; 
        Cv2.MedianBlur(img, denoised, kernelSize);
        return denoised;
    }

     public static Mat GaussBlur(Mat img)
    {
        var blur = new Mat();
        blur = img.GaussianBlur(new Size(5,5), 0);
        return blur;
    }

}