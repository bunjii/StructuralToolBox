using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using CSparse;
using CSparse.Storage;
using CSparse.Double;
using CSparse.Double.Factorization;

namespace StructuralToolBox
{
    public class ST_Elem1DForces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _08_Elem1DForces class.
        /// </summary>
        public ST_Elem1DForces()
          : base("Elem1D_Forces", "E1D_Forces",
              "Resulting forces of element1D",
              Common.category, Common.sub_post)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess
                .item);
            pManager.AddTextParameter("Element tags", "e tags", "element tags", GH_ParamAccess.list);
            pManager.AddIntegerParameter("load cases", "lcs", "load cases", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Element1D(), "Elements", "elems", "selected elements", GH_ParamAccess.list);
            pManager.AddNumberParameter("N", "N", "Axial forces in [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vy", "Vy", "Shear Vz in [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Vz", "Vz", "Shear Vy in [kN]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mx", "Mx", "Moment Mx in [kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("My", "My", "Moment My in [kNm]", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Mz", "Mz", "Moment Mz in [kNm]", GH_ParamAccess.tree);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_mdl = new GH_Model();
            List<string> etags = new List<string>();
            List<int> lcs = new List<int>();

            DataTree<double> N = new DataTree<double>();
            DataTree<double> Vy = new DataTree<double>();
            DataTree<double> Vz = new DataTree<double>();
            DataTree<double> Mx = new DataTree<double>();
            DataTree<double> My = new DataTree<double>();
            DataTree<double> Mz = new DataTree<double>();

            // --- input --- 
            if (!DA.GetData(0, ref gh_mdl)) { return; }
            DA.GetDataList(1, etags);
            DA.GetDataList(2, lcs);

            // --- solve ---

            IEnumerable<Element_1D> elems;
            // // select only relevant elements
            if (etags.Count != 0)
            {
                elems = gh_mdl.Value.Elem1Ds.Where(x => etags.Contains(x.Tag));
            }
            else
            {
                elems = gh_mdl.Value.Elem1Ds;
            }

            if (lcs.Count == 0)
            {
                lcs = new List<int>() { 0 };
            }

            int[] LC_ids = gh_mdl.Value.Loads.Select(x => x.Lc.Value).Distinct().ToArray();

            // // for selected load cases
            foreach (int lc in lcs)
            {
                
                int lc_id = Array.IndexOf(LC_ids, lc);

                for (int i=0; i<elems.Count();i++)
                {
                    double[] F = elems.ElementAt(i).Calc_Forces(lc_id);

                    GH_Path path = new GH_Path(i, lc);

                    N.Add(-F[0] * Math.Pow(10, -3), path);
                    Vy.Add(-F[1] * Math.Pow(10, -3), path);
                    Vz.Add(-F[2] * Math.Pow(10, -3), path);
                    Mx.Add(-F[3] * Math.Pow(10, -3), path);
                    My.Add(-F[4] * Math.Pow(10, -3), path);
                    Mz.Add(-F[5] * Math.Pow(10, -3), path);

                    N.Add(F[6] * Math.Pow(10, -3), path);
                    Vy.Add(F[7] * Math.Pow(10, -3), path);
                    Vz.Add(F[8] * Math.Pow(10, -3), path);
                    Mx.Add(F[9] * Math.Pow(10, -3), path);
                    My.Add(F[10] * Math.Pow(10, -3), path);
                    Mz.Add(F[11] * Math.Pow(10, -3), path);
                }
                
            }

            IEnumerable<GH_Element_1D> gh_elems = elems.Select(x => new GH_Element_1D(x));

            // --- output ---
            DA.SetDataList(0, gh_elems);
            DA.SetDataTree(1, N);
            DA.SetDataTree(2, Vy);
            DA.SetDataTree(3, Vz);
            DA.SetDataTree(4, Mx);
            DA.SetDataTree(5, My);
            DA.SetDataTree(6, Mz);

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
                return StructuralToolBox.Properties.Resources.icons_C_ElemForces;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3da85b6b-41e8-44e9-8ad6-67032ef3da5f"); }
        }
    }
}