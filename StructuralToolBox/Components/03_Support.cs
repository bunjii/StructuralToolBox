using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class ST_Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _03_Support class.
        /// </summary>
        public ST_Support()
          : base("Support", "Sup",
              "Support condition",
              Common.category, Common.sub_sup)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string cond_txt = 

                "support conditions in 6 directions: \n " +
                "   Tx: Translation in X direction \n " +
                "   Ty: Translation in Y direction \n " +
                "   Tz: Translation in Z direction \n " +
                "   Rx: Rotation in X direction \n" +
                "   Ry: Rotation in Y direction \n " +
                "   Rz: Rotation in Z direction \n " +
                "   0: Free, 1: Fixed \n \n" +
                "For instance 111001 means the point is fixed in Tx, Ty, Tz, and Rz directions";

            pManager.AddPointParameter("points", "pts", "Points", GH_ParamAccess.list);
            pManager.AddTextParameter("conditions", "cond", cond_txt, GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Support(), "Support", "Sup", "Support", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<Point3d> pts = new List<Point3d>();
            string cond_string = null;
            List<GH_Support> gh_sups = new List<GH_Support>();

            // --- input --- 
            if (!DA.GetDataList(0, pts)) { return; }
            if (!DA.GetData(1, ref cond_string)) { return; };

            // --- solve ---
            foreach (Point3d p in pts)
            {
                Support sup = new Support(p, cond_string);
                if (sup.Conditions != null)
                {
                    gh_sups.Add(new GH_Support(sup));
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check conditions");
                }


            }

            // --- output ---
            DA.SetDataList(0, gh_sups);

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
                return StructuralToolBox.Properties.Resources.icons_C_Sup;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("56049756-a0c1-4082-aad3-08741fd50cf0"); }
        }
    }
}