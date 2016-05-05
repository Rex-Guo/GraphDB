using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using System.Text.RegularExpressions;

namespace GraphDB.Core
{
    public class Node//图数据库节点类：负责存储单一网络节点的信息，并向上层类提供功能接口函数
    {
        //共享变量
        static int intMaxNodeNum = 0;
        //成员变量
        int intNodeNum;                           //节点编号
        string nodeName;
        string nodeType;
        List<NodeProperty> Attribute;
        List<Edge> OutLink;       //连边 使用字典结构存放（目标节点号，连边对象）
        List<Edge> InLink;
        int intSaveIndex;
        //属性///////////////////////////////
        public int Number
        {
            get
            {
                return intNodeNum;
            }
        }
        public string Name
        {
            get
            {
                return nodeName;
            }
        }
        public string Type
        {
            get
            {
                return nodeType;
            }
        }
        public int InDegree
        {
            get
            {
                return InLink.Count;
            }
        }
        public int OutDegree
        {
            get
            {
                return OutLink.Count;
            }
        }
        public List<Edge> OutBound
        {
            get
            {
                return OutLink;
            }
        }
        public List<Edge> InBound
        {
            get
            {
                return InLink;
            }
        }
        public int SaveIndex
        {
            get
            {
                return intSaveIndex;
            }
            set
            {
                intSaveIndex = value;
            }
        }
        //方法///////////////////////////////
        //节点类Node构造函数
        public Node(string newName, string newType, string sProperities = "")    
        {
            this.intNodeNum = intMaxNodeNum;
            this.nodeName = newName;
            this.nodeType = newType;
            this.intSaveIndex = this.intNodeNum;
            Attribute = new List<NodeProperty>();
            OutLink = new List<Edge>();
            InLink = new List<Edge>();
            if(sProperities != "")
            {
                AddProperty(sProperities);
            }
            intMaxNodeNum++;
        }

        //xml构造函数
        public Node(XmlElement xNode)
        {
            string newType, newName;
            XmlNode xmlProperties;

            this.intNodeNum = intMaxNodeNum;
            //取出制定标签的Inner Text
            newType = GetText(xNode, "Type");
            newName = GetText(xNode, "Name");
            xmlProperties = xNode.GetElementsByTagName("Properties").Item(0);
            //赋值与初始化
            this.nodeType = newType;
            this.nodeName = newName;
            this.intSaveIndex = this.intNodeNum;
            Attribute = new List<NodeProperty>();
            OutLink = new List<Edge>();
            InLink = new List<Edge>();
            intMaxNodeNum++;
            //加入用户自定义属性
            foreach(XmlElement curNode in xmlProperties.ChildNodes)
            {
                Attribute.Add(new NodeProperty(curNode));
            }
        }
        
        //工具函数，从xml节点中读取某个标签的InnerText
        protected string GetText(XmlElement curNode, string sLabel)
        {
            if (curNode == null)
            {
                return "";
            }
            //遍历子节点列表
            foreach (XmlElement xNode in curNode.ChildNodes)
            {
                if (xNode.Name == sLabel)
                {//查找和指定内容相同的标签，返回其Innner Text
                    return xNode.InnerText;
                }
            }
            return "";
        }

        //工具函数，重置节点计数
        public static void ResetIndex()
        {
            intMaxNodeNum = 0;
        }

        //增加自定义属性对
        void AddProperty(string sProperities)
        {
            const string strKeyPairPattern = @"[\w]+=[\w]+";  //匹配目标"名称+取值"组合
            MatchCollection matches;
            Regex regObj;
            NodeProperty newProperty;

            regObj = new Regex(strKeyPairPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sProperities);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                return;
            }
            foreach (Match match in matches)//遍历匹配列表
            {
                newProperty = null;
                newProperty = BuildProperty(match.Value);
                if (newProperty != null && IsPropertyExist(newProperty) == false)
                {
                    this.Attribute.Add(newProperty);
                }
            }
            return;
        }
        //构造属性对
        NodeProperty BuildProperty(string sProperty)
        {
            string[] strSeg;
            NodeProperty newProperty = null;

            strSeg = sProperty.Split(new char[]{'='});
            newProperty = new NodeProperty(strSeg[0], strSeg[1]);
            return newProperty;
        }
        //检查属性对的key是否已经存在
        bool IsPropertyExist(NodeProperty sProperty)
        {
            foreach (NodeProperty tP in Attribute)
            {
                if (tP.Key == sProperty.Key)
                {
                    return true;
                }
            }
            return false;
        }

        //增加连边
        public bool AddEdge(Edge newEdge)
        {
            if (newEdge == null)
            {
                return false;
            }
            //检测条件：当前边的起始节点是本节点，且终止节点不是本节点
            if (newEdge.Start.Number != intNodeNum || newEdge.End.Number == intNodeNum)
            {
                return false;
            }
            //如果OutbOund已经包含该边
            if (OutBoundContainsEdge(newEdge) == true)
            {
                return false;
            }
            //向Links中加入新项目  
            OutLink.Add(newEdge);   
            return true;
        }

