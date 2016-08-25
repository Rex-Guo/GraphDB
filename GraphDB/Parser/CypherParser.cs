using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GraphDB.Core;

namespace GraphDB.Parser
{
    //Cypher语句操作
    public enum CypherOperation
    {
        Invalid = 0,
        Query = 1,
        Create = 2,
        Delete = 3,
        Modify = 4,
    }
    //Cypher语句解析器类
    public class CypherParser
    {
        const bool bolDistinct = true;
        string strCypher;
        CypherOperation op;
        List<MidResult> midRes;
        StartRule sRule;
        List<MatchRule> mRule;
        List<FilterRule> fRule;
        List<Node> Starter;
        List<TreeNode> ResultTree;
        
        //构造函数
        public string QueryExecute(ref Graph graph, string sCypher, ref ErrorCode err)
        {
            string strResult;
            CypherInit(sCypher, ref err);
            if (err != ErrorCode.NoError)
            {
                return "";
            }
            //从图中选取节点，startRule
            SelectStarter(graph, ref err);
            if (err != ErrorCode.NoError)
            {
                return "";
            }
            //根据matchRule查找，将返回数据存入记录树
            Query();
            //过滤结果树
            FiltrateResult(graph);
            //返回结果。依据midResult中的标记
            strResult = ResultOutput(graph);
            return strResult;
        }
        //数据结构初始化
        void CypherInit(string sCypher, ref ErrorCode err)
        {
            strCypher = sCypher;
            op = OperationJudge(strCypher);
            switch (op)
            {
                case CypherOperation.Query:
                    QueryInit(ref err);
                    break;
                case CypherOperation.Create:
                    CreateInit(ref err);
                    break;
                default:
                    err = ErrorCode.CypherInvalid;
                    return;
            }
            if (err != ErrorCode.NoError)
            {
                return;
            }
            err = ErrorCode.NoError;
            return;
        }
        
        //START node(*) MATCH a-[:Rule]->b<-[:Connect 5..5]-c WHERE * RETURN b.Name
        //判断Cypher语句的操作，只检查关键字顺序，不检查各子句具体语法
        CypherOperation OperationJudge(string sCypher)
        {
            const string strQueryPattern = @"START[\s\S]*MATCH[\s\S]*WHERE[\s\S]*RETURN[\s\S]*";  //匹配目标"名称+取值"组合
            const string strCreatePattern = @"CREATE[\s\S]*";  //匹配目标"名称+取值"组合
            MatchCollection matches;
            Regex regObj;

            regObj = new Regex(strQueryPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sCypher);//正则表达式对分词结果进行匹配
            if (matches.Count > 0)
            {
                return CypherOperation.Query;
            }
            regObj = new Regex(strCreatePattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sCypher);//正则表达式对分词结果进行匹配
            if (matches.Count > 0)
            {
                return CypherOperation.Create;
            }
            return CypherOperation.Invalid;
        }
        
        //查询操作结构体初始化
        void QueryInit(ref ErrorCode err)
        {
            const string strStartPattern = @"START[\s\S]*MATCH";
            const string strMatchPattern = @"MATCH[\s\S]*WHERE";
            const string strWherePattern = @"WHERE[\s\S]*RETURN";
            const string strReturnPattern = @"RETURN[\s\S]*";  
            MatchCollection matches;
            Regex regObj;
            string subString;

            //START子句
            regObj = new Regex(strStartPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strCypher);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                err = ErrorCode.StartSegInvalid;
                return;
            }
            subString = matches[0].Value;
            subString = subString.Replace("START", "");
            subString = subString.Replace("MATCH", "");
            sRule = new StartRule(subString.Trim(), ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            //MATCH子句
            regObj = new Regex(strMatchPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strCypher);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                err = ErrorCode.MatchSegInvalid;
                return;
            }
            subString = matches[0].Value;
            subString = subString.Replace("MATCH", "");
            subString = subString.Replace("WHERE", "");
            MatchInit(subString.Trim(), ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            //WHERE子句
            regObj = new Regex(strWherePattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strCypher);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                err = ErrorCode.WhereSegInvalid;
                return;
            }
            subString = matches[0].Value;
            subString = subString.Replace("WHERE", "");
            subString = subString.Replace("RETURN", "");
            FilterInit(subString.Trim());
            
            //RETURN子句
            regObj = new Regex(strReturnPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strCypher);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                err = ErrorCode.ReturnSegInvalid;
                return;
            }
            subString = matches[0].Value;
            subString = subString.Replace("RETURN", "");
            ReturnInit(subString.Trim(), ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            err = ErrorCode.NoError;
            return;
        }

