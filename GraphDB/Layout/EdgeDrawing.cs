using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GraphDB.Layout
{
    public class EdgeDrawing
    {
        //绘图元素
        Point StartPos;
        Point EndPos;
        string strType;
        
        public Point StartPosition
        {
            get
            {
                return StartPos;
            }
        }

        public Point EndPosition
        {
            get
            {
                return EndPos;
            }
        }

        public string Type
        {
            get
            {
                return strType;
            }
        }

        public EdgeDrawing(Point sPos, Point ePos, string sType)
        {
            StartPos = sPos;
            EndPos = ePos;
            strType = sType;
        }
    }
}
