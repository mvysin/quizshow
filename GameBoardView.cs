using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Jeopardy
{
    enum GameBoardViewMode
    {
        Board,
        Clue,
        Answer
    }

    public class GameBoardView : Control, IBoardView
    {
        GameBoardButton[,] btns = new GameBoardButton[6, 6];
        Dictionary<GameBoardButton, Point> btnPoints = new Dictionary<GameBoardButton, Point>();
        bool edit = false;
        bool clueMouseDown = false;
        int trkX = -1, trkY = -1, selX = -1, selY = -1;
        GameBoardButton captureButton, mouseOverButton;
        Color clueBackColor;
        GameBoardViewMode mode = GameBoardViewMode.Board;
        Clue curClue;
        Rectangle contentRect;

        public GameBoardView()
        {
            this.MinimumSize = new Size(4*250/3, 250);

            Jeopardy.Board.AddView(this);

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.StandardDoubleClick, false);
            SetStyle(ControlStyles.StandardClick, true);
            SetStyle(ControlStyles.Selectable, true);

            BackColor = Color.FromArgb(240, 200, 70);
            ClueBackColor = Color.Blue;

            for (int x = 0; x < 6; x++)
            {
                for (int y = -1; y < 5; y++)
                {
                    GameBoardButton b = new GameBoardButton(this);
                    b.Clickable = (y >= 0);
                    b.IsTitle = (y < 0);
                    b.Click += new EventHandler(OnBoardButtonClick);
                    b.TrackingChanged += new EventHandler(OnBoardButtonTrackChange);
                    btnPoints.Add(b, new Point(x, y));

                    if (y >= 0)
                        b.Text = Jeopardy.Board.PointValues[y].ToString();
                    else
                    {
                        if (x < Jeopardy.Board.Category.Length)
                            b.Text = Jeopardy.Board.Category[x].Name;
                        Jeopardy.Board.Category[x].PropertyChanged += new PropertyChangedEventHandler(WeakItemChanged);
                    }

                    btns[x, y+1] = b;
                }
            }

            OnSizeChanged(EventArgs.Empty);
        }

        private GameBoardButton ButtonFromPoint(Point pt)
        {
            // no buttons available when in answer/clue mode
            if (!DisplayingBoard)
                return null;

            if (captureButton != null)
                return captureButton;

            for (int x = 0; x < 6; x++)
                for (int y = 0; y < 6; y++)
                    if (btns[x, y].ContainsPoint(pt))
                        return btns[x, y];

            return null;
        }

        public Color ClueBackColor
        {
            get { return clueBackColor; }
            set { clueBackColor = value; }
        }

        private void CluePanelClick()
        {
            if (!ShowAnswer)
                Jeopardy.Board.NotifyViews(NotifyAction.DisplayAnswer, 0, 0);
            else
                Jeopardy.Board.NotifyViews(NotifyAction.DisplayBoard, 0, 0);
        }

        public bool DisplayingBoard
        {
            get { return Mode == GameBoardViewMode.Board; }
        }

        public bool DisplayingClue
        {
            get { return Mode == GameBoardViewMode.Answer || Mode == GameBoardViewMode.Clue; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Parent != null)
                {
                    Parent.SizeChanged -= new EventHandler(ParentSizeChanged);
                    Parent.ClientSizeChanged -= new EventHandler(ParentSizeChanged);
                }
            }

            base.Dispose(disposing);
        }
        
        public bool Editable
        {
            get { return edit; }
            set {
                edit = value;
                for (int x = 0; x < 6; x++)
                    btns[x, 0].Clickable = edit;
                if (edit)
                    SetTracked(0, -1);
                else
                    SetTracked(-1, -1);
            }
        }

        public void EditClue(Clue c)
        {
            for (int cat = 0; cat < 6; cat++)
            {
                for (int clue = 0; clue < 5; clue++)
                {
                    if (Jeopardy.Board.Category[cat].Clue[clue] == c)
                    {
                        Jeopardy.Board.NotifyViews(NotifyAction.ButtonClicked, cat, clue);
                    }
                }
            }
        }

        public bool HasSelectedButton
        {
            get { return Editable && DisplayingBoard && selX >= 0; }
        }

        public bool HasTrackedButton
        {
            get { return Editable && DisplayingBoard && trkX >= 0; }
        }

        protected override bool IsInputChar(char charCode)
        {
            if (charCode == '\r')
                return true;

            return base.IsInputChar(charCode);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right ||
                keyData == Keys.Up || keyData == Keys.Down ||
                keyData == Keys.Enter)
                return true;

            return base.IsInputKey(keyData);
        }

        private GameBoardViewMode Mode
        {
            get { return mode; }
            set
            {
                GameBoardViewMode oldMode = mode;
                mode = value;
                if (oldMode != value)
                    Refresh();
            }
        }

        public void MoveTracked(int dx, int dy)
        {
            if (!HasTrackedButton)
                return;
            if (dx == 0 && dy == 0)
                return;

            int newX = trkX + dx;
            int newY = trkY + dy;

            if (newX < 0)   newX = 0;
            if (newX > 5)   newX = 5;
            if (newY < -1)  newY = -1;
            if (newY > 4)   newY = 4;

            SetTracked(newX, newY);
        }

        public void Notify(NotifyAction a, int x, int y)
        {
            if (a == NotifyAction.ButtonClicked)
            {
                if (!Editable)
                    btns[x, y+1].Active = false;
                if (y >= 0)
                    Jeopardy.Board.NotifyViews(NotifyAction.DisplayClue, x, y);
            }
            else if (a == NotifyAction.DisplayClue)
            {
                SelectedClue = Jeopardy.Board.Category[x].Clue[y];
                Mode = GameBoardViewMode.Clue;
            }
            else if (a == NotifyAction.DisplayAnswer)
            {
                Mode = GameBoardViewMode.Answer;
            }
            else if (a == NotifyAction.DisplayBoard)
            {
                Mode = GameBoardViewMode.Board;

                // this comes from displaying the answer
                SetSelected(-1, -1);

                if (HasTrackedButton && Focused)
                    TrackedButton.Tracking = true;
            }
        }

        protected virtual void OnBoardButtonClick(object sender, EventArgs e)
        {
            Point pt = btnPoints[(GameBoardButton)sender];
            if (pt.Y < 0 && !Editable)
                return;
            Jeopardy.Board.NotifyViews(NotifyAction.ButtonClicked, pt.X, pt.Y);
        }

        private void OnBoardButtonTrackChange(object sender, EventArgs e)
        {
            // notify views
            GameBoardButton b = (GameBoardButton)sender;
            if (b.Tracking)
            {
                Point pt = btnPoints[b];
                Jeopardy.Board.NotifyViews(NotifyAction.TrackButtonChanged, pt.X, pt.Y);
            }
            else
                Jeopardy.Board.NotifyViews(NotifyAction.TrackButtonChanged, -1, -1);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            if (DisplayingBoard && HasTrackedButton)
                TrackedButton.Tracking = true;
            base.OnGotFocus(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (DisplayingBoard && HasTrackedButton)
            {
                if (e.KeyCode == Keys.Left)
                    MoveTracked(-1, 0);
                else if (e.KeyCode == Keys.Right)
                    MoveTracked(1, 0);
                else if (e.KeyCode == Keys.Up)
                    MoveTracked(0, -1);
                else if (e.KeyCode == Keys.Down)
                    MoveTracked(0, 1);
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                if (DisplayingClue)
                    CluePanelClick();
                else if (DisplayingBoard && HasTrackedButton)
                    OnBoardButtonClick(TrackedButton, EventArgs.Empty);
            }

            base.OnKeyPress(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (DisplayingBoard && HasTrackedButton)
                TrackedButton.Tracking = false;
            base.OnLostFocus(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!Focused)
                Focus();

            // if we're displaying a clue, track that the click started
            if (DisplayingClue)
                clueMouseDown = true;
            else if (DisplayingBoard)
            {
                // if we're over a button, let it process the event
                GameBoardButton btn = ButtonFromPoint(e.Location);
                if (btn != null)
                    btn.OnMouseDown(TranslateMouseEvent(e, btn));
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            clueMouseDown = false;
            if (DisplayingBoard)
                OnMouseOverButton(null);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DisplayingBoard)
            {
                GameBoardButton btn = ButtonFromPoint(e.Location);
                OnMouseOverButton(btn);
                if (btn != null)
                    btn.OnMouseMove(TranslateMouseEvent(e, btn));
            }

            base.OnMouseMove(e);
        }

        private void OnMouseOverButton(GameBoardButton btn)
        {
            // if we're over the same button, nothing's changed (duh)
            if (mouseOverButton == btn)
                return;

            // if we used to be over a button... need to notify it that the mouse left
            if (mouseOverButton != null)
            {
                mouseOverButton.OnMouseLeave(EventArgs.Empty);

                // if there's no new active button, and we're tracking a button but don't
                // have focus, we need to do one of two things:
                if (btn == null && !Focused && HasTrackedButton)
                {
                    if (HasSelectedButton)
                        // set the tracking button back to the one that is selected
                        SetTracked(btnPoints[SelectedButton].X, btnPoints[SelectedButton].Y);
                    else
                    {
                        // or keep the tracking on but just hidden
                        TrackedButton.Tracking = false;
                    }
                }
            }

            mouseOverButton = btn;

            Point pt = btn != null ? btnPoints[btn] : new Point(-1, -1);

            if (btn != null)
            {
                // send mouse-enter event, and update tracking
                mouseOverButton.OnMouseEnter(EventArgs.Empty);
                SetTracked(pt.X, pt.Y);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (DisplayingBoard)
            {
                // first send it to the button
                GameBoardButton btn = ButtonFromPoint(e.Location);
                if (btn != null)
                    btn.OnMouseUp(TranslateMouseEvent(e, btn));
            }

            // if we had started a clue-click, notify that
            if (clueMouseDown && DisplayingClue)
                CluePanelClick();
            clueMouseDown = false;

            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (DisplayingClue)
                PaintCluePanel(g);
            else if (DisplayingBoard)
                PaintBoard(g);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Region rgn = new Region(new Rectangle(Location, Size));
            rgn.Exclude(contentRect);
            e.Graphics.FillRegion(Brushes.Black, rgn);
            e.Graphics.FillRectangle(new SolidBrush(BackColor), contentRect);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
            {
                Parent.SizeChanged += new EventHandler(ParentSizeChanged);
                Parent.ClientSizeChanged += new EventHandler(ParentSizeChanged);
                if (AutoSize)
                    Size = Parent.ClientSize;
                else
                    OnSizeChanged(e);
            }
            base.OnParentChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (Parent == null)
            {
                base.OnSizeChanged(e);
                return;
            }

            Point newLoc;
            Size newSize;

            if (Width >= Height/3*4)
            {
                newSize = new Size(Height / 3 * 4, Height);
                newLoc = new Point((Width-newSize.Width) / 2, 0);
            }
            else
            {
                newSize = new Size(Width, Width/4*3);
                newLoc = new Point(0, (Height-newSize.Height) / 2);
            }

            contentRect = new Rectangle(newLoc, newSize);

            if (btns == null)
                return;

            int dx = contentRect.Width / (6*4+2);
            int dy = dx;

            int ox = (contentRect.Width - (6*dx*4+dx)) / 2;
            int oy = (contentRect.Height - (5*dy*3+2*dy+10*dy/6)) / 2;

            for (int x = 0; x < 6; x++)
            {
                for (int y = -1; y < 5; y++)
                {
                    btns[x, y+1].Location = new Point(
                        contentRect.Left + dx/7+x*dx*4+x*dx/7 + ox,
                        contentRect.Top + (y<0 ? 0+oy : 5*dy/6+y*dy*3+y*dy/6+2*dy + oy));
                    btns[x, y+1].Size = new Size(
                        4*dx,
                        y<0 ? 2*dy : 3*dy);
                }
            }

            base.OnSizeChanged(e);
        }

        private void PaintBoard(Graphics g)
        {
            int lw = btns[0, 1].LineWidth;
            SolidBrush lightBrush = new SolidBrush(Color.FromArgb(255, 248, 223));
            SolidBrush darkBrush = new SolidBrush(Color.FromArgb(150, 120, 10));


            Point ll = btns[5, 0].Location + btns[5, 0].Size;
            ll.X -= btns[0, 0].Location.X;
            ll.Y -= btns[0, 0].Location.Y;
            Rectangle catRect = new Rectangle(btns[0, 0].Location, new Size(ll.X, ll.Y));

            g.FillRectangle(lightBrush, Rectangle.Inflate(catRect, lw, lw));
            g.FillRectangle(Brushes.Black, catRect);
            g.FillPolygon(darkBrush, new Point[] {
                new Point(catRect.Left-lw, catRect.Top-lw), new Point(catRect.Right+lw, catRect.Top-lw),
                new Point(catRect.Right, catRect.Top), new Point(catRect.Left, catRect.Top),
                new Point(catRect.Left, catRect.Bottom), new Point(catRect.Left-lw, catRect.Bottom+lw)
                });


            ll = btns[5, 5].Location + btns[5, 5].Size;
            ll.X -= btns[0, 1].Location.X;
            ll.Y -= btns[0, 1].Location.Y;
            Rectangle clueRect = new Rectangle(btns[0, 1].Location, new Size(ll.X, ll.Y));

            g.FillRectangle(lightBrush, Rectangle.Inflate(clueRect, lw, lw));
            g.FillRectangle(Brushes.Black, clueRect);
            g.FillPolygon(darkBrush, new Point[] {
                new Point(clueRect.Left-lw, clueRect.Top-lw), new Point(clueRect.Right+lw, clueRect.Top-lw),
                new Point(clueRect.Right, clueRect.Top), new Point(clueRect.Left, clueRect.Top),
                new Point(clueRect.Left, clueRect.Bottom), new Point(clueRect.Left-lw, clueRect.Bottom+lw)
                });

            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    GameBoardButton btn = btns[x, y];
                    System.Drawing.Drawing2D.GraphicsState gs = g.Save();
                    g.TranslateTransform(btn.Location.X, btn.Location.Y);
                    g.SetClip(new Rectangle(new Point(0, 0), btn.Size), System.Drawing.Drawing2D.CombineMode.Intersect);
                    btn.OnPaint(new PaintEventArgs(g, new Rectangle()));
                    g.Restore(gs);
                }
            }
        }


        private void PaintCluePanel(Graphics g)
        {
            int dx = contentRect.X;
            int dy = contentRect.Y;

            int w = contentRect.Width;
            int h = contentRect.Height;
            int margin = h / 20;

            Font clueFont = new Font(Jeopardy.GetPrivateFont("Enchanted"), h / 11, FontStyle.Bold, GraphicsUnit.Pixel);
            Font answerFont = new Font("Calibri", h / 20, FontStyle.Bold, GraphicsUnit.Pixel);
            Font sourceFont = new Font("Calibri", margin / 2, FontStyle.Regular, GraphicsUnit.Pixel);

            StringFormat fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Center;

            Clue clue = SelectedClue;

            g.FillRectangle(new SolidBrush(ClueBackColor), contentRect);

            Rectangle clueRect = new Rectangle(margin, margin, w-2*margin, h-5*margin);
            clueRect.Offset(dx, dy);
            clueRect.Offset(margin / 10, margin / 10);
            g.DrawString(clue.Question.ToUpper(), clueFont, Brushes.Black, clueRect, fmt);
            clueRect.Offset(-margin / 10, -margin / 10);
            g.DrawString(clue.Question.ToUpper(), clueFont, Brushes.White, clueRect, fmt);

            if (ShowAnswer)
            {
                Rectangle ansRect = new Rectangle(margin, h - 3 * margin, w - 2 * margin, 2 * margin);
                ansRect.Offset(dx, dy);
                ansRect.Offset(margin / 10, margin / 10);
                g.DrawString(clue.Answer, answerFont, Brushes.Black, ansRect, fmt);
                ansRect.Offset(-margin / 10, -margin / 10);
                g.DrawString(clue.Answer, answerFont, Brushes.White, ansRect, fmt);

                if (clue.Source != null)
                {
                    Rectangle srcRect = new Rectangle(margin, h - margin, w - 2 * margin, margin / 2);
                    srcRect.Offset(dx, dy);
                    fmt.Alignment = StringAlignment.Far;
                    g.DrawString(clue.Source, sourceFont, Brushes.White, srcRect, fmt);
                }
            }
        }


        private void ParentSizeChanged(object sender, EventArgs e)
        {
            if (AutoSize)
                Size = Parent.ClientSize;
        }



        private GameBoardButton SelectedButton
        {
            get { return HasSelectedButton ? btns[selX, selY+1] : null; }
        }

        public void SetSelected(int x, int y)
        {
            if (x == selX && y == selY)
                return;

            if (HasSelectedButton)
                SelectedButton.Selected = false;
            selX = x;
            selY = y;
            if (HasSelectedButton)
                SelectedButton.Selected = true;
        }

        public void SetTracked(int x, int y)
        {
            if (x == trkX && y == trkY)
                return;

            if (HasTrackedButton)
                TrackedButton.Tracking = false;
            trkX = x;
            trkY = y;
            if (HasTrackedButton && (Focused || mouseOverButton == TrackedButton))
                TrackedButton.Tracking = true;
        }

        public Clue SelectedClue
        {
            get { return curClue; }
            set
            {
                if (curClue != null)
                    curClue.PropertyChanged -= new PropertyChangedEventHandler(WeakItemChanged);
                curClue = value;
                if (curClue != null)
                    curClue.PropertyChanged += new PropertyChangedEventHandler(WeakItemChanged);
            }
        }

        public void SetCapture(GameBoardButton btn, bool capture)
        {
            if (!capture)
            {
                Capture = false;
                captureButton = null;
            }
            else
            {
                Capture = true;
                captureButton = btn;
            }
        }

        public bool ShowAnswer
        {
            get { return Editable || Mode == GameBoardViewMode.Answer; }
        }

        private GameBoardButton TrackedButton
        {
            get { return HasTrackedButton ? btns[trkX, trkY+1] : null; }
        }

        private MouseEventArgs TranslateMouseEvent(MouseEventArgs e, GameBoardButton b)
        {
            return new MouseEventArgs(e.Button, e.Clicks, e.X-b.Location.X, e.Y-b.Location.Y, e.Delta);
        }

        private void WeakItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == SelectedClue)
                Refresh();
            else if (sender is Category)
            {
                for (int i = 0; i < 6; i++)
                    btns[i, 0].Text = Jeopardy.Board.Category[i].Name;
            }
        }
    }
}