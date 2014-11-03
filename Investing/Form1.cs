using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace Investing
{
    public partial class Form1 : Form
    {
        private bool inAdmin;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Security.Principal.WindowsIdentity MyIdent = System.Security.Principal.WindowsIdentity.GetCurrent();

            // Create a principal.
            System.Security.Principal.WindowsPrincipal MyPrincipal = new System.Security.Principal.WindowsPrincipal(MyIdent);

            // Check the role using a string.
            if (MyPrincipal.IsInRole(@"BUILTIN\Administrators"))
            {
                richTextBox1.Text += "[" + DateTime.Now.ToString() + "] - You are an administrator. \r\n";
                textBox1.Text = Application.StartupPath + "\\{{filename}}.csv";
                // textBox2.Text = "192.168.0.1:3128";
                //checkBox1.Checked = true;
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                button2_Click(null, null);
                inAdmin = true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("You are not an administrator.");
                inAdmin = false;
            }
        }

        private void SavePrediction()
        {
            try
            {
                var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                string eurcad_url = "http://ru.investing.com/currencies/eur-cad-technical?period=86400";
                string eurgbp_url = "http://ru.investing.com/currencies/eur-gbp-technical?period=86400";
                string audusd_url = "http://ru.investing.com/currencies/aud-usd-technical?period=86400";
                string audcad_url = "http://ru.investing.com/currencies/aud-cad-technical?period=86400";
                string gbpusd_url = "http://ru.investing.com/currencies/gbp-usd-technical?period=86400";



                string userAgentString = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Win64; x64; Trident/4.0; Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1) ; .NET CLR 2.0.50727; SLCC2; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; Tablet PC 2.0; .NET4.0C; .NET4.0E)";

                System.Net.WebClient client = new System.Net.WebClient();

                if (checkBox1.Checked && textBox2.Text != null)
                {
                    System.Net.WebProxy proxy = new System.Net.WebProxy(textBox2.Text);
                    proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    client.Proxy = proxy;
                }

                string DestStr = "";
                string SymbStr = "";
                for (int i = 0; i < 5; i++)
                {
                    if (i == 0)
                    {
                        string baseHtml = "";
                        client.Headers.Add("user-agent", userAgentString);
                        byte[] pageContent = client.DownloadData(eurcad_url);
                        UTF8Encoding utf = new UTF8Encoding();
                        baseHtml = utf.GetString(pageContent);
                        htmlDocument.LoadHtml(baseHtml);
                        SymbStr = "EURCAD;";
                    }
                    else if (i == 1)
                    {
                        string baseHtml = "";
                        client.Headers.Add("user-agent", userAgentString);
                        byte[] pageContent = client.DownloadData(eurgbp_url);
                        UTF8Encoding utf = new UTF8Encoding();
                        baseHtml = utf.GetString(pageContent);
                        htmlDocument.LoadHtml(baseHtml);
                        SymbStr = "EURGBP;";
                    }
                    else if (i == 2)
                    {
                        string baseHtml = "";
                        client.Headers.Add("user-agent", userAgentString);
                        byte[] pageContent = client.DownloadData(audusd_url);
                        UTF8Encoding utf = new UTF8Encoding();
                        baseHtml = utf.GetString(pageContent);
                        htmlDocument.LoadHtml(baseHtml);
                        SymbStr = "AUDUSD;";
                    }
                    else if (i == 3)
                    {
                        string baseHtml = "";
                        client.Headers.Add("user-agent", userAgentString);
                        byte[] pageContent = client.DownloadData(audcad_url);
                        UTF8Encoding utf = new UTF8Encoding();
                        baseHtml = utf.GetString(pageContent);
                        htmlDocument.LoadHtml(baseHtml);
                        SymbStr = "AUDCAD;";
                    }
                    else if (i == 4)
                    {
                        string baseHtml = "";
                        client.Headers.Add("user-agent", userAgentString);
                        byte[] pageContent = client.DownloadData(gbpusd_url);
                        UTF8Encoding utf = new UTF8Encoding();
                        baseHtml = utf.GetString(pageContent);
                        htmlDocument.LoadHtml(baseHtml);
                        SymbStr = "GBPUSD;";
                    }

                    HtmlNodeCollection nodes = htmlDocument.DocumentNode.SelectNodes("//span[@class='studySummaryOval buy arial_12 bold']");
                    if (nodes == null)
                        nodes = htmlDocument.DocumentNode.SelectNodes("//span[@class='studySummaryOval sell arial_12 bold']");
                    if (nodes == null)
                        nodes = htmlDocument.DocumentNode.SelectNodes("//span[@class='studySummaryOval neutral arial_12 bold']");


                    if (nodes != null)
                    {
                        foreach (var Item in nodes)
                        {
                            if (Item != null)
                            {
                                if (Item.InnerText == "АКТИВНО ПОКУПАТЬ" || Item.InnerText == "ПОКУПАТЬ")
                                    SymbStr += "1\r\n";
                                else if (Item.InnerText == "АКТИВНО ПРОДАВАТЬ" || Item.InnerText == "ПРОДАВАТЬ")
                                    SymbStr += "-1\r\n";
                                else
                                    SymbStr += "0\r\n";

                                DestStr += SymbStr;
                            }
                        }
                    }
                }

                richTextBox1.Text += "[" + DateTime.Now.ToString() + "] Prediction:\r\n";
                richTextBox1.Text += DestStr;
                DateTime dt_now = DateTime.Now;
                DateTime dt = dt_now.AddHours(1);
                string filename = dt.Day.ToString() + "_" + dt.Month.ToString() + "_" + dt.Year.ToString();
                string Path = textBox1.Text;
                string dest = ReplaceString(Path, "{{filename}}", filename, StringComparison.CurrentCulture);
                System.IO.File.WriteAllText(@dest, DestStr);
                richTextBox1.Text += "[" + DateTime.Now.ToString() + "] File:"+filename+".csv saved...\r\n";
            }
            catch (Exception ex)
            {
                richTextBox1.Text += "[" + DateTime.Now.ToString() + "] Exception:" + ex.Message + "....\r\n";
            }

        }

        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (!inAdmin)
                this.Close();

            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute == 40)
            {
                SavePrediction();
                timer1.Enabled = false;
                timer2.Enabled = true;
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            this.Show();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.ShowInTaskbar = false;
            this.Hide();
            notifyIcon1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "START")
            {
                if (textBox1.Text != "")
                {
                    richTextBox1.Text = richTextBox1.Text + "[" + DateTime.Now.ToString() + "] Investion started....\r\n";
                    timer1.Enabled = true;
                }
                button2.Text = "STOP";
            }
            else if (button2.Text == "STOP")
            {
                richTextBox1.Text = richTextBox1.Text + "[" + DateTime.Now.ToString() + "] Investion stoped....\r\n";
                timer1.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = Application.ExecutablePath;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = saveFileDialog1.FileName;
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                textBox2.ReadOnly = false;
            else
                textBox2.ReadOnly = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                SavePrediction();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer2.Enabled = false;
        }
    }
}
