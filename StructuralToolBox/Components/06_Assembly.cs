using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class ST_Assembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _06_Assembly class.
        /// </summary>
        public ST_Assembly()
          : base("Assembly", "assembly",
              "Assembly",
              Common.category, Common.sub_assem)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Element1D(), "Element_1Ds", "Elem1Ds", "elements", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Support(), "Supports", "Sups", "Supports", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Load(), "Loads", "Loads", "Loads", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddNumberParameter("Weight", "Weight", "Weight in [kg]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<GH_Element_1D> gh_elem1Ds = new List<GH_Element_1D>();
            List<GH_Support> gh_sups = new List<GH_Support>();
            List<GH_Load> gh_loads = new List<GH_Load>();

            // --- input --- 
            if (!DA.GetDataList(0, gh_elem1Ds)) { return; }
            if (!DA.GetDataList(1, gh_sups)) { return; }
            if (!DA.GetDataList(2, gh_loads)) { return; }


            List<Element_1D> elem1Ds = gh_elem1Ds.Select(x => x.Value).ToList();
            List<Support> sups = gh_sups.Select(x => x.Value).ToList();
            List<Load> loads = gh_loads.Select(x => x.Value).ToList();

            // --- solve ---
            GH_Model gh_mdl = new GH_Model(new Model(elem1Ds, sups, loads));

            if (gh_mdl.Value.Validity[0] == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check Support position.");
            }
            if (gh_mdl.Value.Validity[1] == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Check loading position.");
            }

            // --- output ---
            DA.SetData(0, gh_mdl);
            DA.SetData(1, gh_mdl.Value.Weight);

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
                return StructuralToolBox.Properties.Resources.icons_C_Mdl;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("808d8923-c452-4ccf-9b08-bde47896ba39"); }
        }
    }
}