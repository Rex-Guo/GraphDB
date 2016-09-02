using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GraphDB.Core
{
    public class Edge//图数据库连边类：负责存储网络连边信息
    {
        //成员变量
        int intEdgeNum;
        Node nodeStart;//连边起点
        Node nodeEnd;//连边终点
        string edgeType;//连边类型
        string edgeValue;//连边取值

        //属性//////////////////////////
        public int Number
        {
            get
            {
                return intEdgeNum;
            }
        }
        public Node Start
        {
            get
            {
                return nodeStart;
            }
            set
            {
                nodeStart = value; 
            }
        }
        public Node End
        {
            get
            {
                return nodeEnd;
            }
            set
            {
                nodeEnd = value;
            }
        }
        public string Type
        {
            get
            {
                return edgeType;
            }
            set
            {
                edgeType = value;
            }
        }
        public string Value
        {
            get
            {
                return edgeValue;
            }
            set
            {
                edgeValue = value;
            }
        }
        //方法/////////////////////////
        //连边类Edge构造函数
        public Edge(int intMaxEdgeNum, string newType, string value = "1")//构造函数 对三个变量进行赋值
        {
            this.intEdgeNum = intMaxEdgeNum;
            this.edgeType = newType;
            this.edgeValue = value;
        }
        //连边类Edge构造函数
        public Edge(int intMaxEdgeNum, XmlElement xNode)//构造函数 对三个变量进行赋值
        {
            string newType, newValue;

            newType = newValue = "";
            //取出制定标签的Inner Text
            newType = GetText(xNode, "Type");
            if (newType == "")
            {
                newType = "关联";
            }
            newValue = GetText(xNode, "Value");
            if (newValue == "")
            {
                newValue = "1";
            }
            //赋值与初始化
            this.intEdgeNum = intMaxEdgeNum;
            this.edgeType = newType;
            this.edgeValue = newValue;
        }

        //工具函数，从xml节点中读取某个标签的InnerText
        protected string GetText(XmlElement curNode, string sLabel)
        {
            if (curNode == null)
            {
                return "";
            }
            //遍历当前XML的所有子标签
            foreach (XmlElement xNode in curNode.ChildNodes)
            {
                if (xNode.Name == sLabel)
                {//返回标签内容一致的内部数据
                    return xNode.InnerText;
                }
            }
            return "";
        }

        //将连边数据保存为xml格式
        public virtual XmlElement ToXML(ref XmlDocument doc)
        {
            XmlElement curEdge = doc.CreateElement("Edge");         //创建连边元素
            XmlElement type_xml, value_xml, Start_xml, End_xml;
            XmlText type_txt, value_txt, Start_txt, End_txt;

            //节点类型
            type_xml = doc.CreateElement("Type");
            value_xml = doc.CreateElement("Value");
            //节点位置
            Start_xml = doc.CreateElement("Start");
            End_xml = doc.CreateElement("End");
            //创建各属性的文本元素
            type_txt = doc.CreateTextNode(this.Type);               
            value_txt = doc.CreateTextNode(this.Value);
            Start_txt = doc.CreateTextNode(this.Start.SaveIndex.ToString());
            End_txt = doc.CreateTextNode(this.End.SaveIndex.ToString());
            //将标题元素赋予文本内容
            type_xml.AppendChild(type_txt);                                    
            value_xml.AppendChild(value_txt);
            Start_xml.AppendChild(Start_txt);
            End_xml.AppendChild(End_txt);
            //向当前节点中加入各属性节点
            curEdge.AppendChild(type_xml);                                   
            curEdge.AppendChild(value_xml);
            curEdge.AppendChild(Start_xml);
            curEdge.AppendChild(End_xml);

            return curEdge;
        }
    }
}
