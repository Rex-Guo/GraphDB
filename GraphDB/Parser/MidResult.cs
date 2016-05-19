using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphDB.Parser
{
    public class MidResult
    {
        string strName;//名称，由MATCH语句指定
        int intLevel;//层次，处于树状图的深度，由MATCH语句指定
        bool bolIsFinalResult;//由RETURN语句指定，如果为true，则strProperty被初始化
        bool bolIsAll;
        List<string> strProperty;//由RETURN语句指定，待输出的内容标签

        //属性
        public string Name
        {
            get
            {
                return strName;
            }
        }
        public int Level
        {
            get
            {
                return intLevel;
            }
        }
        public bool IsFinalResult
        {
            get
            {
                return bolIsFinalResult;
            }
        }
        public bool IsAll
        {
            get
            {
                return bolIsAll;
            }
        }
        public List<string> Label
        {
            get
            {
                return strProperty;
            }
        }

        //函数
        public MidResult(string sName, int iLevel)
        {
            strName = sName;
            intLevel = iLevel;
            bolIsFinalResult = false;
            strProperty = new List<string>();
        }

        public void SetResult()
        {
            bolIsFinalResult = true;
        }

        public void SetAll()
        {
            bolIsAll = true;
        }

        public void AddProperty(string sLabel)
        {
            strProperty.Add(sLabel);
        }
    }
}
