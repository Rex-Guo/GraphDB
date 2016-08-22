using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GraphDB.Layout
{
    public class NodeDrawing
    {
        //绘图元素
        Point potPos;
        string strName;

        public Point Position
        {
            get
            {
                return potPos;
            }
        }

        public string Name
        {
            get
            {
                return strName;
            }
        }

        public NodeDrawing(string sName, Point pPos)
        {
            potPos = pPos;
            strName = sName;
        }
    }
}
