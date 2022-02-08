using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class LoadClass
    {
        //Properties

        public Vector3d loadVector;
        public Point3d loadPoint;


        //Constructors

        public LoadClass()
        {

        }
        public LoadClass(Vector3d _loadVector, Point3d _loadPoint)
        {
            loadVector = _loadVector;
            loadPoint = _loadPoint;
        }


    }
}
