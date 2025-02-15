using Autodesk.Revit.DB;
using System.Linq;

namespace pdf2revit.RevitService
{
    public static class ModelLineService
    {
        public static ElementId GetOrCreateModelLineType(this Document document, string lineTypeName)
        {
            var styles = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToElements();
            foreach (Element element in styles)
            {
                var style = element as GraphicsStyle;
                if (style.Name == lineTypeName)
                    return style.Id;
            }

            using (var tr = new Transaction(document, "Create Line Type"))
            {
                var lineStylePatterns = new FilteredElementCollector(document)
                   .OfClass(typeof(LinePatternElement))
                   .Cast<LinePatternElement>()
                   .ToList();

                var lineStylePattern = lineStylePatterns.First<LinePatternElement>(linePattern => linePattern.Name == "Center");

                if (lineStylePattern == null)
                    return ElementId.InvalidElementId;

                Categories categories = document.Settings.Categories;
                Category category = categories.get_Item(BuiltInCategory.OST_Lines);

                tr.Start();
                Category newLineStyle = categories.NewSubcategory(category, lineTypeName);
                document.Regenerate();

                newLineStyle.SetLineWeight(1, GraphicsStyleType.Projection);
                newLineStyle.LineColor = new Color(0xFF, 0x00, 0x00);
                newLineStyle.SetLinePatternId(lineStylePattern.Id, GraphicsStyleType.Projection);

                tr.Commit();

                styles = new FilteredElementCollector(document).OfClass(typeof(GraphicsStyle)).ToElements();
                foreach (Element element in styles)
                {
                    var style = element as GraphicsStyle;
                    if (style.Name == lineTypeName)
                        return style.Id;
                }
            }

            return ElementId.InvalidElementId;
        }


        public static void CreateModelLine(this Document document, Line line, ElementId elementId)
        {
            DetailCurve detailCurve = document.Create.NewDetailCurve(document.ActiveView, line);
            if (document.GetElement(elementId) is GraphicsStyle egs)
                detailCurve.LineStyle = egs;
        }
    }
}
