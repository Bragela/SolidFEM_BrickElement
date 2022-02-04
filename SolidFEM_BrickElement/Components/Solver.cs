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
            pManager.AddVectorParameter("Forces","F","External loads on the box",GH_ParamAccess.item);
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
            Vector3d loadVec = new Vector3d();
            DA.GetData(0, ref mesh);
            DA.GetData(1, ref loadVec);


            //code

            List<NodeClass> nodes = new List<NodeClass>();

            //Extract mesh vertices
            Point3d[] pts =  mesh.Vertices.ToPoint3dArray();

            //Create a list of ForceClass elements
            List<ForceClass> forces = new List<ForceClass>();

            //Create a force vector with all values set to zero
            Vector3d zeroVec = new Vector3d(0,0,0);

            SupportClass fixd = new SupportClass(false, false, false);
            SupportClass free = new SupportClass(false, false, false);

            //For each point in mesh, create a node with ID, coordinates and type of support (fixed along one side). Create a list of forcevectors
            for (int i = 0; i < pts.Length; i++)
            {
                if (i == 0 || i == 3 || i == 4 || i == 7)
                {
                    nodes.Add(new NodeClass(i, i, pts[i],fixd));
                }
                else
                {
                    nodes.Add(new NodeClass(i, i, pts[i], free));
                }

                if (i == 5 || i == 6)
                {
                    forces.Add(new ForceClass(loadVec, pts[i]));
                }
                else
                {
                    forces.Add(new ForceClass(zeroVec, pts[i]));
                }
            }




            //Creates on element with ID, all nodes and mesh
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