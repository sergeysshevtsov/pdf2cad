using System.Collections.Generic;

namespace pdf2cad.Core
{
    public class LineData
    {
        public List<PointData> LinePoints { get; set; }
        
        public bool IsClosed { get; set; }
    }

    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string SvgString { get; set; }
    }

    public enum FileType
    {
        CAD = 1,
        Svg = 2
    }
}
