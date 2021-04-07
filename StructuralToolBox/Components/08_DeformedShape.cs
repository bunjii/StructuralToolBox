using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox.Components
{
    public class ST_DeformedShape : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _08_DeformedShape class.
        /// </summary>
        public ST_DeformedShape()
          : base("Deformed_Shape", "Deform",
              "Deformed shape",
              Common.category, Common.sub_post)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Load Case", "LC", "Load Case", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Factor", "factor", "Factor", GH_ParamAccess.item, 20.0);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Model(), "DefModel", "Def M", "Deformed Model", GH_ParamAccess.item);
            pManager.AddVectorParameter("Disp", "Disp", "Displacement in [m]", GH_ParamAccess.list);
            pManager.AddVectorParameter("Rotation", "Rot", "Rotation in [rad]", GH_ParamAccess.list);
            pManager.AddPointParameter("points", "pts", "points", GH_ParamAccess.list);
            pManager.AddLineParameter("lines", "lns", "lines", GH_ParamAccess.list);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_mdl = new GH_Model();
            Model mdl = new Model();
            int lc = new int();
            double factor = new double();

            List<Vector3d> dispvecs = new List<Vector3d>();
            List<Vector3d> rotvecs = new List<Vector3d>();
            List<Point3d> pts = new List<Point3d>();
            List<Line> lns = new List<Line>();

            // --- input --- 
            if(!DA.GetData(0, ref gh_mdl)) { return; }
            if(!DA.GetData(1, ref lc)) { return; }
            DA.GetData(2, ref factor);

            // --- solve --- 
            mdl = Common.DeepCopy(gh_mdl.Value);

            int[] LCs = mdl.Loads.Select(x => x.Lc.Value).Distinct().ToArray();
            int lc_id = Array.IndexOf(LCs, lc);

            foreach (Node n in mdl.Nodes)
            {
                Point3d pt = new Point3d(
                        n.Pt.X + n.Disps[lc_id][0]*factor,
                        n.Pt.Y + n.Disps[lc_id][1]*factor,
                        n.Pt.Z + n.Disps[lc_id][2]*factor
                    );

                pts.Add(pt);

                dispvecs.Add(new Vector3d(n.Disps[lc_id][0], n.Disps[lc_id][1], n.Disps[lc_id][2]));
                rotvecs.Add(new Vector3d(n.Disps[lc_id][3], n.Disps[lc_id][4], n.Disps[lc_id][5]));

            }

            foreach (Element_1D e in mdl.Elem1Ds)
            {
                Node sn = e.Nodes[0];
                Node en = e.Nodes[1];

                Line defln = new Line(
                        sn.Pt.X + sn.Disps[lc_id][0] * factor,
                        sn.Pt.Y + sn.Disps[lc_id][1] * factor,
                        sn.Pt.Z + sn.Disps[lc_id][2] * factor,
                        en.Pt.X + en.Disps[lc_id][0] * factor,
                        en.Pt.Y + en.Disps[lc_id][1] * factor,
                        en.Pt.Z + en.Disps[lc_id][2] * factor
                    );

                lns.Add(defln);

                e.Line = defln;

                Vector3d vz0 = e.Pln.YAxis;
                Vector3d vy0 = e.Pln.XAxis;
                Vector3d vx0 = e.Pln.ZAxis;

                Vector3d vx1 = e.Line.UnitTangent;
                Vector3d vy1 = Vector3d.CrossProduct(vz0, vx1);
                Vector3d vz1 = Vector3d.CrossProduct(vx1, vy1);

                Plane newPln = new Plane(e.Line.From, vy1, vz1);

                e.Pln = newPln;
            }

            mdl.SelectedLC = lc;

            // --- output ---
            DA.SetData(0, new GH_Model(mdl));
            DA.SetDataList(1, dispvecs);
            DA.SetDataList(2, rotvecs);
            DA.SetDataList(3, pts);
            DA.SetDataList(4, lns);

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
                return StructuralToolBox.Properties.Resources.icons_C_Def;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cc6ded52-947d-4087-836b-d7924eaf310c"); }
        }
    }
}