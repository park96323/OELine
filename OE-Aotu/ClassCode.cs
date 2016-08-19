using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OE_Aotu
{
    delegate void Hangler();
    class ClassCode
    {
        public event Hangler ReadDataBase;
        public event Hangler NoReadDataBase;
        public event Hangler StopRun;
        private string strcode = "";
        public string StrCode
        {
            get
            {
                return strcode;
            }
            set
            {
                strcode = value;
                if (strcode.Length >9)//判断长度
                {
                    ReadDataBase();
                }
                else
                {
                    NoReadDataBase();
                }

            }
        }

      
        //true 表示错误
        private int scanState =2;
        public int  ScanState
        {
            get{return scanState ;}
            set
            {
                scanState = value;
                if (StopState == true && scanState !=2)
                {
                    StopRun();
                }
            }
        }

       
        public int  ActState
        {
            get;
            set;
        }



        public bool  bScanState
        {
            get;
            set;
        }


        public bool bActState
        {
            get;
            set;
        }
        public bool StopState
        {
            get;
            set;
        }

    }
}
