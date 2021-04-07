using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace StructuralToolBox
{
    [Serializable]
    public abstract class Load
    {
        // --- field ---
        public int? Lc { get; protected set; } = null;

        // --- constructors --- 
        public Load() { }

        // --- methods ---

        public Load DeepCopy()
        {
            return (Load)base.MemberwiseClone();
        }

        public virtual string LoadType()
        {
            return "";
        }

        //public override string ToString()
        //{
        //    string cond;

        //    if (Conditions != null)
        //    {
        //        cond = String.Join(",", Conditions);
        //    }

        //    else
        //    {
        //        cond = "error in support condition";
        //    }

        //    string txt = "<Support>" + "Conditions: " + cond;
        //    return txt;
        //}

        public bool IsValid()
        {
            return Lc != null;
        }

    }

    [Serializable]
    public class Load_Point : Load
    {
        // --- field ---
        public Point3d Pt { get; private set; }
        public List<double> Loads { get; } = new List<double>();
        public Node Node { get; set; } = null;

        // --- constructors --- 
        public Load_Point() { }

        public Load_Point(Point3d _pt, Vector3d _fvec, Vector3d _mvec, int _lc)
        {
            // class field
            Pt = _pt;

            Loads.Add(_fvec.X);
            Loads.Add(_fvec.Y);
            Loads.Add(_fvec.Z);
            Loads.Add(_mvec.X);
            Loads.Add(_mvec.Y);
            Loads.Add(_mvec.Z);

            // base class field
            Lc = _lc;

        }

        // --- methods ---
        public override string LoadType()
        {
            return "Point Load";
        }

        public override string ToString()
        {
            string txt = "";
            txt += "Point Load at ";
            txt += Pt.ToString();

            return txt;
        }

    }

    public class GH_Load : GH_Goo<Load>
    {
        public GH_Load() { }
        public GH_Load(GH_Load other) : base(other.Value)
        {
            this.Value = other.Value.DeepCopy();
        }
        public GH_Load(Load load) : base(load)
        {
            this.Value = load;
        }
        public override bool IsValid => base.m_value.IsValid();
        public override string TypeName => "Load";
        public override string TypeDescription => "Load";
        public override IGH_Goo Duplicate()
        {
            return new GH_Load(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }

    }

    public class Param_Load : GH_PersistentParam<GH_Load>
    {
        public Param_Load() : base(
            new GH_InstanceDescription(
                "Load", "load", "Load", Common.category, Common.sub_param
                )
            )
        { }

        public override Guid ComponentGuid => new Guid("e08af909-1c20-43f9-935c-83370e38de67");

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.icons_P_Load_P; } }  //Set icon image

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Load> values)
        {
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Load value)
        {
            return GH_GetterResult.success;
        }


    }

}
