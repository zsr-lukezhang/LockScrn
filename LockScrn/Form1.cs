//
//
//                                                  LockScrn v1.0!
//                                                    2024/10/08
//                                                    Luke Zhang
//                                                admin@lukezhang.win
//
//


using Microsoft.VisualBasic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO; // 添加此行以进行文件操作
using System.Diagnostics; // 这是判断实例的

namespace LockScrn
{
    public partial class Form1 : Form
    {
        private NotifyIcon notifyIcon;
        private bool skipChecks = false; // 将 skipChecks 声明为类的成员变量
        private bool skipMessagebox = false; // 添加此变量
        private ContextMenuStrip contextMenuStrip;

        private bool IsValidDialogResult(string? value)
        {
            return value != null && Enum.IsDefined(typeof(DialogResult), value);
        }


        private bool IsAlreadyRunning()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);
            return processes.Length > 1;
        }

        private string activeModes = ""; // 添加变量来存储激活的模式

        public Form1()
        {
            InitializeComponent();

            // 检查是否已有其他实例在运行
            if (IsAlreadyRunning())
            {
                MessageBox.Show("已有一个实例在运行。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }

            // 检查命令行参数
            string[] args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.Equals("/easy", StringComparison.OrdinalIgnoreCase)))
            {
                skipChecks = true;
                activeModes += "Easy Mode, "; // 添加激活的模式
            }
            if (args.Any(arg => arg.Equals("/showcursor", StringComparison.OrdinalIgnoreCase)))
            {
                ShowCursor(true);
                activeModes += "Show Cursor, "; // 添加激活的模式
            }

            // 检查文件是否存在
            if (!File.Exists(@"C:\LockScrn\showMessagebox.lcrn"))
            {
                skipMessagebox = true;
                activeModes += "No Password, "; // 添加激活的模式
            }

            // 如果同时满足 No Password 和 Show Cursor 条件，则显示 Easy Mode
            if (skipMessagebox && activeModes.Contains("Show Cursor"))
            {
                activeModes = "Easy Mode";
            }
            
            // 如果同时满足 No Password 和 Easy Mode 条件，则显示 Easy Mode
            if (skipMessagebox && activeModes.Contains("Easy Mode"))
            {
                activeModes = "Easy Mode";
            }
            else
            {
                // 如果没有任何模式被激活，则添加默认模式
                if (string.IsNullOrEmpty(activeModes))
                {
                    activeModes = "Normal Mode";
                }
                else
                {
                    // 移除最后一个逗号和空格
                    activeModes = activeModes.TrimEnd(',', ' ');
                }
            }

            //可交换顺序
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // 设置托盘图标和右键菜单
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); // 使用应用程序自带的图标
            notifyIcon.Text = "单击以显示LockScrn";
            notifyIcon.Visible = true;

            // 创建右键菜单
            contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem showItem = new ToolStripMenuItem("显示LockScrn");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出LockScrn");
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("关于LockScrn"); // 添加关于选项

            showItem.Click += ShowItem_Click;
            exitItem.Click += ExitItem_Click;
            aboutItem.Click += AboutItem_Click; // 添加关于选项的点击事件

            contextMenuStrip.Items.Add(showItem);
            contextMenuStrip.Items.Add(aboutItem); // 将关于选项添加到菜单中
            contextMenuStrip.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }



        private void ShowItem_Click(object sender, EventArgs e)
        {
            this.Show();
            if (!skipChecks)
            {
                ShowCursor(false);
                notifyIcon.Visible = false;
            }
        }

        private void ExitItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"LockScrn 黑屏实用应用程序\n版本 1.0\n作者：Luke Zhang\n官网及帮助文档：github.com/zsr-lukezhang/LockScrn\n当前模式：{activeModes}", "关于 LockScrn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // 导入相应的DLL命令
        [DllImport("user32", EntryPoint = "ShowCursor")]
        public extern static bool ShowCursor(bool show);

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            if (skipChecks)
            {
                ShowCursor(true);
                notifyIcon.Visible = true;
            }
            else
            {
                ShowCursor(false);
                notifyIcon.Visible = false;
            }
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Show();
                if (!skipChecks)
                {
                    ShowCursor(false);
                    notifyIcon.Visible = false;
                }
            }
        }

        //重截窗体的OnClosing方法 用于输入密码以及关闭锁屏
        protected override void OnClosing(CancelEventArgs e)
        {
            if (skipChecks)
            {
                this.Visible = false;
                e.Cancel = true;
                return;
            }

            if (skipMessagebox)
            {
                this.Visible = false;
                ShowCursor(true);
                notifyIcon.Visible = true;
                e.Cancel = true;
                return;
            }

            // 读取文件内容
            string passwd1 = File.Exists(@"C:\LockScrn\Passwd\Passwd1.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd1.lcrn").Trim() : null;
            string passwd2 = File.Exists(@"C:\LockScrn\Passwd\Passwd2.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd2.lcrn").Trim() : null;
            string passwd3 = File.Exists(@"C:\LockScrn\Passwd\Passwd3.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd3.lcrn").Trim() : null;
            string passwd4 = File.Exists(@"C:\LockScrn\Passwd\Passwd4.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd4.lcrn").Trim() : null;
            string passwd5 = File.Exists(@"C:\LockScrn\Passwd\Passwd5.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd5.lcrn").Trim() : null;
            string passwd6 = File.Exists(@"C:\LockScrn\Passwd\Passwd6.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd6.lcrn").Trim() : null;
            string passwd7 = File.Exists(@"C:\LockScrn\Passwd\Passwd7.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd7.lcrn").Trim() : null;

            // 验证文件内容是否为有效的 DialogResult 枚举值
            DialogResult? expectedResult1 = IsValidDialogResult(passwd1) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd1) : (DialogResult?)null;
            DialogResult? expectedResult2 = IsValidDialogResult(passwd2) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd2) : (DialogResult?)null;
            DialogResult? expectedResult3 = IsValidDialogResult(passwd3) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd3) : (DialogResult?)null;
            DialogResult? expectedResult4 = IsValidDialogResult(passwd4) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd4) : (DialogResult?)null;
            DialogResult? expectedResult5 = IsValidDialogResult(passwd5) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd5) : (DialogResult?)null;
            DialogResult? expectedResult6 = IsValidDialogResult(passwd6) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd6) : (DialogResult?)null;
            DialogResult? expectedResult7 = IsValidDialogResult(passwd7) ? (DialogResult)Enum.Parse(typeof(DialogResult), passwd7) : (DialogResult?)null;

            DialogResult result1 = expectedResult1.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result2 = expectedResult2.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result3 = expectedResult3.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result4 = expectedResult4.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result5 = expectedResult5.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result6 = expectedResult6.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;
            DialogResult result7 = expectedResult7.HasValue ? MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question) : DialogResult.None;

            if ((!expectedResult1.HasValue || result1 == expectedResult1) &&
                (!expectedResult2.HasValue || result2 == expectedResult2) &&
                (!expectedResult3.HasValue || result3 == expectedResult3) &&
                (!expectedResult4.HasValue || result4 == expectedResult4) &&
                (!expectedResult5.HasValue || result5 == expectedResult5) &&
                (!expectedResult6.HasValue || result6 == expectedResult6) &&
                (!expectedResult7.HasValue || result7 == expectedResult7))
            {
                MessageBox.Show("", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Visible = false;
                ShowCursor(true);
                notifyIcon.Visible = true;
                e.Cancel = true;
            }
            else
            {
                MessageBox.Show("", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }
    }
}
