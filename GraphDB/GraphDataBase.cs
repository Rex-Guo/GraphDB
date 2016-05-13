using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GraphDB.Core;
using GraphDB.IO;
using GraphDB.Parser;

namespace GraphDB
{
    //图数据库类
    public class GraphDataBase
    {
        Graph graph;
        IfIOStrategy IOhandler;
        string strPath;
        CypherParser parser;

        //属性
        public string Path
        {
            get
            {
                return strPath;
            }
        }
        //函数
        public GraphDataBase()
        {
            graph = new Graph();
            parser = new CypherParser();
        }
        //创建数据库，输入文件保存路径
        public void CreateDataBase(string sPath, ref ErrorCode err)
        {
            strPath = sPath;
            IOhandler = new XMLStrategy();
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
        //执行查询语句
        public void DataQueryExecute(string strCypher, ref ErrorCode err)
        {
            //查询语句传入解析器
            parser.QueryExecute(ref graph, strCypher, ref err);
        }
    }
}
