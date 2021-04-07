using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSparse;
using CSparse.Storage;
using CSparse.Double;
using CSparse.Double.Factorization;


namespace StructuralToolBox
{
    public class SolveLS
    {
        // --- field ---
        public Model Mdl { get; private set; }
        public int N_DOF { get; private set; }

        // --- constructors --- 
        public SolveLS() { }
        public SolveLS(ref Model _mdl) 
        {
            Mdl = Common.DeepCopy(_mdl);
            N_DOF = 6;

            Solve();
        }


        // --- methods ---

        private void Solve()
        {
            // global stiffness matrix
            DenseMatrix kG = CreateGlobalStiffMX();
            Mdl.KG = kG.Clone();

            // CoordinateStorage<double> ccs_kG = CreateGlobalStiffMX();

            // load vector
            DenseMatrix lV = CreateLoadMX();
            Mdl.LM = lV.Clone();


            // overwrite boundary condition & load
            foreach (Node n in Mdl.Nodes.Where(n => n.Sup != null))
            {
                for (int i = 0; i < N_DOF; i++)
                {
                    if (n.Sup.Conditions[i] == false) continue;

                    int ind = N_DOF * n.Id.Value + i;

                    // load vector (matrix)
                    for (int j = 0; j < lV.ColumnCount; j++)
                    {
                        lV[ind, j] = 0;
                    }

                    // global stiffness matrix
                    for (int j = 0; j < N_DOF * Mdl.Nodes.Count; j++)
                    {

                        //int id  = GedIndex(ccs_kG, ind, j);
                        //int id2 = GedIndex(ccs_kG, j, ind);

                        if (ind == j)
                        {
                            kG[ind, j] = 1;
                            // ccs_kG.Values[id] = 1;

                        }
                        else
                        {
                            kG[ind, j] = 0;
                            kG[j, ind] = 0;
                            //ccs_kG.Values[id] = 0;
                            //ccs_kG.Values[id2] = 0;
                        }
                    }
                }
            }

            // Solve system
            CompressedColumnStorage<double> kGs = SparseMatrix.OfMatrix(kG);
            // CompressedColumnStorage<double> kGs = SparseMatrix.OfIndexed(ccs_kG);
            CompressedColumnStorage<double> lVs = SparseMatrix.OfMatrix(lV);

            var order = ColumnOrdering.MinimumDegreeAtPlusA;

            for (int i = 0; i < lVs.ColumnCount; i++)
            {
                double[] b = lVs.Column(i);
                SparseLU lu = SparseLU.Create(kGs, order, Common.PRES);
                double[] disp = Vector.Create(N_DOF * Mdl.Nodes.Count, 0.0);

                lu.Solve(b, disp);

                Mdl.Disps.Add(disp);

                for (int j = 0; j < Mdl.Nodes.Count; j++)
                {
                    double[] vals = new double[6];
                    Array.Copy(disp, N_DOF * j, vals, 0, 6);
                    Mdl.Nodes[j].Disps.Add(vals);
                }
            }

            // reaction forces
            for (int i = 0; i < lVs.ColumnCount; i++)
            {
                var Disp = new DenseMatrix(Mdl.Nodes.Count * N_DOF, 1, Mdl.Disps[i]);
                var FM = Mdl.KG.Multiply(Disp);
                foreach (Support s in Mdl.Sups)
                {
                    int sid = s.Node.Id.Value * N_DOF;
                    double[] fs = new double[N_DOF];
                    Array.Copy(FM.Column(0), sid, fs, 0, N_DOF);
                    s.React.Add(fs);
                }
            }

            return;
        }

        //private static int GedIndex(CoordinateStorage<double> cs, int row, int col)
        //{
        //    int ret_id = new int();
        //    IEnumerable<int> colids = cs.ColumnIndices
        //                            .Select((v, i) => new { Val = v, Index = i })
        //                            .Where(ano => ano.Val == col)
        //                            .Select(ano => ano.Index);

        //    foreach (int id in colids)
        //    {
        //        if (id == row)
        //        {
        //            ret_id = id;
        //        }
        //    }

        //    return ret_id;
        //}

        public DenseMatrix CreateGlobalStiffMX()
        {
            int lenMX = Mdl.Nodes.Count * N_DOF;
            DenseMatrix kG = new DenseMatrix(lenMX, lenMX);

            foreach (Element_1D e in Mdl.Elem1Ds)
            {
                int snid = N_DOF * e.Nodes[0].Id.Value;
                int enid = N_DOF * e.Nodes[1].Id.Value;

                for (int i = 0; i < N_DOF; i++)
                {
                    for (int j = 0; j < N_DOF; j++)
                    {
                        kG[snid + i, snid + j] += e.EKG[i, j];
                        kG[snid + i, enid + j] += e.EKG[i, N_DOF + j];
                        kG[enid + i, snid + j] += e.EKG[N_DOF + i, j];
                        kG[enid + i, enid + j] += e.EKG[N_DOF + i, N_DOF + j];
                    }
                }
            }

            return kG;
        }

        //private CoordinateStorage<double> CreateGlobalStiffMX()
        //{
        //    int dim = Mdl.Nodes.Count*N_DOF;
        //    CoordinateStorage<double> ccs_kG =
        //        new CoordinateStorage<double>(dim, dim, dim * dim);

        //    foreach (Element_1D e in Mdl.Elem1Ds)
        //    {
        //        int snid = N_DOF * e.Nodes[0].Id.Value;
        //        int enid = N_DOF * e.Nodes[1].Id.Value;

        //        for (int i = 0; i < N_DOF; i++)
        //        {
        //            for (int j = 0; j < N_DOF; j++)
        //            {
        //                ccs_kG.At(snid + i, snid + j, e.EKG[i, j]);
        //                ccs_kG.At(snid + i, enid + j, e.EKG[i, N_DOF + j]);
        //                ccs_kG.At(enid + i, snid + j, e.EKG[N_DOF + i, j]);
        //                ccs_kG.At(enid + i, enid + j, e.EKG[N_DOF + i, N_DOF + j]);
        //            }
        //        }
        //    }

        //    return ccs_kG;
        //}

        private DenseMatrix CreateLoadMX()
        {
            int[] LCs = Mdl.Loads.Select(x => x.Lc.Value).Distinct().ToArray();
            DenseMatrix loadMX = new DenseMatrix(N_DOF * Mdl.Nodes.Count, LCs.Length);

            foreach (Load l in Mdl.Loads)
            {
                int lc = Array.IndexOf(LCs, l.Lc);
                if (!(l is Load_Point pl)) continue;

                int nid = pl.Node.Id.Value;
                List<double> lds = pl.Loads;

                for (int i = 0; i < N_DOF; i++)
                {
                    double val = lds[i] * Math.Pow(10, 3);
                    // [kN]-->[N], [kNm]-->[Nm]

                    loadMX[N_DOF * pl.Node.Id.Value + i, lc] += val;
                }
            }

            return loadMX;
        }


    }
}
