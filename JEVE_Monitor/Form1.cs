using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


public struct VCI_INIT_CONFIG
    {
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;
    public byte Timing0;
    public byte Timing1;
    public byte Mode;
}

public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved;
}

public struct VCI_ERR_INFO
{
    public UInt32 ErrCode;
    public byte Passive_ErrData1;
    public byte Passive_ErrData2;
    public byte Passive_ErrData3;
    public byte ArLost_ErrData;
}


unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;
    public byte TimeFlag;
    public byte SendType;
    public byte RemoteFlag;//是否是远程帧
    public byte ExternFlag;//是否是扩展帧
    public byte DataLen;

    public fixed byte Data[8];

    public fixed byte Reserved[3];

}

public struct VCI_CAN_STATUS
{
    public byte ErrInterrupt;
    public byte regMode;
    public byte regStatus;
    public byte regALCapture;
    public byte regECCapture;
    public byte regEWLimit;
    public byte regRECounter;
    public byte regTECounter;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}


namespace JEVE_Monitor
{
    public partial class Form1 : Form
    {

        const int VCI_PCI5121 = 1;
        const int VCI_PCI9810 = 2;
        const int VCI_USBCAN1 = 3;
        const int VCI_USBCAN2 = 4;
        const int VCI_USBCAN2A = 4;
        const int VCI_PCI9820 = 5;
        const int VCI_CAN232 = 6;
        const int VCI_PCI5110 = 7;
        const int VCI_CANLITE = 8;
        const int VCI_ISA9620 = 9;
        const int VCI_ISA5420 = 10;
        const int VCI_PC104CAN = 11;
        const int VCI_CANETUDP = 12;
        const int VCI_CANETE = 12;
        const int VCI_DNP9810 = 13;
        const int VCI_PCI9840 = 14;
        const int VCI_PC104CAN2 = 15;
        const int VCI_PCI9820I = 16;
        const int VCI_CANETTCP = 17;
        const int VCI_PEC9920 = 18;
        const int VCI_PCI5010U = 19;
        const int VCI_USBCAN_E_U = 20;
        const int VCI_USBCAN_2E_U = 21;
        const int VCI_PCI5020U = 22;
        const int VCI_EG20T_CAN = 23;
/*==============================================================================================================================*/
        const int Normal_Mode = 0;
        const int Listen_Mode = 1;
/*==============================================================================================================================*/
        const int The_Device_is_Open = 1;  /**************************************/
        const int The_Device_is_close = 0; /*设定状态机判断CAN盒开启或者关闭的状态*/
        int The_Device_Statu = The_Device_is_close;          /**************************************/
/*==============================================================================================================================*/
        const int The_CAN_is_Already_Start = 1;    /*此处是CAN是否打开的状态机，注意要和CAN盒是否开启和关闭区别出来*/
        const int The_CAN_is_Not_Start = 0;
        int The_CAN_Open_Or_Close_Statu = The_CAN_is_Not_Start;
/*==============================================================================================================================*/
        const int CAN_Receive_Wait_Time_100mS = 100;
/*===============================================================================================================================*/
        const int RemoteFlag_DataFrame = 0;
        const int RemoteFlag_RemoteFrame = 1;
        const int ExternFlag_StandardFrame = 0;
        const int ExternFlag_ExternFrame = 1;
        /*===================================================================================================================================*/
        const int Init_Statu_OK = 1;
/*=================================================================================================================================*/

        const UInt32 BaudRate_STATUS_OK = 1;

        static UInt32 Current_Dev_Type = VCI_USBCAN_2E_U;

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        //static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        unsafe static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, byte* pData);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        //static UInt32 m_devtype = 4;//USBCAN2
        static UInt32 m_devtype = 21;//USBCAN-2e-u

        UInt32 Current_Dev_Index;
        UInt32 Current_Can_Gallery;
        UInt32 Current_BaudRate;

        static UInt32[] GCanBrTab = new UInt32[10]{
                    0x060003, 0x060004, 0x060007,
                        0x1C0008, 0x1C0011, 0x160023,
                        0x1C002C, 0x1600B3, 0x1C00E0,
                        0x1C01C1
                };


