using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SolidFEM_BrickElement
{
    public class SolidFEM_BrickElementInfo : GH_AssemblyInfo
    {
        public override string Name => "SolidFEM_BrickElement";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("7FDCED9D-9AC7-410F-9047-FEC3EA846DB6");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}