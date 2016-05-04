using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace GraphDB.Core
{
    public class NodeProperty
    {
        string strKey;
        string strValue;
        //属性
        public string Key
        {
            get
            {
                return strKey;
            }
        }
        public string Value
        {
            get
            {
                return strValue;
            }
            set
            {
                strValue = value;
            }
        }
        //成员函数
        public NodeProperty(string sKey, string sValue)
        {
            strKey = sKey;
            strValue = sValue;
        }

        //节点属性类NodeProperty构造函数
        public NodeProperty(XmlElement xNode)//构造函数 对三个变量进行赋值
        {
            strKey = xNode.Name;
            strValue = xNode.InnerText;
        }

        //将连边数据保存为xml格式
        public XmlElement ToXML(ref XmlDocument doc)
        {
            XmlElement curProperty = doc.CreateElement(strKey);         //创建连边元素
            XmlText value_txt;

            //创建各属性的文本元素
            value_txt = doc.CreateTextNode(this.strValue);
            //向当前节点中加入各属性节点
            curProperty.AppendChild(value_txt);

            return curProperty;
        }

    }
}
