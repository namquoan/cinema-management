namespace Pbl_test
{
    //internal static class Program
    //{
    //    /// <summary>
    //    ///  The main entry point for the application.
    //    /// </summary>
    //    [STAThread]
    //    static void Main()
    //    {
    //        // To customize application configuration such as set high DPI settings or default font,
    //        // see https://aka.ms/applicationconfiguration.
    //        ApplicationConfiguration.Initialize();
    //        Application.Run(new Form1());
    //    }
    //}
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            while (true)
            {
                Form1 loginForm = new Form1();
                DialogResult result = loginForm.ShowDialog();
                if (result != DialogResult.OK)
                    break;
            }
        }
    }
}