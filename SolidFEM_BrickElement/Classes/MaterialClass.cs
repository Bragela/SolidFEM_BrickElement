using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class MaterialClass
    {
        //Properties

        public string Name;
        public int eModulus;
        public double pRatio;

        //Constructor

        public MaterialClass()
        {

        }
        public MaterialClass(string _Name, int _eModulus, double _pRatio)
        {
            Name = _Name;
            eModulus = _eModulus;
            pRatio = _pRatio;
        }
    }
}