        Regex reg_baud = new Regex(@"--.*Kbps"); //设定正则表达式模型用来提取表单中的波特率 
        Regex reg_baud_RegisterValue = new Regex(@"0x\w{6}");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ID_TextBox.Text = "00000123";
            Data_TextBox.Text = "00 01 02 03 04 05 06 07";




        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x4e:
                case 0xd:
                case 0xe:
                case 0x14:
                    base.WndProc(ref m);
                    break;
                case 0x84://鼠标点任意位置后可以拖动窗体
                    this.DefWndProc(ref m);
                    if (m.Result.ToInt32() == 0x01)
                    {
                        m.Result = new IntPtr(0x02);
                    }
                    break;
                case 0xA3://禁止双击最大化
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
                
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作
                return cp;
            }
        }
        /*===========================选定CAN卡型号USBCAN-E-U=============================================================*/
        private void CAN_Dev_USBCan_E_U_Click(object sender, EventArgs e)
        {
            this.CAN_Dev_Type_Show.Text = "CAN卡类型：USBCAN-E-U";

            Current_Dev_Type = VCI_USBCAN_E_U;

            //MessageBox.Show(Current_Dev_Type.ToString());
        }
        /*==========================选定CAN卡型号USBCAN2E-U==============================================================*/
        private void CAN_Dev_USBCan_2E_U_Click(object sender, EventArgs e)
        {
            this.CAN_Dev_Type_Show.Text = "CAN卡类型：USBCAN-2E-U";

            Current_Dev_Type = VCI_USBCAN_2E_U;
        }
        /*==========================选定CAN卡型号USBCANII================================================================*/
        private void DEV_USBCANII_Set_Click(object sender, EventArgs e)
        {
            CAN_Dev_Type_Show.Text = "CAN卡类型: USBCANII";

            Current_Dev_Type = VCI_USBCAN2;
        }

        

        /*===================================================================================================================================*/

        /*配置USBCAN-E-U设备的索引以及相关操作*/
        private void USBCAN_E_U_IndexChose_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Dev_Index_Show.Text = "设备索引："+ USBCAN_E_U_Index_Chose.SelectedItem.ToString(); //程序里实时显示设备的索引值

            Current_Dev_Index = Convert.ToUInt32(USBCAN_E_U_Index_Chose.SelectedIndex); //将设备索引值赋值给 当前设备索引值变量

            //MessageBox.Show(Current_Dev_Index.ToString());

        }
        /*===================================================================================================================================*/
        /*配置USBCAN-E-U设备的通道 以及相关操作*/
        private void USBCAN_E_U_Gallery_Chose_SelectedIndexChanged(object sender, EventArgs e)
        {
            DEV_Gallery_Show.Text = "设备通道：" + USBCAN_E_U_Gallery_Chose.SelectedItem.ToString();

            Current_Can_Gallery = Convert.ToUInt32(USBCAN_E_U_Gallery_Chose.SelectedIndex);

            //MessageBox.Show(Current_Can_Gallery.ToString());
        }

        /*===================================================================================================================================*/
        /*配置USBCAN-2E-U设备的索引以及相关操作*/

        private void USBCAN_2E_U_Index_Chose_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dev_Index_Show.Text = "设备索引：" + USBCAN_2E_U_Index_Chose.SelectedItem.ToString();

            Current_Dev_Index = Convert.ToUInt32(USBCAN_2E_U_Index_Chose.SelectedIndex); //将设备索引值赋值给 当前设备索引值变量

            //MessageBox.Show(Current_Dev_Index.ToString());
        }

        /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
        /*============================配置USBCANII设备索引及相关操作=========================================================================*/
        private void USBCANII_Index_Set_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dev_Index_Show.Text = "设备索引：" + USBCANII_Index_Set.SelectedIndex.ToString();
            Current_Dev_Index = Convert.ToUInt32(USBCANII_Index_Set.SelectedIndex);
        }
        /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
        /*===========================配置USBCANII设备的通道及相关操作==========================================================================*/
        private void USBCANII_Gallery_Set_SelectedIndexChanged(object sender, EventArgs e)
        
        {

            Current_Can_Gallery = Convert.ToUInt32( USBCANII_Gallery_Set.SelectedIndex);

            DEV_Gallery_Show.Text = "设备通道: "+ USBCANII_Gallery_Set.SelectedItem;

        }
        /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
        /*============================配置USBCAN-2E-U 设备的 通道 以及相关设置================================================================*/

        private void USBCAN_2E_U_Gallery_Chose_SelectedIndexChanged(object sender, EventArgs e)
        {
            DEV_Gallery_Show.Text = "设备通道：" + USBCAN_2E_U_Gallery_Chose.SelectedItem.ToString();

            Current_Can_Gallery = Convert.ToUInt32(USBCAN_2E_U_Gallery_Chose.SelectedIndex);

            //MessageBox.Show(Current_Can_Gallery.ToString());
        }

 /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
 /*==========================================USBCAN-E-U 设备通讯波特率选择===================================================*/


        private void USBCAN_E_U_BaudRate_Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            CAN_Comm_BaudRate_Show.Text = "设备波特率:" + reg_baud.Match( USBCAN_E_U_BaudRate_Select.SelectedItem.ToString());


            Current_BaudRate = GCanBrTab[USBCAN_E_U_BaudRate_Select.SelectedIndex];

            //reg_baud_RegisterValue.Match( USBCAN_E_U_BaudRate_Select.SelectedItem.ToString() ).ToString();
             
           // Current_BaudRate = Convert.ToUInt32( USBCAN_E_U_BaudRate_Select.SelectedIndex);

        }

