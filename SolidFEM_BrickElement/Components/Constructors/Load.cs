using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Load : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Load class.
        /// </summary>
        public Load()
          : base("Load", "Load",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "V", "Vector containing forces in X, Y and Z direction", GH_ParamAccess.item);
            pManager.AddPointParameter("Point","P","The point where the load is applied",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "L", "LoadClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Vector3d vec = new Vector3d();
            Point3d pt = new Point3d();

            DA.GetData(0, ref vec);
            DA.GetData(1, ref pt);

            //code

            LoadClass Load = new LoadClass(vec, pt);

            //outputs
            DA.SetData(0, Load);
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
            get { return new Guid("4B213A10-1399-4EDC-8DDA-6B8763F2044D"); }
        }
    }
}