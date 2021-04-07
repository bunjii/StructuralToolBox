using Grasshopper.Kernel;

using Rhino.Commands;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StructuralToolBox
{
    public class ST_Draw_CrossSection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _98_Draw_CrossSection class.
        /// </summary>
        public ST_Draw_CrossSection()
          : base("Draw_CrossSection", "draw cs",
              "draw elements with cross-sections",
              Common.category, Common.sub_info)
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_Model(), "Model", "Model", "Model", GH_ParamAccess.item);
            pManager.AddTextParameter("Material tags", "m tags", "material tags", GH_ParamAccess.list);
            pManager.AddTextParameter("Section tags", "s tags", "section tags", GH_ParamAccess.list);
            pManager.AddTextParameter("Element tags", "e tags", "Element tags", GH_ParamAccess.list);

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
            pManager.RegisterParam(new Param_Element1D(), "Elem", "Elem", "Elem", GH_ParamAccess.list);
            pManager.AddMeshParameter("mesh", "mesh", "mesh", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // --- variables ---
            GH_Model gh_mdl = new GH_Model();
            Model mdl;
            List<string> mtags = new List<string>();
            List<string> stags = new List<string>();
            List<string> etags = new List<string>();

            List<Mesh> meshes = new List<Mesh>();

            // --- input --- 
            if(!DA.GetData(0, ref gh_mdl)) { return; }
            DA.GetDataList(1, mtags);
            DA.GetDataList(2, stags);
            DA.GetDataList(3, etags);

            // --- solve --- 

            mdl = gh_mdl.Value;

            if (gh_mdl.Value.SelectedLC != null)
            {
                mdl = Common.DeepCopy(mdl);

            }

            IEnumerable<Element_1D> elems = mdl.Elem1Ds;

            if (mtags.Count != 0)
            {
                elems = elems.Where(x => mtags.Contains(x.Sec.Mat.Tag));
            }

            if (stags.Count != 0)
            {
                elems = elems.Where(x => stags.Contains(x.Sec.Tag));
            }

            if (etags.Count != 0)
            {
                elems = elems.Where(x => etags.Contains(x.Tag));
            }

            foreach (Element_1D e in elems)
            {
                List<Point3d> pts = new List<Point3d>();
                int pt_cnt = e.Sec.Pts.Count;
                Transform tf = Transform.PlaneToPlane(Plane.WorldXY, e.Pln);
                IEnumerable<Point3d> pts0 = e.Sec.Pts.Select(x => new Point3d(x));

                foreach (Point3d p in pts0)
                {
                    p.Transform(tf);
                    pts.Add(p);
                }

                int num_frame = (int) (e.Line.Length / Common.DIV_DIST_ALONG_AXIS) + 1 ;
                if (num_frame == 1) num_frame++;

                double incr = e.Line.Length / (num_frame-1);

                for (int i = 1; i < num_frame; i++)
                {
                    Vector3d trans_vec = e.Line.UnitTangent * incr * i;
                    
                    for (int j = 0; j < pt_cnt; j++)
                    {
                        Point3d newPt = new Point3d(
                            pts[j].X + trans_vec.X,
                            pts[j].Y + trans_vec.Y,
                            pts[j].Z + trans_vec.Z
                            );

                        pts.Add(newPt);
                    }
                }

                Mesh mesh = new Mesh();
                mesh.Vertices.AddVertices(pts);
                
                if (e.Sec is Section_Rect)
                {
                    for (int i = 0; i < num_frame - 1; i++)
                    {
                        for (int j = 0; j < pt_cnt; j++)
                        {
                            int j_nxt = j + 1;
                            if (j == pt_cnt - 1) j_nxt = 0;
                            mesh.Faces.AddFace( pt_cnt * i + j, 
                                                pt_cnt * (i + 1) + j, 
                                                pt_cnt * (i + 1) + j_nxt, 
                                                pt_cnt * i + j_nxt);
                        }

                    }

                    mesh.Faces.AddFace(0, 1, 2, 3);
                    mesh.Faces.AddFace( mesh.Vertices.Count - 4, 
                                        mesh.Vertices.Count - 1, 
                                        mesh.Vertices.Count - 2, 
                                        mesh.Vertices.Count - 3);

                }

                else if (e.Sec is Section_Circular)
                {
                    for (int i = 0; i < num_frame - 1; i++)
                    {
                        for (int j = 0; j < pt_cnt; j++)
                        {
                            int j_nxt = j + 1;
                            if (j == pt_cnt - 1) j_nxt = 0;
                            mesh.Faces.AddFace( pt_cnt * i + j, 
                                                pt_cnt * i + j_nxt, 
                                                pt_cnt * (i + 1) + j_nxt, 
                                                pt_cnt * (i + 1) + j);
                        }
                    }

                    mesh.Vertices.Add(e.Line.From);
                    mesh.Vertices.Add(e.Line.To);

                    for (int j = 0; j < pt_cnt; j++)
                    {
                        int j_nxt = j + 1;
                        if (j == pt_cnt - 1) j_nxt = 0;
                        int i_last = num_frame - 1;

                        mesh.Faces.AddFace(j_nxt, j, mesh.Vertices.Count - 2);
                        mesh.Faces.AddFace( i_last*pt_cnt+j, 
                                            i_last * pt_cnt+j_nxt, 
                                            mesh.Vertices.Count - 1);
                    }

                }

                else if (e.Sec is Section_I)
                {
                    for (int i = 0; i < num_frame - 1; i++)
                    {
                        for (int j = 0; j < pt_cnt; j++)
                        {
                            int j_nxt = j + 1;
                            if (j == pt_cnt - 1) j_nxt = 0;
                            
                            mesh.Faces.AddFace( pt_cnt * i + j, 
                                                pt_cnt * (i + 1) + j, 
                                                pt_cnt * (i + 1) + j_nxt, 
                                                pt_cnt * i + j_nxt);
                        }
                    }


                    mesh.Faces.AddFace(0, 10, 11);
                    mesh.Faces.AddFace(0, 1, 3, 10);
                    mesh.Faces.AddFace(1, 2, 3);
                    mesh.Faces.AddFace(3, 4, 9, 10);
                    mesh.Faces.AddFace(4, 5, 6);
                    mesh.Faces.AddFace(4, 6, 7, 9);
                    mesh.Faces.AddFace(7, 8, 9);

                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 0,
                                        (num_frame - 1) * pt_cnt + 11,
                                        (num_frame - 1) * pt_cnt + 10);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 0,
                                        (num_frame - 1) * pt_cnt + 10,
                                        (num_frame - 1) * pt_cnt + 3,
                                        (num_frame - 1) * pt_cnt + 1);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 1,
                                        (num_frame - 1) * pt_cnt + 3,
                                        (num_frame - 1) * pt_cnt + 2);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 3,
                                        (num_frame - 1) * pt_cnt + 10,
                                        (num_frame - 1) * pt_cnt + 9,
                                        (num_frame - 1) * pt_cnt + 4);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 4,
                                        (num_frame - 1) * pt_cnt + 6,
                                        (num_frame - 1) * pt_cnt + 5);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 4,
                                        (num_frame - 1) * pt_cnt + 9,
                                        (num_frame - 1) * pt_cnt + 7,
                                        (num_frame - 1) * pt_cnt + 6);
                    mesh.Faces.AddFace( (num_frame - 1) * pt_cnt + 7,
                                        (num_frame - 1) * pt_cnt + 9,
                                        (num_frame - 1) * pt_cnt + 8);

                }

                else if (e.Sec is Section_RHS)
                {
                    for (int i = 0; i < num_frame - 1; i++)
                    {
                        for (int j = 0; j < pt_cnt; j++)
                        {
                            int j_nxt = j + 1;
                            if (j == (int) (0.5 * pt_cnt) - 1) j_nxt = 0;
                            if (j == pt_cnt - 1) j_nxt = (int) (0.5 * pt_cnt);

                            mesh.Faces.AddFace( pt_cnt * i + j,
                                                pt_cnt * (i + 1) + j,
                                                pt_cnt * (i + 1) + j_nxt,
                                                pt_cnt * i + j_nxt);
                        }
                    }

                    int id = (num_frame - 1) * pt_cnt;

                    mesh.Faces.AddFace(0, 1, 5, 4);
                    mesh.Faces.AddFace(1, 2, 6, 5);
                    mesh.Faces.AddFace(2, 3, 7, 6);
                    mesh.Faces.AddFace(0, 4, 7, 3);

                    mesh.Faces.AddFace(id + 0, id + 4, id + 5, id + 1);
                    mesh.Faces.AddFace(id + 1, id + 5, id + 6, id + 2);
                    mesh.Faces.AddFace(id + 2, id + 6, id + 7, id + 3);
                    mesh.Faces.AddFace(id + 0, id + 3, id + 7, id + 4);

                }

                else if (e.Sec is Section_CHS)
                {
                    for (int i = 0; i < num_frame - 1; i++)
                    {
                        for (int j = 0; j < pt_cnt; j++)
                        {
                            int j_nxt = j + 1;
                            if (j == (int)(0.5 * pt_cnt) - 1) j_nxt = 0;
                            if (j == pt_cnt - 1) j_nxt = (int)(0.5 * pt_cnt);

                            mesh.Faces.AddFace( pt_cnt * i + j,
                                                pt_cnt * i + j_nxt,
                                                pt_cnt * (i + 1) + j_nxt,
                                                pt_cnt * (i + 1) + j);
                        }
                    }

                    for (int j = 0; j < Common.DIV_CIRCLE; j++)
                    {
                        int j_nxt = j + 1;
                        int dc = Common.DIV_CIRCLE;
                        if (j == dc - 1) j_nxt = 0;

                        mesh.Faces.AddFace(j, j + dc, j_nxt + dc, j_nxt);
                    
                    }

                    for (int j = 0; j < Common.DIV_CIRCLE; j++)
                    {
                        int j_offst = pt_cnt * (num_frame - 1) + j;
                        int j_nxt = j_offst + 1;
                        int dc = Common.DIV_CIRCLE;
                        if (j == dc - 1) j_nxt = pt_cnt * (num_frame - 1);

                        mesh.Faces.AddFace(j_offst, j_nxt , j_nxt + dc, j_offst + dc);

                    }
                }

                if (Math.Abs(e.Line.ToX - e.Line.FromX) < Common.PRES &&
                    Math.Abs(e.Line.ToY - e.Line.FromY) < Common.PRES)
                {
                    mesh.Flip(true, true, true);
                }

                meshes.Add(mesh);

            }

            // --- output ---
            DA.SetDataList(0, new List<GH_Element_1D>(elems.Select(x => new GH_Element_1D(x))));
            DA.SetDataList(1, meshes);

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
                return StructuralToolBox.Properties.Resources.icons_I_DrawCS;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("52c1d7f3-f847-48ab-a929-c689079f2f2f"); }
        }
    }
}