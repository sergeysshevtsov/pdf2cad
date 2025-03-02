using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using pdf2cad.Core;
using System.IO;
using System.Windows.Forms;
using TallComponents.PDF;

namespace pdf2acad
{
    public class App
    {
        [CommandMethod("pdf2acad", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Modal)]
        public void pdf2acad()
        {
            Autodesk.AutoCAD.ApplicationServices.Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = document.Database;

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select PDF file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var pdfBytes = File.ReadAllBytes(openFileDialog.FileName);
                var pdfDocument = new Document(new MemoryStream(pdfBytes));
                var lineCreator = new LineCreator(pdfDocument, null);
                var lines = lineCreator.GetPdfLines();

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    var btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    foreach (var line in lines)
                    {
                        Entity entity = null;

                        if (line.LinePoints.Count == 2)
                            entity = new Line()
                            {
                                StartPoint = new Autodesk.AutoCAD.Geometry.Point3d(line.LinePoints[0].X, line.LinePoints[0].Y, 0),
                                EndPoint = new Autodesk.AutoCAD.Geometry.Point3d(line.LinePoints[1].X, line.LinePoints[1].Y, 0)
                            };

                        if (line.LinePoints.Count > 2)
                        {
                            Polyline polyline = new Polyline();
                            for (int i = 0; i < line.LinePoints.Count; i++)
                                polyline.AddVertexAt(i, new Autodesk.AutoCAD.Geometry.Point2d(line.LinePoints[i].X, line.LinePoints[i].Y), 0, 0, 0);
                            entity = polyline;
                        }

                        if (entity != null)
                        {
                            btr.AppendEntity(entity);
                            tr.AddNewlyCreatedDBObject(entity, true);
                        }
                    }

                    tr.Commit();
                }
            }
        }
    }
}
