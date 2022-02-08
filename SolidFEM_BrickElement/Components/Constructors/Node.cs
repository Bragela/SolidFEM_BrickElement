using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Node : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Node class.
        /// </summary>
        public Node()
          : base("Node", "Node",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("GlobalID", "GID", "Global ID of the node", GH_ParamAccess.item);
            pManager.AddNumberParameter("LocalID", "LID", "Local ID of the node", GH_ParamAccess.item);
            pManager.AddPointParameter("Point","P","The point coordinates of the node",GH_ParamAccess.item);
            pManager.AddGenericParameter("Support", "S", "The support type of the node, a SupportClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "A NodeClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            int gID = 0;
            int lID = 0;
            Point3d pt = new Point3d();
            SupportClass sup = new SupportClass();

            DA.GetData(0, ref gID);
            DA.GetData(1, ref lID);
            DA.GetData(2, ref pt);
            DA.GetData(3, ref sup);


            //code

            NodeClass node = new NodeClass(gID, lID, pt, sup);

            //outputs
            DA.SetData(0, node);
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
            get { return new Guid("207C44BE-BF29-4795-9DAF-19F0831FF008"); }
        }
    }
}