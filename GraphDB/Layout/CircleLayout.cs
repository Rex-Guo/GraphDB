using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GraphDB.Layout
{
    class CircleLayout
    {
        const double GoldenRatio = 0.618;
        int Num1stLevel;//第一层节点个数
        int intRadius; //节点半径
        int intNodeDistance;//节点间距离
        int intLineLength;//连边最小距离
        int intHeight, intWidth;//画布高度和宽度
        int intMaskRadius;//内环半径

        List<Point> Points;

        public int Height
        {
            get
            {
                return intHeight;
            }
        }

        public int Width
        {
            get
            {
                return intWidth;
            }
        }

        public List<Point> NodePoints
        {
            get
            {
                return Points;
            }
        }

        public int Numberof1stLevel
        {
            get
            {
                return Num1stLevel;
            }
        }

        public CircleLayout(int iNum, int iRadius)
        {
            Num1stLevel = iNum;
            intRadius = iRadius;
            intNodeDistance = Convert.ToInt32(2 * intRadius / GoldenRatio);
            intLineLength = Convert.ToInt32(intNodeDistance / (2 * Math.Sin(Math.PI / Num1stLevel)));
            intHeight = intWidth = intLineLength * 6;
            intMaskRadius = Convert.ToInt32(intLineLength * 1.5);
            Points = new List<Point>();
        }

        public void LayoutInit(int iNum)
        {
            double angle;
            int x, y;
            //核心节点
            Points.Add(new Point(intWidth/2, intHeight/2));
            //第一级邻居节点，等距均匀分布
            for (int index = 0; index < Num1stLevel; index++)
            {
                angle = index * 2 * Math.PI / Num1stLevel;
                x = Convert.ToInt32(Math.Cos(angle) * intLineLength + intWidth / 2);
                y = Convert.ToInt32(Math.Sin(angle) * intLineLength + intHeight / 2);
                Points.Add(new Point(x, y));
            }
            //其他节点
            for (int index = Num1stLevel+1; index < iNum; index++)
            {
                x = GenerateRandomNumber(0, intWidth - 1, index);
                y = GenerateRandomNumber(0, intHeight - 1, index*10);
                Point newPos = new Point(x, y);
                PositionCorrected(ref newPos);
                Points.Add(newPos);
            }
        }

        private int GenerateRandomNumber(int DownLimit, int UpLimit, int iSeed)
        {
            Random magic1;
            Random magic2;

            magic1 = new Random(DateTime.Now.Millisecond * DateTime.Now.Second * iSeed);
            magic2 = new Random(magic1.Next(DownLimit, 1000 * UpLimit * iSeed) * DateTime.Now.Millisecond);

            return magic2.Next(DownLimit, UpLimit);
        }

        private void PositionCorrected(ref Point curPos)
        {
            double dubDistance;
            int deltaX, deltaY;

            dubDistance = Math.Pow(curPos.X - intWidth / 2, 2)  + Math.Pow(curPos.Y - intHeight / 2, 2);
            if (dubDistance > Math.Pow(intMaskRadius, 2))
            {
                return;
            }
            deltaX = Convert.ToInt32(Math.Ceiling((intMaskRadius / Math.Sqrt(dubDistance) - 1) * (curPos.X - intWidth / 2)));
            deltaY = Convert.ToInt32(Math.Ceiling((intMaskRadius / Math.Sqrt(dubDistance) -1) * (curPos.Y - intHeight / 2)));
            curPos.X += deltaX;
            curPos.Y += deltaY;
        }
    }
}
