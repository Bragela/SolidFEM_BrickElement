using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement.Components.Deconstructors
{
    public class DeconstructElement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructElement class.
        /// </summary>
        public DeconstructElement()
          : base("DeconstructElement", "DeconstructElement",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element", "E", "A ElementClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Global ID", "ID", "Global ID of the element", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodes", "N", "The nodes of the element", GH_ParamAccess.list);
            pManager.AddMeshParameter("Mesh", "M", "The elements mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            ElementClass elem = new ElementClass();

            DA.GetData(0, ref elem);

            //outputs
            DA.SetData(0, elem.ID);
            DA.SetData(1, elem.Nodes);
            DA.SetData(2, elem.Mesh);
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
            get { return new Guid("58735854-42FB-47F0-85B8-8C24214304EA"); }
        }
    }
}