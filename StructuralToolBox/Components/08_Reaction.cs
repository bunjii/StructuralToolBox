using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StructuralToolBox.Components
{
    public class ST_Reaction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _08_Reaction class.
        /// </summary>
        public ST_Reaction()
          : base("Reaction", "Reaction",
              "Reaction forces",
              Common.category, Common.sub_post)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Load cases", "LCs", "Load cases", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Support(), GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "Pts", "supporting points", GH_ParamAccess.list);
            pManager.AddNumberParameter("FX", "FX", "reaction force [kN] in global X-direction", GH_ParamAccess.tree);
            pManager.AddNumberParameter("FY", "FY", "reaction force [kN] in global Y-direction", GH_ParamAccess.tree);
            pManager.AddNumberParameter("FZ", "FZ", "reaction force [kN] in global Z-direction", GH_ParamAccess.tree);
            pManager.AddNumberParameter("MX", "MX", "reaction moment [kNm] around X-direction", GH_ParamAccess.tree);
            pManager.AddNumberParameter("MY", "MY", "reaction moment [kNm] around Y-direction", GH_ParamAccess.tree);
            pManager.AddNumberParameter("MZ", "MZ", "reaction moment [kNm] around Z-direction", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_mdl = new GH_Model();
            List<int> lcs = new List<int>();

            DataTree<double> FX = new DataTree<double>();
            DataTree<double> FY = new DataTree<double>();
            DataTree<double> FZ = new DataTree<double>();
            DataTree<double> MX = new DataTree<double>();
            DataTree<double> MY = new DataTree<double>();
            DataTree<double> MZ = new DataTree<double>();

            // --- input --- 
            if (!DA.GetData(0, ref gh_mdl)) { return; }
            DA.GetDataList(1, lcs);

            // --- solve ---
            IEnumerable<Support> sups = gh_mdl.Value.Sups;
            IEnumerable<Point3d> pts = gh_mdl.Value.Sups.Select(x => x.Pt);

            if (lcs.Count == 0)
            {
                lcs = new List<int>() { 0 };
            }

            int[] LC_ids = gh_mdl.Value.Loads.Select(x => x.Lc.Value).Distinct().ToArray();

            // // for selected load cases
            foreach (int lc in lcs)
            {
                int lc_id = Array.IndexOf(LC_ids, lc);

                for (int i=0; i< sups.Count();i++)  
                {
                    Support s = sups.ElementAt(i);
                    GH_Path path = new GH_Path(i);
                    FX.Add(s.React[lc_id][0] * Math.Pow(10, -3), path);
                    FY.Add(s.React[lc_id][1] * Math.Pow(10, -3), path);
                    FZ.Add(s.React[lc_id][2] * Math.Pow(10, -3), path);
                    MX.Add(s.React[lc_id][3] * Math.Pow(10, -3), path);
                    MY.Add(s.React[lc_id][4] * Math.Pow(10, -3), path);
                    MZ.Add(s.React[lc_id][5] * Math.Pow(10, -3), path);
                }
            }

            IEnumerable<GH_Support> gh_sups = sups.Select(x => new GH_Support(x));

            // --- output ---
            DA.SetDataList(0, gh_sups);
            DA.SetDataList(1, pts);
            DA.SetDataTree(2, FX);
            DA.SetDataTree(3, FY);
            DA.SetDataTree(4, FZ);
            DA.SetDataTree(5, MX);
            DA.SetDataTree(6, MY);
            DA.SetDataTree(7, MZ);

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
                return StructuralToolBox.Properties.Resources.icons_C_Reaction;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8aecf318-bc80-4691-afcf-43d7b453ff05"); }
        }
    }
}