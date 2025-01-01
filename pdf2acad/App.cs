using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
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
                var page = pdfDocument.Pages[0];

                var viewerTransform = Common.GetViewerTransform(pdfDocument.Pages[0]);
               
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LineCreator creator = new LineCreator(tr, db);
                    var shapes = page.CreateShapes();
                    creator.WriteShape(shapes, viewerTransform);
                    tr.Commit();
                }
            }
        }
    }
}
