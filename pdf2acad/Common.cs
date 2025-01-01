using System.Drawing.Drawing2D;

namespace pdf2acad
{
    internal class Common
    {
        public static Matrix GetViewerTransform(TallComponents.PDF.Page page)
        {
            var mediaBox = page.MediaBox;

            double height;
            double dx, dy;
            int rotate;

            switch (page.Orientation)
            {
                case TallComponents.PDF.Orientation.Rotate0:
                default:
                    height = mediaBox.Height;
                    rotate = 0;
                    dx = 0;
                    dy = 0;
                    break;
                case TallComponents.PDF.Orientation.Rotate90:
                    height = mediaBox.Width;
                    rotate = 90;
                    dx = 0;
                    dy = -mediaBox.Height;
                    break;
                case TallComponents.PDF.Orientation.Rotate180:
                    height = mediaBox.Height;
                    rotate = 180;
                    dx = -mediaBox.Width;
                    dy = -mediaBox.Height;
                    break;
                case TallComponents.PDF.Orientation.Rotate270:
                    height = mediaBox.Width;
                    rotate = 270;
                    dx = -mediaBox.Width;
                    dy = 0;
                    break;
            }

            Matrix matrix = new Matrix();

            // From PDF coordinate system (zero is at the bottom) to screen (zero is at the top)
            matrix.Translate(0, (float)height);
            matrix.Scale(1, -1);

            // Rotation
            matrix.Rotate(rotate);
            matrix.Translate((float)dx, (float)dy);

            // TODO: cropbox translation should be handled as well
            return matrix;
        }
    }
}