        //查找条件规则和中间变量列表初始化
        void MatchInit(string strSub, ref ErrorCode err)
        {
            //(Kingdom)-[:Rule]->(District)<-[:Connect 5..5]-(Neibhour)
            const string strNodePattern = @"\([\w]+\)";
            List<string> strNode;
            List<string> strEdge;
            MatchRule curRule;
            MatchCollection matches;
            Regex regObj;
            string subString;
            int intLevel = 0;

            regObj = new Regex(strNodePattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count < 2)
            {
                err = ErrorCode.MatchSegInvalid;
                return;
            }
            midRes = new List<MidResult>();
            strNode = new List<string>();
            foreach (Match match in matches)//遍历匹配列表
            {
                subString = match.Value;
                midRes.Add(new MidResult(subString.Replace("(","").Replace(")", ""), intLevel));
                strNode.Add(subString);
                intLevel++;
            }
            mRule = new List<MatchRule>();
            strEdge = SplitSub(strSub, strNode);
            foreach (string sSub in strEdge)//遍历匹配列表
            {
                curRule = new MatchRule(sSub.Trim(), ref err);
                if (err != ErrorCode.NoError)
                {
                    err = ErrorCode.NoError;
                    continue;
                }
                mRule.Add(curRule);
            }
            if (mRule.Count != strNode.Count - 1)
            {//规则数不等于总监变量数-1
                err = ErrorCode.MatchSegInvalid;
                return;
            }
            err = ErrorCode.NoError;
            return;
        }

        List<string> SplitSub(string sMatch, List<string> sNode)
        {
            List<string> strEdge;
            string strStart, strEnd;
            int iStart, iEnd;

            strEdge = new List<string>();
            for (int i = 0; i < sNode.Count - 1; i++)
            {
                strStart = sNode.ElementAt(i);
                strEnd = sNode.ElementAt(i+1);
                iStart = sMatch.IndexOf(strStart)+strStart.Length;
                iEnd = sMatch.IndexOf(strEnd) - iStart;
                strEdge.Add(sMatch.Substring(iStart, iEnd).Trim());
            }
            return strEdge;
        }

        //结果过滤条件初始化
        void FilterInit(string strSub)
        {
            const string strConditionPattern = @"[\w]+\.[\w]+[\s]*(==|!=|>=|>|<=|<)[\s]*[\w]+";
            MatchCollection matches;
            Regex regObj;
            FilterRule curRule;
            ErrorCode err = ErrorCode.NoError;

            fRule = new List<FilterRule>();
            if (strSub == "*")
            {
                return;
            }
            regObj = new Regex(strConditionPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count < 1)
            {
                return;
            }
            foreach (Match match in matches)//遍历匹配列表
            {
                curRule = new FilterRule(match.Value.Trim(), ref err);
                if (err != ErrorCode.NoError)
                {
                    continue;
                }
                fRule.Add(curRule);
            }
            return;
        }

        //返回值初始化
        void ReturnInit(string strSub, ref ErrorCode err)
        {
            const string strConditionPattern = @"[\w]+\.[*\w]+";
            MatchCollection matches;
            Regex regObj;
            string[] strSeg;

            if (strSub == "*")
            {
                foreach (MidResult mr in midRes)
                {
                    mr.SetResult();
                    mr.SetAll();
                }
                err = ErrorCode.NoError;
                return;
            }
            regObj = new Regex(strConditionPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count < 1)
            {
                err = ErrorCode.ReturnSegInvalid;
                return;
            }
            foreach (Match match in matches)//遍历匹配列表
            {
                strSeg = match.Value.Split(new char[] { '.' });
                if (strSeg.Length != 2)
                {
                    continue;
                }
                foreach (MidResult mr in midRes)
                {
                    if (mr.Name == strSeg[0].Trim())
                    {
                        mr.SetResult();
                        if (strSeg[1].Trim() == "*")
                        {
                            mr.SetAll();
                            continue;
                        }
                        mr.AddProperty(strSeg[1].Trim());
                    }
                }
            }
            err = ErrorCode.NoError;
            return;
        }

        //创建操作结构体初始化
        void CreateInit(ref ErrorCode err)
        {

        }

        //初始节点挑选函数
        void SelectStarter(Graph graph, ref ErrorCode err)
        {
            Starter = new List<Node>();
            foreach (Node curNode in graph.Nodes)
            {
                if (sRule.Match(curNode) == true)
                {
                    Starter.Add(curNode);
                }
            }
            if (Starter.Count == 0)
            {
                err = ErrorCode.NoStartNode;
                return;
            }
            err = ErrorCode.NoError;
            return; 
        }

