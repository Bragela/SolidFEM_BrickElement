using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class DeconstructResult : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructResult class.
        /// </summary>
        public DeconstructResult()
          : base("DeconstructResult", "DeconstructResult",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "R", "A ResultClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacements", "D", "List of displacements", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stresses", "s", "List of stresses", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strains", "e", "List of strains", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "Deformed mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input
            ResultClass res = new ResultClass();
            DA.GetData(0, ref res);

            //outputs
            DA.SetDataList(0, res.displacements);
            DA.SetDataList(1, res.stresses);
            DA.SetDataList(2, res.strains);
            DA.SetData(3, res.mesh);
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
            get { return new Guid("74411151-1F39-40E0-952E-73E8B4BDC719"); }
        }
    }
}