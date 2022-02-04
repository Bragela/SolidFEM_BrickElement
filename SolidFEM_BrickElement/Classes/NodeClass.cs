using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class NodeClass
    {
        public int GlobalID;
        public int LocalID;
        public Point3d Point;

        public NodeClass(int _GlobalID, int _LocalID, Point3d _Point)
        {
            GlobalID = _GlobalID;
            LocalID = _LocalID;
            Point = _Point;
        }
    }
}