        //查询函数
        void Query()
        {
            TreeNode newTN;

            ResultTree = new List<TreeNode>();
            foreach (Node curNode in Starter)
            {
                newTN = curNode.Search(mRule, 0);
                if (newTN != null)
                {
                    ResultTree.Add(newTN);
                }
            }
        }

        //过滤函数
        void FiltrateResult(Graph graph)
        {
            List<Node> resList;
            foreach (FilterRule fr in fRule)
            {
                foreach (MidResult mr in midRes)
                {
                    if (mr.Name != fr.Name)
                    {
                        continue;
                    }
                    resList = GetTreeNode(graph, mr.Level);
                    foreach (Node curNode in resList)
                    {
                        if (fr.Filtrate(curNode) == true)
                        {
                            continue;
                        }
                        RemoveTreeNode(curNode.Number.ToString(), mr.Level);
                    }
                }
            }
            
        }

        //结果输出函数
        string ResultOutput(Graph graph)
        {
            string strResult = "";
            foreach(MidResult mr in midRes)
            {
                if (mr.IsFinalResult == false)
                {
                    continue;
                }
                strResult += DataFormat(GetTreeNode(graph, mr.Level, bolDistinct), mr);
            }
            return strResult;
        }

        //组织输出结果
        string DataFormat(List<Node> resList, MidResult mr)
        {
            string strResult = "";

            strResult += mr.Name+":\n";
            if(resList.Count < 1)
            {
                strResult += "No Result\n\n";
                return strResult;
            }
            if (mr.IsAll == true)
            {
                strResult += resList[0].FieldOutputAll();
            }
            else
            {
                strResult += resList[0].FieldOutput(mr.Label);
            }
            foreach (Node curNode in resList)
            {
                if (mr.IsAll == true)
                {
                    strResult += curNode.DataOutputAll();
                }
                else
                {
                    strResult += curNode.DataOutput(mr.Label);
                }
            }
            strResult += "\n";
            return strResult;
        }

        //从图中获取结果树中指定的节点
        List<Node> GetTreeNode(Graph graph, int iLevel, bool bDistinct = false)
        {
            List<Node> resList = new List<Node>();
            List<int> IndexList = new List<int>();

            foreach(TreeNode ctn in ResultTree)
            {
                FindNodeByLevel(ctn, iLevel, ref IndexList);
            }
            if (bDistinct == true)
            {
                IndexList = ClearRepeat(IndexList);
            }
            foreach (int iNum in IndexList)
            {
                resList.Add(graph.GetNodeByIndex(iNum));
            }
            return resList;
        }

        List<int> ClearRepeat(List<int> IndexList)
        {
            List<int> NewList = new List<int>();

            foreach (int iNum in IndexList)
            {
                if (NewList.IndexOf(iNum) < 0)
                {
                    NewList.Add(iNum);
                }
            }
            return NewList;
        }

        //获取结果树中某一层的所有节点
        void FindNodeByLevel(TreeNode ctn, int iLevel, ref List<int> IndexList)
        {
            if (ctn.Level == iLevel)
            {
                IndexList.Add(Convert.ToInt32(ctn.Text));
                return;
            }
            foreach(TreeNode chtn in ctn.Nodes)
            {
                FindNodeByLevel(chtn, iLevel, ref IndexList);
            }
            return;
        }

        //移除树节点
        void RemoveTreeNode(string strText, int iLevel)
        {
            TreeNode tarNode = null, parNode;
            foreach (TreeNode ctn in ResultTree)
            {
                tarNode = FindNodeByText(ctn, strText, 0, iLevel);
                if (tarNode != null)
                {
                    break;
                }
            }
            if (tarNode == null)
            {
                return;
            }
            do
            {
                if (tarNode.Parent == null)
                {
                    ResultTree.Remove(tarNode);
                    return;
                }
                parNode = tarNode.Parent;
                parNode.Nodes.Remove(tarNode);
                tarNode = parNode;
            } while (parNode.Nodes.Count == 0);
            return;
        }

        //根据编号查找树中节点
        TreeNode FindNodeByText(TreeNode ctn, string strText, int curLevel, int iLevel)
        {
            TreeNode tarNode;
            if (curLevel == iLevel)
            {
                if (ctn.Text == strText)
                {
                    return ctn;
                }
            }
            foreach (TreeNode chtn in ctn.Nodes)
            {
                tarNode = FindNodeByText(chtn, strText, curLevel+1, iLevel);
                if (tarNode != null)
                {
                    return tarNode;
                }
            }
            return null;
        }

    }
}
