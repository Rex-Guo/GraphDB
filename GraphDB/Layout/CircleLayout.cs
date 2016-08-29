using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using GraphDB.Core;

namespace GraphDB.Layout
{
    class CircleLayout
    {
        //常量
        const double GoldenRatio = 0.618;
        const int MaxRound = 100;
        const int CanvasRatio = 8;
        const double SmallRatio = 2.2;
        const double MaskRatio = 2.5;
        const double OuterMaskRatio = 3.0;
        //变量
        int Num1stLevel;//第一层节点个数
        int intRadius; //节点半径
        int intNodeDistance;//节点间距离
        int intSmallDistance;
        int intLineLength;//连边最小距离
        int intHeight, intWidth;//画布高度和宽度
        int intMaskRadius;//内环半径
        int intOuterMaskRadius;//外环半径
        List<Point> Points;//节点位置列表
        //属性
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
        //构造函数
        public CircleLayout(int iNum, int iRadius)
        {
            Num1stLevel = iNum;
            intRadius = iRadius;
            intNodeDistance = Convert.ToInt32(4 * intRadius / GoldenRatio);
            intSmallDistance = Convert.ToInt32(intRadius * SmallRatio);
            if (Num1stLevel <= 4)//连边长度下限
            {
                intLineLength = Convert.ToInt32(intNodeDistance / (2 * Math.Sin(Math.PI / 4)));
            }
            else if (Num1stLevel <= 8)//连边长度上限
            {
                intLineLength = Convert.ToInt32(intNodeDistance / (2 * Math.Sin(Math.PI / Num1stLevel)));
            }
            else
            {
                intLineLength = Convert.ToInt32(intNodeDistance / (2 * Math.Sin(Math.PI / 8)));
            }
            intHeight = intWidth = intLineLength * CanvasRatio;
            intMaskRadius = Convert.ToInt32(intLineLength * MaskRatio);
            intOuterMaskRadius = Convert.ToInt32(intLineLength * OuterMaskRatio);
            Points = new List<Point>();
        }
        //布局初始化
        public void LayoutInit(Graph subgraph)
        {
            double angle;
            int x, y;
            Point center, newPos;
            bool bolCover;
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
            //其他节点，先布置在关联点的中心，随后进行位置修正
            for (int index = Num1stLevel + 1; index < subgraph.NodeNum; index++)
            {
                do
                {
                    bolCover = false;
                    center = GetCenterPoint(index, subgraph);
                    x = GenerateRandomNumber(-intRadius, intRadius, index);
                    y = GenerateRandomNumber(-intRadius, intRadius, index * 10);
                    newPos = new Point(center.X + x, center.Y + y);
                    PositionCorrected(ref newPos);
                    //查找是否有重合节点，发现则重新生成随机数
                    foreach (Point oldPot in Points)
                    {
                        if (oldPot.X == newPos.X && oldPot.Y == newPos.Y)
                        {
                            bolCover = true;
                        }
                    }
                } while (bolCover == true);
                Points.Add(newPos);
            }
        }

        //获取索引节点所有关联节点的中心坐标
        private Point GetCenterPoint(int index, Graph subgraph)
        {
            int intSumX =0, intSumY = 0;
            int intTarIndex;
            Node curNode = subgraph.Nodes[index];

            foreach (Edge edge in curNode.OutBound)
            {
                intTarIndex = subgraph.GetIndexByNameAndType(edge.End.Name,edge.End.Type);
                intSumX += Points[intTarIndex].X;
                intSumY += Points[intTarIndex].Y;
                if (curNode.OutDegree == 1)
                {
                    intSumX += Points[0].X;
                    intSumY += Points[0].Y;
                    return new Point(intSumX / 2, intSumY / 2);
                }
            }
            return new Point(intSumX/curNode.OutDegree, intSumY/curNode.OutDegree);
        }
        
