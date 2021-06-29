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
using System.IO;
using ModBus_RTU;
using System.Threading;
using ZedGraph;
using LibUsbDotNet.Main;
using LibUsbDotNet;



namespace READ_TEXT485
{
    public partial class AGV_ver1 : Form
    {
        GraphPane GraphPane;
        MySQL_dosomething mySQL = new MySQL_dosomething();
        ModBus_RS485 _RS485 = new ModBus_RS485();
        ModBus_RS485 _RS232 = new ModBus_RS485();
        private static string root = Application.StartupPath;
        private static string File1 = "Config_1.txt";
        private static string File2 = "Config_2.txt";
        string text = root + "/" + File1;
        string text2 = root + "/" + File2;
        Int16[] Registers = new Int16[12];
        Int16[] WRegisters16 = new Int16[12];
        BackgroundWorker BackgroundWorker1 = new BackgroundWorker();
        BackgroundWorker Rotate_BGR = new BackgroundWorker();
        System.Windows.Forms.Timer Timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer Timer_main = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer RFID_timer = new System.Windows.Forms.Timer();
        FileSystemWatcher FileSystemWatcher = new FileSystemWatcher();
        App_Config App_Config;
        private static UsbDevice HID_RFID;
        private static UsbDeviceFinder UsbDeviceFinder = new UsbDeviceFinder(1133, 50498);
        UsbEndpointReader reader;
        public AGV_ver1()
        {
            InitializeComponent();

            if (!File.Exists(text))
            {
                File.Create(text);
            }
            if (!File.Exists(text2))
            {
                File.Create(text2);
            }
            pictureBox1.Enabled = false;
            pictureBox2.Enabled = false;
            pictureBox3.Enabled = false;
            pictureBox4.Enabled = false;
            BackgroundWorker1.DoWork += BackgroundWorker1_DoWork;
            BackgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
            BackgroundWorker1.WorkerSupportsCancellation = true;
            Rotate_BGR.DoWork += Rotate_BGR_DoWork;
            Rotate_BGR.RunWorkerCompleted += Rotate_BGR_RunWorkerCompleted;
            Rotate_BGR.WorkerSupportsCancellation = true;
            
            Timer.Interval = 50;
            Timer.Enabled = false;
            Timer.Tick += Timer_Tick;
            Timer_main.Interval = 1000;
            Timer_main.Enabled = false;
            Timer_main.Tick += Timer_main_Tick;
            RFID_timer.Interval = 100;
            RFID_timer.Enabled = false;
            RFID_timer.Tick += RFID_timer_Tick;
            
        }

       

        int dem = 0;
        string ID = string.Empty;
        private void RFID_timer_Tick(object sender, EventArgs e)
        {
            dem++;
            if (dem == 1)
            {
                MethodInvoker inv = delegate
                {
                   
                    ID = textBox14.Text;
                    textBox11.Text = ID;
                };
                this.Invoke(inv);         
            }
            else if (dem == 2) 
            {
                if (textBox14.Text != "") textBox14.Text = "";
                dem = 0;
                if (RFID_timer.Enabled) 
                {
                    RFID_timer.Stop();
                    RFID_timer.Enabled = false;
                }
            }
        }

        private void Timer_main_Tick(object sender, EventArgs e)
        {
            dem_lost++;
            if (textBox1.Text != "0" && IO_Check && textBox1.Text != "")  
            {
                out_put1[6] = '1';
                out_put1[7] = '1';
                PLC_WRegister[0] = BinaryToShort(""+out_put1[0]
                    +out_put1[1]
                    +out_put1[2]
                    +out_put1[3]
                    +out_put1[4]
                    +out_put1[5]
                    +out_put1[6]
                    +out_put1[7]);
            }
            else if (textBox1.Text == "0" && IO_Check)
            {
                out_put1[6] = '0';
                out_put1[7] = '0';
                PLC_WRegister[0] = BinaryToShort("" + out_put1[0]
                    + out_put1[1]
                    + out_put1[2]
                    + out_put1[3]
                    + out_put1[4]
                    + out_put1[5]
                    + out_put1[6]
                    + out_put1[7]);
            }


            if (!textBox14.Focused&&!Start_btn.Enabled) 
            {
                textBox14.Focus();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            PIDspeed();
            Zedgraph();
            
        }
        private void Rotate_BGR_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (rotated)
            {
                rotated = false;
                Start_btn.Hide();
                button10.Show();
            }
            else 
            {
                rotated = true;
                button10.Hide();
                Start_btn.Show();
                WRegisters16[5] = 0;
                if(!Start_btn.Enabled && !Timer.Enabled) 
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }
            } 
            pictureBox5.Hide();
        }

