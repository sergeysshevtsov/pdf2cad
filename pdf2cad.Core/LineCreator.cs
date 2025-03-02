using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using TallComponents.PDF;
using TallComponents.PDF.Shapes;

namespace pdf2cad.Core
{
    public class LineCreator
    {
        private readonly Document document;
        private readonly StreamWriter outStream;
        private double width, height = 0;

        Page page;
        public LineCreator(Document document, System.IO.StreamWriter outStream)
        {
            this.document = document;
            page = document.Pages[0];

            this.outStream = outStream;
        }

        public List<LineData> GetPdfLines()
        {
            var result = new List<LineData>();
            WriteShape(result, page.CreateShapes(), Common.GetViewerTransform(page, outStream != null));
            return result;
        }

        public void CreateSvg()
        {
            width = page.Orientation == Orientation.Rotate0 || page.Orientation == Orientation.Rotate180 ? page.Width : page.Height;
            height = page.Orientation == Orientation.Rotate0 || page.Orientation == Orientation.Rotate180 ? page.Height : page.Width;
            outStream?.Write("<!DOCTYPE html>\n<html>\n<body>\n<svg width=\"{0}\" height=\"{1}\">\n", width, height);
            WriteShape(null, page.CreateShapes(), Common.GetViewerTransform(page, outStream != null));
            outStream?.Write("</svg></body></html>");
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
            outStream?.Write("<path stroke=\"blue\" stroke-wdith=\"3\" fill=\"none\" d=\"");
            WriteFreeHandPaths(linesList, freeHandShape.Paths, transform);
            outStream?.Write("\"/>\n");
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

            if (linesList == null)
                return;

            if (points != null || points?.Count != 0)
                if (points.Count > 2)
                    for (int i = 0; i < points.Count - 1; i++)
                        linesList.Add(new LineData()
                        {
                            LinePoints = new List<PointData>
                            {
                                new PointData() { X = points[i].X, Y = points[i].Y },
                                new PointData() { X = points[i + 1].X, Y = points[i + 1].Y }
                            }
                        });
                else
                    linesList.Add(new LineData() { LinePoints = points });
        }

        void WriteFreeHandPath(FreeHandPath path, Matrix transform, List<PointData> points)
        {
            foreach (var segment in path.Segments)
            {
                if (segment is FreeHandStartSegment)
                {
                    var s = (FreeHandStartSegment)segment;
                    outStream?.Write($"M {WriteSvgPoint(s.X, s.Y, transform)} ");
                    points?.Add(WritePoint(s.X, s.Y, transform));
                }
                else if (segment is FreeHandLineSegment)
                {
                    var s = (FreeHandLineSegment)segment;
                    outStream?.Write($"L {WriteSvgPoint(s.X1, s.Y1, transform)} ");
                    points?.Add(WritePoint(s.X1, s.Y1, transform));
                }
                else if (segment is FreeHandBezierSegment)
                {
                    var s = (FreeHandBezierSegment)segment;
                    outStream?.Write($"C {WriteSvgPoint(s.X1, s.Y1, transform)} {WriteSvgPoint(s.X2, s.Y2, transform)} {WriteSvgPoint(s.X2, s.Y2, transform)} ");
                    points?.Add(WritePoint(s.X1, s.Y1, transform));
                    points?.Add(WritePoint(s.X2, s.Y2, transform));
                    points?.Add(WritePoint(s.X3, s.Y3, transform));
                }
            }

            if (path.Closed)
            {
                outStream?.Write("Z ");
            }
        }

        private PointData WritePoint(double x, double y, Matrix transform)
        {
            PointF[] points = { new PointF((float)x, (float)y) };
            transform.TransformPoints(points);
            return new PointData() { X = points[0].X, Y = points[0].Y };
        }

        private string WriteSvgPoint(double x, double y, Matrix transform)
        {
            PointF[] points = { new PointF((float)x, (float)y) };
            transform.TransformPoints(points);
            return String.Format("{0} {1}", points[0].X, points[0].Y);
        }

    }
}
