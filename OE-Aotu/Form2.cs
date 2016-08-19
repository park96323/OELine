using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OE_Aotu
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        ClassSQL qq = new ClassSQL();
        string time1, time2, query;
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //string rqi = dateTimePicker1.Value.Date.ToString("yyyy-MM-dd");
            //string ss = "select * from countNormalTire where 日期='" + rqi + "' ORDER BY 数量 DESC";
            //qq.showDataSet(ss, dataGridView1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
             time1 = dateTimePicker1.Value.ToString("yyyy/MM/dd HH:mm:ss");
             time2 = dateTimePicker2.Value.ToString("yyyy/MM/dd HH:mm:ss");

            query = "select 规格,count(1) as 数量 from normalTire where 日期时间 between '" + time1 + "' and '" + time2 + "' group by 规格";
            qq.showDataSet(query, dataGridView1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            time1 = dateTimePicker1.Value.ToString("yyyy/MM/dd HH:mm:ss");
            time2 = dateTimePicker2.Value.ToString("yyyy/MM/dd HH:mm:ss");

            query = "select 规格 as 异常,count(1) as 数量 from abnormalTire where 日期时间 between '" + time1 + "' and '" + time2 + "' group by 规格";
            qq.showDataSet(query, dataGridView1);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void 日常统计_Enter(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string time3 = DateTime.Now.ToFileTime().ToString();
            ExportExcel(time3, dataGridView1);
        }
        public static void ExportExcel(string fileName, DataGridView myDGV)
        {
            if (myDGV.Rows.Count > 0)
            {

                string saveFileName = "";
                //bool fileSaved = false;
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel文件|*.xlsx";
                saveDialog.FileName = fileName;
                saveDialog.ShowDialog();
                saveFileName = saveDialog.FileName;
                if (saveFileName.IndexOf(":") < 0) return; //被点了取消 
                Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                if (xlApp == null)
                {
                    MessageBox.Show("无法创建Excel对象，可能您的机子未安装Excel");
                    return;
                }

                Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
                Microsoft.Office.Interop.Excel.Workbook workbook = workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];//取得sheet1

                //写入标题
                for (int i = 0; i < myDGV.ColumnCount; i++)
                {
                    worksheet.Cells[1, i + 1] = myDGV.Columns[i].HeaderText;
                }
                //写入数值
                for (int r = 0; r < myDGV.Rows.Count; r++)
                {
                    for (int i = 0; i < myDGV.ColumnCount; i++)
                    {
                        worksheet.Cells[r + 2, i + 1] = myDGV.Rows[r].Cells[i].Value;
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
                worksheet.Columns.EntireColumn.AutoFit();//列宽自适应
                //if (Microsoft.Office.Interop.cmbxType.Text != "Notification")
                //{
                //    Excel.Range rg = worksheet.get_Range(worksheet.Cells[2, 2], worksheet.Cells[ds.Tables[0].Rows.Count + 1, 2]);
                //    rg.NumberFormat = "00000000";
                //}

                if (saveFileName != "")
                {
                    try
                    {
                        workbook.Saved = true;
                        workbook.SaveCopyAs(saveFileName);
                        //fileSaved = true;
                    }
                    catch (Exception ex)
                    {
                        //fileSaved = false;
                        MessageBox.Show("导出文件时出错,文件可能正被打开！\n" + ex.Message);
                    }

                }
                //else
                //{
                //    fileSaved = false;
                //}
                xlApp.Quit();
                GC.Collect();//强行销毁 
                // if (fileSaved && System.IO.File.Exists(saveFileName)) System.Diagnostics.Process.Start(saveFileName); //打开EXCEL
                MessageBox.Show(fileName + "的简明资料保存成功", "提示", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("报表为空,无表格需要导出", "提示", MessageBoxButtons.OK);
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            query = "select * from normalTire where 条码='" + textBox1.Text.Trim() + "'";
          
            qq.showDataSet(query, dataGridView1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            time1 = dateTimePicker1.Value.ToString("yyyy/MM/dd HH:mm:ss");
            time2 = dateTimePicker2.Value.ToString("yyyy/MM/dd HH:mm:ss");
            query = "select * from normalTire where 日期时间 between '" + time1 + "' and '" + time2 + "'";
            qq.showDataSet(query, dataGridView1);
        }
    }
}
