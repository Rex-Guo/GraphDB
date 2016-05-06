using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GraphDB.Core;
using GraphDB.IO;

namespace GraphDB
{
    //系统错误码
    public enum ErrorCode
    {
        NoError = 0,
        OpenFileFailed = 1,
        SaveFileFailed = 2,
        NoXmlRoot = 3,
        InvaildIndex =10,
        NodeExists = 11,
        CreateNodeFailed = 12,
        NodeNotExists = 13,
        EdgeExists = 15,
        CreateEdgeFailed = 16,
        EdgeNotExists = 17,
        AddEdgeFailed  = 18,
        
    }
    //修改操作选项
    public enum ModifyOperation
    {
        Append = 0,
        Replace = 1,
        ReplaceAll = 2,
        Delete = 3,
    }
    //数据库类
    public class GraphDataBase
    {
        Graph graph;
        IfIOStrategy IOhandler;
        string strPath;

        //属性
        public string Path
        {
            get
            {
                return strPath;
            }
        }
        //函数
        //创建数据库，输入文件保存路径
        public void CreateDataBase(string sPath, ref ErrorCode err)
        {
            strPath = sPath;
            IOhandler = new XMLStrategy();
            graph = new Graph();
            DataExport(strPath, ref err);
        }
        //打开数据库，输入当前文件路径
        public void OpenDataBase(string sPath, ref ErrorCode err)
        {
            strPath = sPath;
            IOhandler = new XMLStrategy();
            DataImport(sPath, ref err);
        }
        //XML批量数据导入
        public void DataImport(string sPath, ref ErrorCode err)
        {
            graph = IOhandler.ReadFile(sPath, ref err);
            if (err != ErrorCode.NoError)
            {
                graph = null;
            }
        }
        //XML批量数据导出
        public void DataExport(string sPath, ref ErrorCode err)
        {
            XmlDocument doc;

            doc = graph.ToXML(ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            IOhandler.SaveFile(doc, sPath, ref err);
        }

        //插入数据节点
        public void AddNodeData(string sName, string sType, ref ErrorCode err, string sProperities = "1")
        {
            graph.AddNode(sName, sType, ref err, sProperities);
        }
        //插入关系连边
        public void AddEdgeData(string sStartName, string sStartType,
                                                string sEndName, string sEndType,
                                                string sType, ref ErrorCode err, string sValue = "")
        {
            graph.AddEdge(sStartName, sStartType, sEndName, sEndType, sType, ref err, sValue);
        }
        //修改节点内部数据
        public void ModifyNodeData(string sName, string sType,
                                                     ModifyOperation opt, string sProperities, ref ErrorCode err)
        {
            graph.ModifyNode(sName, sType, opt, sProperities, ref err);
        }
        //修改连边取值
        public void ModifyEdgeData(string sStartName, string sStartType,
                                                    string sEndName, string sEndType,
                                                    string sType, string sValue, ref ErrorCode err)
        {
            graph.ModifyEdge(sStartName, sStartType, sEndName, sEndType, sType, sValue, ref err);
        }
        //删除数据节点
        public void RemoveNodeData(string sName, string sType, ref ErrorCode err)
        {
            graph.RemoveNode(sName, sType, ref err);
        }
        //删除关系连边
        public void RemoveEdgeData(string sStartName, string sStartType,
                                                    string sEndName, string sEndType,
                                                    string sType, ref ErrorCode err)
        {
            graph.RemoveEdge(sStartName, sStartType, sEndName, sEndType, sType, ref err);
        }
    }
}
