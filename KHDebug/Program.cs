using System;

namespace KHDebug
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static MainGame game;
        [STAThread]
        static void Main()
        {
            /*try
            {*/
                game = new MainGame();
                game.Run();
            /*}
            catch (Exception ex)
            {
                if (ex.ToString().ToLower().Contains("notfound"))
                {
                    Console.WriteLine("Error: one file could not be found.\nMake sure you downloaded most of the major files before to run KHDebug.");
                    Console.WriteLine("");
                    Console.WriteLine("More details:");
                }
                Console.WriteLine(ex.ToString());
                Console.ReadKey(true);
            }*/
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
