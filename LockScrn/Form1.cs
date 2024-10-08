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
using System.IO; // ��Ӵ����Խ����ļ�����
using System.Diagnostics; // �����ж�ʵ����

namespace LockScrn
{
    public partial class Form1 : Form
    {
        private NotifyIcon notifyIcon;
        private bool skipChecks = false; // �� skipChecks ����Ϊ��ĳ�Ա����
        private bool skipMessagebox = false; // ��Ӵ˱���
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

        private string activeModes = ""; // ��ӱ������洢�����ģʽ

        public Form1()
        {
            InitializeComponent();

            // ����Ƿ���������ʵ��������
            if (IsAlreadyRunning())
            {
                MessageBox.Show("����һ��ʵ�������С�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }

            // ��������в���
            string[] args = Environment.GetCommandLineArgs();
            if (args.Any(arg => arg.Equals("/easy", StringComparison.OrdinalIgnoreCase)))
            {
                skipChecks = true;
                activeModes += "Easy Mode, "; // ��Ӽ����ģʽ
            }
            if (args.Any(arg => arg.Equals("/showcursor", StringComparison.OrdinalIgnoreCase)))
            {
                ShowCursor(true);
                activeModes += "Show Cursor, "; // ��Ӽ����ģʽ
            }

            // ����ļ��Ƿ����
            if (!File.Exists(@"C:\LockScrn\showMessagebox.lcrn"))
            {
                skipMessagebox = true;
                activeModes += "No Password, "; // ��Ӽ����ģʽ
            }

            // ���ͬʱ���� No Password �� Show Cursor ����������ʾ Easy Mode
            if (skipMessagebox && activeModes.Contains("Show Cursor"))
            {
                activeModes = "Easy Mode";
            }
            
            // ���ͬʱ���� No Password �� Easy Mode ����������ʾ Easy Mode
            if (skipMessagebox && activeModes.Contains("Easy Mode"))
            {
                activeModes = "Easy Mode";
            }
            else
            {
                // ���û���κ�ģʽ����������Ĭ��ģʽ
                if (string.IsNullOrEmpty(activeModes))
                {
                    activeModes = "Normal Mode";
                }
                else
                {
                    // �Ƴ����һ�����źͿո�
                    activeModes = activeModes.TrimEnd(',', ' ');
                }
            }

            //�ɽ���˳��
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // ��������ͼ����Ҽ��˵�
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); // ʹ��Ӧ�ó����Դ���ͼ��
            notifyIcon.Text = "��������ʾLockScrn";
            notifyIcon.Visible = true;

            // �����Ҽ��˵�
            contextMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem showItem = new ToolStripMenuItem("��ʾLockScrn");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("�˳�LockScrn");
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("����LockScrn"); // ��ӹ���ѡ��

            showItem.Click += ShowItem_Click;
            exitItem.Click += ExitItem_Click;
            aboutItem.Click += AboutItem_Click; // ��ӹ���ѡ��ĵ���¼�

            contextMenuStrip.Items.Add(showItem);
            contextMenuStrip.Items.Add(aboutItem); // ������ѡ����ӵ��˵���
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
            MessageBox.Show($"LockScrn ����ʵ��Ӧ�ó���\n�汾 1.0\n���ߣ�Luke Zhang\n�����������ĵ���github.com/zsr-lukezhang/LockScrn\n��ǰģʽ��{activeModes}", "���� LockScrn", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        // ������Ӧ��DLL����
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

        //�ؽش����OnClosing���� �������������Լ��ر�����
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

            // ��ȡ�ļ�����
            string passwd1 = File.Exists(@"C:\LockScrn\Passwd\Passwd1.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd1.lcrn").Trim() : null;
            string passwd2 = File.Exists(@"C:\LockScrn\Passwd\Passwd2.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd2.lcrn").Trim() : null;
            string passwd3 = File.Exists(@"C:\LockScrn\Passwd\Passwd3.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd3.lcrn").Trim() : null;
            string passwd4 = File.Exists(@"C:\LockScrn\Passwd\Passwd4.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd4.lcrn").Trim() : null;
            string passwd5 = File.Exists(@"C:\LockScrn\Passwd\Passwd5.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd5.lcrn").Trim() : null;
            string passwd6 = File.Exists(@"C:\LockScrn\Passwd\Passwd6.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd6.lcrn").Trim() : null;
            string passwd7 = File.Exists(@"C:\LockScrn\Passwd\Passwd7.lcrn") ? File.ReadAllText(@"C:\LockScrn\Passwd\Passwd7.lcrn").Trim() : null;

            // ��֤�ļ������Ƿ�Ϊ��Ч�� DialogResult ö��ֵ
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
