using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OE_Aotu.ServiceReference1 ;
using OE_Aotu.Properties;
using OPCAutomation;
using System.Collections;
using System.Text.RegularExpressions;


namespace OE_Aotu
{

    public partial class Form1 : Form //分部式类定义, 就是一个类定义在不同的类文件里.
    {
        #region DefinedVariable
        //region + endregion 便于组织代码，将相同功能的代码块包含其中，可以展开或关闭
        Socket server;
        Thread oThreadServer, oThreadWait;
        ClassCode p;
        ServiceSoapClient serSoap;
        RetValue retDataBase;
        Queue myQ;
        #endregion

        #region DefinedVariableOPC

        OPCServer objServer;
        OPCGroups objGroups;
        OPCGroup objGroup;
        OPCItems objItems;
        Array strItemIDs;
        Array lClientHandles;
        Array lserverhandles;
        Array lErrors;
        Array lvalue;
        object RequestedDataTypes = null;
        object AccessPaths = null;
        Array lErrors_Wt;
        int lTransID_Wt = 2;
        int lCancelID_Wt;


        //*异步写用到的参数*//
        Array AsyncValue_Wt;
        Array SerHandles;
        object[] tmpWtData = new object[3];//写入的数据必须是object型的，否则会报错
        int[] tmpSerHdles = new int[3];

        object[] aaa = new object[10];
        static object locker = new object();
        int c1, c2;
        #endregion

        #region DefinedVariableSQL
        string[] strSQL = new string[13];  //SQL语句
        ClassSQL q;
        ClassSQL q1;
        ClassDB2 dbufLevel;
        public DateTime fist_time, last_time;

        DateTime t1, t2;
        TimeSpan t3;

        #endregion

        #region DefinedShow
        int i = 1;
        string[] strTGD = new string[3];
        DateTime workt;
        static int number_jin;
        static int number_chu;
        Regex r1 = new Regex("配套"); // 定义一个Regex对象实例
        Regex r2 = new Regex("OE"); // 定义一个Regex对象实例
        Regex r3 = new Regex("不能重复入暂存区"); // 定义一个Regex对象实例
        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            initializtionWeb(); //群科webservice连接测试
            initializtionOPC(); //连接PLC及初始化
            initializtionSQL();
            initializtion();   //初始化连接扫码器 "线程开始")
            //initializtionSQL2();
            showNumber();
        }


        private void initializtionWeb()
        {
            serSoap = new ServiceSoapClient();
            t1 = System.DateTime.Now;
            retDataBase = serSoap.iptZcqByBarcode("2", "500-1", "B5036356413");
            t2 = System.DateTime.Now;
            t3 = t2 - t1;
            string strShowTime = t3.TotalSeconds.ToString();
            controlShow("初次查询" + strShowTime + "秒", label5);
        }

        //初始化连接扫码器 "线程开始")
        private void initializtion()
        {
            number_jin = 0;
            number_chu = 0;

            oThreadServer = new Thread(new ThreadStart(readCode));
            p = new ClassCode();
            p.ScanState = 3;//ScanState!=0 --> StopRun()
            p.ActState = 0;
            p.bScanState = false;

            p.ReadDataBase += new Hangler(ReadDataBaseMethod);
            p.NoReadDataBase += new Hangler(NoReadDataBaseMethod);
            p.StopRun += new Hangler(StopRunMethod);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.0.175"), 2112);
            //连接扫描设备
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                server.Connect(ipep);
                if (server.Connected)
                {
                    pictureBox1.Image = Resources.greed;
                }
                else
                {
                    pictureBox1.Image = Resources.red;
                    MessageBox.Show("连接扫码失败");
                    return;
                }

            }

            catch (Exception e)
            {
                label1.Text = e.ToString() + "Connect Server Fail";

                return;
            }
            finally
            {


            }

