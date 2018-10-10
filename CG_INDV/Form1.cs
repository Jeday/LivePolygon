using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CG_INDV
{
    public partial class Form1 : Form
    {
        private List<PointF> shell = new List<PointF>();

        public Form1()
        {
            InitializeComponent();
        }

        private bool FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                intersection = new PointF(float.NaN, float.NaN);
                return false;
            }

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            return ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));
        }

        private bool point_in_polygon(PointF n)
        {
            for (int i = 0; i < shell.Count; ++i)
                if (shell[i] == n)
                    return true;


            int cnt_intersec = 0;
            PointF intersec;
            PointF finish = new PointF(-pictureBox1.Width, -pictureBox1.Height);
            for (var i = 0; i < shell.Count- 1; ++i)
            {
                if (FindIntersection(n, finish, shell[i], shell[i+1], out intersec) && intersec != shell[i])
                    ++cnt_intersec;
            }

            if (FindIntersection(n, finish, shell.First(), shell.Last(), out intersec) && intersec != shell.First())
                ++cnt_intersec;

            if (cnt_intersec % 2 != 0)     // принадлежит многоугольнику
                return true;
            else                           // не принадлежит
                return false;
        }


        private bool is_left(PointF p, PointF s) {
            foreach (PointF s1 in shell) {
                if (s == s1) continue;
                float pos = (s.X - p.X) * (s1.Y - p.Y) - (s.Y - p.Y) * (s1.X - p.X);
                if (pos < 0.0)            // слева
                    return false;
            }
            return true;
        }

        private bool is_right(PointF p, PointF s)
        {
            foreach (PointF s1 in shell)
            {
                if (s == s1) continue;
                float pos = (s.X - p.X) * (s1.Y - p.Y) - (s.Y - p.Y) * (s1.X - p.X);
                if (pos > 0.0)            // слева
                    return false;
            }
            return true;
        }

        private void add_to_shell(PointF p) {
            if (point_in_polygon(p))
                return;

            /// left line
            int l = -1;
            for (int i = 0; i < shell.Count; i++)
                if (is_left(p, shell[i])) {
                    l = i;
                    break;
                }


            int r = -1;
            for (int i = 0; i < shell.Count; i++)
                if (is_right(p, shell[i]))
                {
                   r = i;
                   break;
                }
            List<PointF> new_shell;
            if (l > r) { int t = l; l = r; r = t; } // l<r
            if ((r - l - 1) < (shell.Count - 1 - r + l)) {
                new_shell = shell.Take(l+1).ToList(); // 0..l
                new_shell.Add(p); // M
                new_shell.AddRange(shell.Skip(r)); // r..end
            }
            else {
                new_shell = new List<PointF>();
                new_shell.Add(p);
                new_shell.AddRange(shell.GetRange(l, r - l + 1));
            }
            shell = new_shell;
            //
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (shell.Count < 3)
                shell.Add(e.Location);
            else {
                add_to_shell(e.Location);
            }
            pictureBox1.Invalidate();
            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var p = new Pen(Color.Black);
            if (shell.Count == 1)
                e.Graphics.DrawEllipse(p, shell[0].X - 1, shell[0].Y - 1, 3, 3);
            else if (shell.Count >= 2) {
                e.Graphics.DrawLines(p, shell.ToArray());
                e.Graphics.DrawLine(p, shell.Last(), shell.First());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            shell.Clear();
            pictureBox1.Invalidate();
        }
    }
}