        private void Rotate_BGR_DoWork(object sender, DoWorkEventArgs e)
        {
            while (textBox2.Text != "0" && textBox2.Text != "1" && textBox2.Text != "-1") 
            {            
                    pictureBox5.Show();              
            }
        }
        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            if (!BackgroundWorker1.IsBusy && !chay)
            {
                BackgroundWorker1.RunWorkerAsync();
            }
            
        }
        bool chay = false;
        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (BackgroundWorker1.CancellationPending)
            {
                e.Cancel = true;
                chay = true;
            }
            //Read_agv();
            read_PLC();


            //Read_From_Text(ref RFID);



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect_btn.Enabled = true;
            Discon_btn.Enabled = false;
            Start_btn.Enabled = false;
            Stop_btn.Enabled = false;
            pictureBox5.Hide();
            string[] serial_port = SerialPort.GetPortNames();
            foreach (string item in serial_port)
            {
                ComP_box.Items.Add(item);
            }
            string[] baud = new string[] { "9600", "19200", "38400", "57600", "115200" };
            foreach (var item in baud)
            {
                Baud_box.Items.Add(item);
            }
            Parity[] parities = new Parity[] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
            foreach (var item in parities)
            {
                Parity_box.Items.Add(item);
            }
            StopBits[] stopBits = new StopBits[] { StopBits.One, StopBits.None, StopBits.Two, StopBits.OnePointFive };
            foreach (var item in stopBits)
            {
                StopB_box.Items.Add(item);
            }
           

            WRegisters16[0] = 0;    //2000 Reset fault
            WRegisters16[1] = 0;    //2001 Enable AGV
            WRegisters16[2] = 0;    //2002 Dir AGV, allways is 0
            WRegisters16[3] = 0;    //2003 Turn left/right, ưu tiên rẻ trái hoặc phải khi có line từ nhánh
            WRegisters16[4] = 0;    //2004 Speed AGV
            WRegisters16[5] = 0;    //2005 Mode, 0 là auto, 1 là manual sẽ sử dụng các WRegister 6-11 khi đó ko sử dụng line từ
            WRegisters16[6] = 1;    //2006 Mode Motor A
            WRegisters16[7] = 1;    //2007 Mode Motor B
            WRegisters16[8] = 1;    //2008 Dir Motor A
            WRegisters16[9] = 0;    //2009 Dir Motor B,  A-B: 1-0 đi tới, A-B: 0-1 đi lùi, tương tự rẻ trái phải
            WRegisters16[10] = 0;   //2010 Speed Motor A
            WRegisters16[11] = 0;   //2011 Speed Motor B
            PLC_WRegister[0] = 0;
            PLC_WRegister[1] = 0;

            for (int i = 0; i < out_put1.Length; i++)
            {
                out_put1[i] = '0';
                out_put2[i] = '0';
                in_put1[i] = '0';
                in_put2[i] = '0';
            }
            //FileSystemWatcher = new FileSystemWatcher(text);
            //FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            //FileSystemWatcher.Filter = "*.txt";
            //FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            //FileSystemWatcher.EnableRaisingEvents = true;
             App_Config = Configxml.GetSystem_Config();
            if (App_Config.COM == "") ComP_box.SelectedIndex = 0;
            else if (App_Config.COM != "") 
            {
                foreach (var item in ComP_box.Items)
                {
                    if (item.ToString() == App_Config.COM) 
                    {
                        ComP_box.SelectedItem = App_Config.COM;
                        break;
                    }
                    else 
                    {
                        ComP_box.Text = "" + App_Config.COM + " Do Not Exist";
                    }
                }
            }
           
            //else ComP_box.SelectedItem = App_Config.COM;
            if (App_Config.COM == "") Baud_box.SelectedIndex = 4;
            else Baud_box.SelectedItem = App_Config.Baud;
            Kp = float.Parse(App_Config.Kp);
            Ki = float.Parse(App_Config.Ki);
            Kd = float.Parse(App_Config.Kd);
            Parity_box.SelectedIndex = 0;
            StopB_box.SelectedIndex = 0;
            try
            {
               
                DataTable dataTable = mySQL.Get_Database_Name();
                foreach (DataRow item in dataTable.Rows)
                {
                    string database_Name = item["database_name"].ToString();
                    comboBox1.Items.Add(database_Name);
                }
                foreach (var item in comboBox1.Items)
                {
                    if (item.ToString() == App_Config.Database)
                    {
                        comboBox1.SelectedItem = App_Config.Database;
                    }
                    else
                    {
                        comboBox1.Text = "" + App_Config.Database + " Do Not Exist";
                    }
                }

                if (mySQL.error_message != string.Empty) throw new Exception();
            }
            catch (Exception ex)
            {
                comboBox1.Text = "SQL Error";
                
            }
            
            

            try
            {
               
                List<string> dataTable = mySQL.Get_table_Name(App_Config.Database);
                foreach (string item in dataTable)
                {
                    comboBox2.Items.Add(item);
                }
                foreach (var item in comboBox2.Items)
                {
                    if (item.ToString() == App_Config.Table) 
                    {
                        comboBox2.SelectedItem = App_Config.Table;
                    }
                    else 
                    {
                        comboBox2.Text = "Table Do Not Exist";
                    }
                }
                if (mySQL.error_message != string.Empty) throw new Exception();
            }
            catch (Exception ex)
            {
                comboBox2.Text = "SQL Error";
            }
            comboBox2.SelectedItem = App_Config.Table;
            GraphPane = zedGraphControl1.GraphPane;
            GraphPane.Title.Text = "Graph of speed data over time";
            GraphPane.XAxis.Title.Text = "Timer(s)";
            GraphPane.YAxis.Title.Text = "Speed";
            RollingPointPairList rollingPointPairList = new RollingPointPairList(60000);
            LineItem lineItem = GraphPane.AddCurve("Data", rollingPointPairList, Color.Red, SymbolType.None);
            GraphPane.XAxis.Scale.Min = 0;
            GraphPane.XAxis.Scale.Max = 5;
            GraphPane.XAxis.Scale.MinorStep = 0.05f;
            GraphPane.XAxis.Scale.MajorStep = 1;
            GraphPane.YAxis.Scale.Min = 0;
            GraphPane.YAxis.Scale.Max = 200;
            //GraphPane.YAxis.Scale.MinorStep = 10;
            //GraphPane.YAxis.Scale.MajorStep = 100;
            GraphPane.AxisChange();
            //build_data();
          

        }
        private void ClearZelGraph() 
        {
            zedGraphControl1.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl1.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            GraphPane = zedGraphControl1.GraphPane;
            GraphPane.Title.Text = "Graph of speed data over time";
            GraphPane.XAxis.Title.Text = "Timer(s)";
            GraphPane.YAxis.Title.Text = "Speed";
            RollingPointPairList rollingPointPairList = new RollingPointPairList(60000);
            LineItem lineItem = GraphPane.AddCurve("Data", rollingPointPairList, Color.Red, SymbolType.None);
            GraphPane.XAxis.Scale.Min = 0;
            GraphPane.XAxis.Scale.Max = 5;
            GraphPane.XAxis.Scale.MinorStep = 0.05f;
            GraphPane.XAxis.Scale.MajorStep = 1;
            GraphPane.YAxis.Scale.Min = 0;
            GraphPane.YAxis.Scale.Max = 200;
            //GraphPane.YAxis.Scale.MinorStep = 10;
            //GraphPane.YAxis.Scale.MajorStep = 100;
            zedGraphControl1.AxisChange();
        }
        //private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        //{

        //}

        #region Connection
        private void Connect_btn_Click(object sender, EventArgs e)
        {
            //string port = ComP_box.Text;
            string port = App_Config.COM;
            //int baud = int.Parse(Baud_box.Text);
            int baud = int.Parse(App_Config.Baud);
            Parity parity = (Parity)Parity_box.SelectedItem;
            StopBits stopBits = (StopBits)StopB_box.SelectedItem;
            
            //try
            //{
            //    serialPort1.PortName = "COM4";
            //    serialPort1.BaudRate = 115200;
            //    serialPort1.Open();
            //}
            //catch (Exception ex1)
            //{

            //    MessageBox.Show("RFID " + ex1.Message);
            //}
          
            bool open = _RS485.Opened(port, baud, 8, parity, stopBits);
           
            if (open )
            {
                MessageBox.Show("<RS485>: "+_RS485.Modbus_status ) ;
                Connect_btn.Enabled = false;
                Discon_btn.Enabled = true;
                Start_btn.Enabled = true;
                if (!BackgroundWorker1.IsBusy) 
                {
                    chay = false;
                    BackgroundWorker1.RunWorkerAsync();
                }
                if (!Timer_main.Enabled) 
                {
                    Timer_main.Enabled = true;
                    Timer_main.Start();
                }
            }
            else
            {
                Connect_btn.Enabled = true;
                Discon_btn.Enabled = false;
                MessageBox.Show("<RS485>: " + _RS485.Modbus_status );
                return;
            }
        }

        private void Discon_btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Stop_btn.Enabled) Stop_btn.PerformClick();
                //if (serialPort1.IsOpen) serialPort1.Close();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Stop button click " + ex.Message);
            }
            bool Close = _RS485.Closed();

            if (BackgroundWorker1.IsBusy) 
            {
                chay = true;
                BackgroundWorker1.CancelAsync();
               
            }
            if (Timer_main.Enabled) 
            {
                Timer_main.Stop();
                Timer_main.Enabled = false;
            }
            if (Close ) 
            {
                MessageBox.Show("<RS485>: " + _RS485.Modbus_status );
                Connect_btn.Enabled = true;
                Discon_btn.Enabled = false;
                if (BackgroundWorker1.IsBusy) 
                {
                    chay = true;
                    BackgroundWorker1.CancelAsync();
                }
            }
            else
            {
                Connect_btn.Enabled = false;
                Discon_btn.Enabled = true;
                MessageBox.Show("<RS485>: " + _RS485.Modbus_status );
                return;
            }
        }
        #endregion
        #region SubFuncTion
        private void Zedgraph() 
        {
            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;
            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            if (curve == null)
                return;

            IPointListEdit list = curve.Points as IPointListEdit;
            if (list == null)
                return;
            list.Add(tickStart, OUT);
            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;
            if (tickStart > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = tickStart + xScale.MajorStep;
                xScale.Min = xScale.Max - 5;
            }
            if (OUT > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = OUT + yScale.MajorStep;
            }
            else if (OUT < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = OUT - yScale.MajorStep;
            }
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }
        short[] PLC_Register_input = new Int16[2];
        short[] PLC_Register_output = new Int16[2];

        Int16[] PLC_WRegister = new Int16[2];
      
        char[] in_put1 = new char[8];
        char[] out_put1 = new char[8];
        char[] in_put2 = new char[8];
        char[] out_put2 = new char[8];
        bool IO_Check = false;
        private void read_PLC() 
        {
            try
            {
                
                bool read_result = _RS485.SendFc01(2, 0, 16, ref PLC_Register_output);
                bool read_result2 = _RS485.SendFc02(2, 0, 16, ref PLC_Register_input);
                if (!read_result || !read_result2)
                {
                    panel2.BackColor = Color.Yellow;
                }
                else 
                {
                    panel2.BackColor = Color.Green;
                    MethodInvoker inv = delegate
                    {
                        var input1 = Convert.ToString(PLC_Register_input[0], 2);
                        var input2 = Convert.ToString(PLC_Register_input[1], 2);
                        var output1 = Convert.ToString(PLC_Register_output[0], 2);
                        var output2 = Convert.ToString(PLC_Register_output[1], 2);
                        string in1 = input1.PadLeft(8, '0');
                        string in2 = input2.PadLeft(8, '0');
                        string out1 = output1.PadLeft(8, '0');
                        string out2 = output2.PadLeft(8, '0');
                        for (int i = 0; i < 8; i++)
                        {
                            in_put1[i] = in1[i];
                            in_put2[i] = in2[i];
                            out_put1[i] = out1[i];
                            out_put2[i] = out2[i];
                        }

                        textBox12.Text = in1 + in2;
                        textBox13.Text = out1 + out2;
                    }; this.Invoke(inv);
                }

               
                bool write = _RS485.SendFc15(2, 0, 16, PLC_WRegister);
                if (!write)
                {
                    panel2.BackColor = Color.Yellow;
                }
                else 
                {
                    panel2.BackColor = Color.Green;                 
                }
                if (write && read_result && read_result2)
                {
                    IO_Check = true;
                }
                else IO_Check = false;
            }
            catch (Exception ex)
            {

                MessageBox.Show("[Read PLC]: " + ex.Message );
            }
        }
        private void IO_State() 
        {
            for (int i = 0; i < 8; i++)
            {
                switch (i)
                {
                    case 7:
                        if (in_put1[i] == '1') X0.Checked = true;
                        else X0.Checked = false;
                        if (in_put2[i] == '1') X8.Checked = true;
                        else X8.Checked = false;
                        if (out_put1[i] == '1') Y0.Checked = true;
                        else Y0.Checked = false;
                        if (out_put2[i] == '1') Y8.Checked = true;
                        else Y8.Checked = false;
                        break;
                    case 6:
                        if (in_put1[i] == '1') X1.Checked = true;
                        else X1.Checked = false;
                        if (in_put2[i] == '1') X9.Checked = true;
                        else X9.Checked = false;
                        if (out_put1[i] == '1') Y1.Checked = true;
                        else Y1.Checked = false;
                        if (out_put2[i] == '1') Y9.Checked = true;
                        else Y9.Checked = false;
                        break;
                    case 5:
                        if (in_put1[i] == '1') X2.Checked = true;
                        else X2.Checked = false;
                        if (in_put2[i] == '1') XA.Checked = true;
                        else XA.Checked = false;
                        if (out_put1[i] == '1') Y2.Checked = true;
                        else Y2.Checked = false;
                        if (out_put2[i] == '1') YA.Checked = true;
                        else YA.Checked = false;
                        break;
                    case 4:
                        if (in_put1[i] == '1') X3.Checked = true;
                        else X3.Checked = false;
                        if (in_put2[i] == '1') XB.Checked = true;
                        else XB.Checked = false;
                        if (out_put1[i] == '1') Y3.Checked = true;
                        else Y3.Checked = false;
                        if (out_put2[i] == '1') YB.Checked = true;
                        else YB.Checked = false;
                        break;
                    case 3:
                        if (in_put1[i] == '1') X4.Checked = true;
                        else X4.Checked = false;
                        if (in_put2[i] == '1') XC.Checked = true;
                        else XC.Checked = false;
                        if (out_put1[i] == '1') Y4.Checked = true;
                        else Y4.Checked = false;
                        if (out_put2[i] == '1') YC.Checked = true;
                        else YC.Checked = false;
                        break;
                    case 2:
                        if (in_put1[i] == '1') X5.Checked = true;
                        else X5.Checked = false;
                        if (in_put2[i] == '1') XD.Checked = true;
                        else XD.Checked = false;
                        if (out_put1[i] == '1') Y5.Checked = true;
                        else Y5.Checked = false;
                        if (out_put2[i] == '1') YD.Checked = true;
                        else YD.Checked = false;
                        break;
                    case 1:
                        if (in_put1[i] == '1') X6.Checked = true;
                        else X6.Checked = false;
                        if (in_put2[i] == '1') XE.Checked = true;
                        else XE.Checked = false;
                        if (out_put1[i] == '1') Y6.Checked = true;
                        else Y6.Checked = false;
                        if (out_put2[i] == '1') YE.Checked = true;
                        else YE.Checked = false;
                        break;
                    case 0:
                        if (in_put1[i] == '1') X7.Checked = true;
                        else X7.Checked = false;
                        if (in_put2[i] == '1') XF.Checked = true;
                        else XF.Checked = false;
                        if (out_put1[i] == '1') Y7.Checked = true;
                        else Y7.Checked = false;
                        if (out_put2[i] == '1') YF.Checked = true;
                        else YF.Checked = false;
                        break;

                }
            }
        }
        private void Read_agv()
        {
            try
            {
                bool Read_result = _RS485.SendFc04(1, 1000, 12, ref Registers);
                if (!Read_result)
                {
                    panel2.BackColor = Color.Red;
                }
                else panel2.BackColor = Color.Green;
                bool Write_result = _RS485.SendFc16(1, 2000, 12, WRegisters16);
                if (!Write_result)
                {
                    panel2.BackColor = Color.Orange;
                }
                else panel2.BackColor = Color.Green;
                MethodInvoker invoker = delegate
                {
                    textBox1.Text = Registers[7].ToString();
                    textBox4.Text = Registers[6].ToString();
                    textBox2.Text = Registers[9].ToString();
                    textBox3.Text = Registers[5].ToString();
                    progressBar1.Value = Registers[0];
                }; this.Invoke(invoker);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }
        private void W2Text(string[] parameter)
        {
            StreamWriter sw = new StreamWriter(text);
            foreach (var item in parameter)
            {
                sw.WriteLine(item);
            }
            sw.Close();
        }
        int[,] RFID = new int[,]
        {          
            { }, 
            { }       
        };
        private void Read_From_Text(ref int[,] RFID) 
        {
            try
            {
                int dong = 0;
                int cot = 0;
                using (StreamReader sr = new StreamReader(text2))
                {
                    string[] dem_cot = sr.ReadLine().Split(' ');
                    cot = dem_cot.Count();
                }
                using (StreamReader sr = new StreamReader(text2))
                {
                    while (sr.ReadLine() != null)
                    {
                        dong++;
                    }
                }
                RFID = new int[dong - 1, cot];
                using (StreamReader sr = new StreamReader(text2))
                {
                    for (int i = 0; i < dong; i++)
                    {
                        string[] element = sr.ReadLine().Split(' ');
                        if (i != 0)
                        {
                            for (int j = 0; j < cot; j++)
                            {
                                RFID[i - 1, j] = int.Parse(element[j]);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void load_action(DataTable dataTable) 
        {
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    RFID[i, j] = dataTable.Rows[i].Field<int>(j);
                }
            }
        }
        private void Triger_RFID(string RFID_ID) 
        {
            try
            {
                for (int i = 0; i < RFID.GetLength(0); i++)
                {
                    if (RFID_ID == RFID[i, 0].ToString())
                    {
                        for (int j = 1; j < RFID.GetLength(0); j++)
                        {
                            WRegisters16[i] = (short)RFID[i, j];
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
              

                MessageBox.Show(ex.Message);
            }
        }
        private void read_rs232(string data_inSP2)
        {
            //string in_rfid = string.Empty;
            //if (data_inSP2.StartsWith("s") && data_inSP2.Contains("e"))
            //{
            //    int vitri_e = data_inSP2.IndexOf("e") - 1;
            //    in_rfid = data_inSP2.Substring(1, vitri_e);
                Compare_RFID(data_inSP2);
            //    MethodInvoker inv = delegate
            //    {
            //        textBox11.Text = in_rfid;
            //    }; this.Invoke(inv);

            //}
            //else 
            //{
            //    serialPort1.Write("A");
            //}
        }
        bool rotated = true;
        private void Rotate(int speed, ref bool ahead) 
        {
            WRegisters16[5] = 1;
            if (ahead) 
            {
                WRegisters16[8] = 1;
                WRegisters16[9] = 1;
                WRegisters16[10] = (short)speed;
                WRegisters16[11] =(short)speed;
                if (!Rotate_BGR.IsBusy) 
                {
                    Rotate_BGR.RunWorkerAsync();
                }
               
            }
            else
            {
                WRegisters16[8] = 0;
                WRegisters16[9] = 0;
                WRegisters16[10] = (short)speed;
                WRegisters16[11] = (short)speed;
                if (!Rotate_BGR.IsBusy) 
                {
                    Rotate_BGR.RunWorkerAsync();
                }
            }
            if (Timer.Enabled)
            {
                Timer.Stop();
                Timer.Enabled = false;
            }
        }
        private void Compare_RFID(string RFID_ID)
        {
            try
            {
                for (int i = 0; i < DataTable.Rows.Count; i++)
                {
                    if (RFID_ID == DataTable.Rows[i][0].ToString()) 
                    {
                        WRegisters16[4] = Convert.ToInt16(DataTable.Rows[i][5].ToString());
                        WRegisters16[3] = Convert.ToInt16(DataTable.Rows[i][4].ToString());
                        WRegisters16[1] = Convert.ToInt16(DataTable.Rows[i][3].ToString());
                        
                        if (DataTable.Rows[i][6].ToString() == "1") 
                        {
                            WRegisters16[8] = 1;
                            WRegisters16[9] = 0;
                        }
                        else if(DataTable.Rows[i][6].ToString() == "0") 
                        {
                            WRegisters16[4] = 0;
                            WRegisters16[5] = 1;
                            rotated = true;
                            Rotate(manual_Speed, ref rotated);
                           
                        }
                        if(DataTable.Rows[i][7].ToString() == "1") 
                        {
                            temp1[5] = '1';
                            short[] value = new short[2];
                            build_data();
                            value[0] = BinaryToShort(data_write1);
                            value[1] = BinaryToShort(data_Write2);
                            PLC_WRegister[0] = value[0];
                            PLC_WRegister[1] = value[1];
                        }
                        else if(DataTable.Rows[i][7].ToString() == "0") 
                        {
                            temp1[5] = '0';
                            short[] value = new short[2];
                            build_data();
                            value[0] = BinaryToShort(data_write1);
                            value[1] = BinaryToShort(data_Write2);
                            PLC_WRegister[0] = value[0];
                            PLC_WRegister[1] = value[1];
                        }
                        
                    }
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message + " " + mySQL.error_message);
            }

        }
        float error, last_error, last_pre_error, set_speed;
        float Kp, Ki, Kd, P, I, D;
        float T, Tchar = 0;
        float OUT, pre_out;
        float tickStart = 0;
        float Ychar = 0;
        bool auto = true;
        public void PIDspeed()
        {
            //var Chart = chart1.ChartAreas[0];
            if (auto) 
            {
                set_speed = float.Parse(textBox9.Text);
            }
            else 
            {
                set_speed = (float)300;
            }

            if (set_speed > Ychar)
            {
                Ychar = set_speed;
            }
            T = 0.05f;
            tickStart += T;
            error = (float)(set_speed - (Registers[5]));
            //last_error = 0;
            //last_pre_error = 0;
            Kp = float.Parse(textBox6.Text);
            Ki = float.Parse(textBox7.Text);
            Kd = float.Parse(textBox8.Text);
            P = Kp * (error - last_error);
            I = (float)(0.5 * Ki * T * (error + last_error));
            D = Kd / T * (error - 2 * last_error + last_pre_error);
            OUT = pre_out + P + I + D;
            last_pre_error = last_error;
            last_error = error;
            pre_out = OUT;
            WRegisters16[4] = (Int16)OUT;
          
            textBox5.Text = OUT.ToString();
            //if (Tchar - temp_time > 1000)
            //{
            //    chart1.Series.Clear();
            //    createNewSeries("PID");
            //    chart1.Series["PID"].ChartType = SeriesChartType.Line;
            //    chart1.Series["PID"].Color = Color.Red;
            //    chart1.Series[0].IsVisibleInLegend = false;
            //    Chart.AxisX.Minimum = Tchar;
            //}
            //Chart.AxisX.Maximum = Tchar;
            //Chart.AxisY.Maximum = Ychar * (1 + 0.3);


        }
        public static short BinaryToShort(string data)
        {
            List<byte> byteList = new List<byte>();
            string strHex = Convert.ToInt32(data, 2).ToString();

            short hex = Convert.ToInt16(strHex);
            //for (int i = 0; i < data.Length; i += 8)
            //{
            //    byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            //}
            //string hex = Encoding.ASCII.GetString(byteList.ToArray());
            return hex;
        }
        string data_write1 = string.Empty;
        string data_Write2 = string.Empty;
        char[] temp1 = new char[8] { '0', '0', '0', '0', '0', '0', '0', '0' };
        char[] temp2 = new char[8] { '0', '0', '0', '0', '0', '0', '0', '0' };
        private void build_data() 
        {

            data_write1 = temp1[0].ToString()
                + temp1[1].ToString()
                + temp1[2].ToString()
                + temp1[3].ToString()
                + temp1[4].ToString()
                + temp1[5].ToString()
                + temp1[6].ToString()
                + temp1[7].ToString();
            data_Write2 = temp2[0].ToString()
                + temp2[1].ToString()
                + temp2[2].ToString()
                + temp2[3].ToString()
                + temp2[4].ToString()
                + temp2[5].ToString()
                + temp2[6].ToString()
                + temp2[7].ToString();


        }
        #endregion
        #region AUTO/MANUAL

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Timer.Enabled)
            {
                Timer.Enabled = true;
                Timer.Start();
            }
            WRegisters16[6] = 1;    //2006 Mode Motor A
            WRegisters16[7] = 1;    //2007 Mode Motor B
            WRegisters16[8] = 0;    //2008 Dir Motor A
            WRegisters16[9] = 1;    //2009 Dir Motor B,  A-B: 1-0 đi tới, A-B: 0-1 đi lùi, tương tự rẻ trái phải
            WRegisters16[10] = 300;   //2010 Speed Motor A
            WRegisters16[11] = 300;   //2011 Speed Motor B
            pictureBox1.BorderStyle = BorderStyle.Fixed3D;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
           
            WRegisters16[6] = 0;    //2006 Mode Motor A
            WRegisters16[7] = 0;    //2007 Mode Motor B
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            if (Timer.Enabled)
            {
                Timer.Stop();
                Timer.Enabled = false;

            }
            error = 0;
            last_error = 0;
            last_pre_error = 0;
            Tchar = 0;
            pre_out = 0;
            OUT = 0;
           // WRegisters16[4] = 0;
            MethodInvoker inv = delegate
            {
                textBox5.Text = "0";
            }; this.Invoke(inv);
            Ychar = 0;
        }
        bool straight = false;
        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {

            if (!Timer.Enabled)
            {
                Timer.Enabled = true;
                Timer.Start();
            }
            WRegisters16[6] = 1;    //2006 Mode Motor A
            WRegisters16[7] = 1;    //2007 Mode Motor B
            WRegisters16[8] = 1;    //2008 Dir Motor A
            WRegisters16[9] = 0;    //2009 Dir Motor B,  A-B: 1-0 đi tới, A-B: 0-1 đi lùi, tương tự rẻ trái phải
            WRegisters16[10] = 300;   //2010 Speed Motor A
            WRegisters16[11] = 300;   //2011 Speed Motor B
            pictureBox3.BorderStyle = BorderStyle.Fixed3D;
        }

        private void pictureBox3_MouseUp(object sender, MouseEventArgs e)
        {
            
            WRegisters16[6] = 0;    //2006 Mode Motor A
            WRegisters16[7] = 0;    //2007 Mode Motor B
            pictureBox3.BorderStyle = BorderStyle.FixedSingle;
            if (Timer.Enabled)
            {
                Timer.Stop();
                Timer.Enabled = false;

            }
            error = 0;
            last_error = 0;
            last_pre_error = 0;
            Tchar = 0;
            pre_out = 0;
            OUT = 0;
            //WRegisters16[4] = 0;
            MethodInvoker inv = delegate
            {
                textBox5.Text = "0";
            }; this.Invoke(inv);
            Ychar = 0;
        }

        private void pictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Timer.Enabled)
            {
                Timer.Enabled = true;
                Timer.Start();
            }
            WRegisters16[6] = 1;    //2006 Mode Motor A
            WRegisters16[7] = 1;    //2007 Mode Motor B
            WRegisters16[8] = 0;    //2008 Dir Motor A
            WRegisters16[9] = 1;    //2009 Dir Motor B,  A-B: 1-0 đi tới, A-B: 0-1 đi lùi, tương tự rẻ trái phải
            WRegisters16[10] = 100;   //2010 Speed Motor A
            WRegisters16[11] = 400;   //2011 Speed Motor B
            pictureBox4.BorderStyle = BorderStyle.Fixed3D;
        }

        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            WRegisters16[6] = 0;    //2006 Mode Motor A
            WRegisters16[7] = 0;    //2007 Mode Motor B
            pictureBox4.BorderStyle = BorderStyle.FixedSingle;
            if (Timer.Enabled)
            {
                Timer.Stop();
                Timer.Enabled = false;

            }
            error = 0;
            last_error = 0;
            last_pre_error = 0;
            Tchar = 0;
            pre_out = 0;
            OUT = 0;
            //WRegisters16[4] = 0;
            MethodInvoker inv = delegate
            {
                textBox5.Text = "0";
            }; this.Invoke(inv);
            Ychar = 0;
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (!Timer.Enabled)
            {
                Timer.Enabled = true;
                Timer.Start();
            }
            WRegisters16[6] = 1;    //2006 Mode Motor A
            WRegisters16[7] = 1;    //2007 Mode Motor B
            WRegisters16[8] = 0;    //2008 Dir Motor A
            WRegisters16[9] = 1;    //2009 Dir Motor B,  A-B: 1-0 đi tới, A-B: 0-1 đi lùi, tương tự rẻ trái phải
            WRegisters16[10] = 400;   //2010 Speed Motor A
            WRegisters16[11] = 100;   //2011 Speed Motor B
            pictureBox2.BorderStyle = BorderStyle.Fixed3D;
        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            WRegisters16[6] = 0;    //2006 Mode Motor A
            WRegisters16[7] = 0;    //2007 Mode Motor B
            pictureBox2.BorderStyle = BorderStyle.FixedSingle;
            if (Timer.Enabled)
            {
                Timer.Stop();
                Timer.Enabled = false;

            }
            error = 0;
            last_error = 0;
            last_pre_error = 0;
            Tchar = 0;
            pre_out = 0;
            OUT = 0;
            //WRegisters16[4] = 0;
            MethodInvoker inv = delegate
            {
                textBox5.Text = "0";
            }; this.Invoke(inv);
            Ychar = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Auto") 
            {
                auto = false;
                button3.Text = "Manual";
                WRegisters16[5] = 1;
                pictureBox1.Enabled = true;
                pictureBox2.Enabled = true;
                pictureBox3.Enabled = true;
                pictureBox4.Enabled = true;
                //if (!BackgroundWorker1.IsBusy)
                //{
                //    chay = false;
                //    BackgroundWorker1.RunWorkerAsync();
                //}
                
            }
            else if(button3.Text == "Manual") 
            {
                auto = true;
                button3.Text = "Auto";
                WRegisters16[5] = 0;
                pictureBox1.Enabled = false;
                pictureBox2.Enabled = false;
                pictureBox3.Enabled = false;
                pictureBox4.Enabled = false;
                //if (BackgroundWorker1.IsBusy)
                //{
                //    chay = true;
                //    BackgroundWorker1.CancelAsync();

                //}

            }

        }
        #endregion
        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }
        #region Controller
        private void Start_btn_Click(object sender, EventArgs e)
        {
            if (!Discon_btn.Enabled) return;
            WRegisters16[1] = 1;
            Start_btn.Enabled = false;
            if (!Timer.Enabled) 
            {
                Timer.Enabled = true;
                Timer.Start();
            }
            //if (!BackgroundWorker1.IsBusy) 
            //{
            //    chay = false;
            //    BackgroundWorker1.RunWorkerAsync();
            //}
           
            Stop_btn.Enabled = true;
            Configxml.UpdateSystem_Config("Kp", Kp.ToString());
            Configxml.UpdateSystem_Config("Ki", Ki.ToString());
            Configxml.UpdateSystem_Config("Kd", Kd.ToString());
            textBox14.LostFocus += TextBox14_LostFocus;
        }
        int dem_lost = 0;
        private void TextBox14_LostFocus(object sender, EventArgs e)
        {
            dem_lost = 0;
        }

        private void Stop_btn_Click(object sender, EventArgs e)
        {
            error = 0;
            last_error = 0;
            last_pre_error = 0;
            Tchar = 0;
            pre_out = 0;
            OUT = 0;
            WRegisters16[4] = 0;
            textBox14.LostFocus -= TextBox14_LostFocus;

            //if (BackgroundWorker1.IsBusy)
            //{
            //    chay = true;
            //    BackgroundWorker1.CancelAsync();

            //}
            Stop_btn.Enabled = false;
            if (Timer.Enabled) 
            {
                Timer.Stop();
                Timer.Enabled = false;
            }
           
            MethodInvoker inv = delegate 
            {
                textBox5.Text = "0";
            };this.Invoke(inv);
            Ychar = 0;
            ClearZelGraph();
            WRegisters16[1] = 0;
            Start_btn.Enabled = true;
        }
        #endregion
        #region Status
        private void Reset_Fault_btn_Click(object sender, EventArgs e)
        {
            short[] value = new short[] { 1 };
            short[] value1 = new short[] { 0 };
            WRegisters16[0] = 0;
            bool Reset1 = _RS485.SendFc16(1, 2000, 1, value);
            if (!Reset1) 
            {
                MessageBox.Show("Reset Fault: " + _RS485.Modbus_status);

            }
            bool Reset0 = _RS485.SendFc16(1, 2000, 1, value1);
            if (!Reset0)
            {
                MessageBox.Show("Reset Fault: " + _RS485.Modbus_status);
            }
        }
        #endregion
        #region Events
        private void comboBox1_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                comboBox1.Items.Clear();
                DataTable dataTable = mySQL.Get_Database_Name();
                foreach (DataRow item in dataTable.Rows)
                {
                    string database_Name = item["database_name"].ToString();
                    comboBox1.Items.Add(database_Name);
                }
                if (mySQL.error_message != string.Empty) throw new Exception();
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]: " + ex.Message + "[SQL]: " + mySQL.error_message);
            }
        }
        string databases = string.Empty;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text!="")
            {
                databases = comboBox1.Text;
                Configxml.UpdateSystem_Config("Database", databases);
            }
          
        }

        private void comboBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (comboBox1.Text=="")
            {
                return;
            }
            else 
            {
                try
                {
                    comboBox2.Items.Clear();
                    List<string> dataTable = mySQL.Get_table_Name(databases);
                    foreach (string item in dataTable)
                    {
                        
                        comboBox2.Items.Add(item);
                    }
                    if (mySQL.error_message != string.Empty) throw new Exception();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("[SYSTEM]: " + ex.Message + "[SQL]: " + mySQL.error_message);
                }
            }
        }
        string table = string.Empty;
        int count_row = 0;

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        private void ComP_box_DropDown(object sender, EventArgs e)
        {
            ComP_box.Items.Clear();
            string[] serial_port = SerialPort.GetPortNames();
            foreach (string item in serial_port)
            {
                ComP_box.Items.Add(item);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
            if (DataTable == null) return;
            panel1.Dispose();
            this.panel1 = new Panel();
            this.label21 = new System.Windows.Forms.Label();
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.panel1.Location = new System.Drawing.Point(370, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(400, 330);
            this.panel1.TabIndex = 0;
            this.tabPage2.Controls.Add(this.panel1);
            this.panel1.Controls.Add(this.label21);

            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(351, 3);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(27, 13);
            this.label21.TabIndex = 0;
            this.label21.Text = "X:Y";
            panel1.MouseDown += panel1_MouseDown;
            int[,] temp = new int[DataTable.Rows.Count, 2];
            for (int i = 0; i < DataTable.Rows.Count; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    temp[i, j] = int.Parse(DataTable.Rows[i][j+1].ToString());
                }
            }
            for (int i = 0; i < DataTable.Rows.Count-1; i++)
            {
                int x1 = ( temp[i, 0] );
                int x2 = ( temp[i+1, 0]);
                int y1 = (panel1.Height - temp[i, 1] );
                int y2 = (panel1.Height - temp[i + 1, 1]);
                Draw(x1.ToString(), y1.ToString(), x2.ToString(), y2.ToString(),i+1);
            }

           
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            if (IO_Check) 
            {
                IO_State();
            }
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            if (IO_Check)
            {
                IO_State();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                short[] value = new short[2];
                value[0] = BinaryToShort(data_write1);
                value[1] = BinaryToShort(data_Write2);

                PLC_WRegister[0] = value[0];
                PLC_WRegister[1] = value[1];
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
           
        }

        private void Y0_CheckedChanged(object sender, EventArgs e)
        {
            if (Y0.Checked) temp1[7] = '1';
            else temp1[7] = '0';
            build_data();
        }
       
        private void Y1_CheckedChanged(object sender, EventArgs e)
        {
            if (Y1.Checked) temp1[6] = '1';
            else temp1[6] = '0';
            build_data();
        }

        private void Y2_CheckedChanged(object sender, EventArgs e)
        {
            if (Y2.Checked) temp1[5] = '1';
            else temp1[5] = '0';
            build_data();
        }

        private void Y4_CheckedChanged(object sender, EventArgs e)
        {
            if (Y4.Checked) temp1[3] = '1';
            else temp1[3] = '0';
            build_data();
        }

        private void Y3_CheckedChanged(object sender, EventArgs e)
        {
            if (Y3.Checked) temp1[4] = '1';
            else temp1[4] = '0';
            build_data();
        }

        private void Y5_CheckedChanged(object sender, EventArgs e)
        {
            if (Y5.Checked) temp1[2] = '1';
            else temp1[2] = '0';
            build_data();
        }

        private void Y6_CheckedChanged(object sender, EventArgs e)
        {
            if (Y6.Checked) temp1[1] = '1';
            else temp1[1] = '0';
            build_data();
        }

        private void Y7_CheckedChanged(object sender, EventArgs e)
        {
            if (Y7.Checked) temp1[0] = '1';
            else temp1[0] = '0';
            build_data();
        }

        private void Y8_CheckedChanged(object sender, EventArgs e)
        {
            if (Y8.Checked) temp2[7] = '1';
            else temp2[7] = '0';
            build_data();
        }

        private void Y9_CheckedChanged(object sender, EventArgs e)
        {
            if (Y9.Checked) temp2[6] = '1';
            else temp2[6] = '0';
            build_data();
        }

        private void YA_CheckedChanged(object sender, EventArgs e)
        {
            if (YA.Checked) temp2[5] = '1';
            else temp2[5] = '0';
            build_data();
        }

        private void YB_CheckedChanged(object sender, EventArgs e)
        {
            if (YB.Checked) temp2[4] = '1';
            else temp2[4] = '0';
            build_data();
        }

        private void YC_CheckedChanged(object sender, EventArgs e)
        {
            if (YC.Checked) temp2[3] = '1';
            else temp2[3] = '0';
            build_data();
        }

        private void YD_CheckedChanged(object sender, EventArgs e)
        {
            if (YD.Checked) temp2[2] = '1';
            else temp2[2] = '0';
            build_data();
        }

        private void YE_CheckedChanged(object sender, EventArgs e)
        {
            if (YE.Checked) temp2[1] = '1';
            else temp2[1] = '0';
            build_data();
        }

        private void YF_CheckedChanged(object sender, EventArgs e)
        {
            if (YF.Checked) temp2[0] = '1';
            else temp2[0] = '0';
            build_data();
        }

        private void Reset_Fault_btn_MouseDown(object sender, MouseEventArgs e)
        {
            WRegisters16[0] = 1;
        }

        private void Reset_Fault_btn_MouseUp(object sender, MouseEventArgs e)
        {
            WRegisters16[0] = 0;
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            Thread t = new Thread(() => {
                try
                {
                    Compare_RFID(ID);
                }
                catch (Exception exx)
                {

                    MessageBox.Show(exx.Message);
                }
                //read_rs232(ID);
               
            });
            t.Start();
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            
            if (!RFID_timer.Enabled) 
            {
                RFID_timer.Enabled = true;
                RFID_timer.Start();
            }
        }

        private void ComP_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            Configxml.UpdateSystem_Config("COM", ComP_box.Text);
        }

        private void Baud_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            Configxml.UpdateSystem_Config("Baud", Baud_box.Text);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.Location.X;
            int y = panel1.Height- e.Location.Y;
            MethodInvoker inv = delegate 
            {
                int X = panel1.Width - label21.Size.Width-10;
                label21.Location = new Point(X, 3);
                label21.Text = "" + x.ToString() + ":" + y.ToString() + "";
            };
            this.Invoke(inv);
        }
        int manual_Speed = 100;
        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            try
            {
                manual_Speed = int.Parse(textBox15.Text);
            }
            catch (Exception )
            {
                MethodInvoker inv = delegate 
                {
                    manual_Speed = 100;
                    textBox15.Text = manual_Speed.ToString();
                };this.Invoke(inv);
                
                
            }
           
        }

        private void Continue_btn_Click(object sender, EventArgs e)
        {
            Rotate(manual_Speed, ref rotated);
        }

        bool hold = false;
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                
                if (comboBox2.Text == "") return;
                hold = false;
                table = comboBox2.Text;
                mySQL.Fill_data(databases, table, ref dataGridView1);
                DataTable = mySQL.Read_data(databases, table);               
                hold = true;              
                Configxml.UpdateSystem_Config("Table", table);
                panel1.Dispose();
                this.panel1 = new Panel();
                this.label21 = new System.Windows.Forms.Label();
                this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
                this.panel1.Location = new System.Drawing.Point(370, 3);
                this.panel1.Name = "panel1";
                this.panel1.Size = new System.Drawing.Size(400, 330);
                this.panel1.TabIndex = 0;
                this.tabPage2.Controls.Add(this.panel1);
                this.panel1.Controls.Add(this.label21);
                
                this.label21.AutoSize = true;
                this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.label21.Location = new System.Drawing.Point(351, 3);
                this.label21.Name = "label21";
                this.label21.Size = new System.Drawing.Size(27, 13);
                this.label21.TabIndex = 0;
                this.label21.Text = "X:Y";
                panel1.MouseDown += panel1_MouseDown;
                if (mySQL.error_message != string.Empty) throw new Exception();
                
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]: "+ex.Message + "[SQL]: " + mySQL.error_message);
            }
        }

        

        DataTable DataTable = new DataTable();
       

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (i > (dataGridView1.Rows.Count - 2)) break;
                    string cmd2 = "UPDATE " + table + " SET `X`= '" + dataGridView1.Rows[i].Cells[1].Value.ToString() + "'," +
                        "`Y`= '" + dataGridView1.Rows[i].Cells[2].Value.ToString() + "'," +
                        "`RUNSTOP`= '" + dataGridView1.Rows[i].Cells[3].Value.ToString() + "'," +
                        "`LR`= '" + dataGridView1.Rows[i].Cells[4].Value.ToString() + "'," +
                        "`SPEED`= '" + dataGridView1.Rows[i].Cells[5].Value.ToString() + "'," +
                        "`DIR`= '" + dataGridView1.Rows[i].Cells[6].Value.ToString() + "'" +
                        "`LIFT`= '" + dataGridView1.Rows[i].Cells[7].Value.ToString() + "'" +
                        " WHERE ID = '" + dataGridView1.Rows[i].Cells[0].Value.ToString() + "'";
                   bool b2= mySQL.SQL_command(cmd2, databases);
                    if ( !b2) 
                    {
                        throw new Exception();
                    }
                    
                }
                DataTable = mySQL.Read_data(databases, table);
                button7.PerformClick();
                MessageBox.Show("Update data successfully");
                
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]" + ex.Message + " :[SQL]" + mySQL.error_message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                hold = false;
                string cmd2 = "DROP TABLE IF EXISTS " + comboBox2.Text + "";
                string cmd = @"CREATE TABLE "+comboBox2.Text+" (ID VARCHAR(50), X TEXT, Y TEXT, RUNSTOP TEXT, LR TEXT, SPEED TEXT, DIR TEXT, LIFT TEXT)";
                bool check = mySQL.SQL_command(cmd2, databases);
                bool kq = mySQL.SQL_command(cmd, databases);
                if (!kq||!check)
                {
                    throw new Exception();
                }
                if (kq) 
                {
                    MessageBox.Show("Create successfully");
                }
                comboBox2.Items.Add(comboBox2.Text);
                mySQL.Fill_data(databases, comboBox2.Text,ref dataGridView1);
                table = comboBox2.Text;
                hold = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]: " + ex.Message + "[SQL]: " + mySQL.error_message);
            }
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
           
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                string cmd = "DROP TABLE IF EXISTS " + comboBox2.Text + "";
                bool check = mySQL.SQL_command(cmd, databases);
                comboBox2.Items.Clear();
                List<string> dataTable = mySQL.Get_table_Name(databases);
                foreach (string item in dataTable)
                {
                    comboBox2.Items.Add(item);
                }
                comboBox2.SelectedIndex = 0;
                if (mySQL.error_message != string.Empty) throw new Exception();
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]: " + ex.Message + "[SQL]: " + mySQL.error_message);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (hold)
                {
                    string data = dataGridView1.Rows[dataGridView1.Rows.Count - 2].Cells[0].Value.ToString();
                    string query = "INSERT INTO " + table + " (ID) VALUES('" + data + "')";
                    bool kq = mySQL.SQL_command(query, databases);
                    if (kq) 
                    {
                        MessageBox.Show("Added Successfully");
                    }
                    if (mySQL.error_message != "") throw new Exception();
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM]: " + ex.Message + "[SQL]: " + mySQL.error_message);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                int sel = dataGridView1.SelectedRows.Count;
                while (sel > 0) 
                {
                    if (!dataGridView1.SelectedRows[0].IsNewRow)
                    {
                        string query_delete = "DELETE FROM " + table + " WHERE `ID`='" + dataGridView1.SelectedRows[0].Cells[0].Value.ToString().Replace(" ",string.Empty) + "'";
                        bool kq = mySQL.SQL_command(query_delete, databases);
                        if (!kq)
                        {
                            throw new Exception();
                        }
                        dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                        sel--;
                    }
                }
                MessageBox.Show("Deleted successfully");

            }
            catch (Exception ex)
            {

                MessageBox.Show("[SYSTEM] " + ex.Message + "[SQL] " + mySQL.error_message);
            }
        }
        #endregion
        #region Drawing Move Point
        private void Draw(string x1, string y1,string x2,string y2,int point) 
        {
            Graphics graphics;
            StringFormat sf = new StringFormat();
            sf.FormatFlags = StringFormatFlags.DirectionRightToLeft;
            SolidBrush solidBrush = new SolidBrush(Color.Red);
            graphics = panel1.CreateGraphics();
            Pen pen = new Pen(Color.Lime, 3);
            int Y1 = panel1.Height - int.Parse(y1);
            int Y2 = panel1.Height - int.Parse(y2);
            graphics.DrawLine(pen, int.Parse(x1), int.Parse(y1), int.Parse(x2), int.Parse(y2));
            graphics.DrawString("["+point.ToString()+"](" + x1 + "," + Y1.ToString() + ")", this.Font, solidBrush, int.Parse(x1), int.Parse(y1),sf);
            graphics.DrawString("["+(point+1).ToString()+"](" + x2 + "," + Y2.ToString() + ")", this.Font, solidBrush, int.Parse(x2), int.Parse(y2), sf);
            //graphics.DrawLine(pen, 0, 0, 200, 200);

            
        }

        #endregion
    }
}
