using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components.Deconstructors
{
    public class DeconstructLoad : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructLoad class.
        /// </summary>
        public DeconstructLoad()
          : base("DeconstructLoad", "DeconstructLoad",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "L", "A LoadClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("LoadVector", "V", "Vector containing forces in X, Y and  direction", GH_ParamAccess.item);
            pManager.AddPointParameter("LoadPoint", "P", "The point where the load is applied", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            LoadClass load = new LoadClass();
            DA.GetData(0, ref load);


            //outputs

            DA.SetData(0, load.loadVector);
            DA.SetData(1, load.loadPoint);
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
            get { return new Guid("98A5B2EA-3248-4B14-BE2E-4F4DC3313243"); }
        }
    }
}