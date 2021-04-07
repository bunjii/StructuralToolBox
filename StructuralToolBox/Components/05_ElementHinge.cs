using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using CSparse;
using CSparse.Storage;
using CSparse.Double;
using CSparse.Double.Factorization;

namespace StructuralToolBox
{
    public class ST_ElementHinge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _05_ElementHinge class.
        /// </summary>
        public ST_ElementHinge()
          : base("Hinge", "Hinge",
              "Add Hinges at Element ends",
              Common.category, Common.sub_elem)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Element1D(), "Element_1D", "Elem1Ds", "elements", GH_ParamAccess.list);
            pManager.AddBooleanParameter("start My","sMy", "set this to False for making a hinge at start point around local-y axis", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("start Mz", "sMz", "set this to False for making a hinge at start point around local-z axis", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("end My", "eMy", "set this to False for making a hinge at end point around local-y axis", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("end Mz", "sMy", "set this to False for making a hinge at start point around local-y axis", GH_ParamAccess.item, true);

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
            pManager.RegisterParam(new Param_Element1D(), GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // --- variables ---
            List<GH_Element_1D> gh_elems = new List<GH_Element_1D>();
            bool sMy = true;
            bool sMz = true;
            bool eMy = true;
            bool eMz = true;

            List<GH_Element_1D> gh_elems_out = new List<GH_Element_1D>();

            // --- input --- 
            if (!DA.GetDataList(0, gh_elems)) { return; }
            DA.GetData(1, ref sMy);
            DA.GetData(2, ref sMz);
            DA.GetData(3, ref eMy);
            DA.GetData(4, ref eMz);

            // --- solve ---
            foreach (GH_Element_1D gh_e in gh_elems)
            {
                Element_1D e = new Element_1D(gh_e.Value.Line, gh_e.Value.Tag, gh_e.Value.Sec, gh_e.Value.Vz);

                e.SetHingeData(sMy, sMz, eMy, eMz);
                e.EK = e.Calc_ElemStiffMX();
                e.TM = e.Calc_TransMX();
                e.EKG = e.TM.Transpose().Multiply(e.EK).Multiply(e.TM) as DenseMatrix;

                gh_elems_out.Add(new GH_Element_1D(e));

            }

            // --- output ---

            DA.SetDataList(0, gh_elems_out);

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
                return StructuralToolBox.Properties.Resources.icons_C_ElemHinge;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5f21c04f-4949-4f56-b2e0-c203f53683b5"); }
        }
    }
}