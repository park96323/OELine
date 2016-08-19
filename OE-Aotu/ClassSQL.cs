using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace OE_Aotu
{
    //  'Dim connstring As String = "provider=microsoft.jet.oledb.4.0;data source=" & My.Application.Info.DirectoryPath & "\500设备部加班数据库.mdb;"
    //Dim connstring As String = "Server=localhost;Initial Catalog=Northwind;DataBase=zc500;UID=sa;PWD=zc500"

    //'Dim conn As OleDb.OleDbConnection = New OleDb.OleDbConnection(connstring)
    //Dim conn As SqlClient.SqlConnection = New SqlClient.SqlConnection(connstring)
    
    class ClassSQL
       
    {
        SqlCommand cmd, cmd1;
      string   strConn = "Server=localhost;Initial Catalog=Northwind;DataBase=zc500;UID=sa;PWD=zc500";
        //SqlConnection=new SqlConnection (conn);
           public void IDU(string strSql)
        {
         
            SqlConnection conn = new SqlConnection(strConn);
            conn.Open();
            try
            {
            if(conn.State== System.Data.ConnectionState.Open)
            {
                cmd = new SqlCommand(strSql, conn);
                cmd.ExecuteNonQuery();
            }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
              
                conn.Dispose();
                //conn.Close();
            }
        }

           public string read(string strSql)
           {
               SqlConnection conn = new SqlConnection(strConn);
               conn.Open();
               string nub = string.Empty;


               try
               {
                   if (conn.State == System.Data.ConnectionState.Open)
                   {
                       cmd = new SqlCommand(strSql, conn);
                       SqlDataReader sdr = cmd.ExecuteReader();
                       while (sdr.Read())
                       {
                           nub = sdr[0].ToString();
                       }
                       return nub;
                   }
                   else
                   {
                       return "";
                   }
               }
               catch (Exception ex)
               {
                   return ex.Message.ToString();
                   throw new Exception(ex.Message);

               }
               finally
               {

                   conn.Dispose();
                   //conn.Close();
               }
           }
           public void UPdate( string strSql,string strInsert, string strSqlUpdate)
           {
             //  string strConn, string strSql,string strInsert, string strSqlUpdate
               //strConn连接  strSql查询是否有值  
               SqlConnection conn = new SqlConnection(strConn);
               conn.Open();
               try
               {
                   if (conn.State == System.Data.ConnectionState.Open)
                   {
                       cmd = new SqlCommand(strSql, conn);
                       SqlDataReader sdr = cmd.ExecuteReader();
                       sdr.Read();

                       //cmd.ExecuteNonQuery();
                       if (sdr.HasRows )
                       {
                           cmd1 = new SqlCommand(strSqlUpdate, conn);
                       }
                       else
                       {
                           cmd1 = new SqlCommand(strInsert, conn);
                       }
                       sdr.Dispose();
                       cmd1.ExecuteNonQuery();
                   }
               }
               catch (Exception ex)
               {
                   throw new Exception(ex.Message);
               }
               finally
               {
                  
                   conn.Dispose();
                   //conn.Close();
               }
           }

           public void showDataSet(string strSql,DataGridView dg )
           {
              
               SqlConnection conn = new SqlConnection(strConn);
               SqlDataAdapter sda = new SqlDataAdapter();
               DataSet ds = new DataSet();
               DataTable dt =new DataTable ();
               conn.Open();
               try
               {
                   if (conn.State == System.Data.ConnectionState.Open)
                   {
                       cmd = new SqlCommand(strSql, conn);      
                       sda.SelectCommand = cmd;
                       sda.Fill(dt);
                       dg.DataSource = dt;
                       sda.Dispose();
                   }
               }
               catch (Exception ex)
               {
                   throw new Exception(ex.Message);
               }
               finally
               {
                  
                   conn.Dispose();
                   //conn.Close();
               }

           }

    }
}
