using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components
{
    public class PreviewDispl : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PreviewDispl()
          : base("PreviewDispl", "PDis",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Displ Points", "DPs", "Displacement points", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scale", "Scale", "Scale of displacement", GH_ParamAccess.item,1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            //pManager.AddMeshParameter("Displ Mesh", "DM", "Displacement Mesh", GH_ParamAccess.list);
            pManager.AddMeshParameter("DisplMesh", "DisplMesh", "Displacement Mesh", GH_ParamAccess.list);
            //pManager.AddCurveParameter("Edges", "E", "Edges", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input______________________________________________________________

        

            GH_Structure<GH_Point>  gh_nodes = new GH_Structure<GH_Point>();
            if (!DA.GetDataTree(0, out gh_nodes)) return;

            double scale = new double();
            DA.GetData(1, ref scale);



            //Make the input as list in list

            List<List<Point3d>> DisPts = new List<List<Point3d>>();

            foreach(var lst in gh_nodes.Branches)
            {
                List<Point3d> ElemPts = new List<Point3d>();

                foreach(var gh_node in lst)
                {
                    Point3d pt;
                    if (gh_node.CastTo(out pt))
                    {
                        ElemPts.Add(pt);
                    }
                }
                DisPts.Add(ElemPts);
            }

            List<Point3d> testList = DisPts[0];




            //code______________________________________________________________




            //Constructing mesh from displacement points
            List<Mesh> meshes = new List<Mesh>();
            List<MeshFace> faces = new List<MeshFace>();
            List<Line> lines = new List<Line>();

            List<Polyline> polyL = new List<Polyline>();

            for (int i = 0; i< DisPts.Count; i++)
            {
                List<Point3d> ElNodes = DisPts[i];
                
                Mesh mesh = new Mesh();

                mesh.Vertices.Add(ElNodes[0] * scale);
                mesh.Vertices.Add(ElNodes[1] * scale);
                mesh.Vertices.Add(ElNodes[2] * scale);
                mesh.Vertices.Add(ElNodes[3] * scale);
                mesh.Vertices.Add(ElNodes[4] * scale);
                mesh.Vertices.Add(ElNodes[5] * scale);
                mesh.Vertices.Add(ElNodes[6] * scale);
                mesh.Vertices.Add(ElNodes[7] * scale);

         
                    mesh.Faces.AddFace(new MeshFace(0, 1, 2, 3));
                    mesh.Faces.AddFace(new MeshFace(0, 1, 5, 4));
                    mesh.Faces.AddFace(new MeshFace(1, 2, 6, 5));
                    mesh.Faces.AddFace(new MeshFace(2, 3, 7, 6));
                    mesh.Faces.AddFace(new MeshFace(3, 0, 4, 7));
                    mesh.Faces.AddFace(new MeshFace(4, 5, 6, 7));

                mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(100, 74, 220,255));



                //mesh.GetBoundingBox(true);

                //Polyline[] polylines = mesh.GetNakedEdges();

                //Polyline[] polylines1 = mesh.GetNakedEdges();
                //var polylines = polylines1;
                //foreach (var polyline in polylines)
                //{
                //    polyL.Add(polyline);
                //}


                meshes.Add(mesh);
               




            


        }
        





            //output______________________________________________________________

            //DA.SetDataList(0, dummy);
            DA.SetDataList(0, meshes);
            //DA.SetData(1, polyL);
        //DA.SetDataList(1, info);


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
            get { return new Guid("8C5B3217-5F55-46EB-9089-0E3F9535E9E0"); }
        }
    }
}