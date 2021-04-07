using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace StructuralToolBox
{
    public class ST_Element1D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _05_Element1D class.
        /// </summary>
        public ST_Element1D()
          : base("Element_1D", "Elem1D",
              "Element 1D",
              Common.category, Common.sub_elem)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "lns", "Lines", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Section(), "Section", "Sec", "Section", GH_ParamAccess.item);
            pManager.AddTextParameter("Tag", "tag", "Tag", GH_ParamAccess.item);
            pManager.AddVectorParameter("z-dir", "z-dir", "z-dir", GH_ParamAccess.list);

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
            pManager.RegisterParam(new Param_Element1D(), "Element_1D", "elem1D", "Element 1D", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<Line> lines = new List<Line>();
            GH_Section gh_sec = new GH_Section();
            string tag = null;
            List<Vector3d> zs = new List<Vector3d>();

            List<GH_Element_1D> gh_elem1Ds; 

            // --- input ---
            if (!DA.GetDataList(0, lines)) { return; }
            if (!DA.GetData(1, ref gh_sec)) { return; }
            if (!DA.GetData(2, ref tag)) { return; }
            DA.GetDataList(3, zs);

            // --- solve ---

            // Rhino.RhinoApp.WriteLine("##########################");

            gh_elem1Ds = new List<GH_Element_1D>(lines.Count);

            for (int i=0; i<lines.Count;i++)
            {
                Vector3d vz;;
                if (i < zs.Count)
                {
                    vz = zs[i];
                    // Rhino.RhinoApp.WriteLine("if vz = : " + zs[i]);
                }
                else if (zs.Count == 0)
                {
                    vz = new Vector3d(0, 0, 0);
                    // Rhino.RhinoApp.WriteLine("else if vz = : " + vz);
                }

                else
                {
                    vz = zs[zs.Count - 1]; // new Vector3d();
                    // Rhino.RhinoApp.WriteLine("else vz = : " + vz.ToString());
                }

                gh_elem1Ds.Add(new GH_Element_1D(new Element_1D(lines[i], tag, gh_sec.Value, vz)));
                // Rhino.RhinoApp.WriteLine("---------------------------");
            }


            // --- output ---
            DA.SetDataList(0, gh_elem1Ds);

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
                return StructuralToolBox.Properties.Resources.icons_C_Elem1D;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("73b7e67d-1efd-4597-8bea-a5bbe05768e6"); }
        }
    }
}