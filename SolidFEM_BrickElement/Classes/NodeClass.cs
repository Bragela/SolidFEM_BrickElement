using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class NodeClass
    {
        //Properties

        public int GlobalID;
        public int LocalID;
        public Point3d Point;
        public SupportClass support;
        

        //Constructors

        public NodeClass()
        {

        }

        public NodeClass(int _GlobalID, int _LocalID, Point3d _Point, SupportClass _support)
        {
            GlobalID = _GlobalID;
            LocalID = _LocalID;
            Point = _Point;
            support = _support;
        }

        
        
 
    }
}
