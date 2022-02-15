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
        //Properties

        public int ID;
        public List<NodeClass> Nodes;
        public Mesh Mesh;

        //Constructors

        public ElementClass()
        {

        }

        public ElementClass(int _ID, List<NodeClass> _Nodes)
        {
            ID = _ID;
            Nodes = _Nodes;
            
        }

        public ElementClass(int _ID, List<NodeClass> _Nodes, Mesh _mesh)
        {
            ID = _ID;
            Nodes = _Nodes;
            Mesh = _mesh;
        }
    }
}
