using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Solver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Solver class.
        /// </summary>
        public Solver()
          : base("Solver", "Solver",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to be calculated", GH_ParamAccess.item);
            pManager.AddGenericParameter("Forces","F","External loads on the box",GH_ParamAccess.item);
            pManager.AddGenericParameter("Supports", "S", "The boxes supports", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "The calculated element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            Mesh mesh = new Mesh();
            DA.GetData(0, ref mesh);

            //code

            List<NodeClass> nodes = new List<NodeClass>();

            Point3d[] pts =  mesh.Vertices.ToPoint3dArray();

            for (int i = 0; i < pts.Length; i++)
            {
                nodes.Add(new NodeClass(i, i, pts[i]));
            }

            ElementClass elem = new ElementClass(0, nodes, mesh);

            //outputs

            DA.SetData(0, elem);
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
            get { return new Guid("B3942AD6-35E6-42CE-A01B-EC3A79F61EE5"); }
        }
    }
}