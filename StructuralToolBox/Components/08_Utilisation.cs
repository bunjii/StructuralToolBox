using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StructuralToolBox
{
    public class ST_Utilisation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _08_Utilisation class.
        /// </summary>
        public ST_Utilisation()
          : base("Utilisation", "Utilisation",
              "Utilisation",
              Common.category, Common.sub_post)
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
            pManager.RegisterParam(new Param_Element1D(), "Elements", "elems", "elements", GH_ParamAccess.tree);
            pManager.AddNumberParameter("utilisaton", "util", "utilisation by EC3", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_m = new GH_Model();
            double gM0 = 1.0;
            double gM1 = 1.0;

            DataTree<GH_Element_1D> elems = new DataTree<GH_Element_1D>();
            DataTree<double> utils = new DataTree<double>();

            // --- input --- 
            if (!DA.GetData(0, ref gh_m)) { return; }

            Model mdl = Common.DeepCopy(gh_m.Value);

            // --- solve ---
            // foreach (Element_1D e in mdl.Elem1Ds)
            for (int i=0; i< mdl.Elem1Ds.Count;i++)
            {
                Element_1D e = mdl.Elem1Ds[i];

                // mat prop
                // double epsilon = Math.Sqrt(235 / e.Sec.Mat.Fy);

                // cross section properties
                double iy = Math.Sqrt(e.Sec.Iy / e.Sec.Area); // [mm]
                double iz = Math.Sqrt(e.Sec.Iz / e.Sec.Area); // [mm]

                // cross section resistence eq.(6.2)
                double N_Rd = e.Sec.Area * e.Sec.Mat.Fy / gM0; // [N];
                double My_Rd = e.Sec.Wy * e.Sec.Mat.Fy / gM0 * Math.Pow(10, -3);// [Nm]
                double Mz_Rd = e.Sec.Wz * e.Sec.Mat.Fy / gM0 * Math.Pow(10, -3);// [Nm]

                double N_Rk = e.Sec.Area * e.Sec.Mat.Fy; // [N]
                double My_Rk = e.Sec.Wpy * e.Sec.Mat.Fy * Math.Pow(10, -3);// [Nm]
                double Mz_Rk = e.Sec.Wpz * e.Sec.Mat.Fy * Math.Pow(10, -3);// [Nm]

                double Lcr = e.Buckling_Length * 1000; //[mm]

                double lambda_1 = Math.PI * Math.Sqrt(e.Sec.Mat.E / e.Sec.Mat.Fy);
                double lambda_bar_y = (Lcr / iz) * (1 / lambda_1);
                double lambda_bar_z = (Lcr / iy) * (1 / lambda_1);

                double alpha2;
                if (e.Sec.Mat.Fy < 460) alpha2 = 0.21;
                else alpha2 = 0.13;

                double Phi_y = 0.5 * (1 + alpha2 * (lambda_bar_y - 0.2) + Math.Pow(lambda_bar_y, 2));
                double Phi_z = 0.5 * (1 + alpha2 * (lambda_bar_z - 0.2) + Math.Pow(lambda_bar_z, 2));

                double Chi_y = Math.Min(1 / (Phi_y + Math.Sqrt(Math.Pow(Phi_y, 2) - Math.Pow(lambda_bar_y, 2))), 1.0);
                double Chi_z = Math.Min(1 / (Phi_z + Math.Sqrt(Math.Pow(Phi_z, 2) - Math.Pow(lambda_bar_z, 2))), 1.0);
                double Chi_LT = 1.0; // no lateral torsional buckling assumed

                double Cmy = 0.9;
                double Cmz = 0.9;

                double util = 0.0;

                // foreach loadcase

                // foreach (int l in mdl.LCs)
                for(int j=0; j<mdl.LCs.Count() ; j++)
                {
                    int l = mdl.LCs[j];

                    // 1. calculate element forces

                    int lc_id = Array.IndexOf(mdl.LCs, l);
                    double[] F = e.Calc_Forces(lc_id);

                    double N_Ed = Math.Max(Math.Abs(F[0]), Math.Abs(F[6])); // N
                    double My_Ed = Math.Max(Math.Abs(F[4]), Math.Abs(F[10])); // Nm
                    double Mz_Ed = Math.Max(Math.Abs(F[5]), Math.Abs(F[11])); // Nm

                    // 2. calculate cross sec util

                    double util_crosec = N_Ed / N_Rd + My_Ed / My_Rd + Mz_Ed / Mz_Rd;


                    // 3. calculate member stresses

                    double N_c_Ed = Math.Min(F[0], F[6]);
                    if (N_c_Ed >= 0)
                    {
                        // tension member
                        if (util_crosec > util) util = util_crosec;
                        continue;
                    }

                    // compression member
                    double kyy = Math.Min(
                      Cmy * (1 + (lambda_bar_y - 0.2) * (Math.Abs(N_Ed) / (Chi_y * N_Rk / gM1))),
                      Cmy * (1 + 0.8 * (Math.Abs(N_Ed) / (Chi_y * N_Rk / gM1)))
                      );

                    double kzz = Math.Min(
                      Cmz * (1 + (lambda_bar_z - 0.2) * (Math.Abs(N_Ed) / (Chi_z * N_Rk / gM1))),
                      Cmz * (1 + 0.8 * (Math.Abs(N_Ed) / (Chi_z * N_Rk / gM1)))
                      );

                    double kyz = 0.6 * kzz;
                    double kzy = 0.6 * kyy;

                    double util661 = (Math.Abs(N_Ed) / (Chi_y * N_Rk / gM1))
                        + kyy * (Math.Abs(My_Ed) / (Chi_LT * My_Rk / gM1))
                        + kyz * (Math.Abs(Mz_Ed) / (Mz_Rk / gM1));

                    double util662 = (Math.Abs(N_Ed) / (Chi_z * N_Rk / gM1))
                        + kzy * (Math.Abs(My_Ed) / (Chi_LT * My_Rk / gM1))
                        + kzz * (Math.Abs(Mz_Ed) / (Mz_Rk / gM1));

                    double util6xx = Math.Max(util661, util662);
                    double util_cro_6xx = Math.Max(util_crosec, util6xx);

                    if (util_cro_6xx > util) util = util_cro_6xx;

                    GH_Path path = new GH_Path(i);
                    if (j == 0)
                    {
                        elems.Add(new GH_Element_1D(e), path);
                    }
                    
                    utils.Add(util, path);
                }


            }

            // --- output ---
            DA.SetDataTree(0, elems);
            DA.SetDataTree(1, utils);

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
                return StructuralToolBox.Properties.Resources.icons_C_Sol_Util;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9b11dd5f-6d34-47bb-bfc7-6a3a5ee197dd"); }
        }
    }
}