/*==========================================USBCAN-2E-U 设备通讯波特率选择===================================================*/

        private void USBCAN_2E_U_BaudRate_Select_SelectedIndexChanged(object sender, EventArgs e)
        {
            CAN_Comm_BaudRate_Show.Text = "设备波特率:" + reg_baud.Match(USBCAN_2E_U_BaudRate_Select.SelectedItem.ToString());

            Current_BaudRate = GCanBrTab[USBCAN_2E_U_BaudRate_Select.SelectedIndex];
        }

/*==========================================实现对CAN设备的连接===============================================================*/

        unsafe private void CAN_Connnection_Button_Click_1(object sender, EventArgs e)
        {
            if (The_Device_Statu == The_Device_is_Open)
            {
                VCI_CloseDevice(Current_Dev_Type, Current_Dev_Index);
                The_Device_Statu = The_Device_is_close;
                CAN_Start_Or_Reset_Button.Text = "启动CAN";
                CAN_Connnection_Button.Text = "建立连接";
            }
            else if(The_Device_Statu == The_Device_is_close)
            {
                if (Current_Dev_Type == VCI_USBCAN_2E_U || Current_Dev_Type == VCI_USBCAN_E_U) //如果设备类型为 USBCAN-2E-U 或者 USBCAN-E-U 则执行以下代码 
                {
                    if (VCI_OpenDevice(Current_Dev_Type, Current_Dev_Index, 0) == 0)
                    {
                        MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    UInt32 baud = Current_BaudRate; // 如果不间接使用则会出现问题。

                    if (VCI_SetReference(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery, 0, (byte*)&baud) != BaudRate_STATUS_OK)
                    {

                        MessageBox.Show("设置波特率错误，打开设备失败!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        VCI_CloseDevice(Current_Dev_Type, Current_Dev_Index);
                        return;
                    }



                    VCI_INIT_CONFIG VCI_Init_Structure = new VCI_INIT_CONFIG();

                    VCI_Init_Structure.AccCode = Convert.ToUInt32("0x00000000", 16);
                    VCI_Init_Structure.AccMask = Convert.ToUInt32("0xFFFFFFFF", 16);
                    VCI_Init_Structure.Timing0 = Convert.ToByte("0x00", 16);
                    VCI_Init_Structure.Timing1 = Convert.ToByte("0x14", 16);
                    VCI_Init_Structure.Filter = 1;
                    VCI_Init_Structure.Mode = Normal_Mode; // 在 USBCAN-E-U/2E-U PCI-5010-U、 PCI-5020-U 时 只有 MODE是 有效的 0 代表正常模式，1 代表只听模式 

                    if (VCI_InitCAN(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery, ref VCI_Init_Structure) == Init_Statu_OK)//调用初始化函数，但是波特率是在 VCI_SETReference 实现的

                    {
                        MessageBox.Show("初始化 CAN 设备成功");

                        CAN_Connnection_Button.Text = "断开连接";

                        The_Device_Statu = The_Device_is_Open;
                    }
                }
                else if (Current_Dev_Type == VCI_USBCAN2) // 如果设备类型为 USBCANII 则执行以下代码
                {
                    if (VCI_OpenDevice(Current_Dev_Type, Current_Dev_Index, 0) == 0)
                    {
                        MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    VCI_INIT_CONFIG VCI_Init_Structure = new VCI_INIT_CONFIG();

                    VCI_Init_Structure.AccCode = Convert.ToUInt32("0x00000000", 16);
                    VCI_Init_Structure.AccMask = Convert.ToUInt32("0xFFFFFFFF", 16);
                    VCI_Init_Structure.Timing0 = Convert.ToByte("0xbf", 16);
                    VCI_Init_Structure.Timing1 = Convert.ToByte("0xff", 16);
                    VCI_Init_Structure.Filter = 1;
                    VCI_Init_Structure.Mode = Normal_Mode; // 在 USBCAN-E-U/2E-U PCI-5010-U、 PCI-5020-U 时 只有 MODE是 有效的 0 代表正常模式，1 代表只听模式 

                    if (VCI_InitCAN(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery, ref VCI_Init_Structure) == Init_Statu_OK && VCI_InitCAN(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery+1, ref VCI_Init_Structure) == Init_Statu_OK) //调用初始化函数，但是波特率是在 VCI_SETReference 实现的

                    {
                        MessageBox.Show("初始化 CAN 设备成功");

                        CAN_Connnection_Button.Text = "断开连接";

                        The_Device_Statu = The_Device_is_Open;
                    }
                    else
                    {
                        MessageBox.Show("未能初始化CAN设备");
                    }

                }
                else
                {
                    MessageBox.Show("设备类型暂未支持");

                    The_Device_Statu = The_Device_is_close;
                }

                
            }
            Scan_Timer.Enabled = The_Device_Statu == The_Device_is_Open ? true : false;
        }

/*==========================================启动CAN按钮和复位CAN按钮按键操作==================================================*/
        private void CAN_Start_Or_Reset_Button_Click(object sender, EventArgs e)
        {
            if (The_Device_Statu == The_Device_is_close) /*如果CAN盒并没有被连接上，那么则告诉用户请先建立对CAN盒的连接再开启CAN*/
            {
                MessageBox.Show("请先建立连接");
                return;
            } 
            else if (The_Device_Statu == The_Device_is_Open)//如果系统已经建立了对CAN盒的连接那么接下来执行
            {
                if (Current_Dev_Type == VCI_USBCAN_2E_U || Current_Dev_Type == VCI_USBCAN_E_U)
                {
                    if (The_CAN_Open_Or_Close_Statu == The_CAN_is_Not_Start) //如果系统已经建立了对CAN盒的连接>>>>>>>>>>如果并没有启动CAN
                    {
                        VCI_StartCAN(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery);
                        CAN_Start_Or_Reset_Button.Text = "复位CAN";
                        The_CAN_Open_Or_Close_Statu = The_CAN_is_Already_Start;

                    }
                    else if (The_CAN_Open_Or_Close_Statu == The_CAN_is_Already_Start)//如果系统已经建立了对CAN盒的连接>>>>>>>>>>>如果已经启动了CAN盒
                    {
                        VCI_ResetCAN(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery);
                        CAN_Start_Or_Reset_Button.Text = "启动CAN";
                        The_CAN_Open_Or_Close_Statu = The_CAN_is_Not_Start;
                    }
                }
                else if (Current_Dev_Type == VCI_USBCAN2)
                {
                    if (The_CAN_Open_Or_Close_Statu == The_CAN_is_Not_Start) //如果系统已经建立了对CAN盒的连接>>>>>>>>>>如果并没有启动CAN
                    {
                        VCI_StartCAN(Current_Dev_Type, Current_Dev_Index, 0);
                        VCI_StartCAN(Current_Dev_Type, Current_Dev_Index, 1);//暂定调试，需要同时打开两路CAN。这样USBCANII才能自发自收
                        CAN_Start_Or_Reset_Button.Text = "复位CAN";
                        The_CAN_Open_Or_Close_Statu = The_CAN_is_Already_Start;

                    }
                    else if (The_CAN_Open_Or_Close_Statu == The_CAN_is_Already_Start)//如果系统已经建立了对CAN盒的连接>>>>>>>>>>>如果已经启动了CAN盒
                    {
                        VCI_ResetCAN(Current_Dev_Type, Current_Dev_Index, 0);
                        VCI_ResetCAN(Current_Dev_Type, Current_Dev_Index, 1);
                        CAN_Start_Or_Reset_Button.Text = "启动CAN";
                        The_CAN_Open_Or_Close_Statu = The_CAN_is_Not_Start;
                    }
                }
                else
                {
                    MessageBox.Show("设备类型暂未支持");
                }
            }
            else
            {
                MessageBox.Show("启动CAN按钮内部错误");
            }
        }
/*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/


/*==========================================系统时钟内部逻辑--负责接收数据========================================================*/
        unsafe private void Scan_Timer_Tick(object sender, EventArgs e)
        {
            UInt32 The_NumOfFrame_is_Received = new UInt32(); //已经被写入缓，未被读取的帧的数目;

            The_NumOfFrame_is_Received = VCI_GetReceiveNum(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery);

            if (The_NumOfFrame_is_Received == 0)
            {
                return;
            }
            //MessageBox.Show(The_NumOfFrame_is_Received.ToString());
            UInt32 Received_Max_Len = 50;

            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)Received_Max_Len); // 指向某一段内存的指针。

            The_NumOfFrame_is_Received = VCI_Receive(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery, pt,
                Received_Max_Len, CAN_Receive_Wait_Time_100mS);

            String Message_To_Show = "";

            for (UInt32 count = 0; count<The_NumOfFrame_is_Received; count++)
            {
                VCI_CAN_OBJ obj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)((UInt32)pt + count * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));

                Message_To_Show = "接收到数据";

                Message_To_Show += "帧ID 0x:" + Convert.ToString((Int32)obj.ID, 16);

                Message_To_Show += " 帧格式:";

                if (obj.RemoteFlag == RemoteFlag_DataFrame)
                {
                    Message_To_Show += " 数据帧 ";
                }
                else if (obj.RemoteFlag == RemoteFlag_RemoteFrame)
                {
                    Message_To_Show += " 远程帧 ";
                }

                if (obj.ExternFlag == ExternFlag_StandardFrame)
                {
                    Message_To_Show += " 标准帧 ";
                }
                else if (obj.ExternFlag == ExternFlag_ExternFrame)
                {
                    Message_To_Show += " 拓展帧 ";
                }

                if (obj.RemoteFlag == RemoteFlag_DataFrame)
                {
                    byte len = (byte)(obj.DataLen % 9);

                    byte j = 0;

                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[0], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[1], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[2], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[3], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[4], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[5], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[6], 16);
                    if (j++ < len)
                        Message_To_Show += " " + System.Convert.ToString(obj.Data[7], 16);
                }

                listBox1.Items.Add(Message_To_Show);


            }

            Marshal.FreeHGlobal(pt);

        }
