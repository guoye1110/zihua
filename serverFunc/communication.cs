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
        const int DATA_TYPE_RESPONSE_DATA = 0;
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

        static byte[] sendPacket = new byte[200];

        static int[] dataPacketIndex = new int[gVariable.MAX_CLIENT_NUM];

        public static void startCommunication()
        {
            int i;
            Socket socket;
            int portNum;
            string HostName;
            string strIPAddr;
            IPHostEntry IpEntry;
            IPAddress ip;
            IPEndPoint ipep;

            //定义通讯端口
            portNum = 8899;

            strIPAddr = " ";
            HostName = Dns.GetHostName();
            IpEntry = Dns.GetHostEntry(HostName);

            //IpEntry 可能会有 IPv6 的地址，必须跳过
            //当服务器同时有不同的介质连接时，比如有有线又有无线连接时，就会有不同的 IP 地址，此时我们需要
            //通过下面的指令找到我们用来通讯的 IPv4 地址
            for (i = 0; i < IpEntry.AddressList.Length; i++)
            {
                if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    strIPAddr = IpEntry.AddressList[i].ToString();
//                    if (strIPAddr.Remove(7) == "192.168")
                        break;
                }
            }

            if (i >= IpEntry.AddressList.Length)
            {
                Console.WriteLine("服务器没有有效的 IPv4 地址，无法通讯!");
                return;
            }

            ip = IPAddress.Parse(strIPAddr);
            ipep = new IPEndPoint(ip, portNum);

            //定义 socket 的地址和通讯类型        
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipep);
            socket.Listen(20);  //20 是最大等待连接数

            while (true)
            {
                try
                {
                    //TCP 服务器端设定为监听模式，等待客户端的接入
                    Socket clientSocket = socket.Accept();

                    //从客户端处得到 sync 指令，建立连接成功
                    ServerThread newclient = new ServerThread(clientSocket);

                    //开启一个新的线程负责和该客户端之间的通讯
                    //Thread newthread = new Thread(new ThreadStart(newclient.ClientServer));
                    //newthread.Start();

                    //100毫秒后会再次进入监听状态，等待下一个客户端的接入
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("建立多线程失败，微软的提示信息为：", ex);
                }
            }
        }

        class ServerThread
        {
            Thread thread1;
			//客户端connect后server发起的第一个线程，目的是确定clientID（processMachineID）
			Thread identify_ClientID_thread;

            const int MAX_DATA_LENGTH = 2000;

            int recCount;
            int clientID;
            int dataSendingTriggerFlag;
            Socket serverSocket;
            string receivedString;
            byte [] receiveBytes = new byte[MAX_DATA_LENGTH];



            public ServerThread(Socket serverSocket)
            {
                this.serverSocket = serverSocket;
                dataSendingTriggerFlag = -1;
				identify_ClientID_thread = new Thread(new ThreadStart(this.identify_ClientID));
            }

			private bool validate_recved_data(byte[] buf)
            {
            	if (buf[0] == 'w' && buf[1] == 'I' && buf[2] == 'F' && buf[3] == 'i' && \
					toolClass.checkCrc32Code(receiveBytes, recCount) == true)
            
            }

			public void identify_ClientID()
            {
            	int len;

				try {
					while(true){
						//接收数据
						recCount = serverSocket.Receive(receiveBytes, receiveBytes.Length, 0);
						if (recCount != 0) {
							check
						}
			}

            public void ClientServer()
            {
                int len;

                try
                {
                    while (true)
                    {
                        //接收数据
                        recCount = serverSocket.Receive(receiveBytes, receiveBytes.Length, 0);
                        if (recCount != 0)
                        {
                            if (receiveBytes[0] == 'w' && receiveBytes[1] == 'I' && receiveBytes[2] == 'F' && receiveBytes[3] == 'i' && toolClass.checkCrc32Code(receiveBytes, recCount) == true)
                            {
                                //数据中第一位是 ID，后面是内容
                                clientID = receiveBytes[PROTOCOL_DATA_POS];
                                dataPacketIndex[clientID] = 0;

                                gVariable.clientsInCommunication[clientID - 1] = 1;

                                //tell client we got the data successfully
                                //communication type is still server which will tell the client that this is a response packet from server, not a real data packet sent from server
                                sendDataToClient("We got the data!", COMMUNICATION_TYPE_BARCODE_TO_SERVER);

                                if (dataSendingTriggerFlag == -1)
                                {
                                    dataSendingTriggerFlag = 0;
                                    thread1 = new Thread(dataSendingThread);
                                    thread1.Start();
                                }

                                if (dataSendingTriggerFlag == 1)
                                {
                                    //this is a data packet we got from form1 textbox2
                                    sendDataToClient(gVariable.sendString, COMMUNICATION_TYPE_BARCODE_TO_CLIENT);

                                    dataSendingTriggerFlag = 0;
                                }

                                //recCount is packet length, MIN_PACKET_LEN_PURE_FRAME is header/tail length, we remove the first byte of ID, so len is the read data length 
                                len = recCount - MIN_PACKET_LEN_PURE_FRAME - 1;

                                //把接收数组中的内容转成字符串
                                receivedString = System.Text.Encoding.Default.GetString(receiveBytes, PROTOCOL_DATA_POS + 1, len);

                                //把字符串存入临时缓存区
                                Form1.addNewDataToArray(receivedString, clientID - 1);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Client want to close connection!");
                            gVariable.clientsInCommunication[clientID - 1] = -1;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Client closed connection abruptly!", ex);
                    gVariable.clientsInCommunication[clientID - 1] = -1;
                }
                serverSocket.Close();
            }

            private void dataSendingThread()
            {
                while (true)
                {
                    gVariable.sendDataRequestEnqueued[clientID - 1].WaitOne();

                    dataSendingTriggerFlag = 1;
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
            private void sendDataToClient(string sendString, int communicationType)
            {
                int i, j;
                int len;
                byte [] buf;

                try
                {
                    buf = System.Text.Encoding.Default.GetBytes(sendString);

                    dataPacketIndex[clientID]++;

                    i = 0;
                    //header
                    sendPacket[i++] = (byte)'w';
                    sendPacket[i++] = (byte)'I';
                    sendPacket[i++] = (byte)'F';
                    sendPacket[i++] = (byte)'w';

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
                    sendPacket[i++] = (byte)dataPacketIndex[clientID];
                    sendPacket[i++] = (byte)(dataPacketIndex[clientID] >> 8);
                    sendPacket[i++] = (byte)(dataPacketIndex[clientID] >> 16);
                    sendPacket[i++] = (byte)(dataPacketIndex[clientID] >> 24);

                    //reserved data
                    sendPacket[i++] = 0;
                    sendPacket[i++] = 0;
                    sendPacket[i++] = 0;
                    sendPacket[i++] = 0;

                    //data type
                    if (communicationType == COMMUNICATION_TYPE_BARCODE_TO_CLIENT)  //server send out data
                        sendPacket[i++] = DATA_TYPE_NEW_TO_COMMON_TABLE;
                    else //if(if (communicationType == COMMUNICATION_TYPE_BARCODE_TO_SERVER)  //server got data from client then send a confirmation response
                        sendPacket[i++] = DATA_TYPE_RESPONSE_DATA;

                    //real data
                    for (j = 0; j < buf.Length; j++)
                    {
                        sendPacket[i++] = (byte)buf[j];
                    }

                    //length of the data plus 32 bit CRC length 
                    len = i + PROTOCOL_CRC_LEN;

                    sendPacket[PROTOCOL_COMMUNICATION_TYPE_POS] = (byte)communicationType;
                    sendPacket[PROTOCOL_LEN_POS] = (byte)(len % 0x100);
                    sendPacket[PROTOCOL_LEN_POS + 1] = (byte)(len / 0x100);
                    toolClass.addCrc32Code(sendPacket, len);
                    serverSocket.Send(sendPacket, len, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("发送数据错误，微软提示信息为：" + ex);
                }
            }
        }
    }
}