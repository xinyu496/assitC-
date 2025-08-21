using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;
using System.Text.RegularExpressions;//正则表达式，加入命名空间。

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        SerialPort sp = null;
        bool isOpen = false;
        bool isSetProperty = false;

        public Form1()
        {
            InitializeComponent();//通过 InitializeComponent()自动加载设计器配置，确保窗体按设计呈现
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            for (int i = 0; i < 100; i++)
            {
                cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxCOMPort.SelectedIndex = 0;
            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 6;

            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;

            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;

            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;

            rbnChar.Checked = true;
            /*添加时间显示*/
            this.toolStripStatusLabel1.Text = "当前时间" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            timer1.Interval = 1000;
            timer1.Start();
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btnCheckCom_Click(object sender, EventArgs e)//检测串口
        {
            bool comExistence = false;
            cbxCOMPort.Items.Clear();

            for (int i = 0; i < 100; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }

                catch (Exception)
                {
                    continue;
                }

                if (comExistence)
                {
                    cbxCOMPort.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("没有找到可用串口！", "错误提示！");
                }
            }




        }
        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxStopBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            return true;
        }
        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            return true;
        }
        private void SetProperty()
        {
            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());
            if (cbxStopBits.Text.Trim() == "0")
            {
                sp.StopBits = StopBits.None;
            }
            else if (cbxStopBits.Text.Trim() == "1.5")
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (cbxStopBits.Text.Trim() == "2")
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());

            if (cbxParity.Text.Trim() == "奇校验")
            {
                sp.Parity = Parity.Odd;

            }
            else if (cbxParity.Text.Trim() == "偶校验")
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;
            sp.RtsEnable = true;

            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs eg)
        {

            System.Threading.Thread.Sleep(100);

            this.Invoke((EventHandler)delegate//异步执行 一个线程
            {
                if (!rbnHex.Checked)//如果未选中name为rbnHex的控件
                {
                    //tbxRecvData.Text += sp.ReadLine();
                    StringBuilder sb = new StringBuilder();
                    long rec_count = 0;
                    int num = sp.BytesToRead;
                    byte[] recbuf = new byte[num];
                    rec_count += num;

                    sp.Read(recbuf, 0, num);
                    sb.Clear();

                    try
                    {
                        Invoke((EventHandler)(delegate
                        {
                            sb.Append(Encoding.ASCII.GetString(recbuf));  //将整个数组解码为ASCII数组
                            tbxRecvData.AppendText(sb.ToString());
                        }
                        )
                        );
                    }

                    catch
                    {
                        MessageBox.Show("请勾选换行", "错误提示");
                    }
                }
                else if (rbnHex.Checked)//如果选中
                {
                    Byte[] ReceivedData = new Byte[sp.BytesToRead];
                    sp.Read(ReceivedData, 0, ReceivedData.Length);

                    String RecvDataText = null;

                    for (int i = 0; i < ReceivedData.Length; i++)
                    {
                        RecvDataText += (ReceivedData[i].ToString("X2") + " ");//数组里接收到的数据转化为16进制
                    }
                    tbxRecvData.Text += RecvDataText;
                }
                sp.DiscardInBuffer();
            });
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())
                {
                    MessageBox.Show("串口未设置", "错误提示");
                    return;
                }
                if (!isSetProperty)
                {
                    SetProperty();
                    isSetProperty = true;
                }
                try
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCom.Text = "关闭串口";
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                }
                catch (Exception)
                {
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用", "错误提示");
                }
            }
            else if (isOpen == true)
            {

                try
                {
                    if (!timeBox3.Checked)
                    {
                        sp.Close();//关闭端口
                        isOpen = false;
                        btnOpenCom.Text = "打开串口";
                        cbxCOMPort.Enabled = true;
                        cbxBaudRate.Enabled = true;
                        cbxDataBits.Enabled = true;
                        cbxParity.Enabled = true;
                        cbxStopBits.Enabled = true;
                        rbnChar.Enabled = true;
                        rbnHex.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("请先关闭自动发送", "错误提示");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭串口时发生错误", "错误提示");
                }
            }
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            if (!timeBox3.Checked)
            {
                tbxRecvData.Text = "";
                tbxSendData.Text = "";
            }
            else
            {
                MessageBox.Show("请先关闭自动发送", "错误提示");
            }
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            byte[] textchar = new byte[1];
            int num2 = 0;
            if (isOpen)
            {
                try
                {
                    if (!checkBox1.Checked)//如果没有选中十六进制发送
                    {
                        if (!checkBox2.Checked)//未选中回车换行
                        {
                            sp.Write(tbxSendData.Text);//串口发送 （发送框里的东西）
                        }
                        else
                        {
                            //sp.WriteLine(tbxSendData.Text);//用这个方法只会在后面加\n没有\r，下面的方法是验证可行的
                            string res = tbxSendData.Text + "\r\n";
                            byte[] byteArray = System.Text.Encoding.Default.GetBytes(res);
                            sp.Write(byteArray, 0, byteArray.Length);
                        }
                    }
                    else//选择十六进制发送的时候
                    {

                        string buf = tbxSendData.Text;
                        string bartenm = @"\s";//正则表达式
                        string replace = "";

                        Regex rgx = new Regex(bartenm);
                        string senddata = rgx.Replace(buf, replace);
                        num2 = (senddata.Length - senddata.Length % 2) / 2;

                        for (int a = 0; a < num2; a++)
                        {
                            textchar[0] = Convert.ToByte(senddata.Substring(a * 2, 2), 16);
                            sp.Write(textchar, 0, 1);
                        }


                        if (senddata.Length % 2 != 0)
                        {
                            textchar[0] = Convert.ToByte(senddata.Substring(tbxSendData.Text.Length - 1, 2), 16);
                            sp.Write(textchar, 0, 1);
                            num2++;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开错误提示！", "错误提示");
            }
            if (!CheckSendData())
            {
                MessageBox.Show("请输入要发送的数据", "错误提示");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "当前时间" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        }

        private void timeBox3_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer txTimer = new System.Windows.Forms.Timer();

            if (timeBox3.Checked)
            {
                if (numericUpDown1.Value != 0)
                {
                    if (CheckSendData())
                    {
                        txTimer.Enabled = false;
                        timer2.Interval = (int)numericUpDown1.Value; //定时器赋初值  
                        timer2.Start();
                    }
                    else if (!CheckSendData())
                    {
                        timer2.Stop();
                    }
                }
                else if (numericUpDown1.Value == 0)
                {
                    timer2.Stop();
                }
            }
            else
            {
                txTimer.Enabled = true;
                timer2.Stop();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            btnSendData_Click(btnSendData, new EventArgs());
        }

        private void cbxCOMPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
           

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void timeBox3_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void rbnHex_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void rbnChar_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
