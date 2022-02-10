﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;


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
            pManager.AddPointParameter("Points", "P", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Debug_1", "D", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Debug_2", "D", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Debug_3", "D", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Debug_4", "D", "", GH_ParamAccess.list);
            // pManager.AddPointParameter("Box", "B", "", GH_ParamAccess.list);
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

            //Extract mesh vertices
            Point3d[] pts = mesh.Vertices.ToPoint3dArray();

            //Create a list of ForceClass elements to be filled
            List<NodeClass> nodes = new List<NodeClass>();

            //Create a list of ForceClass elements to be filles
            List<LoadClass> forces = new List<LoadClass>();

            //Create a force vector with all values set to zero
            Vector3d zeroVec = new Vector3d(0, 0, 0);

            //Create two types of supports, fixed and free, to be assigned to the correct nodes
            SupportClass fixd = new SupportClass(false, false, false);
            SupportClass free = new SupportClass(true, true, true);

            //For each point in mesh, create a node with ID, coordinates and type of support (fixed along one side). Create a list of forcevectors
            for (int i = 0; i < pts.Length; i++)
            {
                if (i == 0 || i == 3 || i == 4 || i == 7)
                {
                    nodes.Add(new NodeClass(i, i, pts[i], fixd));
                }
                else
                {
                    nodes.Add(new NodeClass(i, i, pts[i], free));
                }

                if (i == 5 || i == 6)
                {
                    forces.Add(new LoadClass(loadVec, pts[i]));
                }
                else
                {
                    forces.Add(new LoadClass(zeroVec, pts[i]));
                }
            }

            //Create a long (24x1) force vector to be used in calculation of displacements

            Matrix<double> forceVec = Matrix<double>.Build.Dense(24, 1);

            /*
            for (int i = 0; i < forces.Count; i++)
            {
                LoadClass force = forces[i];
                forceVec[i, 0] = force.loadVector.X;
                forceVec[i + 8, 0] = force.loadVector.Y;
                forceVec[i + 16, 0] = force.loadVector.Z;
            }
            */
            int count = 0;
            foreach(LoadClass load in forces)
            {
                forceVec[count, 0] = load.loadVector.X;
                forceVec[count + 1, 0] = load.loadVector.Y;
                forceVec[count + 2, 0] = load.loadVector.Z;

                count += 3;
            }

            //Create stiffness matrix

            MaterialClass steel = new MaterialClass("Steel", 210000, 0.3);

            double val = 1.0 / Math.Sqrt(3);

            Point3d pt_1 = new Point3d(val, val, val);
            Point3d pt_2 = new Point3d(-val, val, val);
            Point3d pt_3 = new Point3d(val, -val, val);
            Point3d pt_4 = new Point3d(val, val, -val);
            Point3d pt_5 = new Point3d(-val, -val, val);
            Point3d pt_6 = new Point3d(val, -val, -val);
            Point3d pt_7 = new Point3d(-val, val, -val);
            Point3d pt_8 = new Point3d(-val, -val, -val);

            List<Point3d> dummy_list = new List<Point3d>{pt_1, pt_2, pt_3, pt_4, pt_5, pt_6, pt_7, pt_8};
            
            Matrix<double> K = Matrix<double>.Build.Dense(24, 24);

            for(int i = 0; i < 8; i++)
            {
                Matrix<double> k = ConstructStiffnessMatrix(nodes, steel, dummy_list[i]);
                K += k;
            }

            int cnt = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = nodes[i];

                if (node.support == fixd)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        K = K.RemoveRow(cnt).RemoveColumn(cnt);
                        forceVec = forceVec.RemoveRow(cnt);
                    }
                    cnt -= 3;
                }
                cnt += 3;
            }

            Matrix<double> K_inv = K.Inverse(); 
            
            //u = Kr

            Matrix<double> u_red = K_inv.Multiply(forceVec);
            Matrix<double> u = Matrix<double>.Build.Dense(24,1);

            int node_nr = 0;
            int u_cnt = 0;

            for(int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = nodes[node_nr];
                if (node.support == free)
                {
                    u[i ,0] = u_red[u_cnt,0];
                    u[i + 8 ,0] = u_red[u_cnt + 1,0];
                    u[i + 16 , 0] = u_red[u_cnt + 2, 0];

                    u_cnt += 3;
                }
                node_nr += 1;
            }


            Matrix<double> disp = Matrix<double>.Build.Dense(3, 8);

            
            for (int i = 0; i < nodes.Count; i++)
            {
                Point3d evalPt = getGenCoords(i);
                
                Matrix<double> shapeFunc = GetShapeFunctions(nodes.Count, evalPt.X, evalPt.Y, evalPt.Z);
                disp.SetSubMatrix(0, i, shapeFunc.Multiply(u));

            }


            List<NodeClass> dispNodes = nodes;
            List<Point3d> dispPts = new List<Point3d>();

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = dispNodes[i];
                Point3d pt = new Point3d(node.Point.X + disp[0, i], node.Point.Y + disp[1, i], node.Point.Z + disp[2, i]);
                dispPts.Add(pt);
            }
            
            //Creates on element with ID, all nodes and mesh
            ElementClass elem = new ElementClass(0, nodes, mesh);




            //outputs

            DA.SetData(0, elem);
            DA.SetDataList(1, dispPts);
            DA.SetData(2, disp.ToString());
            DA.SetData(3, K.ToString());



            #region Methods

            Point3d getGenCoords(int NodeNr) //Input node ID, and get the corresponding generalized coordinates
            {
                Point3d GenCoords = new Point3d();

                if (NodeNr == 0)
                {
                    GenCoords.X = -1;
                    GenCoords.Y = -1;
                    GenCoords.Z = -1;
                }
                else if (NodeNr == 1)
                {
                    GenCoords.X = 1;
                    GenCoords.Y = -1;
                    GenCoords.Z = -1;
                }
                else if (NodeNr == 2)
                {
                    GenCoords.X = 1;
                    GenCoords.Y = 1;
                    GenCoords.Z = -1;
                }
                else if (NodeNr == 3)
                {
                    GenCoords.X = -1;
                    GenCoords.Y = 1;
                    GenCoords.Z = -1;
                }
                else if (NodeNr == 4)
                {
                    GenCoords.X = -1;
                    GenCoords.Y = -1;
                    GenCoords.Z = 1;
                }
                else if (NodeNr == 5)
                {
                    GenCoords.X = 1;
                    GenCoords.Y = -1;
                    GenCoords.Z = 1;
                }
                else if (NodeNr == 6)
                {
                    GenCoords.X = 1;
                    GenCoords.Y = 1;
                    GenCoords.Z = 1;
                }
                else if (NodeNr == 7)
                {
                    GenCoords.X = -1;
                    GenCoords.Y = 1;
                    GenCoords.Z = 1;
                }

                return GenCoords;

            }

            Matrix<double> GetShapeFunctions(int nrNodes, double xi, double eta, double zeta) //Input number of nodes and the generalized point to be evaluated, and get the corresponding shape functions
            {
                Vector<double> N = Vector<double>.Build.Dense(nrNodes);

                //Construct the N0 vector with shape functions for the eight nodes
                for (int i = 0; i < nrNodes; i++)
                {
                    Point3d genCoord = getGenCoords(i);

                    double x = (1.0 / 8.0) * (1 + (genCoord.X * xi)) * (1 + (genCoord.Y * eta)) * (1 + (genCoord.Z * zeta));
                    N[i] = x;
                }

                Matrix<double> ShapeMat = Matrix<double>.Build.Dense(3, 24);

                //Construct a diagonal matrix with N0 on the diagonal, and zero elsewhere
                for (int i = 0; i < ShapeMat.RowCount; i++)
                {
                    for (int j = 0; j < ShapeMat.ColumnCount; j++)
                    {
                        if (j <= 7 && i == 0)
                        {
                            ShapeMat[i, j] = N[j];
                        }
                        else if (j > 7 && j <= 15 && i == 1)
                        {
                            ShapeMat[i, j] = N[j - 8];
                        }
                        else if (j > 15 && j <= 23 && i == 2)
                        {
                            ShapeMat[i, j] = N[j - 16];
                        }
                        else
                        {
                            ShapeMat[i, j] = 0;
                        }

                    }
                }

                return ShapeMat;
            }

            Matrix<double> GetDerivatedShapeFunctions(int nrNodes, double xi, double eta, double zeta) //Derivate the shape functions with regards to xi, eta and zeta
            {
                Vector<double> Nxi = Vector<double>.Build.Dense(nrNodes);
                Vector<double> Neta = Vector<double>.Build.Dense(nrNodes);
                Vector<double> Nzeta = Vector<double>.Build.Dense(nrNodes);

                //Construct vectors with the shape functions derivated with regards to xi, eta then zeta
                for (int i = 0; i < nrNodes; i++)
                {
                    Point3d genCoord = getGenCoords(i);

                    Nxi[i] = (1.0 / 8.0) * genCoord.X * (1 + genCoord.Y * eta) * (1 + genCoord.Z * zeta);
                    Neta[i] = (1.0 / 8.0) * genCoord.Y * (1 + genCoord.X * xi) * (1 + genCoord.Z * zeta);
                    Nzeta[i] = (1.0 / 8.0) * genCoord.Z * (1 + genCoord.Y * eta) * (1 + genCoord.X * xi);
                }

                Matrix<double> derivatedShapeMat = Matrix<double>.Build.Dense(3, 8);

                //Construct a matrix with all derivated shape functions
                for (int i = 0; i < derivatedShapeMat.RowCount; i++)
                {
                    for (int j = 0; j < derivatedShapeMat.ColumnCount; j++)
                    {
                        if (i == 0)
                        {
                            derivatedShapeMat[i, j] = Nxi[j];
                        }
                        else if (i == 1)
                        {
                            derivatedShapeMat[i, j] = Neta[j];
                        }
                        else if (i == 2)
                        {
                            derivatedShapeMat[i, j] = Nzeta[j];
                        }
                    }
                }

                return derivatedShapeMat;
            }

            Matrix<double> ConstructStiffnessMatrix(List<NodeClass> _nodes, MaterialClass material, Point3d _dummy) //Construction of the B matrix
            {
                //Shape functions       Ni = 1/8(1+XiX)(1+NiN)(1+YiY)

                //Create lists of coordinates

                Matrix<double> coords_hor = Matrix<double>.Build.Dense(8, 3);

                for (int i = 0; i < _nodes.Count; i++)
                {
                    NodeClass node = _nodes[i];

                    coords_hor[i, 0] = node.Point.X;
                    coords_hor[i, 1] = node.Point.Y;
                    coords_hor[i, 2] = node.Point.Z;
                }

                Matrix<double> shapeFuncDerGen = GetDerivatedShapeFunctions(_nodes.Count, _dummy.X, _dummy.Y, _dummy.Z);

                //Create Jacobi matrix

                Matrix<double> jacobi = shapeFuncDerGen.Multiply(coords_hor);

                //Jacobi determinant

                double jacobi_det = jacobi.Determinant();

                //Strain - disp relationship

                Matrix<double> shapeFuncDerCart = jacobi.Inverse().Multiply(shapeFuncDerGen);

                Matrix<double> B = Matrix<double>.Build.Dense(6, 24);

                //Assigning correct values to the B matrix
                B.SetSubMatrix(0, 0, shapeFuncDerCart.SubMatrix(0, 1, 0, 8));
                B.SetSubMatrix(1, 8, shapeFuncDerCart.SubMatrix(1, 1, 0, 8));
                B.SetSubMatrix(2, 16, shapeFuncDerCart.SubMatrix(2, 1, 0, 8));
                B.SetSubMatrix(3, 0, shapeFuncDerCart.SubMatrix(1, 1, 0, 8));
                B.SetSubMatrix(3, 8, shapeFuncDerCart.SubMatrix(0, 1, 0, 8));
                B.SetSubMatrix(4, 8, shapeFuncDerCart.SubMatrix(2, 1, 0, 8));
                B.SetSubMatrix(4, 16, shapeFuncDerCart.SubMatrix(1, 1, 0, 8));
                B.SetSubMatrix(5, 0, shapeFuncDerCart.SubMatrix(2, 1, 0, 8));
                B.SetSubMatrix(5, 16, shapeFuncDerCart.SubMatrix(0, 1, 0, 8));

                double E = material.eModulus;
                double v = material.pRatio;

                //Create constants for easier construction of the C matrix
                double alpha = (1 - v) * (E / ((1 + v) * (1 - 2 * v)));
                double beta = v * (E / ((1 + v) * (1 - 2 * v)));
                double gamma = ((1 - 2 * v) / 2) * (E / ((1 + v) * (1 - 2 * v)));

                //Construct C matrix
                Matrix<double> C = Matrix<double>.Build.Dense(6, 6);

                C[0, 0] = C[1, 1] = C[2, 2] = alpha;
                C[0, 1] = C[0, 2] = C[1, 0] = C[1, 2] = C[2, 0] = C[2, 1] = beta;
                C[3, 3] = C[4, 4] = C[5, 5] = gamma;

                Matrix<double> b_trans = B.Transpose();

                //Constructing the integrand by multiplying B transposed with C, then multiplied with B and lastly multiplied with the determinant of the jacobian
                Matrix<double> integrand = b_trans*(C)*(B)*(jacobi_det);

                return integrand;
            }

            

            #endregion
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