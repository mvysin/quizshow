using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Vysin.QuizShow
{
    public class ScreenEventArgs : EventArgs
    {
        private int id;
        private Screen screen;

        internal ScreenEventArgs(int id, Screen s)
        {
            this.id = id;
            this.screen = s;
        }

        public int Id
        {
            get { return id; }
        }

        public Screen Screen
        {
            get { return screen; }
        }
    }

    public class ScreenInfo
    {
        int id;
        Screen screen;
        Rectangle rect;

        internal ScreenInfo(int i, Screen s, Rectangle r)
        {
            id = i;
            screen = s;
            rect = r;
        }

        internal Rectangle DisplayRect
        {
            get { return rect; }
        }

        public int Id
        {
            get { return id; }
        }

        public Screen Screen
        {
            get { return screen; }
        }

        public override string ToString()
        {
            return String.Format("Screen {0} ({1}x{2})", Id+1, Screen.Bounds.Width, Screen.Bounds.Height);
        }
    }


    public delegate void ScreenDelegate(object sender, ScreenEventArgs ea);


    public class ScreenConfigurationPanel : Control
    {
        private class ScreenOverlayWindow : Form
        {
            Screen screen;

            public ScreenOverlayWindow(Screen s)
            {
                screen = s;

                Location = s.Bounds.Location;
                Size = s.Bounds.Size;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;

                Rectangle rc = s.Bounds;
                rc.Offset(-rc.Left, -rc.Top);
                Region r = new Region(rc);
                rc.Inflate(-30, -30);
                r.Xor(rc);
                Region = r;

                BackColor = SystemColors.HotTrack;
            }

            public Screen Screen
            {
                get { return screen; }
            }

            protected override bool ShowWithoutActivation
            {
                get { return true; }
            }
        }


        List<ScreenInfo> rects = new List<ScreenInfo>();
        ScreenOverlayWindow overlay = null;
        ScreenInfo curScreen = null;

        public event ScreenDelegate ScreenClick;
        public event ScreenDelegate ScreenEnter;
        public event ScreenDelegate ScreenLeave;


        public ScreenConfigurationPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            SystemEvents.DisplaySettingsChanged += new EventHandler(OnDisplaySettingsChanged);
        }

        ~ScreenConfigurationPanel()
        {
            SystemEvents.DisplaySettingsChanged -= new EventHandler(OnDisplaySettingsChanged);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (overlay != null)
            {
                overlay.Close();
                overlay = null;
            }

            base.OnHandleDestroyed(e);
        }

        void OnDisplaySettingsChanged(object sender, EventArgs e)
        {
            Recalc();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            ScreenInfo si = ScreenInfoFromPoint(e.Location);
            if (si != null)
                OnScreenClick(new ScreenEventArgs(si.Id+1, si.Screen));

            base.OnMouseClick(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (curScreen != null)
            {
                OnScreenLeave(new ScreenEventArgs(curScreen.Id+1, curScreen.Screen));
                curScreen = null;
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ScreenInfo si = ScreenInfoFromPoint(e.Location);
            if (si != curScreen)
            {
                if (si != null)
                    OnScreenEnter(new ScreenEventArgs(si.Id+1, si.Screen));
                else
                    OnScreenLeave(new ScreenEventArgs(curScreen.Id+1, curScreen.Screen));
                curScreen = si;
            }

            base.OnMouseMove(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(SystemBrushes.AppWorkspace, e.ClipRectangle);

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            foreach (ScreenInfo si in rects)
            {
                Rectangle rc = si.DisplayRect;
                g.FillRectangle(SystemBrushes.Highlight, rc);
                g.DrawRectangle(SystemPens.HotTrack, rc);

                Font f = new Font("Arial", rc.Height, FontStyle.Bold, GraphicsUnit.Pixel);

                // need to calculate the descent of the font and remove it from consideration
                double asc = (double)f.Height / f.FontFamily.GetEmHeight(FontStyle.Bold) * f.FontFamily.GetCellAscent(FontStyle.Bold);

                Rectangle rcText = rc;
                rcText.Offset(0, (int)((double)rc.Height/2 - (double)asc/2));
                g.DrawString((si.Id+1).ToString(), f, SystemBrushes.HighlightText, rcText, sf);
            }

            Font helpFont = this.Font;
            g.DrawString("Mouse over to identify your screens", helpFont,
                SystemBrushes.WindowText, new RectangleF(0, Height-20, Width, 20), sf);
        }

        protected virtual void OnScreenClick(ScreenEventArgs e)
        {
            if (ScreenClick != null)
                ScreenClick(this, e);
        }

        protected virtual void OnScreenEnter(ScreenEventArgs e)
        {
            if (ScreenEnter != null)
                ScreenEnter(this, e);

            if (overlay != null)
            {
                overlay.Close();
                overlay = null;
            }

            overlay = new ScreenOverlayWindow(e.Screen);
            overlay.Show();
        }

        protected virtual void OnScreenLeave(ScreenEventArgs e)
        {
            if (ScreenLeave != null)
                ScreenLeave(this, e);

            if (overlay != null)
            {
                overlay.Close();
                overlay = null;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Recalc();
            base.OnSizeChanged(e);
        }

        private void Recalc()
        {
            rects.Clear();

            Rectangle rcDraw = new Rectangle(0, 0, Width, Height-20);

            // Calculate the overall bounds
            Screen[] scrs = Screen.AllScreens;
            Rectangle rcBounds = new Rectangle();
            foreach (Screen s in scrs)
                rcBounds = Rectangle.Union(rcBounds, s.Bounds);

            // plus a 10% padding on all sides, calculate a scaling factor
            Rectangle rcOverall = Rectangle.Inflate(rcBounds, rcBounds.Width / 10, rcBounds.Height / 10);

            // calcualte scaling factor length-wise and width-wise, and pick the larger divisor
            double divW = (double)rcOverall.Width / rcDraw.Width;
            double divH = (double)rcOverall.Height / rcDraw.Height;
            double div = Math.Max(divW, divH);

            Point topPoint = new Point(rcDraw.Width / 2 - (int)(rcBounds.Width / 2 / div),
                                       rcDraw.Height / 2 - (int)(rcBounds.Height / 2 / div));

            // now calculate each monitor's display rectangle
            for (int i = 0; i < scrs.Length; i++)
            {
                Screen s = scrs[i];

                Rectangle rc = s.Bounds;
                rc.Offset(-rcBounds.Left, -rcBounds.Top);
                rc = new Rectangle((int)(rc.Left / div),
                                   (int)(rc.Top / div),
                                   (int)(rc.Width / div),
                                   (int)(rc.Height / div));
                rc.Offset(topPoint);

                rects.Add(new ScreenInfo(i, s, rc));
            }
        }

        private ScreenInfo ScreenInfoFromPoint(Point pt)
        {
            foreach (ScreenInfo si in rects)
            {
                if (si.DisplayRect.Contains(pt))
                    return si;
            }
            return null;
        }

        public ICollection<ScreenInfo> Screens
        {
            get { return rects.AsReadOnly(); }
        }
    }
}