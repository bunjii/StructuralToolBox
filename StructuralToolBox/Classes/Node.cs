using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace StructuralToolBox
{
    [Serializable]
    public class Node
    {
        // --- field ---
        public Point3d Pt { get; set; }
        public int? Id { get; private set; } = null;
        public int[] IJK { get; private set; }

        public Support Sup { get; set; } = null;
        public List<double[]> Disps { get; private set; } = new List<double[]>();

        // --- constructors --- 
        public Node() { }
        public Node(Point3d _pt, int _id, BoundingBox _bb)
        {
            Pt = _pt;
            Id = _id;
            IJK = GetIJK(_pt, _bb);

        }

        // --- methods --

        private static int[] GetIJK(Point3d _pt, BoundingBox _bb)
        {
            int[] ijk = new int[3];

            double xseg = (_bb.Max.X - _bb.Min.X) / Common.BBOX_SEGMENT;
            double yseg = (_bb.Max.Y - _bb.Min.Y) / Common.BBOX_SEGMENT;
            double zseg = (_bb.Max.Z - _bb.Min.Z) / Common.BBOX_SEGMENT;

            ijk[0] = Math.Min((int) Math.Floor(_pt.X / xseg), Common.BBOX_SEGMENT);
            ijk[1] = Math.Min((int) Math.Floor(_pt.Y / yseg),
                Common.BBOX_SEGMENT);
            ijk[2] = Math.Min((int) Math.Floor(_pt.Z / zseg), Common.BBOX_SEGMENT);

            return ijk;
        }

        public static Node FindNode(Point3d _pt, List<Node> _nodes, BoundingBox _bb)
        {
            Node nd = null;

            int[] IJK = GetIJK(_pt, _bb);

            IEnumerable<Node> sub_nodes = _nodes.Where(n => n.IJK[0] == IJK[0] && n.IJK[1] == IJK[1] && n.IJK[2] == IJK[2]);

            foreach (Node n in sub_nodes)
            {
                if (_pt.DistanceTo(n.Pt) < Common.PRES)
                {
                    nd = n;
                    break;
                }
            }

            return nd;
        }

        public Node DeepCopy()
        {
            return (Node)base.MemberwiseClone();
        }
        public override string ToString()
        {
            string txt = "Node, " + Id.ToString() + ", " + Pt.ToString();
            return txt;
        }
        public bool IsValid()
        {
            return Id != null;
        }
    }


    public class GH_Node : GH_Goo<Node>
    {
        public GH_Node() { }
        public GH_Node(GH_Node other) : base(other.Value)
        {
            this.Value = other.Value.DeepCopy();
        }
        public GH_Node(Node node) : base(node)
        {
            this.Value = node;
        }
        public override bool IsValid => base.m_value.IsValid();
        public override string TypeName => "Node";
        public override string TypeDescription => "Node";
        public override IGH_Goo Duplicate()
        {
            return new GH_Node(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Param_Node : GH_PersistentParam<GH_Node>
    {
        public Param_Node() : base(
            new GH_InstanceDescription(
                "Node", "Node", "Node properties", Common.category, Common.sub_param
                )
            )
        { }

        public override Guid ComponentGuid => new Guid("c529fc51-8e5c-4790-bd5e-7f4e41b02281");

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.icons_P_Node; } }  //Set icon image

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Node> values)
        {
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Node value)
        {
            return GH_GetterResult.success;
        }


    }


}
