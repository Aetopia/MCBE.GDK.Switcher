using System;
using System.Collections.Generic;
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

        Button button2 = new()
        {
            Dock = DockStyle.Fill,
            Text = "✖",
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Visible = false,
            Enabled = false
        };

        tableLayoutPanel2.ColumnStyles.Add(new() { SizeType = SizeType.AutoSize });
        tableLayoutPanel2.Controls.Add(button2, 1, 0);

        Catalog.Request? request = null;

        Closing += (sender, args) =>
        {
            if (request is { })
            {
                if (button2.Enabled) button2.PerformClick();
                args.Cancel = true;
            }
        };

        button1.Click += async delegate
        {
            tabControl.Enabled = false;
            button1.Visible = false;

            button2.Visible = true;
            button2.Enabled = false;
            progressBar1.Visible = true;


            var tabPage = tabControl.SelectedTab;
            var listBox = (ListBox)tabPage.Controls[0];
            var versions = (Catalog.Versions)tabPage.Tag;

            request = await versions.GetAsync((string)listBox.SelectedItem, (_) => Invoke(() =>
            {
                if (progressBar1.Value != _)
                {
                    progressBar1.Style = ProgressBarStyle.Blocks;
                    progressBar1.Value = _;
                }
            }));

            if (request is { })
            {
                button2.Enabled = true;
                await request;
            }
            request = null;

            tabControl.Enabled = true;
            button1.Visible = true;

            button2.Visible = false;
            button2.Enabled = false;

            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = false;
        };

        button2.Click += delegate
        {
            button2.Enabled = false;
            request?.Cancel();
        };

        Shown += async delegate
        {
            SuspendLayout();

            var catalog = await Catalog.GetAsync();

            tabPage1.Tag = catalog.Release;
            tabPage2.Tag = catalog.Preview;

            foreach (var key in catalog.Release.Keys.Reverse())
            {
                listBox1.Items.Add(key);
                Application.DoEvents();
            }
            listBox1.SelectedIndex = 0;

            foreach (var key in catalog.Preview.Keys.Reverse())
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