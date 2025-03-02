using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using pdf2cad.Core;
using pdf2revit.RevitService;
using System.IO;
using System.Windows.Forms;

namespace pdf2revit.AppCommands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    internal class CmdPDFToLines : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Select PDF file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var pdfBytes = File.ReadAllBytes(openFileDialog.FileName);
                var pdfDocument = new TallComponents.PDF.Document(new MemoryStream(pdfBytes));
                var lineCreator = new LineCreator(pdfDocument, null);
                var lines = lineCreator.GetPdfLines();

                var document = commandData.Application.ActiveUIDocument.Document;
                var lineStyleId = document.GetOrCreateModelLineType("pdf2revit_lineStyle");

                if (lineStyleId == ElementId.InvalidElementId) 
                    return Result.Failed;

                using (Transaction tr = new Transaction(document, "Create model lines"))
                {
                    tr.Start();
                    foreach (var line in lines)
                    {
                        if (line.LinePoints.Count >= 2)
                            for (var i = 0; i < line.LinePoints.Count - 1; i++)
                            {
                                var startPoint = line.LinePoints[i];
                                var endPoint = line.LinePoints[i + 1];

                                var sp = new XYZ(startPoint.X, startPoint.Y, 0);
                                var ep = new XYZ(endPoint.X, endPoint.Y, 0);

                                document.CreateModelLine(Autodesk.Revit.DB.Line.CreateBound(sp, ep), lineStyleId);
                            }
                    }

                    tr.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
