using Org.BouncyCastle.Bcpg;
using System.Collections.Generic;

namespace pdf2cad.Core
{
    public class LineData
    {
        public List<PointData> LinePoints { get; set; }
    }

    public class PointData
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
