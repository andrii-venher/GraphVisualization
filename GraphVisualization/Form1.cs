using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GraphVisualization
{
    struct Edge
    {
        public MyPoint Point1;
        public MyPoint Point2;
        public int Weight;

        public Edge(MyPoint point1, MyPoint point2, int weight)
        {
            Point1 = point1;
            Point2 = point2;
            Weight = weight;
        }
    }

    struct MyPoint
    {
        public int Id;
        public int X;
        public int Y;

        public MyPoint(int id, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
        }
    }

    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private List<MyPoint> points = new List<MyPoint>();
        private List<Edge> edges = new List<Edge>();
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

        private int EdgeLength(MyPoint point1, MyPoint point2)
        {
            return (int) Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));
        }

        private void DrawPoint(MyPoint point, string text)
        {
            graphics.FillEllipse(new SolidBrush(Color.Red),
                point.X - Radius, point.Y - Radius, Radius * 2, Radius * 2);
            graphics.DrawString(text, new Font(FontName, PointLabelFontSize),
                new SolidBrush(Color.Black), point.X + Radius, point.Y + Radius);
            pictureBox.Image = bitmap;
        }

        private void FillMatrix(int number)
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    dataGridViewMatrix.Rows[i].Cells[j].Value = (i < j) ? number : 0;
                }
            }
        }

        private void GenerateMatrix()
        {
            ClearMatrix();

            foreach (var point in points)
            {
                DataGridViewColumn column = new DataGridViewTextBoxColumn();
                column.Name = $"{point.Id}";
                column.Width = 25;
                dataGridViewMatrix.Columns.Add(column);
            }

            foreach (var point in points)
            {
                dataGridViewMatrix.Rows.Add();
                dataGridViewMatrix.Rows[dataGridViewMatrix.Rows.Count - 1].HeaderCell.Value = $"{point.Id}";
            }

            FillMatrix(1);

            foreach (DataGridViewColumn column in dataGridViewMatrix.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        private void GenerateEdges()
        {
            edges.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (i != j && Convert.ToInt32(dataGridViewMatrix.Rows[i].Cells[j].Value) == 1)
                    {
                        MyPoint p1 = new MyPoint(points[i].Id, points[i].X, points[i].Y);
                        MyPoint p2 = new MyPoint(points[j].Id, points[j].X, points[j].Y);
                        edges.Add(new Edge(p1, p2, EdgeLength(p1, p2)));
                    }
                }
            }
        }

        private void ToMST()
        {
            if(edges.Count == 0)
                return;
            List<Edge> notUsedEdges = new List<Edge>(edges);
            List<MyPoint> usedPoints = new List<MyPoint>();
            List<MyPoint> notUsedPoints = new List<MyPoint>();
            foreach (var edge in edges)
            {
                if (!notUsedPoints.Contains(edge.Point1))
                    notUsedPoints.Add(edge.Point1);
                if (!notUsedPoints.Contains(edge.Point2))
                    notUsedPoints.Add(edge.Point2);
            }
            edges.Clear();
            usedPoints.Add(notUsedPoints[0]);
            notUsedPoints.RemoveAt(0);
            while (notUsedPoints.Count > 0)
            {
                int minimumEdge = -1;
                for (int i = 0; i < notUsedEdges.Count; i++)
                {
                    if ((usedPoints.IndexOf(notUsedEdges[i].Point1) != -1) && (notUsedPoints.IndexOf(notUsedEdges[i].Point2) != -1) ||
                        (usedPoints.IndexOf(notUsedEdges[i].Point2) != -1) && (notUsedPoints.IndexOf(notUsedEdges[i].Point1) != -1))
                    {
                        if (minimumEdge != -1)
                        {
                            if (notUsedEdges[i].Weight < notUsedEdges[minimumEdge].Weight)
                                minimumEdge = i;
                        }
                        else
                            minimumEdge = i;
                    }
                }
                if (usedPoints.IndexOf(notUsedEdges[minimumEdge].Point1) != -1)
                {
                    usedPoints.Add(notUsedEdges[minimumEdge].Point2);
                    notUsedPoints.Remove(notUsedEdges[minimumEdge].Point2);
                }
                else
                {
                    usedPoints.Add(notUsedEdges[minimumEdge].Point1);
                    notUsedPoints.Remove(notUsedEdges[minimumEdge].Point1);
                }
                edges.Add(notUsedEdges[minimumEdge]);
                notUsedEdges.RemoveAt(minimumEdge);
            }

            FillMatrix(0);

            foreach (var edge in edges)
            {
                dataGridViewMatrix.Rows[edge.Point1.Id].Cells[edge.Point2.Id].Value = 1;
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
            foreach (var edge in edges)
            {
                MyPoint p1 = edge.Point1;
                MyPoint p2 = edge.Point2;
                var label = EdgeLength(p1, p2).ToString();
                var font = new Font(FontName, EdgeLabelFontSize);
                var size = graphics.MeasureString(label, font, pictureBox.Size);
                graphics.DrawLine(new Pen(Color.Chartreuse, EdgeThickness), 
                    new Point(p1.X, p1.Y), new Point(p2.X, p2.Y));
                graphics.DrawString(label,
                    font, new SolidBrush(Color.Black),
                    (p1.X + p2.X) / 2 - size.Width / 2, (p1.Y + p2.Y) / 2 - size.Height / 2);
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
            points.Add(new MyPoint(points.Count, x, y));
            dataGridViewPoints.Rows.Add(points[points.Count - 1].Id, $"({x}; {y})");
            DrawPoint(points[points.Count - 1], $"{points.Count - 1}");
        }

        private void newGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewMatrix.Rows.Count < dataGridViewPoints.Rows.Count)
                GenerateMatrix();
            GenerateEdges();
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

        private void toSpanningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewMatrix.Rows.Count < dataGridViewPoints.Rows.Count)
                GenerateMatrix();
            GenerateEdges();
            ToMST();
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
