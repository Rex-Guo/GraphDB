using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        string strCypher;
        CypherOperation op;
        MidResult midRes;
        StartRule sRule;
        MatchRule mRule;
        FilterRule fRule;

        //构造函数
        public void QueryExecute(ref Graph graph, string sCypher, ref ErrorCode err)
        {
            CypherInit(sCypher, ref err);
            if (err != ErrorCode.NoError)
            {
                return;
            }
            //从图中选取节点，startRule

            //根据matchRule查找，将返回数据存入记录树

            //过滤记录树，依据FilterRule

            //返回结果。依据midResult中的标记
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
            //sRule = new FilterRule(subString.Trim(), ref err);

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

        void MatchInit(string strSub, ref ErrorCode err)
        {

        }

        void ReturnInit(string strSub, ref ErrorCode err)
        {
        }

        //创建操作结构体初始化
        void CreateInit(ref ErrorCode err)
        {

        }

    }
}
