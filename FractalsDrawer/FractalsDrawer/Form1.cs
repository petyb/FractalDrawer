using System.CodeDom;
using System.Collections.Specialized;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Design;
using System.Security.Cryptography.Xml;
using System.Security.Permissions;
using System.Windows.Forms;

namespace FractalsDrawer
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        Graphics graph;
        Complex c;
        Rectangle selRect;
        Point orig;
        double LeftX = -2.0, LeftY, RightX = 2.0, RightY;
        double tmpX1, tmpY1, tmpX2, tmpY2;
        int maxIteration = 200;
        int cnt = 0;
        public Form1()
        {
            InitializeComponent();
            // WindowState = FormWindowState.Maximized;
            bmp = new Bitmap(picture.Width, picture.Height);
            graph = Graphics.FromImage(bmp);
            c = new Complex();
            c.a = -0.8;
            c.b = 0.156;
            // {-1, 0.2} 70 {-0.74543, 0.11301} {-0.0085, 0.71} {0.285, 0.01} {-0.8, 0.156}
            LeftY = -2.0 * picture.Height / picture.Width;
            RightY = 2.0 * picture.Height / picture.Width;
            picture.Paint += picture_Paint;
            picture.MouseMove += picture_MouseMove;
            picture.MouseUp += picture_MouseUp;
            picture.MouseDown += picture_MouseDown;

            DrawFractal();
        }

        int Iterate(Complex z)
        {
            int res = 0;
            while (z.getMod() <= 2 && res < maxIteration)
            {
                z = z * z + c;
                res++;
            }
            return res;
        }

        Color GetColor(int color, Complex z)
        {
            double val = (double)color / maxIteration;
            return Color.FromArgb(255, (int)(255 * val), (int)(255 * (1 - val)), (int)(255 * (z.getMod() / 0.5 > 1 ? 1 : z.getMod() / 0.5)));
        }
        void DrawFractal()
        {
            double mxa = -1, mxb = -1;
            for (int i = 0; i < picture.Width; i++)
            {
                for (int j = 0; j < picture.Height; j++)
                {
                    Complex z = new Complex();
                    z.a = LeftX + i / (double)picture.Width * (RightX - LeftX);
                    z.b = LeftY + j / (double)picture.Height * (RightY - LeftY);
                    mxa = Math.Max(z.a, mxa);
                    mxb = Math.Max(z.b, mxb);
                    int color = Iterate(z);
                    Color mycolor = GetColor(color, z);
                    bmp.SetPixel(i, j, mycolor);
                }
            }
            picture.Image = bmp;
        }

        void picture_MouseUp(object sender, MouseEventArgs e)
        {
            cnt++;
            tmpX2 = e.X;
            if (cnt > 0)
            {
                double LeftX1 = LeftX;
                double RightX1 = RightX;
                double LeftY1 = LeftY;
                double RightY1 = RightY;
                LeftX = LeftX1 + (double)tmpX1 / picture.Width * (RightX1 - LeftX1);
                RightX = LeftX1 + (double)tmpX2 / picture.Width * (RightX1 - LeftX1);
                LeftY = LeftY1 + tmpY1 / picture.Height * (RightY1 - LeftY1);
                RightY = LeftY + (RightX - LeftX) * picture.Height / picture.Width;
                DrawFractal();
            }
        }

        void picture_MouseDown(object sender, MouseEventArgs e)
        {
            tmpX1 = e.X;
            tmpY1 = e.Y;
            picture.Paint -= picture_Paint;
            picture.Paint += Selection_Paint;
            orig = e.Location;
        }

        private void Selection_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Red, 1);
            // e.Graphics.DrawRectangle(pen, selRect);
        }

        void picture_MouseMove(object sender, MouseEventArgs e)
        {
            selRect = GetSelRectangle(orig, e.Location);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                (sender as PictureBox).Refresh();
        }

        void picture_Paint(object sender, PaintEventArgs e)
        {
            // e.Graphics.DrawRectangle(Pens.Black, selRect);
        }

        Rectangle GetSelRectangle(Point orig, Point location)
        {
            int deltaX = location.X - orig.X, deltaY = location.Y - orig.Y;
            Size s = new Size(Math.Abs(deltaX), Math.Abs(deltaY));
            Rectangle rect = new Rectangle();
            if (deltaX >= 0 & deltaY >= 0)
                rect = new Rectangle(orig, s);
            if (deltaX < 0 & deltaY > 0)
                rect = new Rectangle(location.X, orig.Y, s.Width, s.Height);
            if (deltaX < 0 & deltaY < 0)
                rect = new Rectangle(location, s);
            if (deltaX > 0 & deltaY < 0)
                rect = new Rectangle(orig.X, location.Y, s.Width, s.Height);
            return rect;
        }
    }

    public class Complex
    {
        public double a, b;

        public static Complex operator +(Complex counter1, Complex counter2)
        {
            return new Complex { a = counter1.a + counter2.a, b = counter1.b + counter2.b };
        }

        public static Complex operator +(Complex counter1, int Const)
        {
            return new Complex { a = counter1.a + Const, b = counter1.b };
        }

        public static Complex operator *(Complex counter1, Complex counter2)
        {
            return new Complex
            {
                a = counter1.a * counter2.a - counter1.b * counter2.b,
                b = counter1.b * counter2.a + counter1.a * counter2.b
            };
        }

        public double getMod()
        {
            return Math.Sqrt(a * a + b * b);
        }
    }
}