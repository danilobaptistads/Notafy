using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace East;

public  class EastTextDetector
{
    private string DpModelPath { get; set; }
    public EastTextDetector(string dpModelPath)
    {
        DpModelPath =dpModelPath;
    }
    public Mat ProcessTextDetection(string imagePath)
    {
        Mat originalImage= Cv2.ImRead(imagePath);
        if (originalImage == null)
        {
            System.Console.WriteLine("Imagem n");
        }
        
        var (scores, geometrys) = GetEastOutputs(originalImage);
        var (listBoxs, listConfiance) = BoundBoxBuilder(geometrys, scores);

        CvDnn.NMSBoxes(listBoxs, listConfiance, 0.5f, 0.4f, out int[] detectedBoxs);
        
        var (widthProporcion, heightProporcion) = GetImageProporcion(originalImage);

        foreach (var i in detectedBoxs)
        {
            var box = listBoxs[i];
            box.X = (int)(box.X * widthProporcion);
            box.Y = (int)(box.Y * heightProporcion);
            box.Width = (int)(box.Width * widthProporcion);
            box.Height = (int)(box.Height * heightProporcion);
            
            Cv2.Rectangle(originalImage, box, Scalar.Green, 1);

            return originalImage;
        }
        throw new InvalidOperationException("Erro: não foi possivel detectar o texto");
    }
    private (Mat, Mat) GetEastOutputs(Mat image)
    {
        var eastDetector = CvDnn.ReadNet(DpModelPath);
        var blob = CvDnn.BlobFromImage(image, 1.0, new Size(320, 320), swapRB: true, crop: false);

        eastDetector.SetInput(blob);
        var scores = eastDetector.Forward("feature_fusion/Conv_7/Sigmoid");
        var geometry = eastDetector.Forward("feature_fusion/concat_3");
        return (scores, geometry);
    }
    private  (float, float, float, float, float) GeometryCodenates(Mat geometry, int y, int x)
    {
        float top = geometry.At<float>(0, 0, y, x);
        float right = geometry.At<float>(0, 1, y, x);
        float botton = geometry.At<float>(0, 2, y, x);
        float left = geometry.At<float>(0, 3, y, x);
        float angle = geometry.At<float>(0, 4, y, x);

        return (top, right, botton, left, angle);
    }
    private  (float, float) GetImageProporcion(Mat originalImage)
    {
        float aspctRatio = 320;
        float widthProporcion = originalImage.Cols / aspctRatio;
        float heightProporcion = originalImage.Rows / aspctRatio;
        return (widthProporcion, heightProporcion);
    }
    private  Mat ResizeToEast(Mat originalImage)
    {
        var resizedImage = new Mat();
        Cv2.Resize(originalImage, resizedImage, new Size(320, 320));
        return resizedImage;
    }
    private  void ReadScore(Mat score)
    {
        for (int y = 0; y < score.Size(2); y++)
        {
            for (int x = 0; x < score.Size(3); x++)
            {
                float conf = score.At<float>(0, 0, y, x);

                if (conf > 0.5f)  // threshold típico do EAST
                {
                    Console.WriteLine($"({x}, {y}) — conf = {conf}");
                }
            }
        }

    }
    private  void ReadGeometry(Mat geometry)
    {
        int height = geometry.Size(2);
        int width = geometry.Size(3);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float d0 = geometry.At<float>(0, 0, y, x); // topo
                float d1 = geometry.At<float>(0, 1, y, x); // direita
                float d2 = geometry.At<float>(0, 2, y, x); // base
                float d3 = geometry.At<float>(0, 3, y, x); // esquerda
                float angle = geometry.At<float>(0, 4, y, x); // ângulo

                Console.WriteLine($"(coluna: {x}, linha: {y})  d0={d0}, d1={d1}, d2={d2}, d3={d3}, ang={angle}");


            }
        }
    }
    private  (int, int, int, int) CalculateCoordenatesBoundBox(int x, int y, float top, float right, float botton, float left, float angle)
    {
        var offsetX = x * 4.0;
        var offsetY = y * 4.0;

        var cos = Math.Cos(angle);
        var sin = Math.Sin(angle);

        var height = top + botton;
        var width = right + left;

        var endX = Convert.ToInt32(offsetX + (cos * right) + (sin * botton));
        var endY = Convert.ToInt32(offsetY - (sin * right) + (cos * botton));

        var startX = Convert.ToInt32(endX - width);
        var startY = Convert.ToInt32(endY - height);

        return (startX, startY, endX, endY);

    }
    private  (List<Rect>, List<float>) BoundBoxBuilder(Mat geometry, Mat score)
    {

        int linhas = geometry.Size(2);
        int colunas = geometry.Size(3);
        var listConfiance = new List<float>();
        var listBoxs = new List<Rect>();
        var minConfiance = 0.9;

        for (int y = 0; y < linhas; y++)
        {
            for (int x = 0; x < colunas; x++)
            {
                float confiance = score.At<float>(0, 0, y, x);
                if (confiance < minConfiance)
                {
                    continue;
                }

                var (top, right, botton, left, angle) = GeometryCodenates(geometry, y, x);
                var (startX, startY, endX, endY) = CalculateCoordenatesBoundBox(x, y, top, right, botton, left, angle);

                listConfiance.Add(confiance);
                int width = endX - startX;
                int height = endY - startY;

                listBoxs.Add(new Rect(startX, startY, width, height));

            }
        }
        return (listBoxs, listConfiance);
    }
    private  (int startX, int startY, int endX, int endY) ResizeToOriginalSize(int startX, int startY, int endX, int endY)
    {

        return (startX, startY, endX, endY);
    }

}
