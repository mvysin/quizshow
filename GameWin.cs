using System;
using System.Drawing;
using System.Windows.Forms;

namespace Jeopardy
{
    class GameWin : FullScreenWin
    {
        GameBoardView view;

        public GameWin()
        {
            Text = "Jeopardy! Game";
            ClientSize = new Size(1024, 768);

            view = new GameBoardView();
            view.Dock = DockStyle.Fill;
            Controls.Add(view);
        }
    }
}