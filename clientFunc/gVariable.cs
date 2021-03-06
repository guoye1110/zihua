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

namespace clientFunc
{
    public class gVariable
    {
        public static int rebuild_database = 0;

        //打印扫描软件客户端 ID 范围 100 - 400
        public const int MAX_CLIENT_ID = 50;

        public const int CLIENT_PC_WAREHOUSE_ID1 = 101;  //出入库

        public const int CLIENT_PC_FEED_ID1 = 121;  //上料系统
        public const int CLIENT_PC_FEED_ID2 = 122;  //上料系统
        public const int CLIENT_PC_FEED_ID3 = 123;  //上料系统
        public const int CLIENT_PC_FEED_ID4 = 124;  //上料系统
        public const int CLIENT_PC_FEED_ID5 = 125;  //上料系统

        public const int CLIENT_PC_CAST_ID1 = 141;  //流延设备
        public const int CLIENT_PC_CAST_ID2 = 142;  //流延设备
        public const int CLIENT_PC_CAST_ID3 = 143;  //流延设备
        public const int CLIENT_PC_CAST_ID4 = 144;  //流延设备
        public const int CLIENT_PC_CAST_ID5 = 145;  //流延设备

        public const int CLIENT_PC_PRINT_ID1 = 161;  //印刷设备
        public const int CLIENT_PC_PRINT_ID2 = 162;  //印刷设备
        public const int CLIENT_PC_PRINT_ID3 = 163;  //印刷设备

        public const int CLIENT_PC_SLIT_ID1 = 181;  //分切设备
        public const int CLIENT_PC_SLIT_ID2 = 182;  //分切设备
        public const int CLIENT_PC_SLIT_ID3 = 183;  //分切设备
        public const int CLIENT_PC_SLIT_ID4 = 184;  //分切设备
        public const int CLIENT_PC_SLIT_ID5 = 185;  //分切设备

        public const int CLIENT_PC_INSPECTION_ID1 = 201;  //质检工序

        public const int CLIENT_PC_REUSE_ID1 = 221;  //再造料工序

        public const int CLIENT_PC_PACKING_ID1 = 241;  //打包工序
    }
}
