using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;


using MathNet.Numerics.LinearAlgebra;

namespace SolidFEM_BrickElement
{
    public class loft_mesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Mesh class.
        /// </summary>
        public loft_mesh()
          : base("Loft Mesh", "loft mesh",
              "Creates a mesh by lofting tow Mesh Surfaces",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Base Surface", "B1", "Base surface", GH_ParamAccess.item);
            pManager.AddBrepParameter("Top Surface", "B2", "Top surface", GH_ParamAccess.item);
            pManager.AddCurveParameter("Guiding curve", "C", "Curve to guide lofting", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U Count", "U", "Divitions in U direction", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V Count", "V", "Divitions in V direction", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("W Count", "W", "Divitions in W direction", GH_ParamAccess.item);

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // pManager.AddMeshParameter("Mesh", "M", "Mesh from loft", GH_ParamAccess.list);
            pManager.AddGenericParameter("Elements", "E", "Elements from loft", GH_ParamAccess.list);
          

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region Input
            Brep baseBrep = new Brep();
            Brep topBrep = new Brep();
            Curve guideCurve = new PolylineCurve();
            int nU = 0;
            int nV = 0;
            int nW = 0;

            DA.GetData(0, ref baseBrep);
            DA.GetData(1, ref topBrep);
            DA.GetData(2, ref guideCurve);
            DA.GetData(3, ref nU);
            DA.GetData(4, ref nV);
            DA.GetData(5, ref nW);
            #endregion


            #region Code

            // create points on surface and lines between surfaces

            Rhino.Geometry.Surface baseSurface = baseBrep.Surfaces[0];
            Rhino.Geometry.Surface topSurface = topBrep.Surfaces[0];

            baseSurface.SetDomain(0, new Interval(0.0, 1.0));
            baseSurface.SetDomain(1, new Interval(0.0, 1.0));
            topSurface.SetDomain(0, new Interval(0.0, 1.0));
            topSurface.SetDomain(1, new Interval(0.0, 1.0));

            double dU = 1.0 / (double)nU;
            double dV = 1.0 / (double)nV;

   
      
            List<Curve> allCrvs = new List<Curve>();
            
            for (int i = 0; i < nV + 1; i++)
            {
                for (int j = 0; j < nU + 1; j++)
                {
                    Point3d stPt = baseSurface.PointAt((double)j * dU, (double)i * dV);
                    Point3d enPt = topSurface.PointAt((double)j * dU, (double)i * dV);

                    if (guideCurve.IsValid == false ) 
                    {
                        List<Point3d> startEndPts = new List<Point3d>() { stPt, enPt };
                        Curve curve = new PolylineCurve(startEndPts);
                        allCrvs.Add(curve);
                        
                    }
                    else if (guideCurve.IsValid == true)
                    {
                        Curve curveOriginal = (Curve)guideCurve.Duplicate();
                        bool changeStartPt = curveOriginal.SetStartPoint(stPt);
                        bool changeEndPt = curveOriginal.SetEndPoint(enPt);
                        allCrvs.Add(curveOriginal);
                    }
                }
            }


            //// create points on lines between surfaces

            List<Point3d[]> ptsOnGuideCurve = new List<Point3d[]>();
            foreach (Curve crv in allCrvs)
            {
                Point3d[] pts;
                crv.DivideByCount(nW, true, out pts);
                ptsOnGuideCurve.Add(pts);
            }


            // create global nodes and sort in levels and rows

            int cnt = 0;
            List<List<List<NodeClass>>> levels = new List<List<List<NodeClass>>>();
            for (int i = 0; i < ptsOnGuideCurve[0].Length; i++)
            {
                List<List<NodeClass>> level = new List<List<NodeClass>>();
                for (int j = 0; j < nV + 1; j++)
                {
                    List<NodeClass> row = new List<NodeClass>();
                    for (int k = 0; k < nU + 1; k++)
                    {
                        Point3d[] pts = ptsOnGuideCurve[k + j * (nU + 1)]; 
                        Point3d pt = pts[i];
                        NodeClass node = new NodeClass(cnt, pt);
                        row.Add(node);
                        cnt++;
                    }
                    level.Add(row);
                }
                levels.Add(level);
            }




            // create elements with elementID, Nodes(with globalID and LocalID ) and mesh

            List<ElementClass> elementList = new List<ElementClass>();
          
            int globalElementID = 0;
            for (int i = 0; i < levels.Count - 1; i++)
            {
                List<List<NodeClass>> firstLevel = levels[i];
                List<List<NodeClass>> nextLevel = levels[i + 1];
                for (int j = 0; j < firstLevel.Count-1; j++)
                {
                    List<NodeClass> firstBotRow = firstLevel[j];
                    List<NodeClass> NextBotRow = firstLevel[j + 1];
                    List<NodeClass> firstTopRow = nextLevel[j];
                    List<NodeClass> NextTopRow = nextLevel[j + 1];

                    for (int k = 0; k < firstBotRow.Count - 1; k++)
                    {
                        NodeClass n0 = new NodeClass(firstBotRow[k].GlobalID, 0, firstBotRow[k].Point);
                        NodeClass n1 = new NodeClass(firstBotRow[k + 1].GlobalID, 1, firstBotRow[k + 1].Point);
                        NodeClass n2 = new NodeClass(NextBotRow[k+1].GlobalID, 2, NextBotRow[k+1].Point);
                        NodeClass n3 = new NodeClass(NextBotRow[k].GlobalID, 3, NextBotRow[k].Point);

                        NodeClass n4 = new NodeClass(firstTopRow[k].GlobalID, 4, firstTopRow[k].Point);
                        NodeClass n5 = new NodeClass(firstTopRow[k + 1].GlobalID, 5, firstTopRow[k + 1].Point);
                        NodeClass n6 = new NodeClass(NextTopRow[k+1].GlobalID, 6, NextTopRow[k+1].Point);
                        NodeClass n7 = new NodeClass(NextTopRow[k].GlobalID, 7, NextTopRow[k].Point);

                        List<NodeClass> nodesElem = new List<NodeClass>() { n0, n1, n2, n3, n4, n5, n6, n7 };
                        Mesh mesh = new Mesh();
                        foreach (NodeClass node in nodesElem)
                        {
                            mesh.Vertices.Add(node.Point);
                        }
                        mesh.Faces.AddFace(new MeshFace(0, 1, 2, 3));
                        mesh.Faces.AddFace(new MeshFace(0, 1, 5, 4));
                        mesh.Faces.AddFace(new MeshFace(1, 2, 6, 5));
                        mesh.Faces.AddFace(new MeshFace(2, 3, 7, 6));
                        mesh.Faces.AddFace(new MeshFace(3, 0, 4, 7));
                        mesh.Faces.AddFace(new MeshFace(4, 5, 6, 7));

                        elementList.Add(new ElementClass(globalElementID, nodesElem, mesh));
                        globalElementID++;
                    }
                }

            }
            #endregion


            #region output
            DA.SetDataList(0, elementList);
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
            get { return new Guid("7DB72C1D-A0EA-4D78-BCAC-AEF689A8301B"); }
        }
    }
}