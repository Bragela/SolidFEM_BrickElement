﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace SolidFEM_BrickElement
{
    public class DeconstructAssembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructAssembly class.
        /// </summary>
        public DeconstructAssembly()
          : base("DeconstructAssembly", "DeconstructAssembly",
              "Description",
              "SolidFEM", "SolidFEM_Brick")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "A", "A AssemblyClass object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "E", "List of elements in the assembly", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "L", "List of loads in the assembly", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material", "M", "Material of the assembly", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //inputs
            AssemblyClass assembly = new AssemblyClass();
            DA.GetData(0, ref assembly);

            //outputs
            DA.SetDataList(0, assembly.elements);
            DA.SetDataList(1, assembly.loads);
            DA.SetData(2, assembly.material);
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
            get { return new Guid("8C615F13-4664-4A20-A417-AB457DC713E5"); }
        }
    }
}