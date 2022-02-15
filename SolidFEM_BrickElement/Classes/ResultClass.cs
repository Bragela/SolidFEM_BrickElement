using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class ResultClass
    {
        //Properties
        public List<List<List<double>>> displacements;
        public List<Point3d> pts;
        public List<List<List<double>>> stresses;
        public List<List<List<double>>> strains;
        public Mesh mesh;

        //Constructors
        public ResultClass()
        {

        }

        public ResultClass(List<List<List<double>>> _disp, List<Point3d> _pts, List<List<List<double>>> _stresses, List<List<List<double>>> _strains, Mesh _mesh)
        {
            displacements = _disp;
            pts = _pts;
            stresses = _stresses;
            strains = _strains;
            mesh = _mesh; 
        }
    }
}
