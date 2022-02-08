using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Element : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Element class.
        /// </summary>
        public Element()
          : base("Element", "Element",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("ID", "ID", "Global ID of the element", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodes","N","List of the elements nodes",GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The elements mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "ElementClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            int ID = 0;
            List<NodeClass> nodes = new List<NodeClass>();
            Mesh mesh = new Mesh();

            DA.GetData(0, ref ID);
            DA.GetData(1, ref nodes);
            DA.GetData(2, ref mesh);

            //code

            ElementClass Element = new ElementClass(ID, nodes, mesh);

            //outputs
            DA.SetData(0, Element);
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
            get { return new Guid("0AC6493E-EABB-43F3-B282-6D32F3008BB4"); }
        }
    }
}