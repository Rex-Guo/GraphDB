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
        int intMaxNodeNum;
        int intMaxEdgeNum;
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
        //构造函数（接口）
        public Graph()
        {
            NodeList = new List<Node>();
            EdgeList = new List<Edge>();
            intMaxNodeNum = 0;
            intMaxEdgeNum = 0;
        }
        //将xml文件转化为网络（接口）
        public Graph(XmlDocument doc, ref ErrorCode err)
        {
            XmlNode xmlroot, xmlNodes, xmlEdges;
            Node newNode;

            //取出根节点
            xmlroot = doc.GetElementsByTagName("Graph").Item(0);
            if (xmlroot == null)
            {
                err = ErrorCode.NoXmlRoot;
                return;
            }
            xmlNodes = xmlEdges = null;
            NodeList = new List<Node>();
            EdgeList = new List<Edge>();
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
                err = ErrorCode.NoError;
                return;
            }
            foreach (XmlElement xNode in xmlNodes.ChildNodes)                                      //遍历节点列表
            {
                //生成新节点
                newNode = new Node(intMaxNodeNum, xNode);
                intMaxNodeNum++;
                //加入图
                this.AddNode(newNode);
            }
            //如果没有边也可以返回OK
            if (xmlEdges == null)
            {
                err = ErrorCode.NoError;
                return;
            }
            Edge newEdge;
            string strStart, strEnd;
            Node nodeStart, nodeEnd;
            foreach (XmlElement xNode in xmlEdges.ChildNodes)                                      //遍历连边列表
            {
                //生成新连边
                newEdge = new Edge(intMaxEdgeNum, xNode);
                //获取连边的起始和终止节点编号
                strStart = GetText(xNode, "Start");
                strEnd = GetText(xNode, "End");
                nodeStart = nodeEnd = null;
                nodeStart = this.GetNodeByIndex(Convert.ToInt32(strStart));
                nodeEnd = this.GetNodeByIndex(Convert.ToInt32(strEnd));
                if (nodeStart == null || nodeEnd == null)
                {
                    err = ErrorCode.InvaildIndex;
                    continue;
                }
                intMaxEdgeNum++;
                //加入图
                if (this.AddEdge(nodeStart, nodeEnd, newEdge) == false)
                {
                    err = ErrorCode.AddEdgeFailed;
                    continue;
                }
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

        //将数据保存为XML文件（接口）
        public XmlDocument ToXML(ref ErrorCode err)
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
        
        //加入节点
        void AddNode(Node newNode)
        {
            //节点加入节点列表
            NodeList.Add(newNode);
        }

        //删除节点
        void RemoveNode(Node curNode)
        {
            //清除节点所有连边
            ClearUnusedEdge(curNode.ClearEdge());
            //从节点列表中移除节点
            NodeList.Remove(curNode);
        }
        
        //加入连边
        bool AddEdge(Node curNode, Node tarNode, Edge newEdge)
        {
            //连边的头指针指向起节点
            newEdge.Start = curNode;
            //连边的尾指针指向目标节点
            newEdge.End = tarNode;
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
        bool RemoveEdge(Node curNode, Node tarNode)
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
        bool ClearUnusedEdge(List<Edge> UnusedList)
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

        //查询函数，返回指定索引处的节点
        public Node GetNodeByIndex(int index)
        {
            if (index >= this.NodeNum || index < 0)
            {
                return null;
            }
            return NodeList.ElementAt(index);
        }

        //查询函数，返回节点列表中指定名称和类型的节点
        public Node GetNodesByNameAndType(string sName, string type)
        {
            //遍历节点列表
            foreach (Node curNode in NodeList)
            {
                if (curNode.Name == sName && curNode.Type == type)
                {//将符合Name和type要求的节点返回
                    return curNode;
                }
            }
            return null;
        }

        //查询函数，返回指定名称和类型的节点的索引
        public int GetIndexByNameAndType(string sName, string sType)
        {
            int index = 0;
            //遍历节点列表
            foreach (Node curNode in NodeList)
            {
                if (curNode.Name == sName && curNode.Type == sType)
                {//将符合Name和type要求的节点返回
                    return index;
                }
                index++;
            }
            return -1;
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

        //查询函数，返回指定名称和类型的节点间的连边
        public Edge GetEdgeByNameAndType(string sName, string sType, string eName, string eType)
        {
            Node startNode = null, endNode = null;

            startNode = GetNodesByNameAndType(sName, sType);
            if (startNode == null)
            {
                return null;
            }
            endNode = GetNodesByNameAndType(eName, eType);
            if (endNode == null)
            {
                return null;
            }
            return GetEdgeByType(startNode, endNode);
        }

        //查找两点之间指定Type的连边
        Edge GetEdgeByType(Node start, Node end, string sType = "")
        {
            Edge res;

            res = start.GetEdge(end.Name, end.Type, "Out");
            if (res == null)
            {
                return null;
            }
            if (sType == "")
            {
                return res;
            }
            if (res.Type != sType)
            {
                return null;
            }
            return res;
        }

        //加入节点（接口）
        public void AddNode(string sName, string sType, ref ErrorCode err, string sProperities = "1")
        {
            Node newNode = null;

            //检查节点是否已经存在“名称+类型一致”
            if (GetNodesByNameAndType(sName, sType) != null)
            {
                err = ErrorCode.NodeExists;
                return;
            }
            //构造新的节点
            newNode = new Node(intMaxNodeNum,sName, sType, sProperities);
            if (newNode == null)
            {
                err = ErrorCode.CreateNodeFailed;
                return;
            }
            intMaxNodeNum++;
            AddNode(newNode);
            err = ErrorCode.NoError;
            return;
        }

        //加入节点（接口）
        public void AddNode(Node oriNode, ref ErrorCode err)
        {
            Node newNode = null;

            //检查节点是否已经存在“名称+类型一致”
            if (GetNodesByNameAndType(oriNode.Name, oriNode.Type) != null)
            {
                err = ErrorCode.NodeExists;
                return;
            }
            //构造新的节点
            newNode = new Node(intMaxNodeNum, oriNode);
            if (newNode == null)
            {
                err = ErrorCode.CreateNodeFailed;
                return;
            }
            intMaxNodeNum++;
            AddNode(newNode);
            err = ErrorCode.NoError;
            return;
        }

        //加入连边（接口）
        public void AddEdge(string sStartName, string sStartType,
                                        string sEndName, string sEndType,
                                        string sType, ref ErrorCode err, string sValue = "1")
        {
            Node startNode, endNode;
            Edge newEdge;
            //获取起始节点，不存在报错
            startNode = GetNodesByNameAndType(sStartName, sStartType);
            if (startNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //获取终止节点，不存在报错
            endNode = GetNodesByNameAndType(sEndName, sEndType);
            if (endNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //查找两点间是否存在相同类型关系，存在报错
            if (GetEdgeByType(startNode, endNode, sType) != null)
            {
                err = ErrorCode.EdgeExists;
                return;
            }
            //创建新连边
            newEdge = new Edge(intMaxEdgeNum, sType, sValue);
            if (newEdge == null)
            {
                err = ErrorCode.CreateEdgeFailed;
                return;
            }
            intMaxEdgeNum++;
            //在两点间加入新边
            AddEdge(startNode, endNode, newEdge);
            err = ErrorCode.NoError;
            return;
        }

        //修改节点内部数据（接口）
        public void ModifyNode(string sName, string sType,
                                                     ModifyOperation opt, string sProperities, ref ErrorCode err)
        {
            Node tarNode;

            tarNode = GetNodesByNameAndType(sName, sType);
            //检查节点是否已经存在“名称+类型一致”
            if (tarNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            if (opt == ModifyOperation.Delete)
            {
                tarNode.RemoveProperty(sProperities);
            }
            else
            {
                tarNode.AddProperty(sProperities, opt);
            }
            err = ErrorCode.NoError;
            return;
        }

        //修改连边取值（接口）
        public void ModifyEdge(string sStartName, string sStartType,
                                            string sEndName, string sEndType,
                                            string sType, string sValue, ref ErrorCode err)
        {
            Node startNode, endNode;
            Edge tarEdge;
            //获取起始节点，不存在报错
            startNode = GetNodesByNameAndType(sStartName, sStartType);
            if (startNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //获取终止节点，不存在报错
            endNode = GetNodesByNameAndType(sEndName, sEndType);
            if (endNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //查找两点间是否存在相同类型关系，不存在报错
            tarEdge = GetEdgeByType(startNode, endNode, sType);
            if (tarEdge == null)
            {
                err = ErrorCode.EdgeNotExists;
                return;
            }
            tarEdge.Value = sValue;
            err = ErrorCode.NoError;
            return;
        }
    
        //删除节点（接口）
        public void RemoveNode(string sName, string sType, ref ErrorCode err)
        {
            Node tarNode;

            tarNode = GetNodesByNameAndType(sName, sType);
            //检查节点是否已经存在“名称+类型一致”
            if (tarNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            RemoveNode(tarNode);
            err = ErrorCode.NoError;
            return;
        }

        //删除连边（接口）
        public void RemoveEdge(string sStartName, string sStartType,
                                string sEndName, string sEndType,
                                string sType, ref ErrorCode err)
        {
            Node startNode, endNode;
            Edge tarEdge;
            //获取起始节点，不存在报错
            startNode = GetNodesByNameAndType(sStartName, sStartType);
            if (startNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //获取终止节点，不存在报错
            endNode = GetNodesByNameAndType(sEndName, sEndType);
            if (endNode == null)
            {
                err = ErrorCode.NodeNotExists;
                return;
            }
            //查找两点间是否存在相同类型关系，存在报错
            tarEdge = GetEdgeByType(startNode, endNode, sType);
            if (tarEdge == null)
            {
                err = ErrorCode.EdgeNotExists;
                return;
            }
            //起始节点Outbound中移除连边
            startNode.RemoveEdge(tarEdge);
            //从终止节点InBound中注销连边
            endNode.UnRegisterInbound(tarEdge);
            //全部完成后，从总连边列表中移除该边
            EdgeList.Remove(tarEdge);
            err = ErrorCode.NoError;
            return;
        }
    
    }
}
