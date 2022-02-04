using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    internal class ElementClass
    {
        public int ID;
        public List<NodeClass> Nodes;
        public Mesh Mesh;

        public ElementClass(int _ID, List<NodeClass> _Nodes, Mesh _mesh)
        {
            ID = _ID;
            Nodes = _Nodes;
            Mesh = _mesh;
        }
    }
}
