﻿using System;
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
        public Grasshopper.DataTree<double> displacements;
        public Grasshopper.DataTree<double> stresses;
        public Grasshopper.DataTree<double> strains;
        public Mesh mesh;

        //Constructors
        public ResultClass()
        {

        }

        public ResultClass(Grasshopper.DataTree<double> _disp, Grasshopper.DataTree<double> _stresses, Grasshopper.DataTree<double> _strains, Mesh _mesh)
        {
            displacements = _disp;
            stresses = _stresses;
            strains = _strains;
            mesh = _mesh; 
        }
    }
}
