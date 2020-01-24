using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UUIDConverter
{
    public partial class Form1 : Form
    {
        List<string> playerID = new List<string>();
        Thread Convert_Thread;
        FolderBrowserDialog path;
        DirectoryInfo d;
        const string NO_DATA = "NO_DATA";
        const string ERR_PAGE = "ERR_PAGE";
        public Form1()
        {
            InitializeComponent();
        }

        private string getWebString(string id)
        {
            WebRequest myRequest = WebRequest.Create(textBox_url.Text + id);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream());
            string result = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            if (!result.Contains("id") && !result.Contains("name"))
                return ERR_PAGE;
            if (result.Contains("id"))
                return result;
            else
                return NO_DATA;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox_player.Text = "";
            this.textBox_uuid.Text = "";
            path = new FolderBrowserDialog();
            path.ShowDialog();
            this.label1.Text = "Directory Path:" + path.SelectedPath;
            d = new DirectoryInfo(path.SelectedPath);//Assuming Test is your Folder
            Directory.CreateDirectory(d.FullName + "\\uuid");
            update_btn(false);

            Convert_Thread = new Thread(ConvertUUID);
            Convert_Thread.IsBackground = true;
            Convert_Thread.Start();
        }

        private void ConvertUUID()
        {
            //try
            //{
            FileInfo[] Files = d.GetFiles("*.dat"); //Getting Text files
            if (Files.Length == 0)
            {
                update_btn(true);
                update_err("No File");
                Convert_Thread.Abort();
            }
            string str = "";
            int i = 0;
            foreach (FileInfo file in Files)
            {
                string id = file.Name.Split(new string[] { ".dat" }, StringSplitOptions.None)[0];
                string raw = "", uuid = "";
                //playerID.Add(file.Name.Split(new string[] { ".dat" }, StringSplitOptions.None)[0]);
                if (id.Length <= 32)
                {
                    raw = getWebString(id);
                    update_player_Log(id);
                }
                if (raw == ERR_PAGE)
                {
                    update_err("Error: Page error");
                    Convert_Thread.Abort();
                }
                else if (raw == NO_DATA)
                {
                    update_uuid_Log("NO_DATA");
                }
                else if (raw != "")
                {
                    raw = raw.Split('"')[3];
                    uuid = new Guid(raw).ToString();
                    update_uuid_Log(uuid);
                    File.Copy(file.FullName, Path.Combine(file.DirectoryName + "\\uuid", uuid + ".dat"), true);
                }
                update_progress(i, Files.Length);
                Thread.Sleep(1000);
                i++;

                //if (i == 10)
                //    break;
            }
            update_btn(true);
            Process.Start("explorer.exe", d.FullName + "\\uuid");
            //}
            //catch (Exception e)
            //{
            //    update_err("Exception: " + e.ToString());
            //}

        }
        private void update_progress(int progress, int total)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<int, int>(this.update_progress), progress, total);     // タイムアウト値設定 Time out setting
            }
            else
            {
                toolStripProgressBar1.Value = (progress + 1) * 100 / total;
            }
        }
        private void update_err(String line)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(this.update_err), line);     // タイムアウト値設定 Time out setting
            }
            else
            {
                if (line != "")
                    toolStripStatusLabel_Exception.Text = line;
            }
        }
        private void update_player_Log(String line)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(this.update_player_Log), line);     // タイムアウト値設定 Time out setting
            }
            else
            {
                if (line != "")
                    textBox_player.AppendText(line + Environment.NewLine);
            }
        }
        private void update_uuid_Log(String line)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(this.update_uuid_Log), line);     // タイムアウト値設定 Time out setting
            }
            else
            {
                if (line != "")
                    textBox_uuid.AppendText(line + Environment.NewLine);
            }
        }
        private void update_btn(bool status)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<bool>(this.update_btn), status);     // タイムアウト値設定 Time out setting
            }
            else
            {
                button1.Enabled = status;
            }
        }
    }
}
