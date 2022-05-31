using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
            pManager.AddGenericParameter("Elements", "E", "List of elements containg nodes and meshes to be calculated (lengths in [m])", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads","L","External loads on the model",GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "Supports for the model", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material for the model", GH_ParamAccess.item);
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
            //inputs

            List<ElementClass> elems = new List<ElementClass>();
            List<LoadClass> loads = new List<LoadClass>();
            List<SupportClass> sups = new List<SupportClass>();
            MaterialClass material = new MaterialClass();
            DA.GetDataList(0, elems);
            DA.GetDataList(1, loads);
            DA.GetDataList(2, sups);
            DA.GetData(3, ref material);

            List<string> info = new List<string>();

            //code

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

            //Construct global stiffness matrix
            int nNodes = nodes.Count;
            int nDofs = 3;

            Matrix<double> bigK = ConstructGlobalStiffnessMatrix(elems, nDofs, nNodes, material);
            info.Add(bigK.ToMatrixString());

            //Create list of supports and load vector

            var loadandsup = ConstructSupAndLoadVector(nodes, sups, loads, nDofs);
            Vector<double> _sups = loadandsup.Item1;
            Matrix<double> loadVec = loadandsup.Item2;
            
            //Set columns and rows of fixed DOFS, set det diagonal for these fixed DOFS to 1, to make the matrix inversible

            for (int i = 0; i < _sups.Count; i++)
            {
                if (_sups[i] == 1)
                {
                    bigK.ClearColumn(i);
                    bigK.ClearRow(i);

                    bigK[i, i] = 1; 

                    loadVec.ClearRow(i);
                }
            }


            Matrix<double> bigK_inv = bigK.Inverse(); 
            
            //r = KR

            Matrix<double> r = bigK_inv.Multiply(loadVec);

            //Create results with strains, stresses, displacements and new points

            ResultClass res = ConstructResults(elems, r, bigK, nodes, material, nDofs);

            //outputs

            DA.SetData(0, res);
            DA.SetDataList(1, info);
        }

        #region Methods

        private Point3d getGenCoords(int NodeNr) //Input node ID, and get the corresponding generalized coordinates
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

        private Matrix<double> GetShapeFunctions(int nrNodes, double xi, double eta, double zeta) //Input number of nodes and the generalized point to be evaluated, and get the corresponding shape functions
        {
            Matrix<double> N = Matrix<double>.Build.Dense(1, nrNodes);

            //Construct the N0 vector with shape functions for the eight nodes
            for (int i = 0; i < nrNodes; i++)
            {
                Point3d genCoord = getGenCoords(i);

                double x = (1.0 / 8.0) * (1 + (genCoord.X * xi)) * (1 + (genCoord.Y * eta)) * (1 + (genCoord.Z * zeta));
                N[0, i] = x;
            }

            return N;
        }

        private Matrix<double> GetShapeFunctionMatrix(int nrNodes, double xi, double eta, double zeta)
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

        private Matrix<double> GetDerivatedShapeFunctions(int nrNodes, double xi, double eta, double zeta) //Derivate the shape functions with regards to xi, eta and zeta
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

        private (Matrix<double>, Matrix<double>, Matrix<double>) ConstructStiffnessMatrix(List<NodeClass> _nodes, MaterialClass material, Point3d _dummy) //Construction of the B matrix
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
            Matrix<double> integrand = B_trans * (C) * (B) * (jacobi_det);

            return (integrand, B, C);
        }

        private GH_Structure<GH_Number> ListMatrixToTreeNumber(List<Matrix<double>> list)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            for (int i = 0; i < list.Count; i++) //Going through list
            {
                Matrix<double> list_mat = list[i];
                for (int j = 0; j < list_mat.ColumnCount; j++) //Going through each column
                {
                    for (int l = 0; l < list_mat.RowCount; l++) //Going thrugh each row
                    {
                        GH_Path path = new GH_Path(i,j);
                        GH_Number val = new GH_Number();
                        GH_Convert.ToGHNumber(list_mat[l, j], 0, ref val);
                        tree.Append(val, path);
                    }
                }
            }

            return tree;
        }

        private GH_Structure<GH_Number> ListMatrixToTreeNumber2(List<Matrix<double>> list)
        {
            GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();
            for (int i = 0; i < list.Count; i++) //Going through list
            {
                Matrix<double> list_mat = list[i];
                for (int j = 0; j < list_mat.ColumnCount; j++) //Going through each column
                {
                    for (int l = 0; l < list_mat.RowCount; l++) //Going thrugh each row
                    {
                        GH_Path path = new GH_Path(i);
                        GH_Number val = new GH_Number();
                        GH_Convert.ToGHNumber(list_mat[l, j], 0, ref val);
                        tree.Append(val, path);
                    }
                }
            }

            return tree;
        }

        private GH_Structure<GH_Point> ListListToTreePoint(List<List<Point3d>> list)
        {
            GH_Structure<GH_Point> tree = new GH_Structure<GH_Point>();
            for (int i = 0; i < list.Count; i++) //Going through list
            {

                for (int j = 0; j < list[i].Count; j++) //Going through each column
                {
                    GH_Path path = new GH_Path(i);
                    GH_Point _pt = new GH_Point();
                    GH_Convert.ToGHPoint(list[i][j], 0, ref _pt);
                    tree.Append(_pt, path);
                }
            }

            return tree;
        }

        private List<Point3d> getDummyPoints()
        {
            double val = 1.0 / Math.Sqrt(3);

            //Point3d pt_1 = new Point3d(val, val, val);
            //Point3d pt_2 = new Point3d(-val, val, val);
            //Point3d pt_3 = new Point3d(val, -val, val);
            //Point3d pt_4 = new Point3d(val, val, -val);
            //Point3d pt_5 = new Point3d(-val, -val, val);
            //Point3d pt_6 = new Point3d(val, -val, -val);
            //Point3d pt_7 = new Point3d(-val, val, -val);
            //Point3d pt_8 = new Point3d(-val, -val, -val);


            Point3d pt_1 = new Point3d(-val, -val, -val);
            Point3d pt_2 = new Point3d(val, -val, -val);
            Point3d pt_3 = new Point3d(val, val, -val);
            Point3d pt_4 = new Point3d(-val, val, -val);
            Point3d pt_5 = new Point3d(-val, -val, val);
            Point3d pt_6 = new Point3d(val, -val, val);
            Point3d pt_7 = new Point3d(val, val, val);
            Point3d pt_8 = new Point3d(-val, val, val);

            List<Point3d> dummy_list = new List<Point3d> { pt_1, pt_2, pt_3, pt_4, pt_5, pt_6, pt_7, pt_8 };
            Point3d ptOrig = new Point3d(0, 0, 0);
            List<Point3d> dummy_list1 = new List<Point3d> { ptOrig };
            return dummy_list;
        }

        private Matrix<double> ConstructGlobalStiffnessMatrix(List<ElementClass> elems, int nDofs, int nNodes, MaterialClass material)  
        {

            Matrix<double> bigK = Matrix<double>.Build.Dense(nDofs * nNodes, nDofs * nNodes);
            List<Point3d> dummy_list = getDummyPoints();

            //Loop through all elements (elems.Count). Create connectivity matrix and local stiffness matrix, connect to global stiffness matrix

            for (int i = 0; i < elems.Count; i++)
            {
                List<NodeClass> _nodes = elems[i].Nodes;

                //Create connectivity matrix

                Matrix<double> a_mat = Matrix<double>.Build.Dense(nDofs * _nodes.Count, bigK.ColumnCount);

                for (int j = 0; j < _nodes.Count; j++)
                {
                    NodeClass node = _nodes[j];
                    int gID = node.GlobalID;
                    int lID = node.LocalID;

                    int ID_1_1 = lID + _nodes.Count;
                    int ID_2_1 = lID + (_nodes.Count) * 2;
                    int ID_1_2 = gID + nNodes;
                    int ID_2_2 = gID + nNodes * 2;

                    a_mat[lID, gID] = 1;
                    a_mat[ID_1_1, ID_1_2] = 1;
                    a_mat[ID_2_1, ID_2_2] = 1;
                }

                //Create stiffness matrix

                Matrix<double> K = Matrix<double>.Build.Dense(24, 24);

                for (int j = 0; j < dummy_list.Count; j++)
                {
                    var integrand = ConstructStiffnessMatrix(_nodes, material, dummy_list[j]);
                    Matrix<double> k = integrand.Item1;
                    K += k;
                }

                bigK += a_mat.Transpose() * K * a_mat;
            }

            return bigK;
        }

        private (Vector<double>, Matrix<double>) ConstructSupAndLoadVector(List<NodeClass> nodes, List<SupportClass> sups, List<LoadClass> loads, int nDofs)
        {

            int nNodes = nodes.Count;

            Matrix<double> loadVec = Matrix<double>.Build.Dense(nNodes * nDofs, 1);
            Vector<double> _sups = Vector<double>.Build.Dense(nNodes * nDofs);

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

                for (int j = 0; j < loads.Count; j++)
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

            return (_sups, loadVec);
        }
        
        private ResultClass ConstructResults(List<ElementClass> elems, Matrix<double> r, Matrix<double> bigK, List<NodeClass> nodes, MaterialClass material, int nDofs)
        {
            List<Matrix<double>> disp_list = new List<Matrix<double>>();
            List<Matrix<double>> stress_list = new List<Matrix<double>>();
            List<Matrix<double>> strain_list = new List<Matrix<double>>();
            List<List<Point3d>> new_pts_list = new List<List<Point3d>>();
            int nNodes = nodes.Count;
            List<Point3d> dummy_list = getDummyPoints();

            Dictionary<int, List<Matrix<double>>> nodal_stresses = new Dictionary<int, List<Matrix<double>>>(); 

            for (int i = 0; i < elems.Count; i++)
            {
                List<NodeClass> _nodes = elems[i].Nodes;

                //Create connectivity matrix

                Matrix<double> a_mat = Matrix<double>.Build.Dense(nDofs * _nodes.Count, bigK.ColumnCount);

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

                    Matrix<double> shapeFunc = GetShapeFunctionMatrix(_nodes.Count, evalPt.X, evalPt.Y, evalPt.Z);
                    disp.SetSubMatrix(0, j, shapeFunc.Multiply(v));
                }

                disp_list.Add(disp);

                //Calculate stresses and strains

                // (1x1x1) sampling pt ------------------------------------------------------------------------------------------------------------- alternative 1

                //Point3d stressSamplingPt = new Point3d(0, 0, 0);

                //var integrand1 = ConstructStiffnessMatrix(_nodes, material, stressSamplingPt);
                //Matrix<double> B = integrand1.Item2;
                //Matrix<double> C = integrand1.Item3;

                //Matrix<double> strainAtSamplingPt = B * v;
                //Matrix<double> stressAtSamplingPt = C * strainAtSamplingPt;

                //// shape function for mid node?? N = 1

                //Matrix<double> strainAtNodes = Matrix<double>.Build.Dense(6, 8);
                //Matrix<double> stressAtNodes = Matrix<double>.Build.Dense(6, 8);


                //for (int k = 0; k < 8; k++)
                //{

                //    strainAtNodes.SetSubMatrix(0, k, strainAtSamplingPt);
                //    stressAtNodes.SetSubMatrix(0, k, stressAtSamplingPt);

                //}


                //strain_list.Add(strainAtNodes);
                //stress_list.Add(stressAtNodes);

                // (2x2x2) samplingpoints ----------------------------------------------------------------------------------------------------------- alternative 2


                Matrix<double> strainAtSamplingPoints = Matrix<double>.Build.Dense(6, 8);
                Matrix<double> stressAtSamplingPoints = Matrix<double>.Build.Dense(6, 8);

                for (int j = 0; j < 8; j++)
                {
                    var integrand1 = ConstructStiffnessMatrix(_nodes, material, dummy_list[j]);
                    Matrix<double> B = integrand1.Item2;
                    Matrix<double> C = integrand1.Item3;
                    Matrix<double> strainAtSamplingPt = B * v;
                    Matrix<double> stressAtSamplingPt = C * strainAtSamplingPt;

                    strainAtSamplingPoints.SetSubMatrix(0, j, strainAtSamplingPt);
                    stressAtSamplingPoints.SetSubMatrix(0, j, stressAtSamplingPt);
                }

                Matrix<double> strainAtNodes = Matrix<double>.Build.Dense(6, 8);
                Matrix<double> stressAtNodes = Matrix<double>.Build.Dense(6, 8);

                for (int k = 0; k < 8; k++)
                {
                    Point3d genCoord = getGenCoords(k);
                    Matrix<double> shapeFunc = GetShapeFunctions(8, genCoord.X * Math.Sqrt(3), genCoord.Y * Math.Sqrt(3), genCoord.Z * Math.Sqrt(3));
                    Matrix<double> strainAtNode = shapeFunc.Multiply(strainAtSamplingPoints.Transpose());
                    Matrix<double> strainAtNodeT = strainAtNode.Transpose();
                    Matrix<double> stressAtNode = shapeFunc.Multiply(stressAtSamplingPoints.Transpose());
                    Matrix<double> stressAtNodeT = stressAtNode.Transpose();

                    strainAtNodes.SetSubMatrix(0, k, strainAtNodeT);
                    stressAtNodes.SetSubMatrix(0, k, stressAtNodeT);

                    int gID = elems[i].Nodes[k].GlobalID;

                    if (nodal_stresses.ContainsKey(gID))
                    {
                        nodal_stresses[gID].Append(stressAtNode);
                    }
                    else
                    {
                        nodal_stresses.Add(elems[i].Nodes[k].GlobalID, new List<Matrix<double>> { stressAtNode });
                    }
                    

                }
                strain_list.Add(strainAtNodes);
                stress_list.Add(stressAtNodes);

                //-------------------------------------------------------------------------------------------------------------------------------------- old code


                //Matrix<double> gauss_strains = Matrix<double>.Build.Dense(6, 8);
                //Matrix<double> gauss_stresses = Matrix<double>.Build.Dense(6, 8);

                //for (int j = 0; j < _nodes.Count; j++)
                //{
                //    var integrand = ConstructStiffnessMatrix(_nodes, material, dummy_list[j]);

                //    Matrix<double> gauss_strain = integrand.Item2 * v;
                //    Matrix<double> gauss_stress = integrand.Item3 * gauss_strain;
                //    gauss_stresses.SetSubMatrix(0, j, gauss_stress);
                //    gauss_strains.SetSubMatrix(0, j, gauss_strain);
                //}

                //Matrix<double> elem_strains = Matrix<double>.Build.Dense(6, 8);
                //Matrix<double> elem_stresses = Matrix<double>.Build.Dense(6, 8);

                //for (int j = 0; j < _nodes.Count; j++)
                //{
                //    Point3d genCoord = getGenCoords(j);
                //    Matrix<double> shapeFunc = GetShapeFunctions(8, genCoord.X * Math.Sqrt(3), genCoord.Y * Math.Sqrt(3), genCoord.Z * Math.Sqrt(3));

                //    Matrix<double> nodalStrain = gauss_strains.Multiply(shapeFunc.Transpose());
                //    Matrix<double> nodalStress = gauss_stresses.Multiply(shapeFunc.Transpose());

                //    elem_strains.SetSubMatrix(0, j, nodalStrain.SubMatrix(0, 6, 0, 1));
                //    elem_stresses.SetSubMatrix(0, j, nodalStress.SubMatrix(0, 6, 0, 1));
                //}

                //strain_list.Add(elem_strains);
                //stress_list.Add(elem_stresses);
                // ----------------------------------------------------------------------------------------------------------------------------------------------------------

                //Add displacements to nodes, to get new coordinates

                List<NodeClass> dispNodes = _nodes;
                List<Point3d> dispPts = new List<Point3d>();
                List<Point3d> oldPts = new List<Point3d>();

                for (int j = 0; j < _nodes.Count; j++)
                {
                    NodeClass node = dispNodes[j];
                    Point3d _pt = new Point3d(node.Point.X + disp[0, j], node.Point.Y + disp[1, j], node.Point.Z + disp[2, j]);
                    dispPts.Add(_pt);
                }

                new_pts_list.Add(dispPts);
            }


            List<Matrix<double>> nodal_stresses_list = new List<Matrix<double>>();

            for (int i = 0; i < nNodes; i++)
            {
                List<Matrix<double>> stresses = nodal_stresses[i];

                Matrix<double> stress = Matrix<double>.Build.Dense(1, 6);

                for (int j = 0; j < stresses.Count; j++)
                {
                    stress += stresses[j];
                }

                nodal_stresses_list.Add(stress/stresses.Count);
            }

            
            //Processing results

            GH_Structure<GH_Number> _disps = ListMatrixToTreeNumber(disp_list);
            //GH_Structure<GH_Number> _stresses = ListMatrixToTreeNumber2(nodal_stresses_list);
            GH_Structure<GH_Number> _stresses = ListMatrixToTreeNumber(stress_list);
            GH_Structure<GH_Number> _strains = ListMatrixToTreeNumber(strain_list);
            GH_Structure<GH_Point> _npts = ListListToTreePoint(new_pts_list);

            Vector3d vec = new Vector3d(1, 0, 0);
            

            Point3d pt = new Point3d(0, 0, 0);
            var dir = Transform.Translation(vec);
            pt.Transform(dir);
             

            //Create results
            Mesh new_mesh = new Mesh();


            ResultClass res = new ResultClass(_disps, _stresses, _strains, _npts, new_mesh);

            return res;
        }

        #endregion




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