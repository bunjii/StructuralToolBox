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
    public class Section
    {
        // --- field ---
        public string Tag { get; protected set; } = "N/A";
        public Material Mat {get; protected set; }
        public string Type { get; protected set; }
        public double Area { get; protected set; }
        public double Iy { get; protected set; }
        public double Iz { get; protected set; }
        public double J { get; protected set; }
        public double Wy { get; protected set; }
        public double Wz { get; protected set; }
        public double Theta { get; set; }
        public List<Curve> Curves { get; protected set; } = new List<Curve>();
        public List<Point3d> Pts { get; protected set; } = new List<Point3d>();

        // --- constructors --- 
        public Section() { }

        // --- methods ---
        public Section DeepCopy()
        {
            return (Section)base.MemberwiseClone();
        }

        public virtual string GetDims()
        {
            return "";
        }
        public override string ToString()
        {
            string txt = "Cross-Section, " + Type + ", ";
            txt += Tag + ", ";
            txt += Mat.Tag;

            return txt;
        }
        public bool IsValid()
        {
            return Tag != "N/A";
        }
    }

    [Serializable]
    public class Section_Rect : Section
    {
        // --- field ---
        public double B { get; private set; }
        public double H { get; private set; }


        // --- constructors --- 

        public Section_Rect() { }
        public Section_Rect(Material _mat, string _tag, double _b, double _h, double _theta = 0.0)
        {
            // class field
            B = _b;
            H = _h;

            // base class field
            Tag = _tag;
            Mat = _mat;
            Theta = _theta;
            Type = "Rectangular";

            Area = B * H;

            Iy = Calculate_I(true);
            Iz = Calculate_I(false);

            Wy = Iy / (0.5 * H);
            Wz = Iz / (0.5 * B);

            J = Calculate_J();

            Curves.Add(DrawCurve());

        }

        // --- methods ---

        private Curve DrawCurve()
        {
            List<Point3d> pts = new List<Point3d>
            {
                new Point3d(-0.5 * B * 0.001, 0.5 * H * 0.001, 0),
                new Point3d(0.5 * B* 0.001, 0.5 * H* 0.001, 0),
                new Point3d(0.5 * B* 0.001, -0.5 * H* 0.001, 0),
                new Point3d(-0.5 * B* 0.001, -0.5 * H* 0.001, 0),
                new Point3d(-0.5 * B* 0.001, 0.5 * H* 0.001, 0)
            };

            Pts = pts.GetRange(0, pts.Count - 1);

            return new Polyline(pts).ToNurbsCurve();

        }

        private double Calculate_I(bool _bl)
        {
            double b;
            double h;

            if (_bl)
            {
                b = B;
                h = H;
            }
            else
            {
                b = H;
                h = B;
            }

            double val = 1.0 / 12.0 * (b * Math.Pow(h, 3));

            return val;
        }
        
        private double Calculate_J()
        {
            double a = 0.5 * Math.Max(B, H);
            double b = 0.5 * Math.Min(B, H);

            double val = a * Math.Pow(b, 3) * 
                (16.0 / 3.0 - 3.36 * b / a * (1.0 - Math.Pow(b, 4) / (12.0 * Math.Pow(a, 4))));

            return val;
        }

        public override string GetDims()
        {
            string txt = Type + ", ";
            txt += B.ToString() + ", ";
            txt += H.ToString();

            return txt;
        }

    }

    [Serializable]
    public class Section_Circular : Section
    {
        // --- field ---
        public double D { get; private set; }

        // --- constructors --- 
        public Section_Circular() { }
        public Section_Circular(Material _mat, string _tag, double _d)
        {
            // class field
            D = _d;

            // base class field
            Tag = _tag;
            Mat = _mat;
            Theta = 0.0;
            Type = "Circular";

            Area = 0.25 * Math.PI * Math.Pow(D,2);

            Iy = Math.PI / 64.0 * Math.Pow(D, 4);
            Iz = Iy;

            Wy = Iy / (0.5 * D);
            Wz = Wy;

            J = 1.0 / 32.0 * Math.PI * Math.Pow(D, 4);

            Curves.Add(DrawCurve());

        }

        // --- methods ---

        private Curve DrawCurve()
        {
            Curve crv = new Circle(0.5 * D * 0.001).ToNurbsCurve();
            crv.DivideByCount(Common.DIV_CIRCLE, true, out Point3d[] pts);

            Pts = pts.ToList();

            return crv;
        }
        public override string GetDims()
        {
            string txt = this.Type + ", ";
            txt += this.D.ToString();

            return txt;
        }

    }

    [Serializable]
    public class Section_I : Section
    {
        // --- field ---
        public double H { get; private set; }
        public double W { get; private set; }
        public double Tw { get; private set; }
        public double Tf { get; private set; }

        // --- constructors --- 

        public Section_I() { }
        public Section_I(Material _mat, string _tag, double _h, double _w, double _tw, double _tf, double _theta = 0.0)
        {
            // class field
            H = _h;
            W = _w;
            Tw = _tw;
            Tf = _tf;

            // base class field
            Tag = _tag;
            Mat = _mat;
            Theta = _theta;
            Type = "I";

            Area = W * H - (W-Tw)*(H-2.0*Tf);

            Iy = 1.0 / 12.0 * (W * Math.Pow(H, 3) - (W - Tw) * Math.Pow(H - 2.0 * Tf, 3));
            Iz = 1.0 / 12.0 * (2.0*Tf * Math.Pow(W, 3) + (H - 2.0*Tf) * Math.Pow(Tw, 3));

            Wy = Iy / (0.5 * H);
            Wz = Iz / (0.5 * W);

            J = 1.0 / 3.0 * (2.0 * W * Math.Pow(Tf, 3) + (H - 2.0 * Tf) * Math.Pow(Tw, 3));

            Curves.Add(DrawCurve());

        }

        // --- methods ---

        private Curve DrawCurve()
        {
            List<Point3d> pts = new List<Point3d> {
                new Point3d(-0.5 * W * 0.001, 0.5 * H * 0.001, 0),
                new Point3d(0.5 * W * 0.001, 0.5 * H * 0.001, 0),
                new Point3d(0.5 * W * 0.001, (0.5 * H - Tf) * 0.001, 0),
                new Point3d(0.5 * Tw * 0.001, (0.5 * H - Tf) * 0.001, 0),
                new Point3d(0.5 * Tw * 0.001, (- (0.5 * H - Tf)) * 0.001, 0),
                new Point3d(0.5 * W * 0.001, (- (0.5 * H - Tf)) * 0.001, 0),
                new Point3d(0.5 * W * 0.001, -0.5 * H * 0.001, 0),
                new Point3d(-0.5 * W * 0.001, -0.5 * H * 0.001, 0),
                new Point3d(-0.5 * W * 0.001, (- (0.5 * H - Tf)) * 0.001, 0),
                new Point3d(-0.5 * Tw * 0.001, (- (0.5 * H - Tf))* 0.001, 0),
                new Point3d(-0.5 * Tw * 0.001, (0.5 * H - Tf) * 0.001, 0),
                new Point3d(-0.5 * W * 0.001, (0.5 * H - Tf) * 0.001, 0),
                new Point3d(-0.5 * W * 0.001, 0.5 * H * 0.001, 0)
            };

            Pts = pts.GetRange(0, pts.Count - 1);

            return new Polyline(pts).ToNurbsCurve();
        }

        public override string GetDims()
        {
            string txt = this.Type + ", ";

            txt += this.H.ToString() + ", ";
            txt += this.W.ToString() + ", ";
            txt += this.Tw.ToString() + ", ";
            txt += this.Tf.ToString();

            return txt;
        }

    }

    [Serializable]
    public class Section_RHS : Section
    {
        // --- field ---
        public double H { get; private set; }
        public double W { get; private set; }
        public double Tw { get; private set; }
        public double Tf { get; private set; }

        // --- constructors --- 

        public Section_RHS() { }
        public Section_RHS(Material _mat, string _tag, double _h, double _w, double _tw, double _tf, double _theta=0.0)
        {
            // class field
            H = _h;
            W = _w;
            Tw = _tw;
            Tf = _tf;

            // base class field
            Tag = _tag;
            Mat = _mat;
            Theta = _theta;
            Type = "RHS";

            Area = W * H - (W - 2.0*Tw) * (H - 2.0 * Tf);

            Iy = 1.0 / 12.0 * (W * Math.Pow(H, 3) - (W - 2.0 * Tw) * Math.Pow(H - 2.0 * Tf, 3));
            Iz = 1.0 / 12.0 * (H * Math.Pow(W, 3) - (H - 2.0 * Tf) * Math.Pow(W - 2.0 * Tw, 3));

            Wy = Iy / (0.5 * H);
            Wz = Iz / (0.5 * W);

            J = 2.0 * Math.Pow((W-Tw)*(H-Tf), 2) / ((W-Tw)/Tf+(H-Tf)/Tw);

            Curves.AddRange(DrawCurves());
        }

        // --- methods ---

        private List<Curve> DrawCurves()
        {
            List<Curve> crvs = new List<Curve>();

            List<Point3d> pts = new List<Point3d>
            {
                new Point3d(-0.5*W* 0.001,0.5*H* 0.001,0),
                new Point3d( 0.5*W* 0.001,0.5*H* 0.001,0),
                new Point3d( 0.5*W* 0.001,-0.5*H* 0.001,0),
                new Point3d(-0.5*W* 0.001,-0.5*H* 0.001,0),
                new Point3d(-0.5*W* 0.001,0.5*H* 0.001,0)
            };

            Pts = pts.GetRange(0, pts.Count - 1);

            crvs.Add(new Polyline(pts).ToNurbsCurve());

            pts = new List<Point3d>
            {
                new Point3d((-0.5*W+Tw) * 0.001, (0.5*H-Tf) * 0.001,0),
                new Point3d( (0.5*W-Tw) * 0.001, (0.5*H-Tf) * 0.001,0),
                new Point3d( (0.5*W-Tw) * 0.001,(-0.5*H+Tf) * 0.001,0),
                new Point3d((-0.5*W+Tw) * 0.001,(-0.5*H+Tf) * 0.001,0),
                new Point3d((-0.5*W+Tw) * 0.001, (0.5*H-Tf) * 0.001,0)
            };

            Pts.AddRange(pts.GetRange(0, pts.Count - 1));

            crvs.Add(new Polyline(pts).ToNurbsCurve());

            return crvs;
        }
        public override string GetDims()
        {
            string txt = this.Type + ", ";

            txt += this.H.ToString() + ", ";
            txt += this.W.ToString() + ", ";
            txt += this.Tw.ToString() + ", ";
            txt += this.Tf.ToString();

            return txt;
        }

    }

    [Serializable]
    public class Section_CHS : Section
    {
        // --- field ---
        public double D { get; private set; }
        public double T { get; private set; }

        // --- constructors --- 
        public Section_CHS() { }
        public Section_CHS(Material _mat, string _tag, double _d, double _t)
        {
            // class field
            D = _d;
            T = _t;

            // base class field
            Tag = _tag;
            Mat = _mat;
            Theta = 0.0;
            Type = "CHS";

            Area = 0.25 * Math.PI * (Math.Pow(D, 2) - Math.Pow(D - 2.0 * T, 2));

            Iy = Math.PI / 64.0 * (Math.Pow(D, 4) - Math.Pow(D - 2.0 * T, 4));
            Iz = Iy;

            Wy = Iy / (0.5 * D);
            Wz = Wy;

            J = 1.0 / 32.0 * Math.PI * (Math.Pow(D, 4) - Math.Pow(D - 2.0 * T, 4));

            Curves.AddRange(DrawCurves());
        }

        // --- methods ---

        private List<Curve> DrawCurves()
        {
            List<Curve> crvs = new List<Curve>
            {
                new Circle(0.5 * D * 0.001).ToNurbsCurve(),
                new Circle(0.5 * (D - 2 * T) * 0.001).ToNurbsCurve()
            };

            List<Point3d> pts = new List<Point3d>();

            crvs[0].DivideByCount(Common.DIV_CIRCLE, true, out Point3d[] pts1);

            pts.AddRange(pts1.ToList());

            crvs[1].DivideByCount(Common.DIV_CIRCLE, true, out Point3d[] pts2);

            pts.AddRange(pts2.ToList());

            Pts = pts;

            return crvs;
        }
        public override string GetDims()
        {
            string txt = this.Type + ", ";

            txt += this.D.ToString() + ", ";
            txt += this.T.ToString();

            return txt;
        }
    }

    public class GH_Section : GH_Goo<Section>
    {
        public GH_Section() { }
        public GH_Section(GH_Section other) : base(other.Value)
        {
            this.Value = other.Value.DeepCopy();
        }
        public GH_Section(Section sec) : base(sec)
        {
            this.Value = sec;
        }
        public override bool IsValid => base.m_value.IsValid();
        public override string TypeName => "Section";
        public override string TypeDescription => "Section";
        public override IGH_Goo Duplicate()
        {
            return new GH_Section(this);
        }
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class Param_Section : GH_PersistentParam<GH_Section>
    {
        public Param_Section() : base(
            new GH_InstanceDescription(
                "Section", "Sec", "Cross-Section", Common.category, Common.sub_param
                )
            )
        { }

        public override Guid ComponentGuid => new Guid("8f96d3a2-732a-48b9-88da-70b9b26bd342");

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.icons_P_Sec; } }  //Set icon image

        protected override GH_GetterResult Prompt_Plural(ref List<GH_Section> values)
        {
            return GH_GetterResult.success;
        }

        protected override GH_GetterResult Prompt_Singular(ref GH_Section value)
        {
            return GH_GetterResult.success;
        }


    }

}
