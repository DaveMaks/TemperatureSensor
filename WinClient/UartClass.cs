using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace WinClient
{
    class UartClass
    {
        public static string BufferPort = "";
        public string Port = "";
        public SerialPort mySerialPort;
        public String Logs;

        public UartClass()
        {
            mySerialPort = new SerialPort();
            Logs += "============ Запуск =========" + Environment.NewLine;
        }

        public int InstallComPort()
        {
            try
            {
                mySerialPort.PortName = Port;
                mySerialPort.BaudRate = 9600;
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.DataBits = 8;
                mySerialPort.Handshake = Handshake.None;
                mySerialPort.DtrEnable = true;
                mySerialPort.DataReceived += DataReceivedHandler;
                mySerialPort.Open();
                Logs += "Порт открыт" + Environment.NewLine;
            }
            catch (Exception ex)
            {
                Logs += "Ошибка открытия порта " + ex.Message + Environment.NewLine;
                mySerialPort.Close();
            }
            return 0;
        }

        public int CloseComPort()
        {
            mySerialPort.Close();
            return 0;
        }

        public string[] GetList()
        {
            BufferPort = "";
            var dataArray = new byte[] { 0x6C };
            Logs += DateTime.Now.ToString("U") + "  =======   Отправка " + dataArray[0] + Environment.NewLine;
            mySerialPort.Write(dataArray, 0, 1);
            Application.DoEvents();
            System.Threading.Thread.Sleep(1000);
            Application.DoEvents();
            Logs += "Получено: " + Environment.NewLine + BufferPort + Environment.NewLine;
            return BufferPort.Split(new char[] { '\n' }, 10, StringSplitOptions.RemoveEmptyEntries);
        }

        public List<String[]> GetQuery()
        {
            List<String[]> ret = new List<string[]>();
            string[] t;
            char[] charsToTrim = { '\n', '\t', ' ' };
            BufferPort = "";
            mySerialPort.Write("q");
            Logs += DateTime.Now.ToString("U") + "  =======   Отправка " + "q " + Environment.NewLine;
            System.Threading.Thread.Sleep(1000);
            Logs += "Получено: " + "'"+BufferPort+"'" + Environment.NewLine;
            BufferPort=BufferPort.Trim(new char[] { ' ', '\n', '\t' });
            string[] LineReqest = BufferPort.Split(new char[] { '\n' }, 10, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < LineReqest.Length; j++)
            {
                LineReqest[j] = LineReqest[j].Trim();

                if (!Regex.IsMatch(LineReqest[j], @"^.{2}-.{2}-.{2}-.{2}-.{2}-.{2}-.{2}-.{2}:[\+\-]\d+\.\d", RegexOptions.IgnoreCase))
                {
                    Logs += "Пропущенно: " + LineReqest[j] + Environment.NewLine;
                    continue;
                }
                t = LineReqest[j].Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                ret.Add(new string[] { t[0], t[1] });
            }
            return ret;
        }

        public string[] GetListPorts()
        {
            return SerialPort.GetPortNames();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string sData = String.Empty;
            sData = mySerialPort.ReadExisting();
            if (sData != String.Empty)
            {
                BufferPort += sData;
                //SetText(sData);
            }
        }
    }
}
