using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public Support()
          : base("Support", "Nickname",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("","","",GH_ParamAccess.item);
            pManager.AddBooleanParameter("Translation X", "Tx", "Translation in X direction (true = fixed, false = free)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Translation Y", "Ty", "Translation in Y direction (true = fixed, false = free)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Translation Z", "Tz", "Translation in Z direction (true = fixed, false = free)", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "S", "A SupportClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Point3d pt = new Point3d();
            Boolean Tx = true;
            Boolean Ty = true;
            Boolean Tz = true;

            DA.GetData(0, ref pt);
            DA.GetData(1, ref Tx);
            DA.GetData(2, ref Ty);
            DA.GetData(3, ref Tz);

            //code
            SupportClass sup = new SupportClass(pt, Tx, Ty, Tz);

            //outputs
            DA.SetData(0, sup);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F27732FE-3BCC-415D-98EF-9E80340B5770"); }
        }
    }
}