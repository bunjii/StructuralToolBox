using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StructuralToolBox
{
    public class ST_CroSecOpt : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _07_CroSecOpt class.
        /// </summary>
        public ST_CroSecOpt()
          : base("Crosec_Opt_EC3", "CrosecOpt",
              "Cross-section optimisation using EC3",
              Common.category, Common.sub_analize)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddParameter(new Param_Section(), "Section", "Sec", "Section", GH_ParamAccess.list);
            pManager.AddTextParameter("element tags", "e tags", "Element tags", GH_ParamAccess.list);
            pManager.AddNumberParameter("gamma_M0", "gamma_M0", "override gamma_M0", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("gamma_M1", "gamma_M1", "override gamma_M1", GH_ParamAccess.item, 1.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Model(), GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "Weight", "Weight in [kg]", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iteration", "Iteration", "Iteration", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_m = new GH_Model();
            List<GH_Section> gh_secs = new List<GH_Section>();
            List<string> eids = new List<string>();
            double gM0 = 1.0;
            double gM1 = 1.0;

            // --- input --- 
            if(!DA.GetData(0, ref gh_m)) { return; }
            if(!DA.GetDataList(1, gh_secs)) { return; }
            DA.GetDataList(2, eids);
            DA.GetData(3, ref gM0);
            DA.GetData(4, ref gM1);

            // --- solve ---

            // // copy the model
            Model mdl = Common.DeepCopy(gh_m.Value);
            
            // // select only relevant element and replace crosec
            //IEnumerable<Element_1D> rel_elems = mdl.Elem1Ds
            //    .Where(e => eids.Contains(e.Tag))
            //    .Where(e => e.Sec is Section_CHS || e.Sec is Section_I || e.Sec is Section_RHS)
            //    .Select(e => new Element_1D(e.Line, e.Tag, gh_secs[0].Value, e.Vz, e.Buckling_Length));

            List<Element_1D> new_elems = new List<Element_1D>();
            foreach (Element_1D e in mdl.Elem1Ds)
            {
                if ((e.Sec is Section_CHS || e.Sec is Section_I || e.Sec is Section_RHS) && eids.Contains(e.Tag))
                {
                    new_elems.Add(new Element_1D(e.Line, e.Tag, gh_secs[0].Value, e.Vz, e.Buckling_Length));
                }
                else
                {
                    new_elems.Add(new Element_1D(e.Line, e.Tag, e.Sec, e.Vz, e.Buckling_Length));
                }
            }

            mdl = new Model(new_elems, mdl.Sups, mdl.Loads);
            SolveLS slv = new SolveLS(ref mdl);
            mdl = slv.Mdl;

            // // calculate buckling length
            foreach (Element_1D rel_e in new_elems)
            {
                UpdateBucklingLength(rel_e);
            }

            // // loop for available cross-sections
            int max_iter = gh_secs.Count;
            int cnt_iter = 0;

            while(cnt_iter < max_iter)
            {
                bool continue_flg = false;

                // // select only relevant elements
                foreach (Element_1D e in mdl.Elem1Ds)
                {
                    if (((e.Sec is Section_CHS || e.Sec is Section_I || e.Sec is Section_RHS) && eids.Contains(e.Tag)) == false)
                    {
                        continue;
                    }

                        // mat prop
                        double epsilon = Math.Sqrt(235 / e.Sec.Mat.Fy);

                    // section class

                    //int sec_class = 4;
                    //if (e.Sec is Section_CHS sec_chs)
                    //{
                    //    double d_over_t = sec_chs.D / sec_chs.T;
                    //    if (d_over_t <= 50 * Math.Pow(epsilon, 2)) sec_class = 1;
                    //    else if (d_over_t <= 70 * Math.Pow(epsilon, 2)) sec_class = 2;
                    //    else if (d_over_t <= 90 * Math.Pow(epsilon, 2)) sec_class = 3;
                    //}

                    //else if (e.Sec is Section_I sec_i)
                    //{
                    //    double c_over_t_web = (sec_i.H - 2 * sec_i.Tf) / sec_i.Tw;
                    //    double c_over_t_fl = 0.5 * (sec_i.W - sec_i.Tw) / sec_i.Tf;
                    //    if (c_over_t_fl <= 9 * epsilon && c_over_t_web <= 33 * epsilon) sec_class = 1;
                    //    else if (c_over_t_fl <= 10 * epsilon && c_over_t_web <= 38 * epsilon) sec_class = 2;
                    //    else if (c_over_t_fl <= 14 * epsilon && c_over_t_web <= 42 * epsilon) sec_class = 3;
                    //}

                    //else if (e.Sec is Section_RHS sec_rhs)
                    //{
                    //    double c_over_t = Math.Max((sec_rhs.H - 2 * sec_rhs.Tf) / sec_rhs.Tw, (sec_rhs.W - 2 * sec_rhs.Tw) / sec_rhs.Tf);
                    //    if (c_over_t <= 33 * epsilon) sec_class = 1;
                    //    else if (c_over_t <= 38 * epsilon) sec_class = 2;
                    //    else if (c_over_t <= 42 * epsilon) sec_class = 3;
                    //}

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

                    double alpha2 = 0.0;
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
                    foreach (int l in mdl.LCs)
                    {

                        // 1. calculate element forces

                        int lc_id = Array.IndexOf(mdl.LCs, l);
                        double[] F = e.Calc_Forces(lc_id);

                        double N_Ed  = Math.Max(Math.Abs(F[0]), Math.Abs(F[6])) ; // N
                        double My_Ed = Math.Max(Math.Abs(F[4]), Math.Abs(F[10])); // Nm
                        double Mz_Ed = Math.Max(Math.Abs(F[5]), Math.Abs(F[11])); // Nm

                        // 2. calculate cross sec util

                        double util_crosec = N_Ed / N_Rd + My_Ed / My_Rd + Mz_Ed / Mz_Rd;

                        // Rhino.RhinoApp.WriteLine(util_crosec.ToString());

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

                        // Rhino.RhinoApp.WriteLine(util6xx.ToString());
                        
                        if (util_cro_6xx > util) util = util_cro_6xx;
                    }

                    // // // change cross sections
                    if (util > 1.0)
                    {
                        continue_flg = true;
                        int secid = gh_secs.FindIndex(s => s.Value.Tag == e.Sec.Tag);
                        if (secid == gh_secs.Count - 1) continue;
                        e.Sec = gh_secs[secid + 1].Value;
                        // RhinoApp.WriteLine("updated");
                    }

                }

                // // // solve 
                // // select only relevant element and replace crosec
                //IEnumerable<Element_1D> sel_elems = mdl.Elem1Ds
                //    .Where(e => eids.Contains(e.Tag))
                //    .Where(e => e.Sec is Section_CHS || e.Sec is Section_I || e.Sec is Section_RHS)
                //    .Select(e => new Element_1D(e.Line, e.Tag, e.Sec, e.Vz, e.Buckling_Length));

                List<Element_1D> updated_elems = new List<Element_1D>();
                foreach (Element_1D e in mdl.Elem1Ds)
                {
                    updated_elems.Add(new Element_1D(e.Line, e.Tag, e.Sec, e.Vz, e.Buckling_Length));
                }

                mdl = new Model(updated_elems, mdl.Sups, mdl.Loads);
                slv = new SolveLS(ref mdl);
                mdl = slv.Mdl;

                if (continue_flg == false)
                {
                    break;
                }

                cnt_iter++;
                RhinoApp.WriteLine(cnt_iter.ToString()+ ": finished");
            }

            

            // --- output ---
            DA.SetData(0, new GH_Model(mdl));
            DA.SetData(1, mdl.Weight);
            DA.SetData(2, cnt_iter);

        }

        private void UpdateBucklingLength(Element_1D _e)
        {

            double bucklen = _e.Buckling_Length;

            foreach (Node n in _e.Nodes)
            {
                Node current_n = n;
                Element_1D current_e = _e;

                bool active = true;
                while (active)
                {
                    if (current_n.Elems.Count == 1)
                    {
                        if (current_n.Sup != null)
                        {
                            // support position
                            active = false;
                        }
                        else
                        {
                            // cantilever
                            bucklen *= 2;
                            active = false;
                        }
                    }
                    else if (current_n.Elems.Count == 2)
                    {
                        // it is not a joint
                        current_e = current_n.Elems.Where(e => e.Id != current_e.Id).ToList()[0];
                        current_n = current_e.Nodes.Where(x => x.Id != current_n.Id).ToList()[0];
                        bucklen += current_e.Buckling_Length;

                    }
                    else
                    {
                        // it is a joint
                        active = false;
                    }
                }
            }

            _e.Buckling_Length = bucklen;
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
                return StructuralToolBox.Properties.Resources.icons_C_Sol_CSOpt;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("da6e54d9-b108-46b6-b985-8187cd1b2042"); }
        }
    }
}