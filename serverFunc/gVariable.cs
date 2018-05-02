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
    public class gVariable
    {
        //�ͻ���������
        public const int MAX_CLIENT_NUM = 50;

        //��ӡɨ������ͻ��� ID ��Χ 100 - 400
        public const int MAX_CLIENT_ID = 50;

        public const int CLIENT_PC_WAREHOUSE_ID1 = 101;  //�����

        public const int CLIENT_PC_FEED_ID1 = 121;  //����ϵͳ
        public const int CLIENT_PC_FEED_ID2 = 122;  //����ϵͳ
        public const int CLIENT_PC_FEED_ID3 = 123;  //����ϵͳ
        public const int CLIENT_PC_FEED_ID4 = 124;  //����ϵͳ
        public const int CLIENT_PC_FEED_ID5 = 125;  //����ϵͳ

        public const int CLIENT_PC_CAST_ID1 = 141;  //�����豸
        public const int CLIENT_PC_CAST_ID2 = 142;  //�����豸
        public const int CLIENT_PC_CAST_ID3 = 143;  //�����豸
        public const int CLIENT_PC_CAST_ID4 = 144;  //�����豸
        public const int CLIENT_PC_CAST_ID5 = 145;  //�����豸

        public const int CLIENT_PC_PRINT_ID1 = 161;  //ӡˢ�豸
        public const int CLIENT_PC_PRINT_ID2 = 162;  //ӡˢ�豸
        public const int CLIENT_PC_PRINT_ID3 = 163;  //ӡˢ�豸

        public const int CLIENT_PC_SLIT_ID1 = 181;  //�����豸
        public const int CLIENT_PC_SLIT_ID2 = 182;  //�����豸
        public const int CLIENT_PC_SLIT_ID3 = 183;  //�����豸
        public const int CLIENT_PC_SLIT_ID4 = 184;  //�����豸
        public const int CLIENT_PC_SLIT_ID5 = 185;  //�����豸

        public const int CLIENT_PC_INSPECTION_ID1 = 201;  //�ʼ칤��

        public const int CLIENT_PC_REUSE_ID1 = 221;  //�����Ϲ���

        public const int CLIENT_PC_PACKING_ID1 = 241;  //�������

        //��������Ҫ��ʾ�Ŀͻ��� ID
        public static int clientSelectedForDisplay;

        //to indicate whether a client is in communication with server, -1 not yet, >= 0, client index
        public static int[] clientsInCommunication = new int[MAX_CLIENT_NUM];

        //string that need to be sent to clinet
        public static string sendString;

        //communication thread for every connection will wait for the triggering of this event, if triggered, send data out to client
        public static AutoResetEvent[] sendDataRequestEnqueued = new AutoResetEvent[MAX_CLIENT_NUM];
    }
}
