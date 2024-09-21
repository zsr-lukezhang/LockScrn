using Microsoft.VisualBasic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LockScrn
{
    public partial class Form1 : Form
    {
        private NotifyIcon notifyIcon;
        private bool skipChecks = false; // 将 skipChecks 声明为类的成员变量
        private ContextMenuStrip contextMenuStrip;

        public Form1()
        {
            InitializeComponent();
            // 检查命令行参数
            string[] args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.Equals("/easy", StringComparison.OrdinalIgnoreCase)))
            {
                skipChecks = true;
            }

            //可交换顺序（方式1）
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

            showItem.Click += ShowItem_Click;
            exitItem.Click += ExitItem_Click;

            contextMenuStrip.Items.Add(showItem);
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

            DialogResult result1 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result2 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result3 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result4 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result5 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result6 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            DialogResult result7 = MessageBox.Show("", "", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Question);
            if (result1 == DialogResult.Ignore && result2 == DialogResult.Retry && result3 == DialogResult.Ignore && result4 == DialogResult.Abort && result5 == DialogResult.Retry && result6 == DialogResult.Abort && result7 == DialogResult.Ignore)
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
