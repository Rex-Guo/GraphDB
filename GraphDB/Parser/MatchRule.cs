using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GraphDB.Parser
{
    public class MatchRule
    {
        string strDirection; //"IN","OUT"
	    bool isAll;
	    List<string> labels;//:label1|:label2
	    bool NumLimit;//
	    int uplimit;
	    int downlimit;

        public string Direction
        {
            get
            {
                return strDirection;
            }
        }


        public MatchRule(string sSub, ref ErrorCode err)
	    {
            const string strConditionPattern = @"\[[\s\S][^\[\]]+\]";
            const string strLabelPattern = @":[\w]+";
            const string strLimitPattern = @"[\d]+\.\.[\d]+";
            MatchCollection matches;
            Regex regObj;
            string strCondition;
		    //确定方向<在头为in，>在尾为out
            if (sSub.First<char>() == '<')
            {
                this.strDirection = "IN";
            }
            else if (sSub.Last<char>() == '>')
            {
                this.strDirection = "OUT";
            }
            else
            {
                err = ErrorCode.MatchSegInvalid;
                return;
            }
            regObj = new Regex(strConditionPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sSub);//正则表达式对分词结果进行匹配
            if (matches.Count != 1)
            {
                err = ErrorCode.MatchSegInvalid;
                return;
            }
            strCondition = matches[0].Value;
            //提取"\d+\.\.\d+"分别设置上下限
            regObj = new Regex(strLimitPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sSub);//正则表达式对分词结果进行匹配
            if (matches.Count > 0)
            {
                this.NumLimit = true;
                BuildMatchLimit(matches[0].Value);
                
            }
            //查找*，找到则label
            this.isAll = false;
            if (strCondition.Contains(":*") == true)
            {
                this.isAll = true;
            }
		    //提取":[\w]+",去除：就是规则标签，加入List
            regObj = new Regex(strLabelPattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(sSub);//正则表达式对分词结果进行匹配
            if (matches.Count == 0)
            {
                this.isAll = true;
            }
            else
            {
                labels = new List<string>();
                foreach (Match match in matches)//遍历匹配列表
                {
                    labels.Add(match.Value.Replace(":",""));
                }
            }
            err = ErrorCode.NoError;
            return;
	    }

        //设定上下限
        void BuildMatchLimit(string strLimit)
        {
            const string strSinglePattern = @"[\d]+";
            MatchCollection matches;
            Regex regObj;
            int tmp;

            regObj = new Regex(strSinglePattern);//正则表达式初始化，载入匹配模式
            matches = regObj.Matches(strLimit);//正则表达式对分词结果进行匹配
            if (matches.Count != 2)
            {
                this.NumLimit = false;
            }
            this.uplimit = Convert.ToInt32(matches[0].Value);
            this.downlimit = Convert.ToInt32(matches[1].Value);
            if (this.uplimit < this.downlimit)
            {
                tmp = this.uplimit;
                this.uplimit = this.downlimit;
                this.downlimit = tmp;
            }
        }
	
	    public bool MatchType(string sType)
	    {
            if (this.isAll == true)
		    {//没有设置匹配类型，则直接返回true
			    return true;
		    }
		    foreach(string strLabel in labels)
		    {//只要有一个相同为True
			    if(strLabel == sType)
			    {
				    return true;
			    }
		    }
		    //完全没有匹配上的返回false
		    return false;
	    }
	
	    public bool MatchCount(int iNodes)
	    {
		    //默认>0为true
            if (iNodes <= 0)
            {
                return false;
            }
            if (NumLimit == false)
            {
                return true;
            }
            if (downlimit <= iNodes && iNodes <= uplimit)
            {
                return true;
            }
		    return false;
	    }
    }
}