            //MessageBox.Show("线程开始");
            oThreadServer.Start();
            //oThreadWait = new Thread(new ThreadStart(waitCode));
        }
        private void initializtionOPC()
        {

            objServer = new OPCServer();   //连接opc server
            objServer.Connect("Takebishi.Melsec.1", null);    //(2)建立一个opc组集合
            if (objServer.ServerState == (int)OPCServerState.OPCRunning)
            {
                pictureBox2.Image = Resources.greed;
                //MessageBox.Show("OPC is OK");
            }
            else
            {
                pictureBox2.Image = Resources.red;
                return;
            }

            objGroups = objServer.OPCGroups; //(3)建立一个opc组
            objGroup = objGroups.Add(null); //Group组名字可有可无   //(4)添加opc标签         
            objGroup.IsActive = true; //设置该组为活动状态，连接PLC时，设置为非活动状态也一样
            objGroup.IsSubscribed = true; //设置异步通知
            objGroup.UpdateRate = 100;
            objServer.OPCGroups.DefaultGroupDeadband = 0;
            objGroup.DataChange += new DIOPCGroupEvent_DataChangeEventHandler(KepGroup_DataChange);
            // objGroup.AsyncReadComplete += new DIOPCGroupEvent_AsyncReadCompleteEventHandler(AsyncReadComplete);
            //objGroup.AsyncWriteComplete += new DIOPCGroupEvent_AsyncWriteCompleteEventHandler(AsyncWriteComplete);
            objItems = objGroup.OPCItems; //建立opc标签集合

            string[] tmpIDs = new string[8];
            int[] tmpCHandles = new int[8];
            for (int i = 1; i < 8; i++)
            {
                tmpCHandles[i] = i;
            }
            tmpIDs[1] = "Device1.X2C"; //进ACT工位 光眼X222 下降沿 
            tmpIDs[2] = "Device1.X26";  //进ACT中间工位 光眼X222 下降沿
            tmpIDs[3] = "Device1.M666";          //错误信号
            tmpIDs[4] = "Device1.M667";   //机器停止
            tmpIDs[5] = "Device1.X27";   //扫码光眼
            tmpIDs[6] = "Device1.M668";  //启动停止
            tmpIDs[7] = "Device1.X25";  //启动停止

            strItemIDs = (Array)tmpIDs;//必须转成Array型，否则不能调用AddItems方法
            lClientHandles = (Array)tmpCHandles;
            // 添加opc标签
            objItems.AddItems(7, ref strItemIDs, ref lClientHandles, out lserverhandles, out lErrors, RequestedDataTypes, AccessPaths);

            //AsyncWrite(6, 1);  //让机器启动
            AsyncWrite(4, 0);  //让机器启动


        }
        private void initializtionSQL()
        {
            q1 = new ClassSQL();
            myQ = new Queue();
            dbufLevel = new ClassDB2();
        }

        //private void   initializtionSQL2()
        //{
        //    strSQL[7] = "provider=microsoft.jet.oledb.4.0;data source=" + System.Environment.CurrentDirectory + "\\OE_Auto.mdb;Jet OLEDB:Database Password=Fearandgreed;";
        //    strSQL[8] = "select  * from 表1 where ID=1 ";
        //    ReadTradTime(strSQL[7].ToString(), strSQL[8].ToString());
        //    int day2 = (last_time - fist_time).Days;

        //    if (day2 < 0)
        //    {
        //       // MessageBox.Show(" 紝璁╃煡璇嗗厖婊＄瓒ｃ€�", "Error 561", MessageBoxButtons.OKCancel, MessageBoxIcon.Error );
        //        //this.Close();
        //        Application.Exit();

        //    }
        //}
        //  public void ReadTradTime(string strConn, string strSql)
        //  {
        //      OleDbCommand cmd, cmd1;
        //      OleDbConnection conn = new OleDbConnection(strConn);


        //      int days;
        //      //DateTime rr;
        //      try
        //      {
        //          conn.Open();
        //          cmd = new OleDbCommand(strSql, conn);
        //          OleDbDataReader sdr = cmd.ExecuteReader();

        //          while (sdr.Read())
        //          {

        //              fist_time = (DateTime)sdr[1];
        //              last_time = (DateTime)sdr[2];
        //              days = (int)sdr[3];


        //          }
        //          DateTime now_time = DateTime.Now;
        //          int day1 = (now_time - fist_time).Days;
        //          if (day1 < 0)
        //          {
        //              fist_time = fist_time.AddDays(1);

        //          }
        //          else
        //          {
        //              fist_time = now_time;
        //          }


        //          string strSQL1 = "update 表1 set fist_time= #" + fist_time + "# where ID=1 ";
        //          cmd1 = new OleDbCommand(strSQL1, conn);
        //          cmd1.ExecuteNonQuery();

        //      }
        //      catch (Exception ex)
        //      {

        //          throw new Exception(ex.Message);
        //      }
        //      finally
        //      {

        //          conn.Dispose();
        //          //conn.Close();
        //      }

        //  }
        public void KepGroup_DataChange(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {

            //tmpIDs[1] = "Device1.X2C"; //进ACT工位 光眼X222 下降沿 
            //tmpIDs[2] = "Device1.X26";  //进ACT中间工位 光眼X222 下降沿
            //tmpIDs[3] = "Device1.M666";          //错误信号
            //tmpIDs[4] = "Device1.M667";   //机器停止
            //tmpIDs[5] = "Device1.X27";   //扫码光眼
            //tmpIDs[6] = "Device1.M668";  //启动停止

            for (int i = 0; i < NumItems; i++)
            {
                #region x2c
                if (Convert.ToInt32(ClientHandles.GetValue(i + 1)) == 1)
                {
                    if (p.bScanState == true)
                    {
                        //if (ItemValues.GetValue(i + 1) != null && p.bScanState == true)
                        if (ItemValues.GetValue(i + 1) != null)
                        {
                            if (Convert.ToInt32(ItemValues.GetValue(i + 1)) == 1)  //进胎  x2c
                            {
                                lock (locker) { c1 = p.ScanState; }
                                if (c1 == 2)
                                {
                                    AsyncWrite(4, 1);  //让机器停
                                    p.StopState = true;
                                }

                                else
                                {
                                    p.ActState = p.ScanState;



                                }
                                p.bScanState = false;
                                p.bActState = true;

                            }

                        }

                    }

                }
                #endregion
                #region x26
                if (Convert.ToInt32(ClientHandles.GetValue(i + 1)) == 2) //ACT工位 光眼X222 下降沿 x26
                {

                    if (ItemValues.GetValue(i + 1) != null)
                    {
                        if (Convert.ToInt32(ItemValues.GetValue(i + 1)) == 0)  //进胎
                        {
                            if (p.bActState == true)
                            {
                                p.bActState = false;
                                p.ScanState = 2;
                                AsyncWrite(3, p.ActState);

                            }
                        }
                        else                 //出胎
                        {

                            AsyncWrite(3, 0);
                            p.ActState = 0;
                            controlShow(" ", scanBarcod);
                            controlShow(" ", sanSize);
                            controlShow(" ", textBox5);
                            //p.bActState = false;

                        }
                    }

                }
                #endregion

                #region 扫码
                if (Convert.ToInt32(ClientHandles.GetValue(i + 1)) == 5)
                {
                    if (ItemValues.GetValue(i + 1) != null)
                    {
                        if (Convert.ToInt32(ItemValues.GetValue(i + 1)) == 1)  //出胎
                        {
                            p.bScanState = true;
                        }
                    }
                }
                #endregion

                #region 数量不一致
                if (Convert.ToInt32(ClientHandles.GetValue(i + 1)) == 7)  //x25
                {
                    if (ItemValues.GetValue(i + 1) != null)
                    {
                        if (Convert.ToInt32(ItemValues.GetValue(i + 1)) == 1)
                        {
                            number_chu += 1;
                            if (number_chu > number_jin)
                            {
                                AsyncWrite(6, 1);  //让机器停止
                                controlShow("错误轮胎未打出，确认后启动", scanDataList);
                                number_chu = 0;
                                number_jin = 0;

                            }
                        }
                    }
                }
                #endregion

            }
        }

        public void ShowList()
        {
            if (i >= 30)
            {
                i = 1;
                //listBox1.Text = "";
                controlShow("", scanDataList);
            }
            else
            {
                string str = "";
                this.Invoke((EventHandler)(delegate
                {

                    str = scanDataList.Text;
                }
                ));
                str = i.ToString() + " | " + strTGD[0] + " | " + strTGD[1] + " | " + strTGD[2] + " | " + workt + Environment.NewLine + str;
                controlShow(str, scanDataList);
                i = i + 1;
            }
            strTGD[0] = ""; strTGD[1] = ""; strTGD[2] = "";
            //workt = ;
        }
        public void StopRunMethod()
        {
            p.ActState = p.ScanState;
            AsyncWrite(4, 0);
            p.StopState = false;

        }


        public void AsyncWrite(int y, object X)
        {
            //将输入数据赋给数组，然后再转成Array型送给AsyncValue_Wt
            tmpWtData[1] = X;
            //tmpWtData[2] = (object)"0";
            AsyncValue_Wt = (Array)tmpWtData;
            //将输入数据送给的Item对应服务器句柄赋给数组，然后再转成Array型送给SerHandles
            tmpSerHdles[1] = Convert.ToInt32(lserverhandles.GetValue(y));
            //tmpSerHdles[2] = Convert.ToInt32(lserverhandles.GetValue(4));
            SerHandles = (Array)tmpSerHdles;
            objGroup.AsyncWrite(1, ref SerHandles, ref AsyncValue_Wt, out lErrors_Wt, lTransID_Wt, out lCancelID_Wt);
        }

        public void controlShow(string text, Control control)
        {

            Action<String> AsyncUIDelegate = delegate (string n) { control.Text = n; };
            control.Invoke(AsyncUIDelegate, new object[] { text });
        }

        private void readCode()  //线程读条码
        {
            byte[] data = new byte[128];
            int receive;
            string strBarcode;

            while (true)
            {
                receive = server.Receive(data);
                strBarcode = Encoding.ASCII.GetString(data, 1, receive - 2);//将缓冲区当中的数组，转化为 字符串.
                if (strBarcode != "")
                {
                    p.StrCode = strBarcode;
                }

                Thread.Sleep(50);

            }
        }


        
        public void ReadDataBaseMethod()
        {
            strTGD[0] = p.StrCode;

            string grade;
            string strTrad;
            controlShow(p.StrCode, scanBarcod);
            //t1 = System.DateTime.Now;

            retDataBase = serSoap.iptZcqByBarcode("2", "500-1", p.StrCode);
            //t2 = System.DateTime.Now;
            //t3 = t2 - t1;         
            //string strShowTime = t3.TotalSeconds.ToString();
            //controlShow("查询时间" + strShowTime + "秒", label5);
            grade = dbufLevel.scBarco(p.StrCode);
            controlShow(grade, textBox5);
            string flag = retDataBase.flagField;

            if (flag.Equals("1") && grade.Equals("1"))
            {

                //p.ScanState = false;
                strTrad = retDataBase.param1Field.Trim();

                strTGD[1] = strTrad;
                workt = DateTime.Now;



                Match m1 = r1.Match(strTrad);
                Match m2 = r2.Match(strTrad);
                //if (strTrad.Substring (0,2)== "OE")
                //if (grade == 1)
                //{
                if (m1.Success || m2.Success)
                {

                    strTGD[2] = "不打胎";
                    ShowList();
                    number_jin += 1;

                    lock (locker) { p.ScanState = 0; }
                    controlShow("OK" + strTrad, sanSize);
                    //lock (locker) { myQ.Enqueue(0); }
                    //1.写入正常出库数据库
                    strSQL[0] = "insert into normalTire (规格, 条码 ,日期时间) values('" + strTrad + "','" + p.StrCode + "','" + DateTime.Now + "'   )";
                    q1.IDU(strSQL[0]);

                    //2.写入正常出库计数数据库
                    //strSQL[1] = "select * from countNormalTire where 规格='" + strTrad + "' and 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                    //strSQL[2] = "insert into countNormalTire (规格,数量,日期) values('" + strTrad + "','1','" + DateTime.Now.ToString("yyyy-MM-dd") + "'   )";
                    //strSQL[3] = "UPDATE countNormalTire set 数量 =数量+1 WHERE 规格='" + strTrad + "' and  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                    //q1.UPdate(strSQL[1], strSQL[2], strSQL[3]);

                    strSQL[11] = "UPDATE Table_2 set 数量 =数量+1";
                    q1.IDU(strSQL[11]);
                    //  textBox3.Text = (Convert.ToInt32(textBox3.Text.Trim()) + 1).ToString();
                    controlShow((Convert.ToInt32(checkNumber.Text.Trim()) + 1).ToString(), checkNumber);


                }
                //}
                else  //非OE胎
                {
                    strTGD[2] = "打胎";
                    ShowList();


                    lock (locker) { p.ScanState = 1; }
                    controlShow("打胎" + strTrad, sanSize);
                    //写入异常出库数据库
                    strSQL[6] = "insert into abnormalTire (规格, 条码 ,日期时间) values('" + strTrad + "','" + p.StrCode + "','" + DateTime.Now + "'   )";
                    q1.IDU(strSQL[6]);
                }

            }
            else if (flag.Equals("0"))
            {
                //p.ScanState = true;   
                //lock (locker) { myQ.Enqueue(1); }
                string strMsg = retDataBase.msgField;

                strTGD[1] = strMsg;
                workt = DateTime.Now;
                strTGD[2] = "打胎";
                ShowList();

                Match m3 = r3.Match(strMsg);
                if (m3.Success)
                {
                    lock (locker) { p.ScanState = 0; }
                    controlShow("OK" + strMsg, sanSize);
                    number_jin += 1;
                }
                else
                {
                    lock (locker) { p.ScanState = 1; }
                    controlShow("打胎" + strMsg, sanSize);
                }
                //写入异常出库数据库
                strSQL[4] = "insert into abnormalTire (规格, 条码 ,日期时间) values('" + strMsg + "','" + p.StrCode + "','" + DateTime.Now + "'   )";
                q1.IDU(strSQL[4]);
            }


        }
        public void NoReadDataBaseMethod()
        {
            strTGD[0] = "NoRead"; strTGD[1] = ""; strTGD[2] = "打胎";
            workt = DateTime.Now;
            ShowList();

            string guige = "No Read";
            //lock (locker) { myQ.Enqueue(1); }
            lock (locker) { p.ScanState = 1; }
            controlShow("NoRead", scanBarcod);
            controlShow("打胎+NoRead", sanSize);
            //p.ScanState = true ;

            //写入异常出库数据库
            strSQL[5] = "insert into abnormalTire (规格, 条码 ,日期时间) values('" + guige + "','" + p.StrCode + "','" + DateTime.Now + "'   )";
            q1.IDU(strSQL[5]);


        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //AsyncWrite(6, 0);  //让机器停止
            if (oThreadServer.ThreadState == ThreadState.Running)
            {
                oThreadServer.Abort();
            }
            if (server.Connected)
            {
                server.Close();
            }
            if (objServer.ServerState == (int)OPCServerState.OPCRunning)
            {
                objServer.Disconnect();
            }


        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            strSQL[9] = "UPDATE Table_2 set 数量 =0";
            q1.IDU(strSQL[9]);
            checkNumber.Text = "0";
        }

        private void showNumber()
        {
            strSQL[10] = "select * from Table_2";
            checkNumber.Text = q1.read(strSQL[10]);

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

    }
}