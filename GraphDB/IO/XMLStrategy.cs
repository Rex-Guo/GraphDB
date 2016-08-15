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
    //XML�ļ���ʽʾ��
    /*
     * <Graph>
     *      <Nodes NodeNumber="2">
     *          <Node num="0">
     *              <Name>��</Name>
     *              <string>����</string>
     *              <Property>
     *                  <A>1</A>
     *                  <B>2</B>
     *              </Property>
     *          </Node>
     *          <Node num="1">
     *              <Name>����</Name>
     *              <string>����</string>
     *              <Property>
     *                  <A>1</A>
     *                  <C>3</C>
     *              </Property>
     *          </Node>
     *      </Nodes>
     *      <Edgs EdgeNumber="1">
     *          <Edge>
     *              <string>ͳ��</string>
     *              <Start>0</Start>
     *              <End>1</End>
     *          </Edge>
     *      </Edgs> 
     * </Graph>
     * */

    public class XMLStrategy:IfIOStrategy//XML�ļ���д�㷨
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

       //XMLStrategy�㷨��ȡ����
        Graph IfIOStrategy.ReadFile(ref ErrorCode err)
        {
            FileStream stream = null;
            XmlDocument doc = new XmlDocument();
            Graph NewGraph;

            try
            {
                stream = new FileStream(strPath, FileMode.Open);
                doc.Load(stream);               //�����ļ�����xml�ĵ�
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
            //��������
            NewGraph = new Graph(doc, ref err);
            if (NewGraph == null)
            {
                return null;
            }
            return NewGraph;
        }

        //XMLStrategy�㷨���溯��
        void IfIOStrategy.SaveFile(XmlDocument doc, ref ErrorCode err)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(strPath, FileMode.Create);
                doc.Save(stream);               //����xml�ĵ�����
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
