using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GraphDB.Core;

namespace GraphDB.Parser
{
    public class FilterRule
    {
        string strName;
	    string strProperty;
	    string strOp;
	    string strValue;

        public string Name
        {
            get
            {
                return strName;
            }
        }

        public string Property
        {
            get
            {
                return strProperty;
            }
        }

        public string Op
        {
            get
            {
                return strOp;
            }
        }
        
        public string Value
        {
            get
            {
                return strValue;
            }
        }

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
            strOp = matches[0].Value.Trim();
            err = ErrorCode.NoError;
            return;
        }

        public bool Filtrate(Node curNode)
        {
            double dubRuleValue, dubNpValue;

            if (strProperty == "Name")
            {
                switch (strOp)
                {
                    case "==":
                        if (strValue == curNode.Name)
                        {
                            return true;
                        }
                        break;
                    case "!=":
                        if (strValue != curNode.Name)
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }
                return false;
            }
            if (strProperty == "Type")
            {
                switch (strOp)
                {
                    case "==":
                        if (strValue == curNode.Type)
                        {
                            return true;
                        }
                        break;
                    case "!=":
                        if (strValue != curNode.Type)
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }
                return false;
            }
            foreach (NodeProperty np in curNode.Properties)
            {
                if (strProperty != np.Key)
                {
                    continue;
                }
                if (strOp == "==" || strOp == "!=")
                {
                    switch (strOp)
                    {
                        case "==":
                            if (strValue == np.Value)
                            {
                                return true;
                            }
                            break;
                        case "!=":
                            if (strValue != np.Value)
                            {
                                return true;
                            }
                            break;
                        default:
                            break;
                    }
                    continue;
                }
                if ((double.TryParse(strValue, out dubRuleValue) == false) || (double.TryParse(np.Value, out dubNpValue) == false))
                {
                    return false;
                }
                switch (strOp)
                {
                    case ">=":
                        if (dubNpValue >= dubRuleValue)
                        {
                            return true;
                        }
                        break;
                    case ">":
                        if (dubNpValue > dubRuleValue)
                        {
                            return true;
                        }
                        break;
                    case "<=":
                        if (dubNpValue <= dubRuleValue)
                        {
                            return true;
                        }
                        break;
                    case "<":
                        if (dubNpValue < dubRuleValue)
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }
            }
            return false;
        }

    }
}
