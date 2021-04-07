using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace StructuralToolBox
{
    public class Disassemble_Material : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Material()
          : base("Info_Material", "Info_Mat",
              "Material information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddNumberParameter("E", "E", "Modulus of Elasticity [MPa]=[N/mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("G", "G", "Shear Modulus [MPa]=[N/mm2]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Gamma", "Gamma", "Unit Weight [kN/m3]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Alpha", "Alpha", "Coefficient of linear thermal expansion [K^(-1)]", GH_ParamAccess.item);
            pManager.AddNumberParameter("Fy", "Fy", "Yield stringth [MPa]=[N/mm2]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Material gMat = null;

            // --- input --- 
            if (!DA.GetData(0, ref gMat)) { return; }

            // --- solve ---

            // --- output ---

            DA.SetData(0, gMat.Value.Tag);
            DA.SetData(1, gMat.Value.E);
            DA.SetData(2, gMat.Value.G);
            DA.SetData(3, gMat.Value.Gamma);
            DA.SetData(4, gMat.Value.Alpha);
            DA.SetData(5, gMat.Value.Fy);

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
                return StructuralToolBox.Properties.Resources.icons_I_Mat;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cfe0b1df-8ff7-466d-bf62-b11e13a4d488"); }
        }
    }

    public class Disassemble_Section : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Section()
          : base("Info_Section", "Info_Sec",
              "Cross-section information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Section(), "Section", "Sec", "Section", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new Param_Material(), "Material", "Mat", "Material", GH_ParamAccess.item);
            pManager.AddTextParameter("tag", "tag", "tag", GH_ParamAccess.item);
            pManager.AddTextParameter("dimensions", "dim", "Dimensions [mm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("theta", "theta", "Theta angle [rad]", GH_ParamAccess.item);
            pManager.AddNumberParameter("structural props", "props", "structural props: A, Iy, Iz, J, Wy, Wz ", GH_ParamAccess.list);
            pManager.AddCurveParameter("curves", "crvs", "curves", GH_ParamAccess.list);
        }


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Section gSec = null;
            string dims;
            List<double> structural_props = new List<double>();

            // --- input --- 
            if (!DA.GetData(0, ref gSec)) { return; }

            // --- solve ---
            dims = gSec.Value.GetDims();
            structural_props.Add(gSec.Value.Area);
            structural_props.Add(gSec.Value.Iy);
            structural_props.Add(gSec.Value.Iz);
            structural_props.Add(gSec.Value.J);
            structural_props.Add(gSec.Value.Wy);
            structural_props.Add(gSec.Value.Wz);


            // --- output ---

            DA.SetData(0, new GH_Material(gSec.Value.Mat));
            DA.SetData(1, gSec.Value.Tag);
            DA.SetData(2, dims);
            DA.SetData(3, gSec.Value.Theta);
            DA.SetDataList(4, structural_props);
            DA.SetDataList(5, gSec.Value.Curves);

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
                return StructuralToolBox.Properties.Resources.icons_I_Sec;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("50776b1e-b39f-48ed-abe2-be3c7438da8c"); }
        }
    }

    public class Disassemble_Support : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Support()
          : base("Info_Support", "Info_Sup",
              "Support information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Support(), "Support", "Sup", "Support", GH_ParamAccess.list);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "pts", "points", GH_ParamAccess.list);
            pManager.AddTextParameter("conditions", "cond", "conditions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<GH_Support> gSups = new List<GH_Support>();

            // --- input --- 
            if (!DA.GetDataList(0, gSups)) { return; }

            // --- solve ---
            IEnumerable<Point3d> pts = gSups.Select(x => x.Value.Pt);
            IEnumerable<string> conds = gSups
                .Select(x => String.Join(",", x.Value.Conditions));

            // --- output ---

            DA.SetDataList(0, pts);
            DA.SetDataList(1, conds);

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
                return StructuralToolBox.Properties.Resources.icons_I_Sup;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6c178387-f5a7-4143-b5c4-671a6b2a3370"); }
        }
    }

    public class Disassemble_Element_1D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Element_1D()
          : base("Info_Element_1D", "Info_Elem1D",
              "Element 1D information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Element1D(), "Element_1D", "Elem1D", "Element 1D", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "lns", "Lines", GH_ParamAccess.list);
            pManager.AddParameter(new Param_Node(), "Nodes", "Nodes", "Nodes", GH_ParamAccess.tree);
            pManager.RegisterParam(new Param_Section(), "Sections", "Secs", "Sections", GH_ParamAccess.list);
            pManager.AddNumberParameter("Beta angle", "b angle", "Beta Angle", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "pln", "plane", GH_ParamAccess.list);
            pManager.AddTextParameter("Tags", "tags", "Tags", GH_ParamAccess.list);
            pManager.AddIntegerParameter("IDs", "ids", "Ids", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // --- variables ---
            List<GH_Element_1D> gh_elem1Ds = new List<GH_Element_1D>();
            DataTree<GH_Node> gh_node_tr = new DataTree<GH_Node>();

            // --- input --- 
            if (!DA.GetDataList(0, gh_elem1Ds)) { return; }

            // --- solve ---
            IEnumerable<Line> lns = gh_elem1Ds.Select(x => x.Value.Line);
            IEnumerable<GH_Section> gh_secs = gh_elem1Ds.Select(x => new GH_Section(x.Value.Sec));
            IEnumerable<double> betas = gh_elem1Ds.Select(x => x.Value.Beta);
            IEnumerable<Plane> plns = gh_elem1Ds.Select(x => x.Value.Pln);
            IEnumerable<string> tags = gh_elem1Ds.Select(x => x.Value.Tag);
            IEnumerable<int?> ids = gh_elem1Ds.Select(x => x.Value.Id);

            for (int i=0; i< gh_elem1Ds.Count;i++)
            {
                Element_1D e = gh_elem1Ds[i].Value;
                GH_Path path = new GH_Path(i);

                foreach (Node n in e.Nodes)
                {
                    gh_node_tr.Add(new GH_Node(n), path);
                }
            }

            // --- output ---
            DA.SetDataList(0, lns);
            DA.SetDataTree(1, gh_node_tr);
            DA.SetDataList(2, gh_secs);
            DA.SetDataList(3, betas);
            DA.SetDataList(4, plns);
            DA.SetDataList(5, tags);
            DA.SetDataList(6, ids);

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
                return StructuralToolBox.Properties.Resources.icons_I_Elem1D;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("30be9ffc-f328-4cf8-a62d-b1e2d286c106"); }
        }
    }

    public class Disassemble_Model : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Model()
          : base("Info_Model", "Info_Model",
              "Model information",
               Common.category, Common.sub_info)
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
            pManager.AddPointParameter("Points", "pts", "Points", GH_ParamAccess.list);
            pManager.AddLineParameter("Lines", "lns", "Lines", GH_ParamAccess.list);
            pManager.RegisterParam(new Param_Node(), "Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
            pManager.RegisterParam(new Param_Element1D(), "Element_1Ds", "Elem1Ds", "Element 1Ds", GH_ParamAccess.list);
            pManager.RegisterParam(new Param_Support(),
                "Supports", "Sups", "Supports", GH_ParamAccess.list);
            pManager.RegisterParam(new Param_Load(),
                "Loads", "Loads", "Loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // --- variables ---
            GH_Model gh_model = null;

            // --- input --- 
            if (!DA.GetData(0, ref gh_model)) { return; }

            // --- solve ---

            IEnumerable<Point3d> pts = gh_model.Value.Nodes.Select(x => x.Pt);
            IEnumerable<Line> lns = gh_model.Value.Elem1Ds.Select(x => x.Line);
            IEnumerable<GH_Node> gh_nodes = gh_model.Value.Nodes.Select(x => new GH_Node(x));
            IEnumerable<GH_Element_1D> gh_elem1Ds = gh_model.Value.Elem1Ds.Select(x => new GH_Element_1D(x));
            IEnumerable<GH_Support> gh_sups = gh_model.Value.Sups.Select(x => new GH_Support(x));
            IEnumerable<GH_Load> gh_loads = gh_model.Value.Loads.Select(x => new GH_Load(x));

            // --- output ---
            DA.SetDataList(0, pts);
            DA.SetDataList(1, lns);
            DA.SetDataList(2, gh_nodes);
            DA.SetDataList(3, gh_elem1Ds);
            DA.SetDataList(4, gh_sups);
            DA.SetDataList(5, gh_loads);

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
                return StructuralToolBox.Properties.Resources.icons_I_Mdl;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c6bfbad4-820d-4ce0-afca-840281b85f64"); }
        }
    }

    public class Disassemble_Node : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Node()
          : base("Info_Node", "Info_Node",
              "Node information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Node(), "Node", "Node", "Node", GH_ParamAccess.list);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "pts", "points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("id", "id", "id", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<GH_Node> gh_nds = new List<GH_Node>();

            // --- input --- 
            if (!DA.GetDataList(0, gh_nds)) { return; }

            // --- solve ---
            IEnumerable<Point3d> pts = gh_nds.Select(x => x.Value.Pt);
            IEnumerable<int?> ids = gh_nds
                .Select(x => x.Value.Id);

            // --- output ---

            DA.SetDataList(0, pts);
            DA.SetDataList(1, ids);

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
                return StructuralToolBox.Properties.Resources.icons_I_Node;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0b69c820-7f10-45a8-a1f2-9dd8da51c768"); }
        }
    }

    public class Disassemble_Load_Pt : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _99_Disassemble_Material class.
        /// </summary>
        public Disassemble_Load_Pt()
          : base("Info_PointLoad", "Info_PLoad",
              "Point Load information",
               Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Load(), "Load", "Load", "Load", GH_ParamAccess.list);

            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("points", "pts", "points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Force Vector", "FVec", "Force Vector", GH_ParamAccess.list);
            pManager.AddVectorParameter("Moment Vector", "MVec", "Moment Vector", GH_ParamAccess.list);
            pManager.AddIntegerParameter("load case", "lc", "load case", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            List<GH_Load> gh_lds = new List<GH_Load>();

            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> fvec = new List<Vector3d>();
            List<Vector3d> mvec = new List<Vector3d>();
            List<int> lcs = new List<int>();

            // --- input --- 
            if (!DA.GetDataList(0, gh_lds)) { return; }


            // --- solve ---
            foreach (GH_Load gl in gh_lds)
            {
                Load l = gl.Value;

                if (!(l is Load_Point lp)) continue;
                
                pts.Add(lp.Pt);
                fvec.Add(new Vector3d(lp.Loads[0], lp.Loads[1], lp.Loads[2]));
                mvec.Add(new Vector3d(lp.Loads[3], lp.Loads[4], lp.Loads[5]));
                lcs.Add(lp.Lc.Value);

            }

            // --- output ---

            DA.SetDataList(0, pts);
            DA.SetDataList(1, fvec);
            DA.SetDataList(2, mvec);
            DA.SetDataList(3, lcs);

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
                return StructuralToolBox.Properties.Resources.icons_I_Load_P;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("699a7aa0-9192-411c-b452-4e3496b96d80"); }
        }
    }

}