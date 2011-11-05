using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;

namespace Jeopardy
{
    public class GameBoardButton
    {
        const string TitleFontName = "Calibri",
                     BoardFontName = "Impact";
        const FontStyle TitleFontStyle = FontStyle.Bold,
                        BoardFontStyle = FontStyle.Regular;
        readonly SolidBrush hiliteBrush, shadowBrush, selectBrush, focusBrush;

        const int GBB_PRESSED       = 0x01;
        const int GBB_MOUSEOVER     = 0x02;
        const int GBB_CLICKABLE     = 0x04;
        const int GBB_ACTIVE        = 0x08;
        const int GBB_TITLE         = 0x10;
        const int GBB_SELECTED      = 0x20;
        const int GBB_TRACKED       = 0x40;


        BitVector32 settings = new BitVector32(GBB_ACTIVE | GBB_CLICKABLE);

        string text = String.Empty;
        GameBoardView view;
        Rectangle rect;

        public event EventHandler Click;
        public event EventHandler TrackingChanged;



        public GameBoardButton(GameBoardView view)
        {
            this.view = view;

            hiliteBrush = new SolidBrush(Color.FromArgb(192, 192, 255));
            shadowBrush = new SolidBrush(Color.FromArgb(0, 0, 128));
            selectBrush = new SolidBrush(Color.FromArgb(192, 0, 255));
            focusBrush = new SolidBrush(Color.FromArgb(255, 0, 0));
        }

        public bool Active
        {
            get { return settings[GBB_ACTIVE]; }
            set
            {
                bool old = settings[GBB_ACTIVE];
                settings[GBB_ACTIVE] = value;
                if (old != value)
                    Refresh();
            }
        }

        public bool Clickable
        {
            get { return settings[GBB_CLICKABLE]; }
            set { settings[GBB_CLICKABLE] = value; }
        }

        public bool ContainsPoint(Point pt)
        {
            return (pt.X >= rect.Left && pt.X < rect.Right) &&
                   (pt.Y >= rect.Top && pt.Y < rect.Bottom);
        }

        public int Height
        {
            get { return rect.Height; }
        }

        public bool IsTitle
        {
            get { return settings[GBB_TITLE]; }
            set { settings[GBB_TITLE] = value; Refresh(); }
        }

        public int LineWidth
        {
            get { return Width / 15; }
        }

        public Point Location
        {
            get { return rect.Location; }
            set { rect.Location = value; }
        }

        private bool MouseOver
        {
            get { return settings[GBB_MOUSEOVER]; }
            set
            {
                bool old = settings[GBB_MOUSEOVER];
                settings[GBB_MOUSEOVER] = value;
                if (old != value)
                    Refresh();
            }
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (Clickable && Active)
            {
                Pressed = true;
                view.SetCapture(this, true);
            }
        }

        public void OnMouseEnter(EventArgs e)
        {
            if (Clickable && Active)
                MouseOver = true;
        }

        public void OnMouseLeave(EventArgs e)
        {
            if (Clickable && Active)
                MouseOver = false;
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (Clickable && Active)
            {
                Point p = e.Location;
                MouseOver = !(p.X < 0 || p.Y < 0 || p.X > Width || p.Y > Height);
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            if (Clickable && Active && Pressed)
            {
                bool mouseOver;
                Point p = e.Location;
                if (p.X < 0 || p.Y < 0 || p.X > Width || p.Y > Height)
                    mouseOver = false;
                else
                    mouseOver = true;

                Pressed = false;
                view.SetCapture(this, false);

                if (mouseOver && Click != null)
                    Click(this, EventArgs.Empty);
            }
        }

        public void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            int w = Width, h = Height;
            int lw = LineWidth;

            if (!Active)
            {
                g.FillRectangle(Brushes.Blue, 0, 0, w, h);
                return;
            }

            if (Tracking)
            {
                g.FillRectangle(focusBrush, 0, 0, w, h);
                if (Selected)
                    g.FillRectangle(selectBrush, lw, lw, w-lw*2, h-lw*2);
                else
                    g.FillRectangle(Brushes.Blue, lw, lw, w-lw*2, h-lw*2);
            }
            else
            {
                SolidBrush upperBorderBrush = hiliteBrush;
                SolidBrush borderBrush = shadowBrush;
                if (MouseOver && Pressed)
                {
                    upperBorderBrush = shadowBrush;
                    borderBrush = hiliteBrush;
                }

                // draw the border
                g.FillRectangle(borderBrush, 0, 0, w, h);
                if (Selected)
                    g.FillRectangle(selectBrush, lw, lw, w-lw*2, h-lw*2);
                else
                    g.FillRectangle(Brushes.Blue, lw, lw, w-lw*2, h-lw*2);
                g.FillPolygon(upperBorderBrush, new Point[] {
                    new Point(0, 0), new Point(w, 0), new Point(w-lw, lw),
                    new Point(lw, lw), new Point(lw, h-lw), new Point(0, h)
                });
            }

            if (!String.IsNullOrEmpty(Text))
            {
                string text = Text.ToUpperInvariant();
                int press = (MouseOver && Pressed) ? 2 : 0;
                int shadow = Math.Max(1, IsTitle ? lw/3 : lw/4);
                SolidBrush fontBrush = new SolidBrush(IsTitle ? Color.White : Color.Yellow);

                Font f = IsTitle ?
                    new Font(TitleFontName, Height * 2 / 9, TitleFontStyle) :
                    new Font(BoardFontName, Height / 3, BoardFontStyle);

                StringFormat centered = new StringFormat();
                centered.Alignment = StringAlignment.Center;
                centered.LineAlignment = StringAlignment.Center;

                g.DrawString(text, f, Brushes.Black,
                    new RectangleF(lw+shadow+press, lw+shadow+press, Width-2*lw, Height-2*lw),
                    centered);
                g.DrawString(text, f, fontBrush,
                    new RectangleF(lw+press, lw+press, Width-2*lw, Height-2*lw),
                    centered);
            }
        }

        private bool Pressed
        {
            get { return settings[GBB_PRESSED]; }
            set
            {
                bool old = settings[GBB_PRESSED];
                settings[GBB_PRESSED] = value;
                if (old != value)
                    Refresh();
            }
        }

        public void Refresh()
        {
            view.Invalidate(rect);
            view.Update();
        }

        public bool Selected
        {
            get { return settings[GBB_SELECTED]; }
            set
            {
                bool old = settings[GBB_SELECTED];
                settings[GBB_SELECTED] = value;
                if (old != value)
                    Refresh();
            }
        }

        public Size Size
        {
            get { return rect.Size; }
            set { rect.Size = value; }
        }

        public string Text
        {
            get { return text; }
            set
            {
                string oldText = text;
                text = value;
                if (value != oldText)
                    Refresh();
            }
        }

        public bool Tracking
        {
            get { return settings[GBB_TRACKED]; }
            set
            {
                bool old = settings[GBB_TRACKED];
                settings[GBB_TRACKED] = value;
                if (old != value)
                {
                    Refresh();
                    if (TrackingChanged != null)
                        TrackingChanged(this, EventArgs.Empty);
                }
            }
        }

        public int Width
        {
            get { return rect.Width; }
        }
    }
}