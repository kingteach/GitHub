using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HtmlSave
{
    public partial class FrmMain : Form
    {
        #region 变量
        private System.Timers.Timer t;
        private bool hasChoosePath = false;// 标记是否完成第一次选择路径
        private bool beginSendKey = false;// 标记是否可以发送保存键
        /// <summary>
        /// 需要下载的地址
        /// </summary>
        private List<string> urls = new List<string>();
        #endregion

        #region 构造函数
        public FrmMain()
        {
            InitializeComponent();
        }
        #endregion

        private void FrmMain_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 读取地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRead_Click(object sender, EventArgs e)
        {
            // 在此加载地址
            urls = new List<string>() { 
                "http://www.baidu.com/",
                 "http://www.cnblogs.com/", 
                "http://www.qq.com/",
                "http://www.taobao.com/",
               };
            progressBar1.Maximum = urls.Count;
            progressBar1.Value = 0;//设置当前值
            progressBar1.Step = 1;//设置没次增长多少

            WriteMsg("", "==============读取地址成功，可以点击按钮下载了==============");
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            hasChoosePath = false;
            StarTimer();
            progressBar1.Value = 0;
            int i = 0;
            foreach (var url in urls)
            {
                bool b = this.Navigate(url);
                if (!b)
                {
                    WriteMsg(url, "导航失败");
                    continue;
                }

                if (WaitWebPageLoad())
                {
                    beginSendKey = true;
                    this.TopLevel = true;
                    webBrowser1.ShowSaveAsDialog();
                    i++;
                    WriteMsg(url, "下载成功");
                    hasChoosePath = true;
                    beginSendKey = false;
                }
                else
                {
                    WriteMsg(url, "等待加载失败");
                }
                progressBar1.Value += 1;
            }
            StopTimer();
            progressBar1.Value = 0;
            WriteMsg("", string.Format("==============成功下载：{0}个网址,失败：{1}===============", i, urls.Count - i));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopTimer();
            WriteMsg("", "已经停止下载");
        }

        #region 定时器

        private void StarTimer()
        {
            t = new System.Timers.Timer(600);//实例化Timer类，设置时间间隔
            t.AutoReset = true;//设置是执行一次（false）还是一直执行(true) 
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        private void StopTimer()
        {
            if (t != null)
            {
                t.Enabled = false;
                t.Dispose();
                t.Stop();
            }
        }

        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (hasChoosePath && beginSendKey)
            {
                SendKeys.SendWait("%y");
                SendKeys.SendWait("%y");
                SendKeys.SendWait("%s");
            }
        }

        #endregion

        #region 判断页面是否加载完成
        private void Delay(int Millisecond) //延迟系统时间，但系统又能同时能执行其它任务；
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(Millisecond) > DateTime.Now)
            {
                Application.DoEvents();//转让控制权            
            }
            return;
        }

        private bool WaitWebPageLoad()
        {
            try
            {
                int i = 0;
                string sUrl = "";
                while (true)
                {
                    Delay(50);  //系统延迟50毫秒，够少了吧！             
                    if (webBrowser1.ReadyState == WebBrowserReadyState.Complete) //先判断是否发生完成事件。
                    {
                        if (!webBrowser1.IsBusy) //再判断是浏览器是否繁忙                  
                        {
                            i = i + 1;
                            if (i == 2)   //为什么 是2呢？因为每次加载frame完成时就会置IsBusy为false,未完成就就置IsBusy为false，你想一想，加载一次，然后再一次，再一次...... 最后一次.......
                            {
                                sUrl = webBrowser1.Url.ToString();
                                if (sUrl.Contains("res")) //这是判断没有网络的情况下                           
                                {
                                    return false;
                                }

                                else
                                {
                                    return true;
                                }
                            }
                            continue;
                        }
                        i = 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region Helper
        // Navigates to the given URL if it is valid.
        private bool Navigate(string url)
        {
            if (String.IsNullOrEmpty(url)) return false;
            if (url.Equals("about:blank")) return false;
            if (!url.StartsWith("http://") &&
                !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }
            try
            {
                webBrowser1.Navigate(new Uri(url));
                return true;
            }
            catch (System.UriFormatException)
            {
                return false;
            }
        }

        private void WriteMsg(string url, string msg)
        {
            txtConsole.Text += string.Format("{0}-----{1}\r\n", url, msg);
        }

        #endregion
    }
}
