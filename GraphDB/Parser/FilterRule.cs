using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GraphDB.Parser
{
    public class FilterRule
    {
        string strName;
	    string strProperty;
	    string op;
	    string strValue;

        public FilterRule(string strSub, ref ErrorCode err)
        {
            const string strWordPattern = @"[\w]+";
            const string strOpPattern = @"(==|!=|>=|>|<=|<)";
            MatchCollection matches;
            Regex regObj;

            regObj = new Regex(strWordPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count != 3)
            {
                err = ErrorCode.WhereSegInvalid;
                return;
            }
            strName = matches[0].Value.Trim();
            strProperty = matches[1].Value.Trim();
            strValue = matches[2].Value.Trim();

            regObj = new Regex(strOpPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strSub);//正则表达式对分词结果进行匹配
            if (matches.Count != 1)
            {
                err = ErrorCode.WhereSegInvalid;
                return;
            }
            op = matches[0].Value.Trim();
            err = ErrorCode.NoError;
            return;
        }
    }
}
