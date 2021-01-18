using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphVisualization
{
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private List<Point> points = new List<Point>();
        private Graphics graphics;

        private const int Radius = 6;
        private const int EdgeThickness = 2;
        private const int PointLabelFontSize = 12;
        private const int EdgeLabelFontSize = 10;
        private const string FontName = "Arial";

        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(bitmap);
            dataGridViewPoints.Columns.Add("pointNumber", "№");
            dataGridViewPoints.Columns.Add("pointСoordinates", "Point");
            dataGridViewPoints.Columns[0].Width = 30;
            dataGridViewPoints.Columns[1].Width = 70;
            dataGridViewMatrix.RowHeadersWidth = 50;
        }

        private int EdgeLength(Point point1, Point point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        private void DrawPoint(Point point, string text)
        {
            graphics.FillEllipse(new SolidBrush(Color.Red),
                point.X - Radius, point.Y - Radius, Radius * 2, Radius * 2);
            graphics.DrawString(text, new Font(FontName, PointLabelFontSize),
                new SolidBrush(Color.Black), point.X + Radius, point.Y + Radius);
            pictureBox.Image = bitmap;
        }

        private void GenerateMatrix()
        {
            ClearMatrix();

            for (int i = 0; i < points.Count; i++)
            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.Name = $"{i}";
                column.Width = 25;
                dataGridViewMatrix.Columns.Add(column);
            }

            for (int i = 0; i < points.Count; i++)
            {
                dataGridViewMatrix.Rows.Add();
                dataGridViewMatrix.Rows[i].HeaderCell.Value = $"{i}";
            }

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    dataGridViewMatrix.Rows[i].Cells[j].Value = (i < j) ? 1 : 0;
                }
            }

            foreach (DataGridViewColumn column in dataGridViewMatrix.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void DrawPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                DrawPoint(points[i], $"{i}");
            }
        }

        private void DrawEdges()
        {
            Point p1 = new Point();
            Point p2 = new Point();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (i != j && Convert.ToInt32(dataGridViewMatrix.Rows[i].Cells[j].Value) == 1)
                    {
                        p1.X = points[i].X;
                        p1.Y = points[i].Y;
                        p2.X = points[j].X;
                        p2.Y = points[j].Y;
                        var label = EdgeLength(p1, p2).ToString();
                        var font = new Font(FontName, EdgeLabelFontSize);
                        var size = graphics.MeasureString(label, font, pictureBox.Size);
                        graphics.DrawLine(new Pen(Color.Chartreuse, EdgeThickness), p1, p2);
                        graphics.DrawString(label, 
                            font, new SolidBrush(Color.Black), 
                            (p1.X + p2.X) / 2 - size.Width / 2, (p1.Y + p2.Y) / 2 - size.Height / 2);
                    }
                }
            }
        }

        private void ClearGraph()
        {
            graphics.Clear(Color.White);
            pictureBox.Image = bitmap;
            points.Clear();
            dataGridViewPoints.Rows.Clear();
        }

        private void ClearMatrix()
        {
            dataGridViewMatrix.Rows.Clear();
            dataGridViewMatrix.Columns.Clear();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            int x = MousePosition.X - Location.X - pictureBox.Location.X - 8;
            int y = MousePosition.Y - Location.Y - pictureBox.Location.Y - 32;
            points.Add(new Point(x, y));
            dataGridViewPoints.Rows.Add(dataGridViewPoints.Rows.Count, $"({x}; {y})");
            DrawPoint(points[points.Count - 1], $"{points.Count - 1}");
        }

        private void newGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewMatrix.Rows.Count < dataGridViewPoints.Rows.Count)
                GenerateMatrix();
            DrawEdges();
            Invalidate();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearGraph();
            ClearMatrix();
        }

        private void generateMatrixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateMatrix();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            graphics.Clear(Color.White);
            pictureBox.Image = bitmap;
            DrawPoints();
            DrawEdges();
            base.OnPaint(e);
        }
    }
}
