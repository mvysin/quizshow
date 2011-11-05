using System;
using System.Drawing;
using System.Windows.Forms;

namespace Vysin.QuizShow
{
    class PresenterWin : FullScreenWin, IBoardView
    {
        GameBoardView view;
        Label lblAnswer;


        public PresenterWin()
        {
            Text = "Quiz Show Presenter : Manager View";
            ClientSize = new Size(1024, 768);

            view = new GameBoardView();
            view.Location = new Point(0, 0);
            Controls.Add(view);

            lblAnswer = new Label();
            lblAnswer.ForeColor = Color.White;
            lblAnswer.TextAlign = ContentAlignment.TopCenter;
            lblAnswer.Visible = false;
            lblAnswer.UseMnemonic = false;
            Controls.Add(lblAnswer);

            QuizShow.Board.AddView(this);
            OnSizeChanged(EventArgs.Empty);
        }

        public void Notify(NotifyAction a, int x, int y)
        {
            if (a == NotifyAction.DisplayClue)
            {
                lblAnswer.Text = QuizShow.Board.Category[x].Clue[y].Answer;
                lblAnswer.Visible = true;
            }
            else if (a == NotifyAction.DisplayBoard)
            {
                lblAnswer.Text = String.Empty;
                lblAnswer.Visible = false;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (view != null)
            {
                int viewHeight = ClientSize.Height / 10 * 9;
                view.Location = new Point(0, 0);
                view.Size = new Size(ClientSize.Width, viewHeight);
                if (lblAnswer != null)
                {
                    lblAnswer.Location = new Point(view.Left, view.Bottom);
                    lblAnswer.Size = new Size(view.Width, ClientSize.Height/10);
                    lblAnswer.Font = new Font(SystemFonts.DialogFont.Name, ClientSize.Height/30, GraphicsUnit.Pixel);
                }
            }

            base.OnSizeChanged(e);
        }
    }
}