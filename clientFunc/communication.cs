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
//using database;

namespace clientFunc
{
    public class communication
    {
        //communication type
        const int COMMUNICATION_TYPE_START_HANDSHAKE_WITH_ID_TO_PC = 3;  //data collection board

        const int COMMUNICATION_TYPE_APP_WORKING_BOARD_ID_TO_PC = 0x50;  //Android app communication

        const int COMMUNICATION_TYPE_CLIENT_PC = 0xB0;  //client PC communication 

        const int COMMUNICATION_TYPE_BARCODE_TO_CLIENT = 0xB7;  //bar code printer server
        const int COMMUNICATION_TYPE_BARCODE_TO_SERVER = 0xB8;  //bar code printer server

        const int COMMUNICATION_TYPE_EMAIL_HEART_BEAT = 0xFD;  //to indicate the email server is still alive
        const int COMMUNICATION_TYPE_EMAIL_FORWARDER = 0xFE;  //email server

        //data type
        const int DATA_TYPE_NEW_TO_COMMON_TABLE = 2;

        //data format related definitions
        const int PROTOCOL_CRC_LEN = 4;
        const int MIN_PACKET_LEN_PURE_FRAME = 32;   //header(4) + len(2) + communicationtype(1) + time(12) + index(4) + reserved(4) + type(1) + CRC(4)

        //通讯数据包中不同位置的含义
        const int PROTOCOL_HEAD_POS = 0;
        const int PROTOCOL_LEN_POS = 4;
        const int PROTOCOL_COMMUNICATION_TYPE_POS = 6;
        const int PROTOCOL_TIME_POS = 7;
        const int PROTOCOL_PACKET_INDEX_POS = 19;
        const int PROTOCOL_RESERVED_DATA_POS = 23;
        const int PROTOCOL_DATA_TYPE_POS = 27;
        const int PROTOCOL_DATA_POS = 28;
        //end of data format related definitions

        public static int clientID;
        public static IPAddress HostIP;
        public static int serverConnected;
        public static IPEndPoint point;
        public static Socket clientSocket;
        public static int dataPacketIndex;

        static byte[] sendPacket = new byte[200];

        public static void startCommunication()
        {
            int recCount;
            int len;
            string receivedString;
            byte [] receiveBytes = new byte[200];


            try
            {
                //根据输入的服务器 IP 地址和固定端口号 8899 进行通讯 
                point = new IPEndPoint(HostIP, 8899);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(point);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("通讯失败，一个可能是输入的服务器 IP 地址有误，另一个可能是服务器未进入监听状态，请在服务器端按下《开始接收数据》按钮！", "信息提示", MessageBoxButtons.OK);
                return;
            }

            while (true)
            {
                try
                {
                    serverConnected = 1;
                    recCount = clientSocket.Receive(receiveBytes, 200, 0);
                    //TCP/IP requires 0 length packet as a indication of graceful disconnection, so simply exit
                    if (recCount == 0)  
                    {
                        System.Environment.Exit(0);
                    }

                    len = recCount - MIN_PACKET_LEN_PURE_FRAME;

                    //把接收数组中的内容转成字符串
                    receivedString = System.Text.Encoding.Default.GetString(receiveBytes, PROTOCOL_DATA_POS, len);

                    if (receiveBytes[PROTOCOL_DATA_TYPE_POS] == DATA_TYPE_NEW_TO_COMMON_TABLE)  //real data packet
                    {
                        putReceiveDataToDatabase(receivedString);
                    }
                    else  //response from server that data are received correctly
                    {
                        if (receivedString == "We got the data!")
                        {
                            Console.WriteLine("Send/receive data OK!");
                        }
                        else
                        {
                            Console.WriteLine("Receive data failed!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("服务器中断连接，通讯结束！ " + ex);
                    serverConnected = -2;
                    return;
                }
            }

        }

        //data packet format:
        //0-3: header
        //4-5: length of the packet
        //6: communication type
        //7 - 18: time:171130214859
        //19 - 22: dataPacketIndex
        //23 - 26: reserved
        //27: dataType
        //28 - : data
        //last 4 bytes: CRC
        public static void sendDataToHostPC()
        {
            int i;
            int v;
            int len;

            try
            {
                dataPacketIndex++;

                Random rand = new Random();

                v = rand.Next(1, 26);

                i = 0;
                //header
                sendPacket[i++] = (byte)'w';
                sendPacket[i++] = (byte)'I';
                sendPacket[i++] = (byte)'F';
                sendPacket[i++] = (byte)'i';

                i = 7;
                //date and time
                sendPacket[i++] = (byte)(DateTime.Now.Year % 100 / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Year % 100 % 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Month / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Month % 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Day / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Day % 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Hour / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Hour % 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Minute / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Minute % 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Second / 10 + '0');
                sendPacket[i++] = (byte)(DateTime.Now.Second % 10 + '0');

                //data packet index
                sendPacket[i++] = (byte)dataPacketIndex;
                sendPacket[i++] = (byte)(dataPacketIndex >> 8);
                sendPacket[i++] = (byte)(dataPacketIndex >> 16);
                sendPacket[i++] = (byte)(dataPacketIndex >> 24);

                //reserved data
                sendPacket[i++] = 0;
                sendPacket[i++] = 0;
                sendPacket[i++] = 0;
                sendPacket[i++] = 0;

                //data type, not used here
                sendPacket[i++] = DATA_TYPE_NEW_TO_COMMON_TABLE;

                //real data, 5 bytes
                sendPacket[i++] = (byte)clientID;
                sendPacket[i++] = (byte)(v + 'A');
                sendPacket[i++] = (byte)(v + 'A');
                sendPacket[i++] = (byte)(v + 'A');
                sendPacket[i++] = (byte)(v + 'A');

                //length of the data plus 32 bit CRC length 
                len = i + PROTOCOL_CRC_LEN;

                sendPacket[PROTOCOL_COMMUNICATION_TYPE_POS] = (byte)COMMUNICATION_TYPE_BARCODE_TO_SERVER;
                sendPacket[PROTOCOL_LEN_POS] = (byte)(len % 0x100);
                sendPacket[PROTOCOL_LEN_POS + 1] = (byte)(len / 0x100);
                toolClass.addCrc32Code(sendPacket, len);
                clientSocket.Send(sendPacket, len, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data sending error!：" + ex);
            }
        }

        static void putReceiveDataToDatabase(string receivedString)
        {
            string [] dataArray = new string [5];

            dataArray[0] = "31085";
            dataArray[1] = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
            dataArray[2] = clientID.ToString();
            dataArray[3] = receivedString;
            dataArray[4] = "";
            //mySQLClass.addNewRecordToTable(mySQLClass.sampleDatabaseName, mySQLClass.UI_InputTableName + clientID.ToString(), mySQLClass.UI_InputFileName, dataArray);
        }
    }
}