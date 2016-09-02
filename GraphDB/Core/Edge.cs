using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GraphDB.Core
{
    public class Edge//ͼ���ݿ������ࣺ����洢����������Ϣ
    {
        //��Ա����
        int intEdgeNum;
        Node nodeStart;//�������
        Node nodeEnd;//�����յ�
        string edgeType;//��������
        string edgeValue;//����ȡֵ

        //����//////////////////////////
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
        //����/////////////////////////
        //������Edge���캯��
        public Edge(int intMaxEdgeNum, string newType, string value = "1")//���캯�� �������������и�ֵ
        {
            this.intEdgeNum = intMaxEdgeNum;
            this.edgeType = newType;
            this.edgeValue = value;
        }
        //������Edge���캯��
        public Edge(int intMaxEdgeNum, XmlElement xNode)//���캯�� �������������и�ֵ
        {
            string newType, newValue;

            newType = newValue = "";
            //ȡ���ƶ���ǩ��Inner Text
            newType = GetText(xNode, "Type");
            if (newType == "")
            {
                newType = "����";
            }
            newValue = GetText(xNode, "Value");
            if (newValue == "")
            {
                newValue = "1";
            }
            //��ֵ���ʼ��
            this.intEdgeNum = intMaxEdgeNum;
            this.edgeType = newType;
            this.edgeValue = newValue;
        }

        //���ߺ�������xml�ڵ��ж�ȡĳ����ǩ��InnerText
        protected string GetText(XmlElement curNode, string sLabel)
        {
            if (curNode == null)
            {
                return "";
            }
            //������ǰXML�������ӱ�ǩ
            foreach (XmlElement xNode in curNode.ChildNodes)
            {
                if (xNode.Name == sLabel)
                {//���ر�ǩ����һ�µ��ڲ�����
                    return xNode.InnerText;
                }
            }
            return "";
        }

        //���������ݱ���Ϊxml��ʽ
        public virtual XmlElement ToXML(ref XmlDocument doc)
        {
            XmlElement curEdge = doc.CreateElement("Edge");         //��������Ԫ��
            XmlElement type_xml, value_xml, Start_xml, End_xml;
            XmlText type_txt, value_txt, Start_txt, End_txt;

            //�ڵ�����
            type_xml = doc.CreateElement("Type");
            value_xml = doc.CreateElement("Value");
            //�ڵ�λ��
            Start_xml = doc.CreateElement("Start");
            End_xml = doc.CreateElement("End");
            //���������Ե��ı�Ԫ��
            type_txt = doc.CreateTextNode(this.Type);               
            value_txt = doc.CreateTextNode(this.Value);
            Start_txt = doc.CreateTextNode(this.Start.SaveIndex.ToString());
            End_txt = doc.CreateTextNode(this.End.SaveIndex.ToString());
            //������Ԫ�ظ����ı�����
            type_xml.AppendChild(type_txt);                                    
            value_xml.AppendChild(value_txt);
            Start_xml.AppendChild(Start_txt);
            End_xml.AppendChild(End_txt);
            //��ǰ�ڵ��м�������Խڵ�
            curEdge.AppendChild(type_xml);                                   
            curEdge.AppendChild(value_xml);
            curEdge.AppendChild(Start_xml);
            curEdge.AppendChild(End_xml);

            return curEdge;
        }
    }
}
