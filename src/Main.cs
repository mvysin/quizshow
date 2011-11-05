using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Vysin.QuizShow
{
    class QuizShow
    {
        private static Board board;
        private static ResourceFontCollection fonts;

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                fonts = new ResourceFontCollection();
                fonts.AddResource(Resources.EnchantedFont);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e)
                {
                    HandleException(e.Exception);
                };
                Application.Run(new MainWin());

                fonts.Dispose();
                fonts = null;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public static Board Board
        {
            get { return board; }
            set { board = value; }
        }

        private static void HandleException(Exception e)
        {
            MessageBox.Show(e.ToString());
            Environment.Exit(1);
        }

        public static FontFamily GetPrivateFont(string name)
        {
            return fonts.GetPrivateFont(name);
        }
    }
}