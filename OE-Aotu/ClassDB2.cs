using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Data.Odbc;

namespace OE_Aotu
{
    class ClassDB2
    {
        //ODBC连接
        string i="0"; //用于返回查询结果0：不是配套；1是配套
        string conString = "DSN=ExcelDB2;" +
                            "UID=ZC;" +
                            "PWD=ZC;";
        //"SELECT * FROM ZAGE.GEA0010 WHERE FAC ='102'"; //数据库sql查询语句
        //uf_db最终结果
        //SELECT TUGGRD FROM ZALT.LTA2032 WHERE BARCOD ='B6031124093' and 
        //TUGGRD = 'A' and DBGRD = 'A'
        public string scBarco(string barcode)
        {
            string sql = "SELECT TUGGRD,DBGRD FROM ZALT.LTA2032 WHERE BARCOD ='" + barcode + "' and TUGGRD = '1' and DBGRD = '1'";
            OdbcConnection con = new OdbcConnection(conString);
            try
            {
                con.Open();
                OdbcCommand com = new OdbcCommand(sql, con);
                //int i = Convert.ToInt32(com.ExecuteScalar());
                i = Convert.ToString(com.ExecuteScalar());
                Console.WriteLine(i);
            }
            //catch
            //{
            //    //throw new Exception(ex.Message);
            //    i = 0; //报错时设置为0
            //}
            finally
            {
                con.Close();
            }
            return i;

        }
    }
}
