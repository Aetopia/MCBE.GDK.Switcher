using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

sealed class Form : System.Windows.Forms.Form
{
    internal Form()
    {
        Text = "MCBE GDK Switcher";
        StartPosition = FormStartPosition.CenterScreen;
        Font = SystemFonts.MessageBoxFont;
        FormBorderStyle = FormBorderStyle.FixedDialog;

        MinimizeBox = false;
        MaximizeBox = false;

        ClientSize = new(400, 300);
        MinimumSize = Size;

        TableLayoutPanel tableLayoutPanel1 = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Enabled = false,
            Padding = new(5)
        };

        Controls.Add(tableLayoutPanel1);

        TabControl tabControl = new() { Dock = DockStyle.Fill };

        tableLayoutPanel1.RowStyles.Add(new() { SizeType = SizeType.Percent, Height = 100 });
        tableLayoutPanel1.Controls.Add(tabControl, 0, 0);

        ListBox listBox1 = new() { Dock = DockStyle.Fill };
        ListBox listBox2 = new() { Dock = DockStyle.Fill };

        TabPage tabPage1 = new() { Text = "Release" };
        tabPage1.Controls.Add(listBox1);

        TabPage tabPage2 = new() { Text = "Preview" };
        tabPage2.Controls.Add(listBox2);

        tabControl.TabPages.AddRange([tabPage1, tabPage2]);

        TableLayoutPanel tableLayoutPanel2 = new()
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        tableLayoutPanel1.RowStyles.Add(new() { SizeType = SizeType.AutoSize });
        tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);

        Button button1 = new()
        {
            Dock = DockStyle.Fill,
            Text = "🡻",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Visible = false
        };

        tableLayoutPanel2.ColumnStyles.Add(new() { SizeType = SizeType.Percent, Width = 100 });
        tableLayoutPanel2.Controls.Add(button1, 0, 0);

        ProgressBar progressBar1 = new()
        {
            Dock = DockStyle.Bottom,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 1
        };
        tableLayoutPanel2.Controls.Add(progressBar1, 0, 0);

        tableLayoutPanel2.ColumnStyles.Add(new() { SizeType = SizeType.AutoSize });

        Task? task = null;

        Closing += (sender, args) => args.Cancel = !task?.IsCompleted ?? false;

        button1.Click += async delegate
        {
            tabControl.Enabled = false;
            button1.Visible = false;
            progressBar1.Visible = true;

            var tabPage = tabControl.SelectedTab;
            var listBox = (ListBox)tabPage.Controls[0];
            var versions = (Catalog.Versions)tabPage.Tag;

            await (task = versions.InstallAsync((string)listBox.SelectedItem, (_) => Invoke(() =>
            {
                if (progressBar1.Value != _)
                {
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Value = _;
                }
            }))); task = null;

            tabControl.Enabled = true;
            button1.Visible = true;

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = false;
        };

        Shown += async (_, _) =>
        {
            SuspendLayout();

            var catalog = await Catalog.GetAsync();

            tabPage1.Tag = catalog.Release;
            tabPage2.Tag = catalog.Preview;

            foreach (var key in catalog.Release.Reverse())
            {
                listBox1.Items.Add(key);
                Application.DoEvents();
            }
            listBox1.SelectedIndex = 0;

            foreach (var key in catalog.Preview.Reverse())
            {
                listBox2.Items.Add(key);
                Application.DoEvents();
            }
            listBox2.SelectedIndex = 0;

            ResumeLayout();

            button1.Visible = true;
            progressBar1.Visible = false;
            tableLayoutPanel1.Enabled = true;
        };
    }
}