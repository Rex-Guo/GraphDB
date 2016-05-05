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
    public class Node//ͼ���ݿ�ڵ��ࣺ����洢��һ����ڵ����Ϣ�������ϲ����ṩ���ܽӿں���
    {
        //�������
        static int intMaxNodeNum = 0;
        //��Ա����
        int intNodeNum;                           //�ڵ���
        string nodeName;
        string nodeType;
        List<NodeProperty> Attribute;
        List<Edge> OutLink;       //���� ʹ���ֵ�ṹ��ţ�Ŀ��ڵ�ţ����߶���
        List<Edge> InLink;
        int intSaveIndex;
        //����///////////////////////////////
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
        //����///////////////////////////////
        //�ڵ���Node���캯��
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

        //xml���캯��
        public Node(XmlElement xNode)
        {
            string newType, newName;
            XmlNode xmlProperties;

            this.intNodeNum = intMaxNodeNum;
            //ȡ���ƶ���ǩ��Inner Text
            newType = GetText(xNode, "Type");
            newName = GetText(xNode, "Name");
            xmlProperties = xNode.GetElementsByTagName("Properties").Item(0);
            //��ֵ���ʼ��
            this.nodeType = newType;
            this.nodeName = newName;
            this.intSaveIndex = this.intNodeNum;
            Attribute = new List<NodeProperty>();
            OutLink = new List<Edge>();
            InLink = new List<Edge>();
            intMaxNodeNum++;
            //�����û��Զ�������
            foreach(XmlElement curNode in xmlProperties.ChildNodes)
            {
                Attribute.Add(new NodeProperty(curNode));
            }
        }
        
        //���ߺ�������xml�ڵ��ж�ȡĳ����ǩ��InnerText
        protected string GetText(XmlElement curNode, string sLabel)
        {
            if (curNode == null)
            {
                return "";
            }
            //�����ӽڵ��б�
            foreach (XmlElement xNode in curNode.ChildNodes)
            {
                if (xNode.Name == sLabel)
                {//���Һ�ָ��������ͬ�ı�ǩ��������Innner Text
                    return xNode.InnerText;
                }
            }
            return "";
        }

        //���ߺ��������ýڵ����
        public static void ResetIndex()
        {
            intMaxNodeNum = 0;
        }

        //�����Զ������Զ�
        void AddProperty(string sProperities)
        {
            const string strKeyPairPattern = @"[\w]+=[\w]+";  //ƥ��Ŀ��"����+ȡֵ"���
            MatchCollection matches;
            Regex regObj;
            NodeProperty newProperty;

            regObj = new Regex(strKeyPairPattern);//������ʽ��ʼ��������ƥ��ģʽ
            matches = regObj.Matches(sProperities);//������ʽ�Էִʽ������ƥ��
            if (matches.Count == 0)
            {
                return;
            }
            foreach (Match match in matches)//����ƥ���б�
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
        //�������Զ�
        NodeProperty BuildProperty(string sProperty)
        {
            string[] strSeg;
            NodeProperty newProperty = null;

            strSeg = sProperty.Split(new char[]{'='});
            newProperty = new NodeProperty(strSeg[0], strSeg[1]);
            return newProperty;
        }
        //������ԶԵ�key�Ƿ��Ѿ�����
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

        //��������
        public bool AddEdge(Edge newEdge)
        {
            if (newEdge == null)
            {
                return false;
            }
            //�����������ǰ�ߵ���ʼ�ڵ��Ǳ��ڵ㣬����ֹ�ڵ㲻�Ǳ��ڵ�
            if (newEdge.Start.Number != intNodeNum || newEdge.End.Number == intNodeNum)
            {
                return false;
            }
            //���OutbOund�Ѿ������ñ�
            if (OutBoundContainsEdge(newEdge) == true)
            {
                return false;
            }
            //��Links�м�������Ŀ  
            OutLink.Add(newEdge);   
            return true;
        }

        //Inbound��ע��
        public bool RegisterInbound(Edge newEdge)
        {
            if (newEdge == null)
            {
                return false;
            }
            //�����������ǰ�ߵ���ʼ�ڵ㲻�Ǳ��ڵ㣬����ֹ�ڵ��Ǳ��ڵ�
            if (newEdge.End.Number != intNodeNum || newEdge.Start.Number == intNodeNum)
            {
                return false;
            }
            //���Inbound�����ñ���ע��
            if (InBoundContainsEdge(newEdge) == true)
            {
                return false;
            }
            //�����±�
            InLink.Add(newEdge);
            return true;
        }

        //ȥ������
        public bool RemoveEdge(Edge curEdge)
        {
            if (curEdge == null)
            {
                return false;
            }
            //�����������ǰ�ߵ���ʼ�ڵ��Ǳ��ڵ㣬����ֹ�ڵ㲻�Ǳ��ڵ�
            if (curEdge.Start.Number != intNodeNum || curEdge.End.Number == intNodeNum)
            {
                return false;
            }
            //���OutbOund�������ñ����˳�
            if (OutBoundContainsEdge(curEdge) == false)
            {
                return false;
            }
            OutLink.Remove(curEdge);
            return true;
        }

        //�����������,���ر�����ı��б�
        public List<Edge> ClearEdge()
        {
            List<Edge> EdgeList = new List<Edge>();
            //���Ƚ�OutBound���������ߵ���ֹ�ڵ���ע���ñ�
            foreach (Edge edge in this.OutBound)
            {
                edge.End.UnRegisterInbound(edge);
                edge.Start = null;
                edge.End = null;
                //��ǰ�߼��뷵�ؽ���б�
                EdgeList.Add(edge);
            }
            //��OutBound��������б�
            this.OutBound.Clear();
            //���Ƚ�InBound���������ߵ���ʼ�ڵ���ȥ���ñ�
            foreach (Edge edge in this.InBound)
            {
                edge.Start.RemoveEdge(edge);
                edge.Start = null;
                edge.End = null;
                //��ǰ�߼��뷵�ؽ���б�
                EdgeList.Add(edge);
            }
            //��InBound��������б�
            this.InBound.Clear();
            //���ر��ڵ��漰�������б�
            return EdgeList;
        }

        //Inboundע��
        public bool UnRegisterInbound(Edge curEdge)
        {
            if (curEdge == null)
            {
                return false;
            }
            //�����������ǰ�ߵ���ʼ�ڵ㲻�Ǳ��ڵ㣬����ֹ�ڵ��Ǳ��ڵ�
            if (curEdge.End.Number != intNodeNum || curEdge.Start.Number == intNodeNum)//�����������ǰ�ڵ���Ŀ��ڵ㲻��������Ŀ��ڵ㲻�ǵ�ǰ�ڵ�
            {
                return false;
            }
            //���Inbound��������ǰ����ע��
            if (InBoundContainsEdge(curEdge) == false)
            {
                return false;
            }
            InLink.Remove(curEdge);
            return true;

        }

        //����OutBound�Ƿ������Ŀ��ڵ�������
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

        //����InBound�Ƿ������Ŀ��ڵ�������
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

        //���ڵ����ݱ���Ϊxml��ʽ
        public virtual XmlElement ToXML(ref XmlDocument doc)
        {
            XmlElement curNode = doc.CreateElement("Node");
            XmlElement curProperties = doc.CreateElement("Properties");
            XmlElement type_xml, name_xml;
            XmlText type_txt, name_txt;

            curNode.SetAttribute("num", this.SaveIndex.ToString());                   //���������Ե�TagԪ��
            //�ڵ�����
            name_xml = doc.CreateElement("Name");
            type_xml = doc.CreateElement("Type");
            //���������Ե��ı�Ԫ��
            name_txt = doc.CreateTextNode(this.Name); 
            type_txt = doc.CreateTextNode(this.Type);
            //������Ԫ�ظ����ı�����
            name_xml.AppendChild(name_txt); 
            type_xml.AppendChild(type_txt);
            foreach (NodeProperty np in Attribute)
            {
                curProperties.AppendChild(np.ToXML(ref doc));
            }
            //��ǰ�ڵ��м�������Խڵ�
            curNode.AppendChild(name_xml);
            curNode.AppendChild(type_xml);
            curNode.AppendChild(curProperties);     
            return curNode;
        }
    
        //��������
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
