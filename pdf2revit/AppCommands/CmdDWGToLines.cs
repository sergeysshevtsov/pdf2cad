using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using pdf2revit.RevitService;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace pdf2revit.AppCommands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    internal class CmdDWGToLines : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "DWG files (*.dwg)|*.dwg",
                Title = "Select DWG file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var document = commandData.Application.ActiveUIDocument.Document;
                var lineStyleId = document.GetOrCreateModelLineType("pdf2revit_lineStyle");

                if (lineStyleId == ElementId.InvalidElementId)
                    return Result.Failed;

                using (TransactionGroup tg = new TransactionGroup(document, "Create model lines"))
                {
                    tg.Start();
                    var modelLinesToCreate = new List<(XYZ, XYZ)>();
                   
                    using (Transaction tr0 = new Transaction(document, "Import DWG"))
                    {
                        tr0.Start();
                        DWGImportOptions importOptions = new DWGImportOptions
                        {
                            Placement = ImportPlacement.Centered,
                            ColorMode = ImportColorMode.BlackAndWhite,
                            ThisViewOnly = true, 
                            Unit = ImportUnit.Default,
                            VisibleLayersOnly = true,
                            OrientToView = true
                        };

                        var view = document.ActiveView;
                        var result = document.Import(openFileDialog.FileName, importOptions, view, out ElementId importedElementId);

                        if (!result || importedElementId == ElementId.InvalidElementId)
                        {
                            message = "Failed to import DWG file.";
                            return Result.Failed;
                        }

                        var element = document.GetElement(importedElementId);   

                        Options options = new Options
                        {
                            ComputeReferences = true,
                            IncludeNonVisibleObjects = false,
                            View = document.ActiveView
                        };

                        GeometryElement geomElement = element.get_Geometry(options);

                        if (geomElement == null)
                        {
                            TaskDialog.Show("Geometry", "No geometry found.");
                            return Result.Failed;
                        }

                        foreach (GeometryObject geomObj in geomElement)
                        {
                            if (geomObj is GeometryInstance instance)
                            {
                                GeometryElement instGeom = instance.GetInstanceGeometry();
                                foreach (GeometryObject instObj in instGeom)
                                {
                                    if (instObj is Line line1)
                                    {
                                        XYZ startPoint = line1.GetEndPoint(0);
                                        XYZ endPoint = line1.GetEndPoint(1);
                                        modelLinesToCreate.Add((startPoint, endPoint));
                                    }
                                    else if (instObj is Arc arc1)
                                    {
                                        var tes = arc1.Tessellate();
                                        for (int i = 0; i < tes.Count - 1; i++)
                                        {
                                            XYZ startPoint = tes[i];
                                            XYZ endPoint = tes[i + 1];
                                            modelLinesToCreate.Add((startPoint, endPoint));
                                        }
                                    }
                                    else if (instObj is PolyLine pl)
                                    {
                                        var cs = pl.GetCoordinates();
                                        for (int i = 0; i < cs.Count - 1; i++)
                                        {
                                            XYZ startPoint = cs[i];
                                            XYZ endPoint = cs[i + 1];
                                            modelLinesToCreate.Add((startPoint, endPoint));
                                        }
                                    }
                                }
                            }
                        }

                        tr0.RollBack();
                    }

                    using (Transaction tr1 = new Transaction(document, "Create model lines"))
                    {

                        tr1.Start();
                        foreach (var (startPoint, endPoint) in modelLinesToCreate)
                        {
                            var sp = new XYZ(startPoint.X, startPoint.Y, 0);
                            var ep = new XYZ(endPoint.X, endPoint.Y, 0);
                            document.CreateModelLine(Autodesk.Revit.DB.Line.CreateBound(sp, ep), lineStyleId);
                        }
                        tr1.Commit();
                    }

                    tg.Commit();
                }
            }

            return Result.Succeeded;
        }
    }
}
