using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class ResultClass
    {
        //Properties
        public GH_Structure<GH_Number> displacements;
        public GH_Structure<GH_Number> stresses;
        public GH_Structure<GH_Number> strains;
        public GH_Structure<GH_Point> new_pts;
        public GH_Structure<GH_Point> old_pts;
        public Mesh mesh;

        //Constructors
        public ResultClass()
        {

        }

        public ResultClass(GH_Structure<GH_Number> _disp, GH_Structure<GH_Number> _stresses, GH_Structure<GH_Number> _strains, GH_Structure<GH_Point> _npts, GH_Structure<GH_Point> _opts, Mesh _mesh)
        {
            displacements = _disp;
            stresses = _stresses;
            strains = _strains;
            new_pts = _npts;
            old_pts = _opts;
            mesh = _mesh;
            
        }
    }
}
