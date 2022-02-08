using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Material : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Material class.
        /// </summary>
        public Material()
          : base("Material", "Material",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name","N","Name of material",GH_ParamAccess.item);
            pManager.AddNumberParameter("Youngs Modulus", "E", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Poissions ratio", "v", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "MaterialClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            string Name = ("Name");
            int eModulus = 200000;
            double pRatio = 0.3;

            DA.GetData(0, ref Name);
            DA.GetData(1, ref eModulus);
            DA.GetData(2, ref pRatio);

            //code

            MaterialClass material = new MaterialClass(Name, eModulus, pRatio);

            //outputs

            DA.SetData(0, material);
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
            get { return new Guid("E94D88BB-D602-4F5B-8F50-ED560BB525F9"); }
        }
    }
}