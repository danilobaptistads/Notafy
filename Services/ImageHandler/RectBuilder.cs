using OpenCvSharp;

public static class RectBuilder
{
    static Mat originalImage;

    static Point startPoint;
    static Point endPoint;
    static bool drawing = false;

    static Point[] quad = null;

    static bool editingCorner = false;
    static int activeCorner = -1;

    static bool confirmed = false;

    // BOTÃO "CONFIRMAR"
    static Rect confirmButton = new Rect(20, 20, 140, 40);

    public static Mat Exec(string imagePath)
    {
        originalImage = Cv2.ImRead(imagePath);
        if (originalImage.Empty())
            throw new Exception("Não foi possível carregar a imagem");

        Cv2.NamedWindow("img");
        Cv2.SetMouseCallback("img", MouseCallback);

        while (!confirmed)
        {
            Mat clone = originalImage.Clone();
            DrawButton(clone);

            if (drawing)
            {
                var pts = GetRectPoints(startPoint, endPoint);
                DrawQuad(clone, pts, Scalar.Yellow);
            }


            if (quad != null)
            {
                DrawQuad(clone, quad, Scalar.Green);
                foreach (var p in quad)
                    Cv2.Circle(clone, p, 6, Scalar.Red, -1);
            }

            Cv2.ImShow("img", clone);

            if (Cv2.WaitKey(20) == 27)
            {
                Cv2.DestroyAllWindows();
                return null;
            }
        }

        Cv2.DestroyAllWindows();
        return WarpFromQuad(originalImage, quad);
    }

    // =========================
    // BOTÃO
    // =========================
static void DrawButton(Mat img)
{
    confirmButton = GetConfirmButtonRect(img);

    Scalar bg = quad != null ? Scalar.DarkGreen : Scalar.Gray;

    Cv2.Rectangle(img, confirmButton, bg, -1);
    Cv2.Rectangle(img, confirmButton, Scalar.Black, 2);

    double fontScale = img.Width * 0.0012;

    Cv2.PutText(
        img,
        "Prosseguir",
        new Point(
            confirmButton.X + confirmButton.Width / 10,
            confirmButton.Y + confirmButton.Height * 2 / 3
        ),
        HersheyFonts.HersheySimplex,
        fontScale,
        Scalar.White,
        2
    );
}


    static bool IsInsideConfirm(Point p)
    {
        return confirmButton.Contains(p);
    }

    // =========================
    // MOUSE
    // =========================
    static void MouseCallback(
        MouseEventTypes ev,
        int x,
        int y,
        MouseEventFlags flags,
        IntPtr userdata)
    {
        var mouse = new Point(x, y);

        // clique no botão
        if (ev == MouseEventTypes.LButtonDown && IsInsideConfirm(mouse))
        {
            if (quad != null)
                confirmed = true;
            return;
        }

        // desenho inicial
        if (quad == null && !editingCorner)
        {
            if (ev == MouseEventTypes.LButtonDown)
            {
                startPoint = mouse;
                endPoint = mouse;
                drawing = true;
            }
            else if (ev == MouseEventTypes.MouseMove && drawing)
            {
                endPoint = mouse;
            }
            else if (ev == MouseEventTypes.LButtonUp && drawing)
            {
                drawing = false;
                quad = GetRectPoints(startPoint, endPoint);
            }
            return;
        }

        // edição
        if (ev == MouseEventTypes.LButtonDown)
        {
            int hit = HitTestCorner(mouse, quad);
            if (hit != -1)
            {
                activeCorner = hit;
                editingCorner = true;
            }
        }
        else if (ev == MouseEventTypes.MouseMove && editingCorner)
        {
            quad[activeCorner] = mouse;
        }
        else if (ev == MouseEventTypes.LButtonUp && editingCorner)
        {
            editingCorner = false;
            activeCorner = -1;
        }
    }

    // =========================
    // WARP
    // =========================
static Mat WarpFromQuad(Mat src, Point[] quad)
{
    double w1 = Distance(quad[0], quad[1]);
    double w2 = Distance(quad[2], quad[3]);
    double h1 = Distance(quad[1], quad[2]);
    double h2 = Distance(quad[3], quad[0]);

    int width = (int)Math.Max(w1, w2);
    int height = (int)Math.Max(h1, h2);

    Point2f[] srcPts =
    {
        new Point2f(quad[0].X, quad[0].Y),
        new Point2f(quad[1].X, quad[1].Y),
        new Point2f(quad[2].X, quad[2].Y),
        new Point2f(quad[3].X, quad[3].Y)
    };

    Point2f[] dstPts =
    {
        new Point2f(0, 0),
        new Point2f(width, 0),
        new Point2f(width, height),
        new Point2f(0, height)
    };

    Mat M = Cv2.GetPerspectiveTransform(srcPts, dstPts);
    Mat dst = new();

    Cv2.WarpPerspective(src, dst, M, new Size(width, height));

    return dst;
}

    // =========================
    // UTIL
    // =========================
    static Point[] GetRectPoints(Point p1, Point p2)
    {
        int left = Math.Min(p1.X, p2.X);
        int right = Math.Max(p1.X, p2.X);
        int top = Math.Min(p1.Y, p2.Y);
        int bottom = Math.Max(p1.Y, p2.Y);

        return new[]
        {
            new Point(left, top),
            new Point(right, top),
            new Point(right, bottom),
            new Point(left, bottom)
        };
    }

    static void DrawQuad(Mat img, Point[] pts, Scalar color)
    {
        for (int i = 0; i < 4; i++)
            Cv2.Line(img, pts[i], pts[(i + 1) % 4], color, 2);
    }

    static int HitTestCorner(Point mouse, Point[] pts, int radius = 10)
    {
        for (int i = 0; i < pts.Length; i++)
        {
            double dx = mouse.X - pts[i].X;
            double dy = mouse.Y - pts[i].Y;
            if (Math.Sqrt(dx * dx + dy * dy) <= radius)
                return i;
        }
        return -1;
    }

    static double Distance(Point a, Point b)
{
    double dx = a.X - b.X;
    double dy = a.Y - b.Y;
    return Math.Sqrt(dx * dx + dy * dy);
}
static Rect GetConfirmButtonRect(Mat img)
{
    int w = img.Width;
    int h = img.Height;

    int btnWidth  = (int)(w * 0.20);
    int btnHeight = (int)(h * 0.06);

    int margin = (int)(w * 0.03);

    return new Rect(
        w - btnWidth - margin,
        h - btnHeight - margin,
        btnWidth,
        btnHeight
    );
}

}
