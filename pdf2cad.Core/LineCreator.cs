using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using TallComponents.PDF;
using TallComponents.PDF.Shapes;

namespace pdf2cad.Core
{
    public class LineCreator
    {
        private readonly Document document;

        public LineCreator(Document document)
        {
            this.document = document;
        }

        public List<LineData> GetPdfLines()
        {
            var result = new List<LineData>();
            var page = document.Pages[0];

            //getting page with and height for Revit cropbox and AutoCAD border
            double viewerWidth = page.Orientation == Orientation.Rotate0 || page.Orientation == Orientation.Rotate180 ? page.Width : page.Height;
            double viewerHeight = page.Orientation == Orientation.Rotate0 || page.Orientation == Orientation.Rotate180 ? page.Height : page.Width;

            WriteShape(result, page.CreateShapes(), Common.GetViewerTransform(page));

            return result;
        }

        private void WriteShape(List<LineData> linesList, Shape shape, Matrix transform)
        {
            // ContentShape is a bse class of most shapes. It has an associated transformation. 
            // First it is merged with the root one.
            // The exact shaped based behavior is handled after that.
            if (shape is TallComponents.PDF.Shapes.ContentShape)
            {
                ContentShape contentshape = (ContentShape)shape;
                Matrix newTransform = contentshape.Transform.CreateGdiMatrix();
                newTransform.Multiply(transform, MatrixOrder.Append);

                if (shape is TallComponents.PDF.Shapes.FreeHandShape)
                {
                    WriteFreeHandShape(linesList, shape as FreeHandShape, newTransform);
                }
                if (shape is TallComponents.PDF.Shapes.ImageShape)
                {
                    //writeImageShape(shape as ImageShape, newTransform);
                }
                else if (shape is TallComponents.PDF.Shapes.ShapeCollection)
                {
                    WriteShapes(linesList, (ShapeCollection)shape, newTransform);
                }
                else if (shape is TallComponents.PDF.Shapes.LayerShape)
                {
                    WriteLayerShape(linesList, (LayerShape)shape, newTransform);
                }
            }
        }

        private void WriteFreeHandShape(List<LineData> linesList, FreeHandShape freeHandShape, Matrix transform)
        {
            WriteFreeHandPaths(linesList, freeHandShape.Paths, transform);
        }

        private void WriteShapes(List<LineData> linesList, ShapeCollection shapes, Matrix transform)
        {
            foreach (var shape in shapes)
                WriteShape(linesList, shape, transform);
        }

        private void WriteLayerShape(List<LineData> linesList, LayerShape shapes, Matrix transform)
        {
            foreach (var shape in shapes)
                WriteShape(linesList, shape, transform);
        }

        private void WriteFreeHandPaths(List<LineData> linesList, FreeHandPathCollection paths, Matrix transform)
        {
            var points = new List<PointData>();
            foreach (var path in paths)
                WriteFreeHandPath(path, transform, points);

            if (points != null || points?.Count != 0)
                linesList.Add(new LineData() { LinePoints = points });
        }

        void WriteFreeHandPath(FreeHandPath path, Matrix transform, List<PointData> points)
        {
            foreach (var segment in path.Segments)
            {
                if (segment is FreeHandStartSegment)
                {
                    var s = (FreeHandStartSegment)segment;
                    points.Add(WritePoint(s.X, s.Y, transform));
                }
                else if (segment is FreeHandLineSegment)
                {
                    var s = (FreeHandLineSegment)segment;
                    points.Add(WritePoint(s.X1, s.Y1, transform));
                }
                else if (segment is FreeHandBezierSegment)
                {
                    //Not sure if I want to bring it to CAD 
                }
            }
        }

        private PointData WritePoint(double x, double y, Matrix transform)
        {
            PointF[] points = { new PointF((float)x, (float)y) };
            transform.TransformPoints(points);
            return new PointData() { X = points[0].X, Y = points[0].Y };
        }
    }
}
