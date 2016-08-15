using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using GraphDB.Core;

namespace GraphDB.IO
{
    //XML文件格式示例
    /*
     * <Graph>
     *      <Nodes NodeNumber="2">
     *          <Node num="0">
     *              <Name>秦</Name>
     *              <string>国家</string>
     *              <Property>
     *                  <A>1</A>
     *                  <B>2</B>
     *              </Property>
     *          </Node>
     *          <Node num="1">
     *              <Name>关中</Name>
     *              <string>地区</string>
     *              <Property>
     *                  <A>1</A>
     *                  <C>3</C>
     *              </Property>
     *          </Node>
     *      </Nodes>
     *      <Edgs EdgeNumber="1">
     *          <Edge>
     *              <string>统治</string>
     *              <Start>0</Start>
     *              <End>1</End>
     *          </Edge>
     *      </Edgs> 
     * </Graph>
     * */

    public class XMLStrategy:IfIOStrategy//XML文件读写算法
    {
        string strPath;

        string IfIOStrategy.Path
        {
            get
            {
                return strPath;
            }
            set
            {
                strPath = value;
            }
        }

        public XMLStrategy(string sPath)
        {
            strPath = sPath;
        }

       //XMLStrategy算法读取函数
        Graph IfIOStrategy.ReadFile(ref ErrorCode err)
        {
            FileStream stream = null;
            XmlDocument doc = new XmlDocument();
            Graph NewGraph;

            try
            {
                stream = new FileStream(strPath, FileMode.Open);
                doc.Load(stream);               //从流文件读入xml文档
                stream.Close();
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    ex.ToString();
                    stream.Dispose();
                }
                err = ErrorCode.OpenFileFailed;
                return null;
            }
            stream.Dispose();
            //创建网络
            NewGraph = new Graph(doc, ref err);
            if (NewGraph == null)
            {
                return null;
            }
            return NewGraph;
        }

        //XMLStrategy算法保存函数
        void IfIOStrategy.SaveFile(XmlDocument doc, ref ErrorCode err)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(strPath, FileMode.Create);
                doc.Save(stream);               //保存xml文档到流
                stream.Close();
            }
            catch (Exception ex)
            {
                if (stream != null)
                {
                    ex.ToString();
                    stream.Dispose();
                }
                err = ErrorCode.SaveFileFailed;
            }
            stream.Dispose();
        }
    }
}
