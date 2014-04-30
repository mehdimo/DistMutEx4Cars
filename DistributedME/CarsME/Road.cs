using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ClientServer
{
    public class Road
    {
        System.Drawing.Pen myPen;
        System.Drawing.Graphics formGraphics { set; get; }
        public Form Form { set; get; }
        public bool ChangedDirection;

        public Point Start { set; get; }
        public Point End { set; get; }

        public bool IsBridge { set; get; }

        private void SwitchPoints()
        {
            Point t = new Point(Start.X, Start.Y);
            Start = End;
            End = t;
        }

        public void Create()
        {
            if (Start != null && End != null && Form != null)
            {
                myPen = new System.Drawing.Pen(System.Drawing.Color.Black, 30);
                formGraphics = Form.CreateGraphics();
                {
                    formGraphics.DrawLine(myPen, Start.X, Start.Y, End.X, End.Y);
                    myPen.Dispose();
                    formGraphics.Dispose();
                }
            }
        }

        public void ChangeDirection()
        {
            if (IsBridge)
                SwitchPoints();
        }
    }
}
