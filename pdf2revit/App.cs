using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace pdf2revit
{
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
        public Result OnStartup(UIControlledApplication application)
        {
            if (!AddMenu(application, string.Empty))
                return Result.Failed;

            return Result.Succeeded;
        }

        private bool AddMenu(UIControlledApplication application, string tabname)
        {
            try
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var resourceString = "pack://application:,,,/pdf2revit;component/Resources/Images/Ribbon/";
                var rb = GetRibbonPanel(application, "pdf2revit", tabname);
                rb.AddItem(
                    new PushButtonData("cmdPDFToLines", "PDF\nTo lines", assemblyPath, "pdf2revit.AppCommands.CmdPDFToLines")
                    {
                        ToolTip = "Export PDF to model lines",
                        LargeImage = new BitmapImage(new Uri(string.Concat(resourceString, "32x32/PDFToLines.png")))
                    });
                rb.AddItem(
                    new PushButtonData("cmdDWGToLines", "DWG\nTo lines", assemblyPath, "pdf2revit.AppCommands.CmdDWGToLines")
                    {
                        ToolTip = "Export DWG to model lines",
                        LargeImage = new BitmapImage(new Uri(string.Concat(resourceString, "32x32/DWGToLines.png")))
                    });

            }
            catch
            {
                return false;
            }

            return true;
        }

        //get ribbon panel by ribbon and tab name
        private RibbonPanel GetRibbonPanel(UIControlledApplication application, string ribbonName, string tabName = null)
        {
            IList<RibbonPanel> ribbonPanels;
            if (!string.IsNullOrEmpty(tabName))
            {
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch { }
                ribbonPanels = application.GetRibbonPanels(tabName);
            }
            else
                ribbonPanels = application.GetRibbonPanels();

            RibbonPanel ribbonPanel = null;
            foreach (RibbonPanel rp in ribbonPanels)
            {
                if (rp.Name.Equals(ribbonName))
                {
                    ribbonPanel = rp;
                    break;
                }
            }

            if (ribbonPanel == null)
                ribbonPanel = (string.IsNullOrEmpty(tabName)) ?
                    application.CreateRibbonPanel(ribbonName) :
                    application.CreateRibbonPanel(tabName, ribbonName);

            return ribbonPanel;
        }
    }
}
