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
    public class Support
    {
        // --- field ---
        public Point3d Pt { get; private set; }

        public List<bool> Conditions { get; private set; } = null;
        public Node Node { get; set; } = null;
        public List<double[]> React { get; private set; } = new List<double[]>();

        // --- constructors --- 
        public Support() { }

        public Support(Point3d _pt, string _conditions)
        {
            Pt = _pt;
            Conditions = new List<bool>();

            ProcessConditions(_conditions);
        }

        // --- methods ---
        private void ProcessConditions(string _conditions)
        {
            if (_conditions.Length != 6)
            {
                Conditions = null;
                return;
            }

            char[] cs = _conditions.ToCharArray();

            for(int i=0; i<6; i++)
            {
                if (cs[i] == '0')
                {
                    Conditions.Add(false);
                }
                else if (cs[i] == '1')
                {
                    Conditions.Add(true);
                }
                else
                {
                    Conditions = null;
                    break;
                }
            }

        }

        public Support DeepCopy()
        {
            return (Support)base.MemberwiseClone();
        }
        public override string ToString()
        {
            string cond;

            if (Conditions != null)
            {
                cond = String.Join(",", Conditions);
            }

            else
            {
                cond = "error in support condition";
            }

            string txt = "Support, " + cond;
            return txt;
        }
        public bool IsValid()
        {
            return Conditions != null;
        }

    }

    public class GH_Support : GH_Goo<Support>
    {
        public GH_Support() { }
        public GH_Support(GH_Support other) : base(other.Value)
        {
            this.Value = other.Value.DeepCopy();
        }
        public GH_Support(Support sup) : base(sup)
        {
            this.Value = sup;
        }
        public override bool IsValid => base.m_value.IsValid();
        public override string TypeName => "Support";
        public override string TypeDescription => "Support";
        public override IGH_Goo Duplicate()
        {
            return new GH_Support(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Param_Support : GH_PersistentParam<GH_Support>
    {
        public Param_Support() : base(
            new GH_InstanceDescription(
                "Support", "Sup", "Support conditions", Common.category, Common.sub_param
                )
            )
        { }

        public override Guid ComponentGuid => new Guid("20865c43-3c73-4371-94a4-58890294c7bd");

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.icons_P_Sup; } }  //Set icon image

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Support> values)
        {
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Support value)
        {
            return GH_GetterResult.success;
        }


    }

}
