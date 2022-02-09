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
        public List<double> displacements;
        public List<double> stresses;
        public List<double> strains;
        public Mesh mesh;

        //Constructors
        public ResultClass()
        {

        }

        public ResultClass(List<double> _disp, List<double> _stresses, List<double> _strains, Mesh _mesh)
        {
            displacements = _disp;
            stresses = _stresses;
            strains = _strains;
            mesh = _mesh; 
        }
    }
}
