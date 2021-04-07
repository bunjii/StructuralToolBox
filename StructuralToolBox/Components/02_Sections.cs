using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class ST_Section_Rect : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _02_Section class.
        /// </summary>
        public ST_Section_Rect()
          : base("Sec_Rect", "Sec_R",
              "Rectangular cross-section",
              Common.category, Common.sub_sec)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("width", "w", "Width [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "h", "Height [mm]", GH_ParamAccess.item);
            // pManager.AddNumberParameter("theta", "theta", "theta angle [rad]", GH_ParamAccess.item, 0.0);
            
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            // pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Section(), "Section", "Sec", "Section", GH_ParamAccess.item);
            pManager.AddCurveParameter("Curves", "crvs", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;
            string tag = null;
            double b = new double();
            double h = new double();
            // double theta = new double();

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }
            if (!DA.GetData(1, ref tag)) { return; }
            if (!DA.GetData(2, ref b)) { return; }
            if (!DA.GetData(3, ref h)) { return; }
            // DA.GetData(4, ref theta);

            // --- solve ---
            GH_Section gh_sec = new GH_Section(new Section_Rect(gMat.Value, tag, b, h));

            // --- output ---
            DA.SetData(0, gh_sec);
            DA.SetDataList(1, gh_sec.Value.Curves);
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
                return StructuralToolBox.Properties.Resources.icons_C_Sec_R;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e616de28-6370-422d-9964-078450b17df3"); }
        }
    }

    public class ST_Section_Circular : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _02_Section class.
        /// </summary>
        public ST_Section_Circular()
          : base("Sec_Circ", "Sec_Circ",
              "Circular cross-section",
              Common.category, Common.sub_sec)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("diameter", "d", "Diameter [mm]", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Section(), "Section", "Sec", "Section");
            pManager.AddCurveParameter("Curves", "crvs", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;
            string tag = null;
            double d = new double();

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }
            if (!DA.GetData(1, ref tag)) { return; }
            if (!DA.GetData(2, ref d)) { return; }

            // --- solve ---
            GH_Section gh_sec = new GH_Section(new Section_Circular(gMat.Value, tag, d));

            // --- output ---
            DA.SetData(0, gh_sec);
            DA.SetDataList(1, gh_sec.Value.Curves);

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
                return StructuralToolBox.Properties.Resources.icons_C_Sec_C;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0789bcab-2bae-46e5-ad95-d6274ff3cd50"); }
        }
    }

    public class ST_Section_I : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _02_Section class.
        /// </summary>
        public ST_Section_I()
          : base("Sec_I", "Sec_I",
              "I cross-section",
              Common.category, Common.sub_sec)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "h", "Height [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("width", "w", "Width [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("t web", "tw", "thickness of web [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("t flange", "tf", "thickness of flange [mm]", GH_ParamAccess.item);
            //pManager.AddNumberParameter("theta", "theta", "theta angle [rad]", GH_ParamAccess.item, 0.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            //pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Section(), "Section", "Sec", "Section");
            pManager.AddCurveParameter("Curves", "crvs", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;
            string tag = null;
            double h = new double();
            double w = new double();
            double tw = new double();
            double tf = new double();
            //double theta = new double();

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }
            if (!DA.GetData(1, ref tag)) { return; }
            if (!DA.GetData(2, ref h)) { return; }
            if (!DA.GetData(3, ref w)) { return; }
            if (!DA.GetData(4, ref tw)) { return; }
            if (!DA.GetData(5, ref tf)) { return; }
            //DA.GetData(6, ref theta);

            // --- solve ---
            GH_Section gh_sec = new GH_Section(new Section_I(gMat.Value, tag, h , w, tw, tf));

            // --- output ---
            DA.SetData(0, gh_sec);
            DA.SetDataList(1, gh_sec.Value.Curves);

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
                return StructuralToolBox.Properties.Resources.icons_C_Sec_I;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("678f5356-2c17-49da-83b2-69cf83707ff3"); }
        }
    }

    public class ST_Section_RHS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _02_Section class.
        /// </summary>
        public ST_Section_RHS()
          : base("Sec_RHS", "Sec_RHS",
              "Rectangular hollow cross-section",
              Common.category, Common.sub_sec)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "h", "Height [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("width", "w", "Width [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("t web", "tw", "thickness of web [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("t flange", "tf", "thickness of flange [mm]", GH_ParamAccess.item);
            //pManager.AddNumberParameter("theta", "theta", "theta angle [rad]", GH_ParamAccess.item, 0.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            //pManager[6].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Section(), "Section", "Sec", "Section");
            pManager.AddCurveParameter("Curves", "crvs", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;
            string tag = null;
            double h = new double();
            double w = new double();
            double tw = new double();
            double tf = new double();
            // double theta = new double();

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }
            if (!DA.GetData(1, ref tag)) { return; }
            if (!DA.GetData(2, ref h)) { return; }
            if (!DA.GetData(3, ref w)) { return; }
            if (!DA.GetData(4, ref tw)) { return; }
            if (!DA.GetData(5, ref tf)) { return; }
            //DA.GetData(6, ref theta);

            // --- solve ---
            GH_Section gh_sec = new GH_Section(new Section_RHS(gMat.Value, tag, h, w, tw, tf));

            // --- output ---
            DA.SetData(0, gh_sec);
            DA.SetDataList(1, gh_sec.Value.Curves);
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
                return StructuralToolBox.Properties.Resources.icons_C_Sec_RHS;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("afaf4c62-82df-49ca-ba0e-b6ba5cc43a32"); }
        }
    }

    public class ST_Section_CHS : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _02_Section class.
        /// </summary>
        public ST_Section_CHS()
          : base("Sec_CHS", "Sec_CHS",
              "Circular hollow cross-section",
              Common.category, Common.sub_sec)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("diameter", "d", "Diameter [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("thickness", "t", "Thickness [mm]", GH_ParamAccess.item);

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
            pManager.RegisterParam(new Param_Section(), "Section", "Sec", "Section");
            pManager.AddCurveParameter("Curves", "crvs", "curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;
            string tag = null;
            double d = new double();
            double t = new double();

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }
            if (!DA.GetData(1, ref tag)) { return; }
            if (!DA.GetData(2, ref d)) { return; }
            if (!DA.GetData(3, ref t)) { return; }

            // --- solve ---
            GH_Section gh_sec = new GH_Section(new Section_CHS(gMat.Value, tag, d, t));

            // --- output ---
            DA.SetData(0, gh_sec);
            DA.SetDataList(1, gh_sec.Value.Curves);
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
                return StructuralToolBox.Properties.Resources.icons_C_Sec_CHS;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4d501331-6cd3-4e43-ab38-77b631484737"); }
        }
    }

}