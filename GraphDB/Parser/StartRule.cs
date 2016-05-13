using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GraphDB.Core;

namespace GraphDB.Parser
{
    public class StartRule
    {
        bool isName;//"Index", "NameType"
        bool isAll;
        List<string> subRule;//子规则

        public StartRule(string strSub, ref ErrorCode err)
        {
            //1.正则表达式提取“”,没有则为Index，有则为NameType
            const string strIndexPattern = @"node\(([^\']*)\)";
            const string strNamePattern = @"node\(\'([^\']*)\'\)";
            MatchCollection matches;
            Regex regObj;
            string subString;

            regObj = new Regex(strNamePattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count == 1)
            {
                isName = true;
            }
            else
            {
                regObj = new Regex(strIndexPattern);//正则表达式初始化，载入匹配模式
                matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
                if (matches.Count == 1)
                {
                    isName = false;
                }
                else
                {
                    err = ErrorCode.StartSegInvalid;
                    return;
                }
            }
            subString = matches[0].Value;
            if (isName == true)
            {
                subString = subString.Replace("node('", "");
                subString = subString.Replace("')", "");
            }
            else
            {
                subString = subString.Replace("node(", "");
                subString = subString.Replace(")", "");
            }
            subString.Trim();
            //2.对于Index场景，只有一个*就是全部节点 isAll = true;
            if (subString == "*")
            {
                isAll = true;
                err = ErrorCode.NoError;
                return;
            }
            //3在子句中匹配非负整数“\d+”和非负整数段“\d+\.\.\d+”
            if (isName == false)
            {
                FillListIndex(subString);
                err = ErrorCode.NoError;
                return;
            }
            //4.对于NameType场景，通配符格式为“[\w]+-[\w]+”，“*-[\w]+”，“[\w]+-*”
            FillListName(subString);
            if (subRule.Count == 0)
            {
                isAll = true;
            }
            err = ErrorCode.NoError;
            return;
        }

        void FillListIndex(string sSub)
        {
            string[] strSeg = sSub.Split(new char[] { ',' });
            const string strSinglePattern = @"[\d]+";
            const string strGroupPattern = @"[\d]+\.\.[\d]+";
            MatchCollection matches;
            Regex regObj;

            subRule = new List<string>();
            if (strSeg == null)
            {
                return;
            }
            foreach (string sSeg in strSeg)
            {
                regObj = new Regex(strGroupPattern);//正则表达式初始化，载入匹配模式
                matches = regObj.Matches(sSeg);//正则表达式对分词结果进行匹配
                if (matches.Count == 1)
                {
                    subRule.Add(matches[0].Value.Trim());
                    continue;
                }
                regObj = new Regex(strSinglePattern);//正则表达式初始化，载入匹配模式
                matches = regObj.Matches(sSeg);//正则表达式对分词结果进行匹配
                if (matches.Count == 1)
                {
                    subRule.Add(matches[0].Value.Trim());
                }
            }
        }

        void FillListName(string sSub)
        {
            string[] strSeg = sSub.Split(new char[] { ',' });
            const string strNamePattern = @"[\w\*]+-[\w\*]+";
            MatchCollection matches;
            Regex regObj;

            subRule = new List<string>();
            if (strSeg == null)
            {
                return;
            }
            foreach (string sSeg in strSeg)
            {
                regObj = new Regex(strNamePattern);//正则表达式初始化，载入匹配模式
                matches = regObj.Matches(sSeg);//正则表达式对分词结果进行匹配
                if (matches.Count == 1)
                {
                    subRule.Add(matches[0].Value.Trim());
                }
            }
        }

        public bool Match(Node tarNode)
        {
            if (isAll == true)
            {
                return true;
            }
            if (isName == false)
            {
                foreach (string sRule in subRule)
                {
                    if (MatchNumber(sRule, tarNode.Number) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                foreach (string sRule in subRule)
                {
                    if (MatchNameType(sRule, tarNode.Name, tarNode.Type) == true)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool MatchNumber(string strRule, int index)
        {
            int intUp, intDown, intDot, intTemp;
            string strUp, strDown;

            intDot = strRule.IndexOf("..");
            if (intDot < 0)
            {
                intUp = Convert.ToInt32(strRule);
                if (intUp == index)
                {
                    return true;
                }
                return false;
            }
            strDown = strRule.Remove(intDot);
            strUp = strRule.Replace(strDown + "..", "");

            intDown = Convert.ToInt32(strDown);
            intUp = Convert.ToInt32(strUp);
            if (intUp < intDown)
            {
                intTemp = intUp;
                intUp = intDown;
                intDown = intTemp;
            }
            if (intDown <= index && index <= intUp)
            {
                return true;
            }
            return false;
        }

        bool MatchNameType(string strRule, string sName, string sType)
        {
            string[] strSeg = strRule.Split(new char[]{'-'});
            bool bolName, bolType;

            if (strSeg == null)
            {
                return false;
            }
            bolName = bolType = false;
            if (strSeg[0].Trim() == "*")
            {
                bolName = true;
            }
            else
            {
                if (strSeg[0] == sName)
                {
                    bolName = true;
                }
            }
            if (strSeg[1].Trim() == "*")
            {
                bolType = true;
            }
            else
            {
                if (strSeg[1] == sType)
                {
                    bolType = true;
                }
            }
            if (bolName == true && bolType == true)
            {
                return true;
            }
            return false;
        }
	
    }
}
