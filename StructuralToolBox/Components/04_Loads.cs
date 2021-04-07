using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class ST_Load_Point : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _04_Loads class.
        /// </summary>
        public ST_Load_Point()
          : base("point load", "pt load",
              "Point Load [kN]",
              Common.category, Common.sub_load)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("point", "pt", "loading point", GH_ParamAccess.list);
            pManager.AddVectorParameter("force vector", "fvec", "force vector [kN]", GH_ParamAccess.item, new Vector3d(0, 0, 0));
            pManager.AddVectorParameter("moment vector", "mvec", "moment vector [kNm]", GH_ParamAccess.item, new Vector3d(0,0,0));
            pManager.AddIntegerParameter("load case", "lc", "load case", GH_ParamAccess.item, 0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Load(), "Load", "Load", "Load", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<Point3d> pts = new List<Point3d>();
            Vector3d fvec = new Vector3d();
            Vector3d mvec = new Vector3d();
            int lc = new int();
            List <GH_Load> gh_lds = new List<GH_Load>();
            
            // --- input --- 
            if (!DA.GetDataList(0, pts)) { return; }
            DA.GetData(1, ref fvec);
            DA.GetData(2, ref mvec);
            DA.GetData(3, ref lc);

            // --- solve ---
            foreach (Point3d p in pts)
            {
                gh_lds.Add(new GH_Load(new Load_Point(p, fvec, mvec, lc)));
            }

            // --- output ---
            DA.SetDataList(0, gh_lds);
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
                return StructuralToolBox.Properties.Resources.icons_C_Load_P;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("04f0386e-58ee-4a1f-aae7-4f4b72f0020f"); }
        }
    }
}