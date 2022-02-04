using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class GH_Mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Mesh class.
        /// </summary>
        public GH_Mesh()
          : base("GH_Mesh", "GH_Mesh",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep to be meshed", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh","M","Meshed brep",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input
            Brep br = new Brep();
            DA.GetData(0, ref br);

            //code
            Mesh mesh = new Mesh();

            BoundingBox bB = br.GetBoundingBox(true);       //Convert brep to boundbox to easily extract cornerpoints

            //Add cornerpoints from brep to mesh vertices
            foreach (Point3d pt in bB.GetCorners())
            {
                mesh.Vertices.Add(pt);
            }
            
            //Construct mesh faces from vertices
            mesh.Faces.AddFace(new MeshFace(0, 1, 2, 3));
            mesh.Faces.AddFace(new MeshFace(0, 1, 5, 4));
            mesh.Faces.AddFace(new MeshFace(1, 2, 6, 5));
            mesh.Faces.AddFace(new MeshFace(2, 3, 7, 6));
            mesh.Faces.AddFace(new MeshFace(3, 0, 4, 7));
            mesh.Faces.AddFace(new MeshFace(4, 5, 6, 7));


            //output
            DA.SetData(0, mesh);
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
            get { return new Guid("7DB72C1D-A0EA-4D78-BCAC-AEF689A8301B"); }
        }
    }
}