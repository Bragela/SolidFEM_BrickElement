﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class SupportClass
    {
        //Properties
        // True = fixed, false = free

        public Point3d pt;
        public Boolean Tx;
        public Boolean Ty;
        public Boolean Tz;

        //Constructors

        public SupportClass()
        {

        }

        public SupportClass(Point3d _pt, Boolean _Tx, Boolean _Ty, Boolean _Tz)
        {
            pt = _pt;
            Tx = _Tx;
            Ty = _Ty;
            Tz = _Tz;
        }
    }
}
