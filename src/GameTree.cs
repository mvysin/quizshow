using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Jeopardy
{
    [SuppressUnmanagedCodeSecurity]
    internal class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        public static extern bool LockWindowUpdate(IntPtr hWndLock);

        [DllImport("user32.dll")]
        public static extern bool ValidateRect(IntPtr hWnd, IntPtr rc);
    }

    class GameTree : TreeView
    {
        const int WM_PAINT = 0x000F;
        const int WM_ERASEBKGND = 0x0014;

        private TreeNode updatingNode = null;


        public GameTree()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawAll;
            ItemHeight = 2*Font.Height+2;
            ShowRootLines = false;
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        public void LockUpdate()
        {
            SafeNativeMethods.LockWindowUpdate(Handle);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            Graphics g = e.Graphics;
            TreeNode n = e.Node;
            Font f = Font;

            int indent = 20 * e.Node.Level;

            string text = e.Node.Text;
            string[] lines = text.Split('\n');
            int width = 0;
            foreach (string line in lines)
            {
                int lineWidth = TextRenderer.MeasureText(line, f).Width;
                if (lineWidth > width)
                    width = lineWidth;
            }

            Rectangle bounds = new Rectangle(e.Bounds.X+indent, e.Bounds.Y, Math.Max(width+4, Width-indent), e.Bounds.Height);
            Rectangle textRect = Rectangle.Inflate(bounds, 1, 0);       // to align with mouse-over tooltip
            TextFormatFlags format = TextFormatFlags.Top;
            if (e.Node.Tag == null)  // category, center it
            {
                f = new Font(f.Name, f.Size*1.5f, f.Style, f.Unit);
                format = TextFormatFlags.VerticalCenter;
            }
            format |= TextFormatFlags.NoPrefix;

            g.FillRectangle(SystemBrushes.Window, e.Bounds);

            if ((e.State & TreeNodeStates.Selected) != 0 || (e.State & TreeNodeStates.Grayed) != 0)
            {
                g.FillRectangle(SystemBrushes.Highlight, bounds);
                TextRenderer.DrawText(g, text, f, textRect, SystemColors.HighlightText, SystemColors.Highlight, format);
            }
            else
                TextRenderer.DrawText(g, text, f, textRect, SystemColors.WindowText, SystemColors.Window, format);

            g.DrawLine(SystemPens.ControlLight, bounds.Left, bounds.Bottom-1, Width, bounds.Bottom-1);

            base.OnDrawNode(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            TreeNode clickedNode = GetNodeAt(e.Location);
            if (clickedNode != null)
                SelectedNode = clickedNode;

            base.OnMouseClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            TreeNode clickedNode = GetNodeAt(e.Location);
            if (clickedNode != null)
                clickedNode.Toggle();

            base.OnMouseDoubleClick(e);
        }

        public TreeNode UpdatingNode
        {
            get { return updatingNode; }
            set { updatingNode = value; }
        }

        public void UnlockUpdate()
        {
            SafeNativeMethods.LockWindowUpdate(IntPtr.Zero);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_ERASEBKGND)
            {
                m.Result = IntPtr.Zero;
                return;
            }
            else if (m.Msg == WM_PAINT)
            {
                if (UpdatingNode != null)
                {
                    SafeNativeMethods.ValidateRect(Handle, IntPtr.Zero);
                    Rectangle rc = UpdatingNode.Bounds;
                    rc.Width = Width-rc.X;
                    Invalidate(rc);
                    UpdatingNode = null;
                }
            }
            base.WndProc(ref m);
        }
    }
}