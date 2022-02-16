using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics;
using System.Linq;


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
            pManager.AddGenericParameter("Elements", "E", "List of elements containg nodes and meshes to be calculated", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads","L","External loads on the structure",GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "Supports for the structure", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "R", "Result of the solver, with displacements, stresses, strains and deformed mesh", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "I", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region

            List<ElementClass> elems = new List<ElementClass>();
            List<LoadClass> loads = new List<LoadClass>();
            List<SupportClass> sups = new List<SupportClass>();
            DA.GetDataList(0, elems);
            DA.GetDataList(1, loads);
            DA.GetDataList(2, sups);

            List<string> info = new List<string>();

            #endregion


            #region code

            MaterialClass steel = new MaterialClass("Steel", 210000, 0.3);  // creates a material class steel


            // creates 2x2x2 gauss integration points (full integration) 

            double val = 1.0 / Math.Sqrt(3);

            Point3d pt_1 = new Point3d(val, val, val);
            Point3d pt_2 = new Point3d(-val, val, val);
            Point3d pt_3 = new Point3d(val, -val, val);
            Point3d pt_4 = new Point3d(val, val, -val);
            Point3d pt_5 = new Point3d(-val, -val, val);
            Point3d pt_6 = new Point3d(val, -val, -val);
            Point3d pt_7 = new Point3d(-val, val, -val);
            Point3d pt_8 = new Point3d(-val, -val, -val);

            List<Point3d> dummy_list = new List<Point3d> { pt_1, pt_2, pt_3, pt_4, pt_5, pt_6, pt_7, pt_8 };


            //Create a list of nodes with no duplicates

            List<NodeClass> nodes = new List<NodeClass>();
            List<int> IDs = new List<int>();
            for (int i = 0; i < elems.Count; i++)
            {
                for(int j = 0; j < elems[i].Nodes.Count; j++)
                {
                    NodeClass node = elems[i].Nodes[j];
                    if (IDs.Contains(node.GlobalID) == false )
                    {
                        nodes.Add(node);
                        IDs.Add(node.GlobalID);
                    } 
                }
            }


            //Create empty global stiffness matrix [nNodes*ndof,nNodes*ndof], and empty connectivity matrix

            int nNodes = nodes.Count;
            int nDofs = 3;

            Matrix<double> globalK = Matrix<double>.Build.Dense(nDofs * nNodes, nDofs * nNodes);   // global stiffness matrix for the system

            //Loop through all elements (elems.Count). Create connectivity matrix (a) and local stiffness matrix (localK), connect to global stiffness matrix

            for(int i = 0; i < elems.Count; i++)
            {
                List<NodeClass> _nodes_ = elems[i].Nodes;   // the 8 nodes for element i

                //Create connectivity matrix for element i

                Matrix<double> a_mat = Matrix<double>.Build.Dense(nDofs * _nodes_.Count, globalK.ColumnCount);     // as many rows as localDOF for element i, as many columns as globalDOF in the system

                for(int j = 0; j < _nodes_.Count; j++)
                {
                    NodeClass node = _nodes_[j];
                    int gID = node.GlobalID;
                    int lID = node.LocalID;

                    int ID_1_1 = lID + _nodes_.Count ;
                    int ID_2_1 = lID + (_nodes_.Count)*2;
                    int ID_1_2 = gID + nNodes;
                    int ID_2_2 = gID + nNodes*2;

                    a_mat[lID, gID] = 1;
                    a_mat[ID_1_1, ID_1_2] = 1;
                    a_mat[ID_2_1, ID_2_2] = 1;
                }

                //Create local stiffness matrix for element i

                Matrix<double> localK = Matrix<double>.Build.Dense(24, 24);

                for (int j = 0; j < _nodes_.Count; j++)
                {
                    var integrand = ConstructStiffnessMatrix(_nodes_, steel, dummy_list[j]);
                    Matrix<double> k = integrand.Item1;
                    localK += k;
                }

                globalK += a_mat.Transpose() * localK * a_mat;      // assembly of the global striffness matrix
            }


            //Create list of supports and load vector

            Matrix<double> loadVec = Matrix<double>.Build.Dense(nNodes * nDofs, 1);     // create load vector
            Vector<double> _sups = Vector<double>.Build.Dense(nNodes * nDofs);          // create support vector

            for (int i = 0; i < nNodes; i++)
            {
                //Create support list zeroing columns and rows in K

                NodeClass node = nodes[i];             
                Point3d nodePt = node.Point;            
                int ID = node.GlobalID;                
                for (int j = 0; j < sups.Count; j++)
                {
                    SupportClass sup = sups[j];         

                    if (nodePt == sup.pt)
                    {
                        if (sup.Tx == true)
                        {
                            _sups[ID] = 1;
                        }
                        if (sup.Ty == true)
                        {
                            _sups[ID + nNodes] = 1;
                        }
                        if (sup.Tz == true)
                        {
                            _sups[ID + nNodes * 2] = 1;
                        }
                    }

                }

                //Create load vector

                for(int j = 0; j < loads.Count; j++)
                {
                    LoadClass load = loads[j];

                    if (nodePt == load.loadPoint)
                    {
                        loadVec[ID, 0] = load.loadVector.X;
                        loadVec[ID + nNodes, 0] = load.loadVector.Y;
                        loadVec[ID + nNodes * 2, 0] = load.loadVector.Z;
                    }
                }
            }
            info.Add(globalK.ToMatrixString());
            
            //Set columns and rows of fixed nodes to 0

            for (int i = 0; i < _sups.Count; i++)
            {
                if (_sups[i] == 1)
                {
                    globalK.ClearColumn(i);
                    globalK.ClearRow(i);

                    loadVec.ClearRow(i);
                }
            }

            //Add 1 on the diagonal for all fixed nodes to make it inverseable

            for (int i = 0; i < _sups.Count; i++)
            {
                if (_sups[i] == 1)
                {
                    globalK[i, i] = 1;
                    globalK[i, i] = 1;
                    globalK[i, i] = 1;
                }
            }


            Matrix<double> globalK_inv = globalK.Inverse(); 
            
            //u = Kr

            Matrix<double> r = globalK_inv.Multiply(loadVec);

            info.Add(r.ToMatrixString());

            List<Matrix<double>> disp_list = new List<Matrix<double>>();
            List<Matrix<double>> stress_list = new List<Matrix<double>>();
            List<Matrix<double>> strain_list = new List<Matrix<double>>();
            List<Point3d> pts_list = new List<Point3d>();

            for (int i = 0; i < elems.Count; i++)
            {
                List<NodeClass> _nodes = elems[i].Nodes;

                //Create connectivity matrix

                Matrix<double> a_mat = Matrix<double>.Build.Dense(nDofs * _nodes.Count, globalK.ColumnCount);

                a_mat.Clear();

                for (int j = 0; j < _nodes.Count; j++)
                {
                    NodeClass node = _nodes[j];
                    int gID = node.GlobalID;
                    int lID = node.LocalID;

                    a_mat[lID, gID] = 1;
                    a_mat[lID + _nodes.Count, gID + nNodes] = 1;
                    a_mat[lID + _nodes.Count * 2, gID + nNodes * 2] = 1;

                }


                Matrix<double> v = a_mat * r;

                

                //Convert displacements back to cartesian coordinates
                Matrix<double> disp = Matrix<double>.Build.Dense(3, _nodes.Count);

                for (int j = 0; j < _nodes.Count; j++)
                {
                    NodeClass node = _nodes[j];
                    Point3d evalPt = getGenCoords(node.LocalID);

                    Matrix<double> shapeFunc = GetShapeFunctions(_nodes.Count, evalPt.X, evalPt.Y, evalPt.Z); //-------------------------------------------------
                    Matrix<double> x = shapeFunc.Multiply(v);
                    disp[0, j] = x[0, 0];
                    disp[1, j] = x[1, 0];
                    disp[2, j] = x[2, 0];

                    // disp.SetSubMatrix(0, i, shapeFunc.Multiply(v));
                }
                
                disp_list.Add(disp);
                
                //Calculate stresses and strains

                Matrix<double> strains = Matrix<double>.Build.Dense(6, 8);
                Matrix<double> stresses = Matrix<double>.Build.Dense(6, 8);

                for (int j = 0; j < 8; j++)
                {
                    var integrand = ConstructStiffnessMatrix(_nodes, steel, dummy_list[i]);

                    Matrix<double> strain = integrand.Item2 * v;
                    strains.SetSubMatrix(0, j, strain);

                    Matrix<double> stress = integrand.Item3 * strain;
                    stresses.SetSubMatrix(0, j, stress);
                }

                stress_list.Add(stresses);
                strain_list.Add(strains);
                

                //Add displacements to nodes, to get new coordinates

                List<NodeClass> dispNodes = _nodes;
                List<Point3d> dispPts = new List<Point3d>();

                for (int j = 0; j < _nodes.Count; j++)
                {
                    NodeClass node = dispNodes[j];
                    Point3d pt = new Point3d(node.Point.X + disp[0, j], node.Point.Y + disp[1, j], node.Point.Z + disp[2, j]);
                    pts_list.Add(pt);
                }

                //pts_list.Add(dispPts);
             }

            List<Point3d> clean_pts = new List<Point3d>();
            for (int i = 0; i < pts_list.Count; i++)
            {
                if (clean_pts.Contains(pts_list[i]) == false)
                {
                    clean_pts.Add(pts_list[i]);
                }
            }


            //Processing results

            List<List<List<double>>> _disps = matrixToListInListInList(disp_list);
            List<List<List<double>>> _stresses = matrixToListInListInList(stress_list);
            List<List<List<double>>> _strains = matrixToListInListInList(strain_list);

            //Create results
            Mesh new_mesh = new Mesh();


            ResultClass res = new ResultClass(_disps, clean_pts, _stresses, _strains, new_mesh);
            info.Add(_disps.ToString());
            #endregion

            #region outputs

            DA.SetData(0, res);
            DA.SetDataList(1, info);

            #endregion



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

            (Matrix<double>, Matrix<double>, Matrix<double>) ConstructStiffnessMatrix(List<NodeClass> _nodes, MaterialClass material, Point3d _dummy) //Construction of the B matrix
            {
                //Shape functions       Ni = 1/8(1+XiX)(1+NiN)(1+YiY)

                //Create lists of coordinates

                Matrix<double> coords_hor = Matrix<double>.Build.Dense(_nodes.Count, 3);

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

                Matrix<double> B_trans = B.Transpose();

                //Constructing the integrand by multiplying B transposed with C, then multiplied with B and lastly multiplied with the determinant of the jacobian
                Matrix<double> integrand = B_trans*(C)*(B)*(jacobi_det);

                return (integrand,B,C);
            }

            List<List<List<double>>> matrixToListInListInList(List<Matrix<double>> list)
            {
                List < List < List<double> >> listInList = new List<List<List<double>>>();
                for (int i = 0; i < list.Count; i++)
                {
                    List<List<double>> list_1 = new List<List<double>>();
                    Matrix<double> list_mat = list[i];
                    for (int j = 0; j < list_mat.ColumnCount; j++)
                    {
                        List<double> list_2 = new List<double>();
                        for (int l = 0; l < list_mat.RowCount; l++)
                        {
                            list_2.Add(list_mat[l, j]);
                        }
                        list_1.Add(list_2);
                    }
                    listInList.Add(list_1);
                }

                return listInList;
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