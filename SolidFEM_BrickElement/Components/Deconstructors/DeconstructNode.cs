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
          : base("DeconstructNode", "DeconstructNode",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "N", "A NodeClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Global ID", "gID", "Global ID of the node", GH_ParamAccess.item);
            pManager.AddNumberParameter("Local ID", "lID", "Local ID of the node", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "The point coordinates of the node", GH_ParamAccess.item);
            pManager.AddGenericParameter("Support", "S", "Support type of the node", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            NodeClass node = new NodeClass();
            DA.GetData(0, ref node);


            //outputs
            DA.SetData(0, node.GlobalID);
            DA.SetData(0, node.LocalID);
            DA.SetData(0, node.Point);
            DA.SetData(0, node.support);
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