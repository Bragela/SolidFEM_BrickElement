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
            pManager.AddNumberParameter("Displacments", "Disp", "Displacements", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Scale", "Scale", "Scale of displacement", GH_ParamAccess.item,1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            
            pManager.AddMeshParameter("DisplMesh", "DisplMesh", "Displacement Mesh", GH_ParamAccess.list);

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

            GH_Structure<GH_Number> gh_disps = new GH_Structure<GH_Number>();
            if (!DA.GetDataTree(1, out gh_disps)) return;


            double sc_1 = new double();
            DA.GetData(2, ref sc_1);
            double sc = sc_1;



            //Make the input as list in list

            //#1 Displacement points

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

            //#2 Displacement values

            List<List<double>> disp_vals = new List<List<double>>();


            foreach (var dlst in gh_disps.Branches)
            {
                List<double> ElemDisps = new List<double>();

                foreach (var gh_disp in dlst)
                {
                    double dval = gh_disp.Value;

                    ElemDisps.Add(dval);
                }

                disp_vals.Add(ElemDisps);
            }





            //code______________________________________________________________




            //Constructing mesh from displacement points
            List<Mesh> meshes = new List<Mesh>();
            
            List<Point3d> testpoints = new List<Point3d>();

           

            for (int i = 0; i< DisPts.Count; i++)
            {
                List<Point3d> ElNodes = DisPts[i];

                int j = ElNodes.Count * i;

                Mesh mesh = new Mesh();

                
                Point3d no0 = new Point3d(ElNodes[0].X + disp_vals[j+0][0] * (sc - 1), ElNodes[0].Y + disp_vals[j+0][1] * (sc - 1), ElNodes[0].Z + disp_vals[j+0][2] * (sc - 1));
                mesh.Vertices.Add(no0);

                Point3d no1 = new Point3d(ElNodes[1].X + disp_vals[j+1][0] * (sc - 1), ElNodes[1].Y + disp_vals[j+1][1] * (sc - 1), ElNodes[1].Z + disp_vals[j+1][2] * (sc - 1));
                mesh.Vertices.Add(no1);

                Point3d no2 = new Point3d(ElNodes[2].X + disp_vals[j+2][0] * (sc - 1), ElNodes[2].Y+ disp_vals[j+2][1] * (sc - 1), ElNodes[2].Z + disp_vals[j+2][2] * (sc - 1));
                mesh.Vertices.Add(no2);

                Point3d no3 = new Point3d(ElNodes[3].X + disp_vals[j+3][0] * (sc - 1), ElNodes[3].Y + disp_vals[j+3][1] * (sc - 1), ElNodes[3].Z + disp_vals[j+3][2] * (sc - 1));
                mesh.Vertices.Add(no3);

                Point3d no4 = new Point3d(ElNodes[4].X + disp_vals[j+4][0] * (sc - 1), ElNodes[4].Y + disp_vals[j+4][1] * (sc - 1), ElNodes[4].Z + disp_vals[j+4][2] * (sc - 1));
                mesh.Vertices.Add(no4);

                Point3d no5 = new Point3d(ElNodes[5].X + disp_vals[j+5][0] * (sc - 1), ElNodes[5].Y + disp_vals[j+5][1] * (sc - 1), ElNodes[5].Z + disp_vals[j+5][2] * (sc - 1));
                mesh.Vertices.Add(no5);

                Point3d no6 = new Point3d(ElNodes[6].X + disp_vals[j+6][0] * (sc - 1), ElNodes[6].Y + disp_vals[j+6][1] * (sc - 1), ElNodes[6].Z + disp_vals[j+6][2] * (sc - 1));
                mesh.Vertices.Add(no6);

                Point3d no7 = new Point3d(ElNodes[7].X + disp_vals[j+7][0] * (sc - 1), ElNodes[7].Y + disp_vals[j+7][1] * (sc - 1), ElNodes[7].Z + disp_vals[j+7][2] * (sc - 1));
                mesh.Vertices.Add(no7);
                



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


            DA.SetDataList(0, meshes);


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