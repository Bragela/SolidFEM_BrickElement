using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components
{
    public class PreviewStress : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PreviewStress()
          : base("PreviewStress", "PS",
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
            pManager.AddNumberParameter("Stress", "Str", "Stress", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stress direction","D","Direction of stress to preview",GH_ParamAccess.tree);  
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Stress mesh", "Stress mesh", "Colored mesh by stress values", GH_ParamAccess.list);
            pManager.AddNumberParameter("TestNumber", "NT", "Numbertest", GH_ParamAccess.list);
            pManager.AddTextParameter("","","",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input______________________________________________________________

            GH_Structure<GH_Point> gh_nodes = new GH_Structure<GH_Point>();
            GH_Structure<GH_Number> gh_stresses = new GH_Structure<GH_Number>();
            int num = 0;

            if (!DA.GetDataTree(0, out gh_nodes)) return;
            if (!DA.GetDataTree(1, out gh_stresses)) return;
            DA.GetData(2, ref num);





            List<string> info = new List<string>();


            //Make the input as list in list
            //#1 Displacement points

            List<List<Point3d>> DisPts = new List<List<Point3d>>();

            foreach (var lst in gh_nodes.Branches)
            {
                List<Point3d> ElemPts = new List<Point3d>();

                foreach (var gh_node in lst)
                {
                    Point3d pt;
                    if (gh_node.CastTo(out pt))
                    {
                        ElemPts.Add(pt);
                    }
                }
                DisPts.Add(ElemPts);
            }


            //#2 Stresses

            List<List<double>> stresses = new List<List<double>>();

            //List<double> ElemStress1 = new List<double>();

            foreach (var slst in gh_stresses.Branches)
            {
                List<double> ElemStress = new List<double>();

                    foreach (var gh_stress in slst)
                    {
                     double spt = gh_stress.Value;
                   
                     ElemStress.Add(spt);
                    }

                stresses.Add(ElemStress); 
            }
            



            //code______________________________________________________________

            //Find max, min og range of stresses;

            List<double> stress_x = new List<double>();
            List<double> stress_y = new List<double>(); 
            List<double> stress_z = new List<double>();

            List<double> stress_xy = new List<double>();
            List<double> stress_yz = new List<double>();
            List<double> stress_zx = new List<double>();

            List<double> stress_Mises = new List<double>();

                        for (int i = 0; i < stresses.Count; i++)
                        {
                            stress_x.Add(stresses[i][0]);
                            stress_y.Add(stresses[i][1]);
                            stress_z.Add(stresses[i][2]);
                            stress_xy.Add(stresses[i][3]);
                            stress_yz.Add(stresses[i][4]);
                            stress_zx.Add(stresses[i][5]);

                double str_mises = (Math.Pow(0.5*(Math.Pow(stresses[i][0] - stresses[i][1], 2) + Math.Pow(stresses[i][1] - stresses[i][2], 2)
                                           + Math.Pow(stresses[i][2] - stresses[i][0], 2)) 
                                           + 3*(Math.Pow(stresses[i][3],2) + Math.Pow(stresses[i][4], 2) + Math.Pow(stresses[i][5], 2)),0.5));
                        
                            stress_Mises.Add(str_mises);
                        }
            List<List<double>> stress_dir = new List<List<double>>();
            stress_dir.Add(stress_x);
            stress_dir.Add(stress_y);
            stress_dir.Add(stress_z);
            stress_dir.Add(stress_xy);
            stress_dir.Add(stress_yz);
            stress_dir.Add(stress_zx);
            stress_dir.Add(stress_Mises);




            // Set direction (dir) -> x = 0, y = 1, z = 2 , xy = 3, yz = 4, zx = 5, Mises = 6

            int dir = num;

            double stress_max = stress_dir[dir].Max();
            double stress_min = stress_dir[dir].Min();
            double stress_range = stress_max - stress_min;

            info.Add(stress_max.ToString());
            info.Add(stress_min.ToString());
            info.Add(stress_Mises.ToString());
       

            //Constructing mesh from displacement points and giving color from stress values
            List <Mesh> meshes = new List<Mesh>();
            List<double> percents = new List<double>();
    

            for (int i = 0; i < DisPts.Count; i++)
            {
                
                List<Point3d> ElNodes = DisPts[i];

                int j = ElNodes.Count*i;

                Mesh mesh = new Mesh();

                mesh.Vertices.Add(ElNodes[0]);
                //double str0 = stresses[j][dir];
                double str0_dir = stress_dir[dir][j];

                mesh.Vertices.Add(ElNodes[1]);
                //double str1 = stresses[j+1][dir];
                double str1_dir = stress_dir[dir][j+1];

                mesh.Vertices.Add(ElNodes[2]);
                //double str2 = stresses[j+2][dir];
                double str2_dir = stress_dir[dir][j+2];

                mesh.Vertices.Add(ElNodes[3]);
                //double str3 = stresses[j+3][dir];
                double str3_dir = stress_dir[dir][j+3];

                mesh.Vertices.Add(ElNodes[4]);
                //double str4 = stresses[j+4][dir];
                double str4_dir = stress_dir[dir][j+4];

                mesh.Vertices.Add(ElNodes[5]);
                //double str5 = stresses[j+5][dir];
                double str5_dir = stress_dir[dir][j+5];

                mesh.Vertices.Add(ElNodes[6]);
                //double str6 = stresses[j+6][dir];
                double str6_dir = stress_dir[dir][j+6];

                mesh.Vertices.Add(ElNodes[7]);
               // double str7 = stresses[j+7][dir];
                double str7_dir = stress_dir[dir][j+7];

               // double str_avg = (str0+str1+str2+str3+str4+str5+str6+str7)/ ElNodes.Count;
                double str_dir_avg = (str0_dir + str1_dir + str2_dir + str3_dir + str4_dir + str5_dir + str6_dir + str7_dir) / ElNodes.Count;

                //double str_pos = str_avg + Math.Abs(stress_min);
                //double str_rgb = (str_pos / stress_range)*255;

                double str_dir_pos = str_dir_avg + Math.Abs(stress_min);
                double str_dir_rgb = (str_dir_pos / stress_range) * 255;

                int red_val = ((int)str_dir_rgb);
                int green_val = 255 - ((int)str_dir_rgb);



               
                percents.Add(str_dir_rgb);
                



                mesh.Faces.AddFace(new MeshFace(0, 1, 2, 3));
                mesh.Faces.AddFace(new MeshFace(0, 1, 5, 4));
                mesh.Faces.AddFace(new MeshFace(1, 2, 6, 5));
                mesh.Faces.AddFace(new MeshFace(2, 3, 7, 6));
                mesh.Faces.AddFace(new MeshFace(3, 0, 4, 7));
                mesh.Faces.AddFace(new MeshFace(4, 5, 6, 7));

                mesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(170, red_val, green_val,0 ));

                meshes.Add(mesh);

            }

            
            //output______________________________________________________________
            
            DA.SetDataList(0, meshes);
            DA.SetDataList(1, percents);
            DA.SetDataList(2, info);

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
            get { return new Guid("43FAD0C7-A45D-49B3-9914-66ABD8433EE8"); }
        }
    }
}