using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class Assembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Assembly class.
        /// </summary>
        public Assembly()
          : base("Assembly", "Assembly",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "List of elements to assemble", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "List of loads to assemble", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material of the assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "A", "A AssemblyClass object", GH_ParamAccess.item);
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
            MaterialClass mat = new MaterialClass();

            DA.GetData(0, ref elems);
            DA.GetData(1, ref loads);
            DA.GetData(2, ref mat);

            //code
            AssemblyClass assembly = new AssemblyClass(elems, loads, mat);

            //outputs
            DA.SetData(0, assembly);
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
            get { return new Guid("7A48565A-B41D-4F11-B7B2-6D37AE2491E8"); }
        }
    }
}