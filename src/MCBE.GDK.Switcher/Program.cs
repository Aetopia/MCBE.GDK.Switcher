using System;
using System.Windows.Forms;

static class Program
{
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = (Exception)args.ExceptionObject;
            while (exception.InnerException is { }) exception = exception.InnerException;

            MessageBox.Show($"{exception}", exception.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Environment.Exit(1);
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Application.Run(new Form());
    }
}