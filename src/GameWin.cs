using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vysin.QuizShow
{
    class GameWin : FullScreenWin
    {
        GameBoardView view;

        public GameWin()
        {
            Text = "Quiz Show Presenter : Game View";
            ClientSize = new Size(1024, 768);

            view = new GameBoardView();
            view.Dock = DockStyle.Fill;
            Controls.Add(view);
        }
    }
}