/*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/




/*=======================================发送数据的配置=========================================================================*/
        unsafe private void CAN_SendMessage_Button_Click(object sender, EventArgs e)
        {
            if(The_Device_Statu== The_Device_is_close)
                return;

            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.SendType = (byte)Send_Mode_Set_Combox.SelectedIndex;
            sendobj.RemoteFlag = (byte)Data_Remote_Combox.SelectedIndex;
            sendobj.ExternFlag = (byte)Extern_Standard_Set_Combox.SelectedIndex;
            sendobj.ID = System.Convert.ToUInt32("0x" + ID_TextBox.Text, 16);
            int len = (Data_TextBox.Text.Length + 1) / 3;
            sendobj.DataLen = System.Convert.ToByte(len);
            String strdata = Data_TextBox.Text;
            int i = -1;
            if (i++ < len - 1)
                sendobj.Data[0] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[1] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[2] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[3] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[4] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[5] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[6] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            if (i++ < len - 1)
                sendobj.Data[7] = System.Convert.ToByte("0x" + strdata.Substring(i * 3, 2), 16);
            int nTimeOut = 3000;

            VCI_SetReference(Current_Dev_Type,Current_Dev_Index, Current_Can_Gallery, 4, (byte*)&nTimeOut);

            if (VCI_Transmit(Current_Dev_Type, Current_Dev_Index, Current_Can_Gallery, ref sendobj, 1) == 0)
            {
                MessageBox.Show("发送失败", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


    }


  
}

