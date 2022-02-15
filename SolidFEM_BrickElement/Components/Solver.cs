using System;
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
            pManager.AddMeshParameter("Mesh", "", "", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Elements", "E", "List of elements containg nodes and meshes to be calculated", GH_ParamAccess.list);
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
            //inputs

            //List<ElementClass> elems = new List<ElementClass>();
            Mesh mesh = new Mesh();
            List<LoadClass> loads = new List<LoadClass>();
            List<SupportClass> sups = new List<SupportClass>();
            //DA.GetDataList(0, elems);
            DA.GetData(0, ref mesh);
            DA.GetDataList(1, loads);
            DA.GetDataList(2, sups);

            List<string> info = new List<string>();


            //code

            //Create empty global stiffness matrix [nNodes*ndof,nNodes*ndof], and empty connectivity matrix

            //Loop through all elements (elems.Count). Create connectivity matrix and local stiffness matrix, connect to global stiffness matrix

            //Mesh mesh = elems[0].Mesh;
            Vector3d loadVec = loads[0].loadVector;

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

            //Creates on element with ID, all nodes and mesh
            ElementClass elem = new ElementClass(0, nodes, mesh);

            //Create a long (24x1) force vector to be used in calculation of displacements
            Matrix<double> forceVec = Matrix<double>.Build.Dense(24, 1);

            
            for (int i = 0; i < forces.Count; i++)
            {
                LoadClass force = forces[i];
                forceVec[i, 0] = force.loadVector.X;
                forceVec[i + 8, 0] = force.loadVector.Y;
                forceVec[i + 16, 0] = force.loadVector.Z;
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

            for (int i = 0; i < 8; i++)
            {
                var integrand = ConstructStiffnessMatrix(nodes, steel, dummy_list[i]);
                Matrix<double> k = integrand.Item1;
                K += k;
            }
            
            //Set columns and rows of fixed nodes to 0

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = nodes[i];
                if (node.support == fixd)
                {
                    K.ClearColumns(i, i + 8, i + 16);
                    K.ClearRows(i,i + 8,i + 16);

                    forceVec.ClearRows(i, i + 8, i + 16);
                }
            }

            //Add 1 on the diagonal for all fixed nodes to make it inverseable

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = nodes[i];
                if (node.support == fixd)
                {
                    K[i, i] = 1;
                    K[i + 8, i + 8] = 1;
                    K[i + 16, i + 16] = 1;
                }
            }


            Matrix<double> K_inv = K.Inverse(); 
            
            //u = Kr

            Matrix<double> u = K_inv.Multiply(forceVec);


            //Convert displacements back to cartesian coordinates
            Matrix<double> disp = Matrix<double>.Build.Dense(3, 8);

            
            for (int i = 0; i < nodes.Count; i++)
            {
                Point3d evalPt = getGenCoords(i);
                
                Matrix<double> shapeFunc = GetShapeFunctions(nodes.Count, evalPt.X, evalPt.Y, evalPt.Z);
                disp.SetSubMatrix(0, i, shapeFunc.Multiply(u));

            }

            //Calculate stresses and strains

            Matrix<double> strains = Matrix<double>.Build.Dense(6, 8);
            Matrix<double> stresses = Matrix<double>.Build.Dense(6, 8);

            for (int i = 0; i < 8; i++)
            {
                var integrand = ConstructStiffnessMatrix(nodes, steel, dummy_list[i]);

                Matrix<double> strain = integrand.Item2 * u;
                strains.SetSubMatrix(0, i, strain);

                Matrix<double> stress = integrand.Item3 * strain;
                stresses.SetSubMatrix(0, i, stress);
            }

            //Add displacements to nodes, to get new coordinates

            List<NodeClass> dispNodes = nodes;
            List<Point3d> dispPts = new List<Point3d>();

            for (int i = 0; i < nodes.Count; i++)
            {
                NodeClass node = dispNodes[i];
                Point3d pt = new Point3d(node.Point.X + disp[0, i], node.Point.Y + disp[1, i], node.Point.Z + disp[2, i]);
                dispPts.Add(pt);
            }

            //Processing results

            Grasshopper.DataTree<double> _stresses = matrixToDataTree(stresses);
            Grasshopper.DataTree<double> _strains = matrixToDataTree(strains);
            Grasshopper.DataTree<double> _disps = matrixToDataTree(disp);


            Mesh new_mesh = new Mesh();

            //Create results
            ResultClass res = new ResultClass(_disps, _stresses, _strains, new_mesh);
            

            //outputs

            DA.SetData(0, res);
            DA.SetDataList(1, info);




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
                double volume = jacobi_det * 8;

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

            Grasshopper.DataTree<double> matrixToDataTree(Matrix<double> matrix)
            {
                Grasshopper.DataTree<double> _dataTree = new Grasshopper.DataTree<double>();
                for (int i = 0; i < matrix.ColumnCount; i++)
                {
                    for (int j = 0; j < matrix.RowCount; j++)
                    {
                        double x = matrix.At(i, j);
                        Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(i);
                        _dataTree.Add(x,path);
                    }

                }

                return _dataTree;
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