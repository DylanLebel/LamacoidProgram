using System;
using System.Windows.Forms;

namespace Lamacoid_Creator
{
    /// <summary>
    /// Main program entry point for Lamacoid Creator application
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VisualLibraryForm());
        }
    }
}