        //Inbound边注册
        public bool RegisterInbound(Edge newEdge)
        {
            if (newEdge == null)
            {
                return false;
            }
            //检测条件：当前边的起始节点不是本节点，且终止节点是本节点
            if (newEdge.End.Number != intNodeNum || newEdge.Start.Number == intNodeNum)
            {
                return false;
            }
            //如果Inbound包含该边则不注册
            if (InBoundContainsEdge(newEdge) == true)
            {
                return false;
            }
            //加入新边
            InLink.Add(newEdge);
            return true;
        }

        //去除连边
        public bool RemoveEdge(Edge curEdge)
        {
            if (curEdge == null)
            {
                return false;
            }
            //检测条件：当前边的起始节点是本节点，且终止节点不是本节点
            if (curEdge.Start.Number != intNodeNum || curEdge.End.Number == intNodeNum)
            {
                return false;
            }
            //如果OutbOund不包含该边则退出
            if (OutBoundContainsEdge(curEdge) == false)
            {
                return false;
            }
            OutLink.Remove(curEdge);
            return true;
        }

        //清除所有连边,返回被清除的边列表
        public List<Edge> ClearEdge()
        {
            List<Edge> EdgeList = new List<Edge>();
            //首先将OutBound中所有连边的终止节点中注销该边
            foreach (Edge edge in this.OutBound)
            {
                edge.End.UnRegisterInbound(edge);
                edge.Start = null;
                edge.End = null;
                //当前边加入返回结果列表
                EdgeList.Add(edge);
            }
            //从OutBound中清除所有边
            this.OutBound.Clear();
            //首先将InBound中所有连边的起始节点中去除该边
            foreach (Edge edge in this.InBound)
            {
                edge.Start.RemoveEdge(edge);
                edge.Start = null;
                edge.End = null;
                //当前边加入返回结果列表
                EdgeList.Add(edge);
            }
            //从InBound中清除所有边
            this.InBound.Clear();
            //返回本节点涉及的连边列表
            return EdgeList;
        }

        //Inbound注销
        public bool UnRegisterInbound(Edge curEdge)
        {
            if (curEdge == null)
            {
                return false;
            }
            //检测条件：当前边的起始节点不是本节点，且终止节点是本节点
            if (curEdge.End.Number != intNodeNum || curEdge.Start.Number == intNodeNum)//检测条件：当前节点与目标节点不相连，且目标节点不是当前节点
            {
                return false;
            }
            //如果Inbound不包含当前边则不注销
            if (InBoundContainsEdge(curEdge) == false)
            {
                return false;
            }
            InLink.Remove(curEdge);
            return true;

        }

        //返回OutBound是否包含和目标节点间的连边
        bool OutBoundContainsEdge(Edge newEdge)
        {
            if (OutLink.Contains(newEdge))
            {
                return true;
            }
            foreach (Edge edge in OutLink)
            {
                if (edge.End.Number == newEdge.End.Number)
                {
                    if (edge.Type == newEdge.Type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //返回InBound是否包含和目标节点间的连边
        bool InBoundContainsEdge(Edge newEdge)
        {
            if (InLink.Contains(newEdge))
            {
                return true;
            }
            foreach (Edge edge in InLink)
            {
                if (edge.Start.Number == newEdge.Start.Number)
                {
                    if (edge.Type == newEdge.Type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //将节点数据保存为xml格式
        public virtual XmlElement ToXML(ref XmlDocument doc)
        {
            XmlElement curNode = doc.CreateElement("Node");
            XmlElement curProperties = doc.CreateElement("Properties");
            XmlElement type_xml, name_xml;
            XmlText type_txt, name_txt;

            curNode.SetAttribute("num", this.SaveIndex.ToString());                   //创建各属性的Tag元素
            //节点类型
            name_xml = doc.CreateElement("Name");
            type_xml = doc.CreateElement("Type");
            //创建各属性的文本元素
            name_txt = doc.CreateTextNode(this.Name); 
            type_txt = doc.CreateTextNode(this.Type);
            //将标题元素赋予文本内容
            name_xml.AppendChild(name_txt); 
            type_xml.AppendChild(type_txt);
            foreach (NodeProperty np in Attribute)
            {
                curProperties.AppendChild(np.ToXML(ref doc));
            }
            //向当前节点中加入各属性节点
            curNode.AppendChild(name_xml);
            curNode.AppendChild(type_xml);
            curNode.AppendChild(curProperties);     
            return curNode;
        }
    
        //查找连边
        public Edge GetEdge(string sType, string opt)
        {
            if (opt == "In")
            {
                foreach (Edge edge in InBound)
                {
                    if (edge.Type == sType)
                    {
                        return edge;
                    }
                }
                return null;
            }
            else if (opt == "Out")
            {
                foreach (Edge edge in OutBound)
                {
                    if (edge.Type == sType)
                    {
                        return edge;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }
    
    }
}
