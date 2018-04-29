using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Sockets;
//using database;

namespace clientFunc
{
    public partial class Form1 : Form
    {
        //number of records displayed in main UI
        const int MAX_DISPLAY_NUM = 23;

        //communication type
        const int COMMUNICATION_TYPE_BARCODE_TO_CLIENT = 0xB7;
        const int COMMUNICATION_TYPE_BARCODE_TO_SERVER = 0xB8;

        const int PROTOCOL_CRC_LEN = 4;

        //通讯数据包中不同位置的含义
        const int PROTOCOL_HEAD_POS = 0;
        const int PROTOCOL_LEN_POS = 4;
        const int PROTOCOL_COMMUNICATION_TYPE_POS = 6;
        const int PROTOCOL_TIME_POS = 7;
        const int PROTOCOL_PACKET_INDEX_POS = 19;
        const int PROTOCOL_RESERVED_DATA_POS = 23;
        const int PROTOCOL_DATA_TYPE_POS = 27;
        const int PROTOCOL_DATA_POS = 28;

        System.Windows.Forms.Timer aTimer;

        public Form1()
        {
            InitializeComponent();
            initializeVariables();
        }


        private void initializeVariables()
        {
            communication.clientSocket = null;
            communication.dataPacketIndex = 0;
            communication.serverConnected = 0;
            communication.clientID = 1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //initialize mySQL parameters
            //mySQLClass mySQL = new mySQLClass();

            //if (gVariable.rebuild_database == 1) //we need to clear current data and rebuild a new one
            //{
            //    mySQLClass.buildBasicDatabase();
            //}

            aTimer = new System.Windows.Forms.Timer();

            //发送数据的时间间隔为 500 毫秒
            aTimer.Interval = 500;
            aTimer.Enabled = true;

            aTimer.Tick += new EventHandler(timer_listview);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            aTimer.Stop();
            System.Environment.Exit(0);
        }

        //用来刷新屏幕, 不断显示从客户端接收到的数据
        private void timer_listview(Object source, EventArgs e)
        {
            int i, j;
            int num;
            int start, end;
            int columnNum;
            string commandText;
            string[,] dataArray;

            /*try
            {
                aTimer.Stop();

                //说明和服务器连接成功
                if (communication.serverConnected == 1)
                    communication.sendDataToHostPC();

                //server exit, so client also exit
                if (communication.serverConnected == -2)
                    System.Environment.Exit(0);

                this.listView1.Clear();
                this.listView1.BeginUpdate();

                listView1.GridLines = true;
                listView1.Dock = DockStyle.Fill;
                listView1.Columns.Add(" ", 1, HorizontalAlignment.Center);
                listView1.Columns.Add("序号", 60, HorizontalAlignment.Center);
                listView1.Columns.Add("工号", 100, HorizontalAlignment.Center);
                listView1.Columns.Add("接收时间", 150, HorizontalAlignment.Center);
                listView1.Columns.Add("设备号", 80, HorizontalAlignment.Center);
                listView1.Columns.Add("数据", 150, HorizontalAlignment.Center);
                listView1.Columns.Add("备注", 100, HorizontalAlignment.Center);

//                Console.WriteLine(DateTime.Now.ToString());

                //get number of data in this table
                num = mySQLClass.getRecordNumInTable(mySQLClass.sampleDatabaseName, mySQLClass.UI_InputTableName + communication.clientID.ToString());
                end = num;
                if(num > MAX_DISPLAY_NUM)
                {
                    start = num - MAX_DISPLAY_NUM;
                    num = MAX_DISPLAY_NUM;
                }
                else
                {
                    start = 1;
                }

                //id between start and end, listed in decreasing order, could also be(if parameters are string, we need to add '' to quote the parameter):
                //instruction like: "where time > '" + time1 + "' and time <= '" +  time2 + "' order by time DESC"
                //becomes the final string: "where time > 2017-12-08 12:30:03 and time <= 2018-01-06 21:23:03 order by time DESC"
                commandText = "where id >= " + start + " and id <= " + end + " order by id DESC";

                dataArray = mySQLClass.getAllRecordFromTableByCondition(mySQLClass.sampleDatabaseName, mySQLClass.UI_InputTableName + communication.clientID.ToString(), commandText);
                if (dataArray != null)
                {
                    columnNum = dataArray.GetLength(1);

                    for (i = 0; i < num; i++)
                    {
                        ListViewItem OptionItem = new ListViewItem();

                        for (j = 0; j < columnNum; j++)
                            OptionItem.SubItems.Add(dataArray[i, j]);

                        listView1.Items.Add(OptionItem);
                    }
                }
                this.listView1.EndUpdate();
                this.listView1.Show();

                //打开 timer, 重新开始计时
                aTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("显示列表框出错，微软的提示信息为：" + ex);
            }*/
        }

        //start communication
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolClass.isDigitalNum(textBox2.Text) != 1)
                {
                    MessageBox.Show("请输入 1 - 50 的纯数字，谢谢！", "信息提示", MessageBoxButtons.OK);
                    return;
                }
                else
                {
                    communication.clientID = Convert.ToInt32(textBox2.Text);
                    if (communication.clientID < 1 || communication.clientID > 50)
                    {
                        MessageBox.Show("请输入 1 - 50 的纯数字，谢谢！", "信息提示", MessageBoxButtons.OK);
                        return;
                    }
                }

                //只接受客户端 ID 为 1 到 50
                if (communication.clientID > gVariable.MAX_CLIENT_ID || communication.clientID < 1)
                    communication.clientID = 1;

                if (communication.serverConnected == 0)
                {
                    communication.HostIP = IPAddress.Parse(textBox1.Text);

                    Thread thread1 = new Thread(new ThreadStart(communication.startCommunication));
                    thread1.Start();
                }
                /*else
                {
                    aTimer.Start();
                }*/

                communication.serverConnected = 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deal with new cient ID failed! " + ex);
            }
        }

        //stop communication
        private void button2_Click(object sender, EventArgs e)
        {
            //aTimer.Stop();
        }
    }
}