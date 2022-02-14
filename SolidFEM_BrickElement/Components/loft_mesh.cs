﻿using System;
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
            pManager.AddMeshParameter("Mesh", "M", "Base mesh", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "M", "Topmesh", GH_ParamAccess.item);
            pManager.AddIntegerParameter("U Count", "U", "Divitions in U direction base mesh", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("V Count", "V", "Divitions in V direction base mesh", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Devitions", "nDiv", "Number of divisions between mesh surfaces", GH_ParamAccess.item);
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
            Mesh baseMesh = new Mesh();
            Mesh topMesh = new Mesh();
            int nU = 0;
            int nV = 0;
            int div = 0;

            DA.GetData(0, ref baseMesh);
            DA.GetData(1, ref topMesh);
            DA.GetData(2, ref nU);
            DA.GetData(3, ref nV);
            DA.GetData(4, ref div);
            #endregion


            #region code

            // create lines between vertices in baseMesh and topMesh

            Rhino.Geometry.Collections.MeshVertexList baseMeshVertices = baseMesh.Vertices;
            Rhino.Geometry.Collections.MeshVertexList topMeshVertices = topMesh.Vertices;

            List<PolylineCurve> guidecurves = new List<PolylineCurve>();
            for (int i = 0; i < baseMeshVertices.Count; i++)
            {
                Point3d startPt = baseMeshVertices[i];
                Point3d endPt = topMeshVertices[i];
                List<Point3d> startEndPts = new List<Point3d>() { startPt, endPt };
                var curve = new PolylineCurve(startEndPts);
                guidecurves.Add(curve);
            }


            // create points on lines between baseMesh and topMesh

            List<Point3d[]> ptsOnGuideCurve = new List<Point3d[]>();
            foreach (Curve curve in guidecurves)
            {
                Point3d[] pts;
                curve.DivideByCount(div, true, out pts);
                ptsOnGuideCurve.Add(pts);
            }


            // create nodes and sort in levels and rows

            
            int cnt = 0;
            List<List<List<NodeClass>>> levels = new List<List<List<NodeClass>>>();
            for (int i = 0; i < ptsOnGuideCurve[0].Length; i++)
            {
                List<List<NodeClass>> level = new List<List<NodeClass>>();
                for (int j = 0; j < nU + 1; j++)
                {
                    List<NodeClass> row = new List<NodeClass>();
                    for (int k = 0; k < nV +1 ; k++)
                    {
                        Point3d[] pts = ptsOnGuideCurve[k + j * (nV +1)];
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
            int globalNodeID = 0;
            int globalElementID = 0;
            for (int i = 0; i < levels.Count - 1; i++)
            {
                List<List<NodeClass>> firstLevel = levels[i];
                List<List<NodeClass>> nextLevel = levels[i + 1];
                for (int j = 0; j < firstLevel.Count - 1; j++)
                {
                    List<NodeClass> firstBotRow = firstLevel[j];
                    List<NodeClass> NextBotRow = firstLevel[j + 1];
                    List<NodeClass> firstTopRow = nextLevel[j];
                    List<NodeClass> NextTopRow = nextLevel[j + 1];

                    for (int k = 0; k < firstBotRow.Count - 1; k++)
                    {
                        NodeClass n0 = new NodeClass(firstBotRow[k].GlobalID, 0, firstBotRow[k].Point);
                        NodeClass n1 = new NodeClass(firstBotRow[k + 1].GlobalID, 1, firstBotRow[k + 1].Point);
                        NodeClass n2 = new NodeClass(NextBotRow[k].GlobalID, 2, NextBotRow[k].Point);
                        NodeClass n3 = new NodeClass(NextBotRow[k + 1].GlobalID, 3, NextBotRow[k + 1].Point);
                       


                        NodeClass n4 = new NodeClass(firstTopRow[k].GlobalID, 4, firstTopRow[k].Point);
                        NodeClass n5 = new NodeClass(firstTopRow[k + 1].GlobalID, 5, firstTopRow[k + 1].Point);
                        NodeClass n6 = new NodeClass(NextTopRow[k].GlobalID, 6, NextTopRow[k].Point);
                        NodeClass n7 = new NodeClass(NextTopRow[k + 1].GlobalID, 7, NextTopRow[k + 1].Point);
                        



                        globalNodeID += 8;

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
            // DA.SetDataList(0, meshList);
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