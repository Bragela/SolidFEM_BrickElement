using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class DeconstructNode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructNode class.
        /// </summary>
        public DeconstructNode()
          : base("Deconstruct Node", "DeconstructNode",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "A NodeClass object", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Global ID", "gID", "Global ID of the node", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Local ID", "lID", "Local ID of the node", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "The point coordinates of the node", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "S", "Support type of the node", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            List<List<NodeClass>> nodes = new List<List<NodeClass>>();
            DA.GetDataList(0, nodes);
            Type hei= DA.GetType();


            List<int> globalIdList = new List<int>();
            List<int> localIdList = new List<int>();
            List<Point3d> nodePointList = new List<Point3d>();
            List<SupportClass> supportList = new List<SupportClass>();


            foreach (NodeClass node in nodes[0])
            {
               
                globalIdList.Add(node.GlobalID);
                localIdList.Add(node.LocalID);
                nodePointList.Add(node.Point);
                supportList.Add(node.support);
                
            }


            //outputs
            DA.SetDataList(0, globalIdList);
            DA.SetDataList(1, localIdList);
            DA.SetDataList(2, nodePointList);
            DA.SetDataList(3, supportList);
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
            get { return new Guid("5FA60215-DA15-4C51-AD7D-6003CAD14F03"); }
        }
    }
}