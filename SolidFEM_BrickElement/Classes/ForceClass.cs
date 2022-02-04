using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class ForceClass
    {
        //Properties

        public Vector3d loadVector;
        public Point3d loadPoint;


        //Constructors
        public ForceClass(Vector3d _loadVector, Point3d _loadPoint)
        {
            loadVector = _loadVector;
            loadPoint = _loadPoint;
        }


    }
}
