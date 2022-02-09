using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components.Deconstructors
{
    public class DeconstructMaterial : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructMaterial class.
        /// </summary>
        public DeconstructMaterial()
          : base("DeconstructMaterial", "DeconstructMaterial",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "A MaterialClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Material name", GH_ParamAccess.item);
            pManager.AddNumberParameter("Youngs Modulus", "E", "The materials youngs modulus", GH_ParamAccess.item);
            pManager.AddNumberParameter("Poissons ratio", "v", "The materials poissons ratio", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            MaterialClass mat = new MaterialClass();
            DA.GetData(0, ref mat);

            //outputs
            DA.SetData(0, mat.Name);
            DA.SetData(1, mat.eModulus);
            DA.SetData(2, mat.pRatio);
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
            get { return new Guid("6F40B140-3093-4003-9DA2-5EFF8A831753"); }
        }
    }
}