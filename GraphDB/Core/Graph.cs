using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;


namespace GraphDB.Core
{
    public class Graph//图数据库类，存放节点列表和连边列表
    {
        List<Node> NodeList;
        List<Edge> EdgeList;
        //属性///////////////////////
        public int NodeNum
        {
            get
            {
                return NodeList.Count;
            }
        }
        
        public int EdgeNum
        {
            get
            {
                return EdgeList.Count;
            }
        }
        
        public List<Node> Nodes
        {
            get
            {
                return NodeList;
            }
        }
        
        public List<Edge> Edges
        {
            get
            {
                return EdgeList;
            }
        }
        //方法///////////////////////
        //构造函数
        public Graph()
        {
            NodeList = new List<Node>();
            EdgeList = new List<Edge>();
        }
        
        //加入节点
        public bool AddNode(Node newNode)
        {
            //节点加入节点列表
            NodeList.Add(newNode);
            return true;
        }
        
        //删除节点
        public bool RemoveNode(Node curNode)
        {
            //清除节点所有连边
            ClearUnusedEdge(curNode.ClearEdge());
            //从节点列表中移除节点
            NodeList.Remove(curNode);
            return true;
        }
        //加入连边
        public bool AddEdge(Node curNode, Node tarNode, Edge newEdge)
        {
            try
            {
                //连边的头指针指向起节点
                newEdge.Start = curNode;
                //连边的尾指针指向目标节点
                newEdge.End = tarNode;
            }
            catch (Exception e)
            {//如果连边和起始/目标节点类型不匹配则会报错
                MessageBox.Show(e.Message, "警告", MessageBoxButtons.OK);
                return false;
            }
            //将新连边加入起始节点的outbound
            if (curNode.AddEdge(newEdge) == false)
            {
                return false;
            }
            //将新连边加入目标节点的Inbound
            if (tarNode.RegisterInbound(newEdge) == false)
            {
                return false;
            }
            //全部完成后将连边加入网络连边列表
            EdgeList.Add(newEdge);
            return true;
        }
        
        //移除连边
        public bool RemoveEdge(Node curNode, Node tarNode)
        {
            Edge curEdge = null;
            //从起始节点的出边中遍历
            foreach (Edge edge in curNode.OutBound)
            {//查找终止节点编号和目标节点编号一致的连边
                if (edge.End.Number == tarNode.Number)
                {//找到则返回，本图数据库不支持两点间多连边
                    curEdge = edge;
                    break;
                }
            }
            if (curEdge == null)
            {//没找到直接返回
                return false;
            }
            //起始节点Outbound中移除连边
            curNode.RemoveEdge(curEdge);
            //从终止节点InBound中注销连边
            tarNode.UnRegisterInbound(curEdge);
            //全部完成后，从总连边列表中移除该边
            EdgeList.Remove(curEdge);
            return true;
        }
        
        //删除所有被解除绑定的连边
        public bool ClearUnusedEdge(List<Edge> UnusedList)
        {
            //将入参列表中所有连边从总连边列表中删除
            foreach (Edge edge in UnusedList)
            {
                EdgeList.Remove(edge);
            }
            //清空入参列表本身内容
            UnusedList.Clear();
            return true;
        }
        
        //将xml文件转化为网络
        public Graph(XmlDocument doc)
        {
            XmlNode xmlroot, xmlNodes, xmlEdges;
            Node newNode;

            //取出根节点
            xmlroot = doc.GetElementsByTagName("Graph").Item(0);
            if (xmlroot == null)
            {
                return;
            }
            xmlNodes = xmlEdges = null;
            foreach (XmlElement xNode in xmlroot.ChildNodes)
            {
                if (xNode.Name == "Nodes")
                {//获取Nodes节点
                    xmlNodes = xNode;
                }
                if (xNode.Name == "Edges")
                {//获取Edges节点
                    xmlEdges = xNode;
                }
            }
            if (xmlNodes == null)
            {
                return;
            }
            NodeList = new List<Node>();
            EdgeList = new List<Edge>();
            foreach (XmlElement xNode in xmlNodes.ChildNodes)                                      //遍历节点列表
            {
                //生成新节点
                newNode = new Node(xNode);
                //加入图
                this.AddNode(newNode);
            }
            //如果没有边也可以返回OK
            if (xmlEdges == null)
            {
                return;
            }
            Edge newEdge;
            string strStart, strEnd;
            Node nodeStart, nodeEnd;
            foreach (XmlElement xNode in xmlEdges.ChildNodes)                                      //遍历连边列表
            {
                //生成新连边
                newEdge = new Edge(xNode);
                //获取连边的起始和终止节点编号
                strStart = GetText(xNode, "Start");
                strEnd = GetText(xNode, "End");
                nodeStart = this.GetNodeAtIndex(Convert.ToInt32(strStart));
                nodeEnd = this.GetNodeAtIndex(Convert.ToInt32(strEnd));
                //加入图
                this.AddEdge(nodeStart, nodeEnd, newEdge);
            }
            return;
        }
        
        //工具函数，从xml节点中读取某个标签的InnerText
        string GetText(XmlElement curNode, string sLabel)
        {
            if (curNode == null)
            {
                return "";
            }
            foreach (XmlElement xNode in curNode.ChildNodes)
            {
                if (xNode.Name == sLabel)
                {
                    return xNode.InnerText;
                }
            }
            return "";
        }
        
        //将数据保存为XML文件
        public XmlDocument ToXML()
        {
            XmlDocument doc = new XmlDocument();
            //所有网络数据都保存为xml格式
            XmlElement root = doc.CreateElement("Graph");
            XmlElement Nodes, Edges;

            AdjustNodeIndex();
            Nodes = doc.CreateElement("Nodes");
            Nodes.SetAttribute("NodeNumber", this.NodeNum.ToString());
            foreach (Node curNode in NodeList)
            {
                Nodes.AppendChild(curNode.ToXML(ref doc));     //循环调用底层节点的输出函数
            }
            root.AppendChild(Nodes);
            Edges = doc.CreateElement("Edges");
            Edges.SetAttribute("EdgeNumber", this.EdgeNum.ToString());
            foreach (Edge curEdge in EdgeList)
            {
                Edges.AppendChild(curEdge.ToXML(ref doc)); //循环调用底层节点的输出函数
            }
            root.AppendChild(Edges);
            doc.AppendChild(root);
            return doc;
        }
        
        //调整节点实际索引(用于保存，和编号不完全相同)
        void AdjustNodeIndex()
        {
            int index = 0;
            foreach (Node curNode in NodeList)
            {
                curNode.SaveIndex = index;
                index++;
            }
        }
        
        //查询函数，返回指定索引处的节点
        public Node GetNodeAtIndex(int index)
        {
            return NodeList.ElementAt(index);
        }
        
        //查询函数，返回节点列表中指定类型的所有节点
        public List<Node> GetNodesOfType(string type)
        {
            List<Node> ResultList = new List<Node>();
            //遍历节点列表
            foreach (Node curNode in NodeList)
            {
                if (curNode.Type == type)
                {//将符合type要求的节点加入返回结果列表
                    ResultList.Add(curNode);
                }
            }
            return ResultList;
        }

    }
}
