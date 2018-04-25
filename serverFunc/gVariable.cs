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
        //客户端最大个数
        public const int MAX_CLIENT_NUM = 50;

        //服务器需要显示的客户端 ID
        public static int clientSelectedForDisplay;

        //to indicate whether a client is in communication with server, -1 not yet, >= 0, client index
        public static int[] clientsInCommunication = new int[MAX_CLIENT_NUM];

        //string that need to be sent to clinet
        public static string sendString;

        //communication thread for every connection will wait for the triggering of this event, if triggered, send data out to client
        public static AutoResetEvent[] sendDataRequestEnqueued = new AutoResetEvent[MAX_CLIENT_NUM];
    }
}
