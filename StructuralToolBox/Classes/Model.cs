using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using CSparse;
using CSparse.Storage;
using CSparse.Double;
using CSparse.Double.Factorization;

namespace StructuralToolBox
{
    [Serializable]
    public class Model
    {
        // --- field ---
        public List<Element_1D> Elem1Ds { get; private set; } = null;
        public List<Support> Sups { get; private set; } = null;
        public List<Load> Loads { get; private set; } = null;

        public BoundingBox Bbox { get; private set; }

        public List<Node> Nodes { get; private set; }
        public double Weight { get; private set; }

        public List<bool> Validity { get; private set; } = new List<bool>();
        public DenseColumnMajorStorage<double> KG { get; set; } = null;
        public DenseColumnMajorStorage<double> LM { get; set; } = null;
        public List<double[]> Disps { get; private set; } = new List<double[]>();
        public int? SelectedLC { get; set; } = null;

        // --- constructors --- 
        public Model() { }
        public Model(List<Element_1D> _elem1Ds, List<Support> _sups, List<Load> _loads)
        {
            Elem1Ds = _elem1Ds;
            Sups = _sups;
            Loads = _loads;

            Weight = Calc_Weight();
            
            Bbox = CreateBBox();
            Nodes = CreateNodes_SetElemIds();

            Validity.Add(CheckSupports());
            Validity.Add(CheckLoads());
        }

        // --- methods --

        private double Calc_Weight()
        {
            double weight = 0.0;
            
            foreach (Element_1D e in Elem1Ds)
            {
                weight += e.Weight;
            }

            return weight;
        }

        private List<Node> CreateNodes_SetElemIds()
        {
            int cnt_elemid = 0;
            List<Node> nodes = new List<Node>();
            foreach (Element_1D elem in Elem1Ds)
            {
                NodeCheckAndRegister(elem, ref nodes);
                elem.Id = cnt_elemid;
                cnt_elemid++;
            }

            return nodes;
        }

        private void NodeCheckAndRegister(Element_1D _elem, ref List<Node> _nodes)
        {
            _elem.Nodes.Clear();

            List<Point3d> pts = new List<Point3d>(2) 
                                    { _elem.Line.From, _elem.Line.To };

            foreach (Point3d p in pts)
            {
                Node nd = Node.FindNode(p, _nodes, Bbox);

                if (nd == null)
                {
                    nd = new Node(p, _nodes.Count, Bbox);
                    _nodes.Add(nd);

                }

                _elem.Nodes.Add(nd);
            }

        }

        private BoundingBox CreateBBox()
        {
            BoundingBox bb = new BoundingBox();
            IEnumerable<Line> lns = Elem1Ds.Select(x => x.Line);
            foreach (Line l in lns)
            {
                bb.Union(l.From);
                bb.Union(l.To);
            }

            return bb;
        }

        private bool CheckSupports()
        {
            foreach (Support s in Sups)
            {
                Node nd = Node.FindNode(s.Pt, Nodes, Bbox);

                if (nd == null)
                {
                    return false;
                }

                else
                {
                    s.Node = nd;
                    nd.Sup = s;
                }
            }

            return true;
        }

        private bool CheckLoads()
        {
            foreach (Load l in Loads)
            {
                if (!(l is Load_Point pl)) continue;

                Node nd = Node.FindNode(pl.Pt, Nodes, Bbox);

                if (nd == null)
                {
                    return false;
                }

                else
                {
                    pl.Node = nd;
                }
            }

            return true;
        }

        public Model DeepCopy()
        {
            return (Model)base.MemberwiseClone();
        }

        public override string ToString()
        {
            string txt = "Model, Node: " + Nodes.Count.ToString();
            return txt;
        }
        public bool IsValid()
        {

            return (Elem1Ds != null) && (Sups != null) && (Loads != null);
        }
    }

    public class GH_Model : GH_Goo<Model>
    {
        public GH_Model() { }
        public GH_Model(GH_Model other) : base(other.Value)
        {
            this.Value = other.Value.DeepCopy();
        }
        public GH_Model(Model mdl) : base(mdl)
        {
            this.Value = mdl;
        }
        public override bool IsValid => base.m_value.IsValid();
        public override string TypeName => "Model";
        public override string TypeDescription => "Model";
        public override IGH_Goo Duplicate()
        {
            return new GH_Model(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Param_Model : GH_PersistentParam<GH_Model>
    {
        public Param_Model() : base(
            new GH_InstanceDescription(
                "Model", "Model", "Model 1D", Common.category, Common.sub_param
                )
            )
        { }

        public override Guid ComponentGuid => new Guid("6c011bb1-e5ae-4f80-9125-6a7f9615975b");

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.icons_P_Mdl; } }  //Set icon image

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Model> values)
        {
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Model value)
        {
            return GH_GetterResult.success;
        }


    }


}