        //随机数生成器
        private int GenerateRandomNumber(int DownLimit, int UpLimit, int iSeed)
        {
            Random magic1;
            Random magic2;

            magic1 = new Random(DateTime.Now.Millisecond * DateTime.Now.Second * iSeed);
            magic2 = new Random(magic1.Next(DownLimit, 1000 * UpLimit * iSeed) * DateTime.Now.Millisecond);

            return magic2.Next(DownLimit, UpLimit);
        }
        //位置修正
        private void PositionCorrected(ref Point curPos)
        {
            double dubDistance;
            int deltaX, deltaY;

            dubDistance = Math.Pow(curPos.X - intWidth / 2, 2)  + Math.Pow(curPos.Y - intHeight / 2, 2);
            if (dubDistance < Math.Pow(intMaskRadius, 2))
            {//距离中心坐标小于intMaskRadius范围的节点向外推
                deltaX = Convert.ToInt32(Math.Ceiling((intMaskRadius / Math.Sqrt(dubDistance) - 1) * (curPos.X - intWidth / 2 +0.0000001)));
                deltaY = Convert.ToInt32(Math.Ceiling((intMaskRadius / Math.Sqrt(dubDistance) - 1) * (curPos.Y - intHeight / 2 + 0.0000001)));
                curPos.X += deltaX;
                curPos.Y += deltaY;
            }
            else if (dubDistance > Math.Pow(intOuterMaskRadius, 2))
            {//距离中心坐标大于intOuterMaskRadius范围的节点向里拉
                deltaX = Convert.ToInt32(Math.Ceiling((intOuterMaskRadius / Math.Sqrt(dubDistance) - 1) * (curPos.X - intWidth / 2 + 0.0000001)));
                deltaY = Convert.ToInt32(Math.Ceiling((intOuterMaskRadius / Math.Sqrt(dubDistance) - 1) * (curPos.Y - intHeight / 2 + 0.0000001)));
                curPos.X += deltaX;
                curPos.Y += deltaY;
            }
            return;
        }
        //浮动算法函数
        public void Float(Graph SubGraph)
        {
            Point curPosition;
            int intCloseCount, intRound = 0;
            PointF force;
            List<PointF> forces = new List<PointF>();
            
            do{
                intCloseCount = 0;
                //只循环随机分布的几个节点
                for (int index = Num1stLevel + 1; index < SubGraph.NodeNum; index++)
                {
                    force = new PointF(0, 0);
                    Node node = SubGraph.Nodes[index];
                    //循环所有同一层节点，计算斥力
                    for (int intNei = Num1stLevel + 1; intNei < SubGraph.NodeNum; intNei++)
                    {
                        if (intNei == index)
                        {
                            continue;
                        }
                        if (IsClose(Points[index], Points[intNei], intSmallDistance) == false)
                        {//如果不靠近则不管
                            continue;
                        }
                        intCloseCount++;
                        CalRejectForce(Points[index], Points[intNei], ref force);
                    }
                    //加入合力列表
                    forces.Add(force);
                }
                for (int index = Num1stLevel + 1; index < SubGraph.NodeNum; index++)
                {
                    curPosition = new Point(Points[index].X, Points[index].Y);
                    //得出合力，退火处理输出移动分量，进行移动
                    MoveNode(forces[index - (Num1stLevel + 1)], ref curPosition);
                    //进行mask修正
                    PositionCorrected(ref curPosition);
                    Points[index] = curPosition;
                }
                intRound++;
                if (intRound > 20)
                {//如果超过回合数则退出，防止死循环
                    break;
                }
            }while (intCloseCount > 0);
        }

        //检查两点间距离是否小于设定的距离
        private bool IsClose(Point pot1, Point pot2, double dSmallDistance)
        {
            double dubDistance;
            int deltaX, deltaY;

            deltaX = pot2.X - pot1.X;
            deltaY = pot2.Y - pot1.Y;
            dubDistance = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
            if (dubDistance < dSmallDistance * dSmallDistance)
            {
                return true;
            }
            return false;
        }

        //计算节点受到的排斥力
        private void CalRejectForce(Point curPos, Point tarPos, ref PointF force)
        {
            int deltaX, deltaY;
            double Frx, Fry, dubDistance;

            deltaX = tarPos.X - curPos.X;
            deltaY = tarPos.Y - curPos.Y;
            if ((deltaX * deltaX + deltaY * deltaY) > (intSmallDistance * intSmallDistance))
            {
                return;
            }
            dubDistance = Math.Sqrt((deltaX * deltaX + deltaY * deltaY));
            Frx = -((intSmallDistance - dubDistance) * deltaX) / (2 * (dubDistance+0.1));
            Fry = -((intSmallDistance - dubDistance) * deltaY) / (2 * (dubDistance+0.1));
            force.X += (float)(Frx );
            force.Y += (float)(Fry );
        }

        //移动当前节点，但不能超过距离限制
        private void MoveNode(PointF force, ref Point curPos)
        {
            
            if ((force.X * force.X + force.Y * force.Y) > intSmallDistance * intSmallDistance)
            {
                curPos.X += (int)((force.X * intSmallDistance)/Math.Sqrt((force.X * force.X + force.Y * force.Y)));
                curPos.Y += (int)((force.Y * intSmallDistance) / Math.Sqrt((force.X * force.X + force.Y * force.Y)));
                return;
            }
            curPos.X+= (int)(force.X);
            curPos.Y += (int)(force.Y);
        }
    }
}
