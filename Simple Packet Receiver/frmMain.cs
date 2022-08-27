using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimplePacketLib;
using System.Diagnostics;

namespace Simple_Packet_Receiver
{
    public partial class frmMain : Form
    {
        SerialPort serialPort = new SerialPort(); // = new SerialPort();
        private int numDisplayChans = 8;
        private int numPacketsReceived = 0;
        //SimplePacket currPacket = new SimplePacket(;
        private SimplePacketParser packetizer = new SimplePacketParser();
        private SimplePacket packetBuffer = new SimplePacket();
        private Stopwatch stopwatch = new Stopwatch();

        public frmMain()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialReceivedHandler);
            
            //get com ports
            var ports = SerialPort.GetPortNames();
            cmbPort.DataSource = ports;

            if (ports.Contains<String>(Properties.Settings.Default.SelectedPort))
            {
                cmbPort.Text = Properties.Settings.Default.SelectedPort;
            }
        }

        private void btnAcquire_Click(object sender, EventArgs e)
        {
            serialPort.PortName = cmbPort.Text;
            serialPort.BaudRate = 921600;
            serialPort.Parity = Parity.None;
            serialPort.DataBits = 8;
            serialPort.StopBits = StopBits.One;
            serialPort.Open();
            //timer1.Enabled = true;
            stopwatch.Start();
        }

        private void SerialReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            //string indata = sp.ReadExisting();
            for (int i = 0; i < sp.BytesToRead; i++)
            {
                byte b = Convert.ToByte(sp.ReadByte());
                bool newPacket = this.packetizer.AddByte(b, packetBuffer);
                if (newPacket)
                {
                    this.Invoke((MethodInvoker)delegate { txtOutput.AppendText(Environment.NewLine); });
                    this.Invoke((MethodInvoker)delegate { txtOutput.AppendText(packetBuffer.StartFlag + " " + packetBuffer.Counter); });
                    for (int j = 0; j < packetBuffer.ChannelSet.Length; j++)
                    {
                        this.Invoke((MethodInvoker)delegate { txtOutput.AppendText(" " + packetBuffer.ChannelSet[j]); });
                    }
                    numPacketsReceived++;
                }
            }            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                serialPort.DataReceived -= SerialReceivedHandler;
                stopwatch.Reset();
            }
        }
    }
}
