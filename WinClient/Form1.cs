using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace WinClient
{
    public partial class Form1 : Form
    {
        BackgroundWorker bgGetUart;
        string Connect;

        Dictionary<string, string> UIDName;
        string[] ContineUID;
        DateTime LastTimeSendMail = DateTime.Now;
        public Form1()
        {
            InitializeComponent();
            bgGetUart = new BackgroundWorker();
            bgGetUart.WorkerReportsProgress = true;
            bgGetUart.DoWork += BgGetUart_DoWork;
            bgGetUart.RunWorkerCompleted += BgGetUart_RunWorkerCompleted;
            UIDName = new Dictionary<string, string>();
            string[] ls = File.ReadAllLines("./ListNameUID.dat");
            foreach (string line in ls)
            {
                string[] tm = line.Split(';');
                if (tm.Length > 1 && Regex.IsMatch(tm[0], @"^.{2}-.{2}-.{2}-.{2}-.{2}-.{2}-.{2}-.{2}", RegexOptions.IgnoreCase))
                {
                    UIDName.Add(tm[0], tm[1]);
                }
            }
            ContineUID = Properties.Settings.Default.ContineUID.Split(',');
        }

        private void BgGetUart_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pictureBox1.Visible = false;
            string Alarm = null;
            dataGridView1.Rows.Clear();
            ReqestPort ret = (ReqestPort)e.Result;
            if (ret.ListDevice != null)
            {
                for (int j = 0; j < ret.ListDevice.Count; j++)
                {

                    if (ret.ListDevice[j][0] != null)
                    {
                        ImgChart chart = new ImgChart();

                        chart.Size = new Size(300, 30);
                        chart.Data = GetHistoryT(ret.ListDevice[j][0], 60);
                        int id = dataGridView1.Rows.Add((UIDName.ContainsKey(ret.ListDevice[j][0]) ? UIDName[ret.ListDevice[j][0]] : ret.ListDevice[j][0]), ret.ListDevice[j][1] + " C°", chart.GetChart(), chart.Data.Values?.Min().ToString(), chart.Data.Values?.Max().ToString());
                        if (Array.IndexOf(ContineUID, ret.ListDevice[j][0]) >= 0)
                        {
                            chart = null;
                            continue;
                        }
                        if (chart.Data.Values?.Last() > Properties.Settings.Default.MaxTempAlarm)
                        {
                            dataGridView1.Rows[id].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 125, 125);
                            Alarm += "Превышение порога температуры (" + Properties.Settings.Default.MaxTempAlarm + "C°)" + ret.ListDevice[j][0] + " -> " + ret.ListDevice[j][1] + "C°" + "<br/>" +Environment.NewLine;
                        }
                    }
                }
                if (Alarm!=null && LastTimeSendMail < DateTime.Now.AddMinutes(-5))
                {
                    SendAlarmMail(Alarm);
                    LastTimeSendMail = DateTime.Now;
                    toolStripStatusLabel8.Text = "Последнее уведомл: " + LastTimeSendMail + "";
                }
                dataGridView1.ClearSelection();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                LogToFile(ret.log);
            }
            else
            {
                LogToFile(DateTime.Now.ToString("U") + "Ошибка, данные не получены." + Environment.NewLine);
            }
        }

        /// <summary>
        /// Запускаем отдельный процеес опроса ComPort'а, собираем показания с датчиков и ложим в базу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BgGetUart_DoWork(object sender, DoWorkEventArgs e)
        {
            UartClass uart = new UartClass();
            if (!uart.mySerialPort.IsOpen)
            {
                uart.Port = (string)e.Argument;
                uart.InstallComPort();
            }
            ReqestPort ret = new ReqestPort();

            try
            {
                ret.ListDevice = uart.GetQuery();
                ret.log = uart.Logs;
                if (ret.ListDevice != null)
                    using (MySqlConnection myConnection = new MySqlConnection(Connect))
                    {
                        myConnection.Open();

                        for (int j = 0; j < ret.ListDevice.Count(); j++)
                        {
                            try
                            {
                                if (ret.ListDevice[j][0] == null) continue;
                                string sql = "INSERT INTO " + Properties.Settings.Default.DBBaseName + ".td_TemperatureLog(  UID , value , date)" +
                                            "VALUES ( '" + @ret.ListDevice[j][0] + "', '" + @ret.ListDevice[j][1] + "' , NOW()); ";
                                MySqlCommand myCommand = new MySqlCommand(sql, myConnection);
                                myCommand.ExecuteNonQuery();
                            }
                            catch (MySqlException ex)
                            {
                                ret.log += "Ошибка :" + ex.Message + Environment.NewLine;
                            }
                        }
                        myConnection.Close();
                    }
                e.Result = ret;
            }
            catch (Exception ex)
            {
                ret.log += "!!!!!!!!! Ошибка :" + Environment.NewLine + ex.Message + Environment.NewLine + "!!!!!!!!!!!!!" + Environment.NewLine;
            }
            uart.mySerialPort.Close();
            e.Result = ret;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Interval = Properties.Settings.Default.TimeOut * 1000;
            if (timer1.Enabled)
            {
                button1.Text = "Старт";
                button1.Image = (Image)Properties.Resources.ResourceManager.GetObject("MediaPlay1"); //Resources.ResourceManager.GetObject("MediaStop");
                timer1.Enabled = false;
            }
            else
            {
                button1.Text = "Стоп";
                button1.Image = (Image)Properties.Resources.ResourceManager.GetObject("MediaStop1");
                timer1.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Properties.Settings.Default?.ComPort == "")
            {
                LogToFile(DateTime.Now.ToString("U") + "Не найден ComPort" + Environment.NewLine);
                return;
            }

            pictureBox1.Visible = true;
            bgGetUart.RunWorkerAsync(Properties.Settings.Default.ComPort);
      }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default?.ComPort == "")
            {
                MessageBox.Show("Не найден ComPort", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            pictureBox1.Visible = true;
            bgGetUart.RunWorkerAsync(Properties.Settings.Default.ComPort);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "Порт: " + Properties.Settings.Default.ComPort;
            toolStripStatusLabel2.Text = "Время опроса: " + Properties.Settings.Default.TimeOut + " c.";
            if (Properties.Settings.Default.IsStartDefault)
                toolStripStatusLabel3.Image = WinClient.Properties.Resources.if_icons_check_1564499___копия;
            toolStripStatusLabel4.Text = "Сервер БД: " + Properties.Settings.Default.DBHost;
            toolStripStatusLabel5.Text = "Имя БД: " + Properties.Settings.Default.DBBaseName;
            toolStripStatusLabel6.Text = "Пользователь БД: " + Properties.Settings.Default.DBLogin;
            toolStripStatusLabel7.Text = "Температура ув.: " + Properties.Settings.Default.MaxTempAlarm + "C°";
            Connect = "Database=" + Properties.Settings.Default.DBBaseName + ";Data Source=" + Properties.Settings.Default.DBHost + ";User Id=" + Properties.Settings.Default.DBLogin + ";Password=" + Properties.Settings.Default.DBPwd + "";

            if (Properties.Settings.Default.IsStartDefault)
            {
                timer1.Interval = Properties.Settings.Default.TimeOut * 1000;
                if (timer1.Enabled)
                {
                    button1.Text = "Старт";
                    button1.Image = (Image)Properties.Resources.ResourceManager.GetObject("MediaPlay1"); //Resources.ResourceManager.GetObject("MediaStop");
                    timer1.Enabled = false;
                }
                else
                {
                    button1.Text = "Стоп";
                    button1.Image = (Image)Properties.Resources.ResourceManager.GetObject("MediaStop1");
                    timer1.Enabled = true;
                }
            }
        }

        void LogToFile(string text)
        {
            File.AppendAllText(@"./log.log", text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            while (bgGetUart.IsBusy)
            {
                Thread.Sleep(1000);
            }

            Application.Exit();
        }

        public Dictionary<DateTime, decimal> GetHistoryT(string UID, int CountMinutHistory)
        {
            Dictionary<DateTime, decimal> ret = new Dictionary<DateTime, decimal>();
            using (MySqlConnection myConnection = new MySqlConnection(Connect))
            {
                myConnection.Open();
                string sql = @"SELECT ROUND(avg(CAST(`value` AS DECIMAL(5,1))),1) AS t,CAST(DATE_FORMAT(`date`,'%Y-%m-%d %H:%i') AS DATETIME) as dat FROM " +
                              @"" + Properties.Settings.Default.DBBaseName + ".td_TemperatureLog WHERE `date` > ADDDATE(NOW(), INTERVAL -" + CountMinutHistory.ToString() + " MINUTE)" +
                              @"AND UID = '" + @UID + "' " +
                              @"GROUP BY dat ORDER BY dat ASC;";
                MySqlCommand myCommand = new MySqlCommand(sql, myConnection);
                using (MySqlDataReader reader = myCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ret.Add(reader.GetDateTime("dat"), reader.GetDecimal("t"));
                    }
                    reader.Close();
                }
                myConnection.Close();
            }
            return ret;
        }

        public void SendAlarmMail(string text)
        {
            string[] mailsTo = Properties.Settings.Default.EmailTo.Split(',');
            MailMessage mail = new MailMessage(Properties.Settings.Default.EmailFrom, mailsTo[0]);
            if (mailsTo.Length > 1) {
                for (int a = 1; a < mailsTo.Length; a++)
                    mail.To.Add(mailsTo[a]);
            }
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential(Properties.Settings.Default.EmailLogin, Properties.Settings.Default.EmailPwd);
            client.Host = Properties.Settings.Default.EmailServer;
            mail.Subject = "ВАЖНО! Уведомление о изменении температуры! ";
            mail.Priority = MailPriority.High;
            mail.IsBodyHtml = true;
            mail.Body = "<h2>Важно!!</h2><br/>\n"+text;
            client.Send(mail);
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            SendAlarmMail("TEST");
        }
    }

    class ReqestPort
    {
        public List<String[]> ListDevice = null;
        public string log;
    }


    class ImgChart
    {
        public Size Size;
        public Color Color = Color.FromArgb(100, Color.Green);
        public Dictionary<DateTime, decimal> Data;
        const int CountMinutHistory = 60;
        const int MaxT = 30;

        public Image GetChart()
        {
            Image ret = new Bitmap(Size.Width, Size.Height);
            Graphics g = Graphics.FromImage(ret);
            float kWidth = (float)Size.Width / CountMinutHistory;
            float kHeight = (float)Size.Height / 2 / MaxT;
            g.DrawLine(new Pen(Color.Blue, 1), 0, Size.Height / 2, Size.Width, Size.Height / 2);
            if (Data.Count > 0)
            {
                float i = 0;
                DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                for (DateTime t = now.AddMinutes(-CountMinutHistory); t < now; t = t.AddMinutes(1))
                {
                    g.DrawLine(new Pen(Color, kWidth), i, Size.Height / 2, i, Size.Height / 2 - ((float)((Data.ContainsKey(t)) ? Data[t] : 0) * kHeight));
                    i += kWidth;
                }
            }
            return ret;
        }
    }
}
