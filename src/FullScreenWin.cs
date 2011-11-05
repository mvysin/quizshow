using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Jeopardy
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    enum WmSizingEdge
    {
        Left=1,
        Right,
        Top,
        TopLeft,
        TopRight,
        Bottom,
        BottomLeft,
        BottomRight
    }

    class FullScreenWin : Form
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int WM_SIZING = 0x0214;
        private const uint SC_MAXIMIZE = 0xF030;
        private const uint SC_RESTORE = 0xF120;

        Size ncAdjust;
        Button btnRestore, btnMinimize, btnExit;

        public FullScreenWin()
        {
            ncAdjust = Size - ClientSize;
            BackColor = Color.Black;

            btnMinimize = new Button();
            InitWindowButton(btnMinimize);
            btnMinimize.Text = "\x30";
            btnMinimize.Click += delegate(object s, EventArgs e)
            {
                WindowState = FormWindowState.Minimized;
            };
            Controls.Add(btnMinimize);

            btnRestore = new Button();
            InitWindowButton(btnRestore);
            btnRestore.Text = "\x32";
            btnRestore.Click += delegate(object s, EventArgs e)
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.Sizable;
                btnExit.Visible = btnRestore.Visible = btnMinimize.Visible = false;
            };
            Controls.Add(btnRestore);

            btnExit = new Button();
            InitWindowButton(btnExit);
            btnExit.Text = "\x72";
            btnExit.Click += delegate(object s, EventArgs e)
            {
                Close();
            };
            Controls.Add(btnExit);
        }

        private void InitWindowButton(Button btn)
        {
            btn.Visible = false;
            btn.Size = new Size(35, 25);
            btn.Font = new Font("Marlett", 12f, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Popup;
            btn.ForeColor = Color.White;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            e.Control.ControlAdded += delegate(object s, ControlEventArgs ea)
            {
                OnControlAdded(ea);
            };
            e.Control.MouseMove += delegate(object s, MouseEventArgs me)
            {
                Point pt = ((Control)s).PointToScreen(me.Location);
                pt = PointToClient(pt);
                OnMouseMove(new MouseEventArgs(me.Button, me.Clicks, pt.X, pt.Y, me.Delta));
            };

            foreach (Control c in e.Control.Controls)
                OnControlAdded(new ControlEventArgs(c));

            base.OnControlAdded(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // only show buttons if maximized
            if (WindowState == FormWindowState.Maximized)
            {
                if (e.X > btnMinimize.Left && e.Y < btnMinimize.Bottom)
                {
                    btnRestore.Visible = btnMinimize.Visible = btnExit.Visible = true;
                    btnExit.BringToFront();
                    btnRestore.BringToFront();
                    btnMinimize.BringToFront();
                    btnRestore.Refresh();
                    btnExit.Refresh();
                    btnMinimize.Refresh();
                }
                else
                    btnRestore.Visible = btnMinimize.Visible = btnExit.Visible = false;
            }

            base.OnMouseMove(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (btnMinimize != null)
                btnMinimize.Location = new Point(ClientSize.Width-btnExit.Width-btnRestore.Width-btnMinimize.Width, 0);
            if (btnRestore != null)
                btnRestore.Location = new Point(ClientSize.Width-btnExit.Width-btnRestore.Width, 0);
            if (btnExit != null)
                btnExit.Location = new Point(ClientSize.Width-btnExit.Width, 0);
            base.OnSizeChanged(e);
        }

        public void SetScreen(Screen s)
        {
            StartPosition = FormStartPosition.Manual;
            Location = s.Bounds.Location;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (((uint)m.WParam & 0x0FFF0) == SC_MAXIMIZE)
                {
                    btnExit.Visible = btnRestore.Visible = btnMinimize.Visible = true;
                    FormBorderStyle = FormBorderStyle.None;
                }
                else if (((uint)m.WParam & 0x0FFF0) == SC_RESTORE)
                {
                    if (WindowState == FormWindowState.Maximized)
                    {
                        btnExit.Visible = btnRestore.Visible = btnMinimize.Visible = false;
                        FormBorderStyle = FormBorderStyle.Sizable;
                    }
                }
            }
            else if (m.Msg == WM_SIZING)
            {
                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                WmSizingEdge edge = (WmSizingEdge)m.WParam;

                if (edge == WmSizingEdge.Top || edge == WmSizingEdge.Bottom)
                {
                    int newHeight = rect.bottom - rect.top - ncAdjust.Height;
                    int newWidth = newHeight / 3 * 4;
                    if (edge == WmSizingEdge.Top)
                        rect.left = rect.right - newWidth - ncAdjust.Width;
                    else
                        rect.right = rect.left + newWidth + ncAdjust.Width;
                }
                else
                {
                    int newWidth = rect.right - rect.left - ncAdjust.Width;
                    int newHeight = newWidth / 4 * 3;
                    if (edge == WmSizingEdge.Left || edge == WmSizingEdge.TopLeft || edge == WmSizingEdge.TopRight)
                        rect.top = rect.bottom - newHeight - ncAdjust.Height;
                    else
                        rect.bottom = rect.top + newHeight + ncAdjust.Height;
                }

                Marshal.StructureToPtr(rect, m.LParam, false);
                m.Result = (IntPtr)1;
            }

            base.WndProc(ref m);
        }
    }
}
