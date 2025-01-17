using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using TallComponents.PDF.Shapes;

namespace pdf2acad
{
    public class LineCreator
    {
        private readonly BlockTable bt;
        private readonly BlockTableRecord btr;
        private readonly Transaction tr;

        public LineCreator(Transaction tr, Database db)
        {
            this.tr = tr;
            bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
        }

        public void WriteShape(TallComponents.PDF.Shapes.Shape shape, Matrix transform)
        {
            // ContentShape is a bse class of most shapes. It has an associated transformation. 
            // First it is merged with the root one.
            // The exact shaped based behavior is handled after that.
            if (shape is TallComponents.PDF.Shapes.ContentShape)
            {
                ContentShape contentshape = (ContentShape)shape;
                Matrix newTransform = contentshape.Transform.CreateGdiMatrix();
                //TODO: rotation depends on page orientation 
                //have to work on matrix orientation to bring lines with a correct placement
                newTransform.Rotate(180);
                var tr = contentshape.Transform;
                newTransform.Multiply(transform, MatrixOrder.Append);

                if (shape is TallComponents.PDF.Shapes.FreeHandShape)
                {
                    WriteFreeHandShape(shape as FreeHandShape, newTransform);
                }
                if (shape is TallComponents.PDF.Shapes.ImageShape)
                {
                    //writeImageShape(shape as ImageShape, newTransform);
                }
                else if (shape is TallComponents.PDF.Shapes.ShapeCollection)
                {
                    WriteShapes((ShapeCollection)shape, newTransform);
                }
                else if (shape is TallComponents.PDF.Shapes.LayerShape)
                {
                    WriteLayerShape((LayerShape)shape, newTransform);
                }
            }
        }

        private void WriteFreeHandShape(FreeHandShape freeHandShape, Matrix transform)
        {
            WriteFreeHandPaths(freeHandShape.Paths, transform);
        }

        private void WriteFreeHandPaths(FreeHandPathCollection paths, Matrix transform)
        {
            var points = new List<Point2d>();
            foreach (var path in paths)
                WriteFreeHandPath(path, transform, points);

            Polyline polyline = new Polyline();
            for (int i = 0; i < points.Count; i++)
            {
                polyline.AddVertexAt(i, points[i], 0, 0, 0); 
            }

            btr.AppendEntity(polyline);
            tr.AddNewlyCreatedDBObject(polyline, true);
        }

        void WriteFreeHandPath(FreeHandPath path, Matrix transform, List<Point2d> points)
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

        private Point2d WritePoint(double x, double y, Matrix transform)
        {
            PointF[] points = { new PointF((float)x, (float)y) };
            transform.TransformPoints(points);
            return new Point2d(points[0].X, points[0].Y);
        }

        private void WriteShapes(ShapeCollection shapes, Matrix transform)
        {
            foreach (var shape in shapes)
                WriteShape(shape, transform);
        }

        private void WriteLayerShape(LayerShape shapes, Matrix transform)
        {
            foreach (var shape in shapes)
                WriteShape(shape, transform);
        }
    }
}
