using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components.Deconstructors
{
    public class DeconstructSupport : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructSupport class.
        /// </summary>
        public DeconstructSupport()
          : base("DeconstructSupport", "DeconstructSupport",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Support", "S", "A SupportClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Tx", "Tx", "Type of support in X direction (true = fixed, false = free)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Ty", "Ty", "Type of support in Y direction (true = fixed, false = free)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Tz", "Tz", "Type of support in Z direction (true = fixed, false = free)", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            SupportClass sup = new SupportClass();
            DA.GetData(0, ref sup);

            //outputs
            DA.SetData(0, sup.Tx);
            DA.SetData(0, sup.Ty);
            DA.SetData(0, sup.Tz);

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
            get { return new Guid("14FC4728-7ACF-4004-8313-018CEFB7187E"); }
        }
    }
}