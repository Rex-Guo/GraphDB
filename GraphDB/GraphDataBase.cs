using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using GraphDB.Core;
using GraphDB.IO;
using GraphDB.Parser;
using GraphDB.Layout;

namespace GraphDB
{
    //图数据库类
    public class GraphDataBase
    {
        Graph graph;
        IfIOStrategy IOhandler;
        CypherParser parser;
        CircleLayout circo;

        //属性
        //获取文件存放路径
        public string Path
        {
            get
            {
                if (IOhandler == null)
                {
                    return "";
                }
                return IOhandler.Path;
            }
        }
        //获取数据库节点总数
        public int NodeNum
        {
            get
            {
                return graph.NodeNum;
            }
        }
        //获取数据库连边总数
        public int EdgeNum
        {
            get
            {
                return graph.EdgeNum;
            }
        }
        //获取节点列表
        public List<Node> Nodes
        {
            get
            {
                return graph.Nodes;
            }
        }
        //返回画布宽度
        public int Width
        {
            get
            {
                if (circo == null)
                {
                    return 0;
                }
                return circo.Width;
            }
        }
        //返回画布高度
        public int Height
        {
            get
            {
                if (circo == null)
                {
                    return 0;
                }
                return circo.Height;
            }
        }

        //函数
        public GraphDataBase()
        {
            graph = new Graph();
            parser = new CypherParser();
        }

        public GraphDataBase(int iNum, int iRadius)
        {
            graph = new Graph();
            circo = new CircleLayout(iNum, iRadius);
        }
        //创建数据库，输入文件保存路径
        public void CreateDataBase(string sPath, ref ErrorCode err)
        {
            IOhandler = new XMLStrategy(sPath);
            DataExport(ref err);
        }
        //打开数据库，输入当前文件路径
        public void OpenDataBase(string sPath, ref ErrorCode err)
        {
            IOhandler = new XMLStrategy(sPath);
            DataImport(ref err);
        }
        //XML批量数据导入
        public void DataImport(ref ErrorCode err)
        {
            graph = IOhandler.ReadFile(ref err);
            if (err != ErrorCode.NoError)
            {
                graph = null;
            }
        }
        //XML批量数据导出
        public void DataExport(ref ErrorCode err)
        {
            XmlDocument doc;

            doc = graph.ToXML(ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            IOhandler.SaveFile(doc, ref err);
        }

        //数据库另存为
        public void DataBaseSaveAs(string newPath, ref ErrorCode err)
        {
            this.IOhandler.Path = newPath;
            DataExport(ref err);
        }

        //插入数据节点
        public void AddNodeData(string sName, string sType, ref ErrorCode err, string sProperities = "1")
        {
            graph.AddNode(sName, sType, ref err, sProperities);
        }

        //插入数据节点2
        public void AddNodeData(Node oriNode, ref ErrorCode err)
        {
            graph.AddNode(oriNode, ref err);
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
        public string DataQueryExecute(string strCypher, ref ErrorCode err)
        {
            //查询语句传入解析器
            return parser.QueryExecute(ref graph, strCypher, ref err);
        }
        //查询函数，返回指定索引处的节点
        public Node GetNodeByIndex(int index)
        {
            return graph.GetNodeByIndex(index);
        }
        //查询函数，返回节点列表中指定名称和类型的节点
        public Node GetNodeByName(string sName, string sType)
        {
            return graph.GetNodesByNameAndType(sName, sType);
        }

        //查询函数，返回指定名称和类型的节点的索引
        public int GetIndexByNameAndType(string sName, string sType)
        {
            return graph.GetIndexByNameAndType(sName, sType);
        }

        //查询函数，返回指定名称和类型的节点间的连边
        public Edge GetEdgeByNameAndType(string sName, string sType, string eName, string eType)
        {
            return graph.GetEdgeByNameAndType(sName, sType, eName, eType);
        }

        //启动环形布局
        public void StartCicro()
        {
            if (this.circo == null)
            {
                return;
            }
            //初始化所有布局点
            circo.LayoutInit(graph);
            //进入退火循环
            circo.Float(graph);
        }

        public List<NodeDrawing> GetLayoutNodePoints()
        {
            List<NodeDrawing> nodeDrawing;
            int index;

            if(this.circo == null)
            {
                return null;
            }
            nodeDrawing = new List<NodeDrawing>();
            index = 0;
            foreach (Node node in graph.Nodes)
            {
                nodeDrawing.Add(new NodeDrawing(node.Name, circo.NodePoints[index]));
                index++;
            }

            return nodeDrawing;
        }

        public List<EdgeDrawing> GetLayoutEdgePoints()
        {
            List<EdgeDrawing> edgeDrawing;
            int intStart, intEnd;

            if (this.circo == null)
            {
                return null;
            }
            edgeDrawing = new List<EdgeDrawing>();
            foreach (Edge edge in graph.Edges)
            {
                intStart = graph.GetIndexByNameAndType(edge.Start.Name, edge.Start.Type);
                intEnd = graph.GetIndexByNameAndType(edge.End.Name, edge.End.Type);
                edgeDrawing.Add(new EdgeDrawing(circo.NodePoints[intStart], circo.NodePoints[intEnd],edge.Type));
            }

            return edgeDrawing;
        }

    }
}
