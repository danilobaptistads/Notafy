using OpenCvSharp;

public static class ImageUtils
{

    public static Point[][] findCountours(Mat img)
    {
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(
            img,
            out contours,
            out hierarchy,
            RetrievalModes.List,
            ContourApproximationModes.ApproxSimple);

        var bigestContours = contours
        .OrderByDescending(c => Cv2.ContourArea(c))
        .Take(6)
        .ToArray();

        return bigestContours;

    }


}