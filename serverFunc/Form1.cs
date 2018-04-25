using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace serverFunc
{
    public partial class Form1 : Form
    {
        //每个客户端最多显示的信息条目个数
        public const int MAX_DATA_NUM = 35;

        //每个客户端已发送的数据个数
        static int[] receivedDataIndexArray = new int[gVariable.MAX_CLIENT_NUM];

        //每个客户端最近发送的 MAX_DATA_NUM 笔数据被保存在这个 2 维数组中
        static string[,] receivedDataStringArray = new string[gVariable.MAX_CLIENT_NUM, MAX_DATA_NUM];

        private int startCommunication;

        private System.Windows.Forms.Timer aTimer;

        Thread thread1 = null;

        public Form1()
        {
            InitializeComponent();
            initializeVariables();
        }


        private void initializeVariables()
        {
            int i;

            for (i = 0; i < gVariable.MAX_CLIENT_NUM; i++)
                receivedDataIndexArray[i] = 0;

            for (i = 1; i <= gVariable.MAX_CLIENT_NUM; i++)
            {
                comboBox1.Items.Add(i + " 号客户端");
                gVariable.clientsInCommunication[i - 1] = -1;
            }

            comboBox1.SelectedIndex = 0;

            startCommunication = 0;

            gVariable.clientSelectedForDisplay = 0;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            int i;

            for (i = 0; i < gVariable.MAX_CLIENT_NUM; i++)
            {
                gVariable.sendDataRequestEnqueued[i] = new AutoResetEvent(false);
            }

            aTimer = new System.Windows.Forms.Timer();

            //刷新接收数据列表的时间间隔为 250 毫秒
            aTimer.Interval = 250;
            aTimer.Enabled = true;

            //定义时间中断的处理函数
            aTimer.Tick += new EventHandler(timer_listview);

            button1.Text = "停止接收数据";
            startCommunication = 1;

            button1.Enabled = false;

            label2.Text = "通讯中...";

            thread1 = new Thread(new ThreadStart(communication.startCommunication));
            thread1.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            aTimer.Stop();
            System.Environment.Exit(0);
        }

        //用来刷新屏幕, 不断显示从客户端接收到的数据
        private void timer_listview(Object source, EventArgs e)
        {
            int i;
            int start, num;

            try
            {
                aTimer.Stop();

                gVariable.clientSelectedForDisplay = comboBox1.SelectedIndex;

                if (gVariable.clientsInCommunication[gVariable.clientSelectedForDisplay] == -1)
                {
                    button2.Enabled = false;
                }
                else
                {
                    button2.Enabled = true;
                }
        

                this.listView1.Clear();
                this.listView1.BeginUpdate();

                listView1.GridLines = true;
                listView1.Dock = DockStyle.Fill;
                listView1.Columns.Add(" ", 1, HorizontalAlignment.Center);
                listView1.Columns.Add("序号", 80, HorizontalAlignment.Center);
                listView1.Columns.Add("数据内容", 287, HorizontalAlignment.Center);

//                Console.WriteLine(DateTime.Now.ToString());

                //计算需要显示的数据个数及起始位置
                start = 0;
                if (receivedDataIndexArray[gVariable.clientSelectedForDisplay] >= MAX_DATA_NUM)
                {
                    num = receivedDataIndexArray[gVariable.clientSelectedForDisplay];
                    start = num - MAX_DATA_NUM;
                }
                else
                    num = receivedDataIndexArray[gVariable.clientSelectedForDisplay];

                //显示数据
                for (i = start; i < num; i++)
                {
                    ListViewItem OptionItem = new ListViewItem();

                    OptionItem.SubItems.Add((i + 1).ToString());
                    OptionItem.SubItems.Add(receivedDataStringArray[gVariable.clientSelectedForDisplay, i - start]);

                    listView1.Items.Add(OptionItem);
                }

                this.listView1.EndUpdate();
                this.listView1.Show();

                //打开 timer, 重新计数 250 毫秒
                aTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("显示列表框出错，微软的提示信息为：" + ex);
            }
        }
        //开启新线程用于数据接收
        private void button1_Click(object sender, EventArgs e)
        {
            if (startCommunication == 0)
            {
                button1.Text = "停止接收数据";
                startCommunication = 1;

                button1.Enabled = false;

                label2.Text = "通讯中...";

                thread1 = new Thread(new ThreadStart(communication.startCommunication));
                thread1.Start();
            }
        }

        //暂存接收到的数据
        public static void addNewDataToArray(string receivedString, int clientIndex)
        {
            int i;

            try
            {
                if (receivedDataIndexArray[clientIndex] >= MAX_DATA_NUM)
                {
                    for (i = 1; i< MAX_DATA_NUM; i++)
                        receivedDataStringArray[clientIndex, i - 1] = receivedDataStringArray[clientIndex, i];
                    receivedDataStringArray[clientIndex, MAX_DATA_NUM - 1] = receivedString;

                    receivedDataIndexArray[clientIndex]++;
                }
                else
                {
                    receivedDataStringArray[clientIndex, receivedDataIndexArray[clientIndex]] = receivedString;
                    receivedDataIndexArray[clientIndex]++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("服务器接收数据后临时存储失败，微软的提示信息为：", ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            gVariable.sendString = textBox1.Text;
            if (gVariable.clientsInCommunication[gVariable.clientSelectedForDisplay] >= 0)
            {
                gVariable.sendDataRequestEnqueued[gVariable.clientSelectedForDisplay].Set();
            }
            else
            {
                MessageBox.Show("请确认该客户端已经通讯成功后再执行发送功能！", "信息提示", MessageBoxButtons.OK);
            }

        }
    }

}