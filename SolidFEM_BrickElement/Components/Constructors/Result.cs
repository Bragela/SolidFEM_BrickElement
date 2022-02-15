using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Result : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Result class.
        /// </summary>
        public Result()
          : base("Result", "Result",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMatrixParameter("Displacements", "D", "List of displacements", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "P", "List of new points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stresses", "s", "List of stresses", GH_ParamAccess.tree);
            pManager.AddMatrixParameter("Strains", "e", "List of strains", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "M", "Deformed mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "R", "A ResultClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Grasshopper.DataTree<double> disp = new Grasshopper.DataTree<double>();
            List<Point3d> pts = new List<Point3d>();
            Grasshopper.DataTree<double> stresses = new Grasshopper.DataTree<double>();
            Grasshopper.DataTree<double> strains = new Grasshopper.DataTree<double>();
            Mesh mesh = new Mesh();

            DA.GetData(0, ref disp);
            DA.GetDataList(0, pts);
            DA.GetData(2, ref stresses);
            DA.GetData(3, ref strains);
            DA.GetData(4, ref mesh);

            //code

            ResultClass res = new ResultClass(disp, pts, stresses, strains, mesh);

            //outputs
            DA.SetData(0, res);
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
            get { return new Guid("6B0768E7-A517-43CA-9B2A-3E660E7C828B"); }
        }
    }
}