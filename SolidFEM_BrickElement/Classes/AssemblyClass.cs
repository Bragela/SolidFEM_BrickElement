using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class AssemblyClass
    {
        //Properties

        public List<ElementClass> elements;
        public List<LoadClass> loads;
        public MaterialClass material;
        

        //Constructors

        public AssemblyClass()
        {

        }

        public AssemblyClass(List<ElementClass> _elems, List<LoadClass> _loads, MaterialClass _mat)
        {
            elements = _elems;
            loads = _loads;
            material = _mat;
        }
    }
}
