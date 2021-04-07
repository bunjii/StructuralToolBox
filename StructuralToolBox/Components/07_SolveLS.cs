using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class ST_SolveLS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _07_AnalyseLS class.
        /// </summary>
        public ST_SolveLS()
          : base("Solve Linear Static", "Solve LS",
              "Solve Linear Static",
              Common.category, Common.sub_analize)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_mdl = null;

            // --- input --- 
            if (!DA.GetData(0, ref gh_mdl)) { return; }

            // --- solve ---

            Model mdl = gh_mdl.Value;
            SolveLS slv = new SolveLS(ref mdl);

            // --- output ---

            DA.SetData(0, new GH_Model(slv.Mdl));
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
                return StructuralToolBox.Properties.Resources.icons_C_Sol_LS;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e995046e-59df-41d7-b4f3-5de8341cc49c"); }
        }
    }
}