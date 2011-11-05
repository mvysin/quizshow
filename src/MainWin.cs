using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Vysin.QuizShow
{
    enum EditMode
    {
        None,
        Category,
        Clue
    }

    class MainWin : Form, IBoardView
    {
        MenuStrip menu = new MenuStrip();
        ToolStripPanel tspTop = new ToolStripPanel();
        ToolStripMenuItem fileMenu;
        ToolStripItem fileSave, fileSaveAs, fileClose, tbSave, tbStart;
        GameBoardView view;

        SplitContainer treeSplit = new SplitContainer(), viewSplit = new SplitContainer();
        GameTree tree = new GameTree();
        TableLayoutPanel table = new TableLayoutPanel();
        TextBox tbClue = new TextBox(), tbAnswer = new TextBox();
        Label lblWhat = new Label();

        string docName;
        bool docDirty;

        EditMode curEditMode;
        Clue selectedClue;
        Category selectedCategory;

        Dictionary<Category, TreeNode> categoryToNodeMap = new Dictionary<Category, TreeNode>();
        Dictionary<Clue, TreeNode> clueToNodeMap = new Dictionary<Clue, TreeNode>();

        MRUFileList mruFiles = new MRUFileList();
        ToolStripItem[] mruFileMenus;
        int mruInsertIndex;
        const int maxRecentFiles = 30;

        public MainWin()
        {
            DialogResult = DialogResult.Cancel;
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            ClientSize = new Size(800, 650);

            //weakEvents = new WeakEventHandler(this);
            AcceptButton = new EditDoneButton(this);

            tree.Dock = DockStyle.Fill;
            tree.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(TreeDoubleClick);

            lblWhat.Dock = DockStyle.Fill;
            lblWhat.UseMnemonic = false;
            lblWhat.Text = "Nothing Selected - Click a category or clue to edit";

            tbClue.Dock = DockStyle.Fill;
            tbClue.Multiline = true;
            tbClue.WordWrap = true;
            tbClue.AcceptsReturn = true;
            tbClue.ScrollBars = ScrollBars.Vertical;
            tbClue.TextChanged += new EventHandler(ClueChanged);

            tbAnswer.Dock = DockStyle.Fill;
            tbAnswer.TextChanged += new EventHandler(AnswerChanged);

            table.Dock = DockStyle.Fill;
            table.RowCount = 3;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, lblWhat.PreferredHeight));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, tbAnswer.PreferredHeight*3/2));
            table.Controls.Add(lblWhat, 0, 0);
            table.Controls.Add(tbClue, 0, 1);
            table.Controls.Add(tbAnswer, 0, 2);

            viewSplit.Dock = DockStyle.Fill;
            viewSplit.Orientation = Orientation.Horizontal;
            viewSplit.Panel1.BackColor = Color.Black;
            viewSplit.Panel1.Click += delegate(object s, EventArgs e) { if (view != null) view.Focus(); };
            viewSplit.Panel2.Controls.Add(table);
            viewSplit.SplitterDistance = viewSplit.Height / 5 * 4;

            treeSplit.Dock = DockStyle.Fill;
            treeSplit.Orientation = Orientation.Vertical;
            treeSplit.Panel1.Controls.Add(tree);
            treeSplit.Panel2.Controls.Add(viewSplit);
            treeSplit.SplitterDistance = treeSplit.Width / 4;
            Controls.Add(treeSplit);

            ToolStrip tsTop = new ToolStrip();
            tsTop.Items.Add("New", Resources.NewDocument, delegate(object s, EventArgs e) { DoNew(); });
            tsTop.Items.Add("Open", Resources.Open, delegate(object s, EventArgs e) { DoOpen(); });
            tbSave = tsTop.Items.Add("Save", Resources.Save, delegate(object s, EventArgs e) { DoSave(true); });
            tsTop.Items.Add(new ToolStripSeparator());
            tbStart = tsTop.Items.Add("Start Game", null, delegate(object s, EventArgs e) { DoGame(); });

            tspTop.Dock = DockStyle.Top;
            tspTop.Join(tsTop);
            Controls.Add(tspTop);

            fileMenu = new ToolStripMenuItem();
            fileMenu.Text = "&File";
            fileMenu.DropDownItems.Add("&New", Resources.NewDocument, delegate(object s, EventArgs e) { DoNew(); });
            fileMenu.DropDownItems.Add("&Open...", Resources.Open, delegate(object s, EventArgs e) { DoOpen(); });
            fileSave = fileMenu.DropDownItems.Add("&Save", Resources.Save, delegate(object s, EventArgs e) { DoSave(true); });
            fileSaveAs = fileMenu.DropDownItems.Add("Save &As...", null, delegate(object s, EventArgs e) { DoSave(false); });
            fileClose = fileMenu.DropDownItems.Add("&Close", null, delegate(object s, EventArgs e) { DoClose(); });
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            mruInsertIndex = fileMenu.DropDownItems.Count;
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("E&xit", null, delegate(object s, EventArgs e) { Close(); });

            ToolStripMenuItem helpMenu = new ToolStripMenuItem();
            helpMenu.Text = "&Help";
            helpMenu.DropDownItems.Add("Check for &Updates...", null, delegate(object s, EventArgs e) { DoUpdateCheck(); });
            helpMenu.DropDownItems.Add(new ToolStripSeparator());
            helpMenu.DropDownItems.Add("&About", null, delegate(object s, EventArgs e) { DoAbout(); });

            menu.Items.Add(fileMenu);
            menu.Items.Add(helpMenu);
            MainMenuStrip = menu;
            Controls.Add(menu);

            EditMode = EditMode.None;
            UpdateTitle();
            UpdateUI();
            UpdateMRUMenu();

            ResumeLayout();
            CenterToScreen();

            viewSplit.Panel1MinSize = 250;
            viewSplit.Panel2MinSize = 100;
            treeSplit.Panel1MinSize = 100;
            treeSplit.Panel2MinSize = 250*4/3;
            MinimumSize = new Size(500, 500);
        }

        private void AnswerChanged(object sender, EventArgs e)
        {
            if (EditMode == EditMode.Category && SelectedCategory != null)
            {
                SelectedCategory.Name = tbAnswer.Text;
                IsDirty = true;
            }
            else if (EditMode == EditMode.Clue && SelectedClue != null)
            {
                SelectedClue.Answer = tbAnswer.Text;
                IsDirty = true;
            }
        }

        private void ClueChanged(object sender, EventArgs e)
        {
            if (EditMode == EditMode.Clue && SelectedClue != null)
            {
                SelectedClue.Question = tbClue.Text;
                IsDirty = true;
            }
        }

        private string CurrentDocName
        {
            get
            {
                if (CurrentFileName != null)
                    return Path.GetFileName(CurrentFileName);
                else
                    return "Untitled";
            }
        }

        private string CurrentFileName
        {
            get { return docName; }
            set { docName = value; UpdateTitle(); }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (menu != null)
                    menu.Dispose();
                if (view != null)
                    view.Dispose();
                categoryToNodeMap.Clear();
                clueToNodeMap.Clear();
            }

            base.Dispose(disposing);
        }

        private void DoAbout()
        {
            MessageBox.Show(this,
                "Quiz Show Presenter Version " + Application.ProductVersion + "\r\n" +
                "Copyright (c) 2011 " + Application.CompanyName + "\r\n\r\n" +
                Environment.OSVersion.VersionString + "\r\n" +
                "Microsoft .NET Framework Version " + Environment.Version.ToString() + "\r\n\r\n" +
                "For more information, visit http://mvysin.com/projects/quizshow",
                Application.ProductName);
        }

        private bool DoClose()
        {
            if (QuizShow.Board == null)
                return true;

            if (IsDirty)
            {
                DialogResult dr = MessageBox.Show(this, "Do you want to save changes to " + CurrentDocName + " before closing?",
                    Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Cancel)
                    return false;
                else if (dr == DialogResult.Yes)
                {
                    if (!DoSave(false))
                        return false;
                }
            }

            view.Dispose();
            view = null;
            QuizShow.Board = null;

            CurrentFileName = null;
            IsDirty = false;
            UpdateUI();
            UpdateTree();

            return true;
        }

        private void DoGame()
        {
            SelectWindowForm select = new SelectWindowForm();
            select.ShowDialog(this);
            if (select.DialogResult == DialogResult.Cancel)
                return;

            PresenterWin presWin = null;
            GameWin gameWin = null;

            // if both screens are null, just display two windows
            if (select.PresenterScreen == null && select.PlayerScreen == null)
            {
                gameWin = new GameWin();
                presWin = new PresenterWin();
                presWin.Load += delegate(object s, EventArgs e) { gameWin.Show(); };
            }

            if (select.PresenterScreen != null)
            {
                presWin = new PresenterWin();
                presWin.SetScreen(select.PresenterScreen);
            }
            if (select.PlayerScreen != null)
            {
                gameWin = new GameWin();
                gameWin.SetScreen(select.PlayerScreen);
                if (presWin != null)
                    presWin.Load += delegate(object s, EventArgs e) { gameWin.Show(); };
            }

            if (presWin == null)
                gameWin.ShowDialog(this);
            else
                presWin.ShowDialog(this);

            if (gameWin != null)
                gameWin.Dispose();
            if (presWin != null)
                presWin.Dispose();
        }

        private bool DoLoad(string filename)
        {
            if (filename == null)
                QuizShow.Board = new Board();
            else
            {
                try
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(Board));
                        QuizShow.Board = (Board)xs.Deserialize(fs);
                    }
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show(this, "The document you selected is not a valid board\n\n" +
                        "Reason: " + e.Message,
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            QuizShow.Board.AddView(this);

            view = new GameBoardView();
            view.Editable = true;
            view.Dock = DockStyle.Fill;
            viewSplit.Panel1.Controls.Add(view);

            if (filename != null)
                mruFiles.AddFile(filename);

            CurrentFileName = filename;
            IsDirty = false;
            UpdateUI();
            UpdateTree();
            UpdateMRUMenu();

            return true;
        }

        private bool DoNew()
        {
            if (!DoClose())
                return true;

            return DoLoad(null);
        }

        private bool DoOpen()
        {
            if (!DoClose())
                return true;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "xml";
            ofd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            ofd.FilterIndex = 1;
            DialogResult dr = ofd.ShowDialog(this);
            if (dr == DialogResult.Cancel)
                return true;

            return DoLoad(ofd.FileName);
        }

        private bool DoOpen(string filename)
        {
            if (!DoClose())
                return true;
            return DoLoad(filename);
        }

        private bool DoSave(bool fast)
        {
            if (CurrentFileName == null)
                fast = false;

            if (!fast)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = "xml";
                sfd.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                sfd.FilterIndex = 1;
                if (HasDocument)
                    sfd.FileName = CurrentFileName;
                DialogResult dr = sfd.ShowDialog(this);
                if (dr == DialogResult.Cancel)
                    return false;

                CurrentFileName = sfd.FileName;
            }

            using (FileStream fs = new FileStream(CurrentFileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Board));
                xs.Serialize(fs, QuizShow.Board);
            }

            mruFiles.AddFile(CurrentFileName);

            IsDirty = false;
            return true;
        }

        private void DoUpdateCheck()
        {
            MessageBox.Show(this, "Automatic updates not yet implemented.\n" +
                "Please visit http://mvysin.com/projects/quizshow for the latest version.");
        }

        private void EditDoneClick()
        {
            QuizShow.Board.NotifyViews(NotifyAction.DisplayBoard, 0, 0);
        }

        private EditMode EditMode
        {
            get { return curEditMode; }
            set
            {
                curEditMode = value;
                switch (curEditMode)
                {
                case EditMode.None:
                    SelectedCategory = null;
                    SelectedClue = null;
                    tbClue.Enabled = tbAnswer.Enabled = false;
                    tbClue.Text = tbAnswer.Text = String.Empty;
                    break;
                case EditMode.Category:
                    tbClue.Enabled = false;
                    tbAnswer.Enabled = true;
                    SelectedClue = null;
                    tbClue.Text = String.Empty;
                    break;
                case EditMode.Clue:
                    tbClue.Enabled = tbAnswer.Enabled = true;
                    SelectedCategory = null;
                    break;
                }
            }
        }

        private bool HasDocument
        {
            get { return QuizShow.Board != null; }
        }

        private bool IsDirty
        {
            get { return docDirty; }
            set { docDirty = value; UpdateTitle(); }
        }

        void MruFileClick(object sender, EventArgs e)
        {
            ToolStripMenuItem m = (ToolStripMenuItem)sender;
            string filename = (string)m.Tag;
            if (!File.Exists(filename))
            {
                // remove it from the MRU list?
                DialogResult dr = MessageBox.Show(this,
                    "\"" + filename + "\" refers to a file that does not exist or is unavailable.\n" +
                    "Would you like to remove it from the list of recent files?",
                    Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Yes)
                {
                    mruFiles.RemoveFile(filename);
                    UpdateMRUMenu();
                }
            }
            else
            {
                // open the file
                if (!DoOpen(filename))
                {
                    DialogResult dr = MessageBox.Show(this,
                        "\"" + filename + "\" is an invalid file.\n" +
                        "Would you like to remove it from the list of recent files?",
                        Application.ProductName, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                    if (dr == DialogResult.Yes)
                    {
                        mruFiles.RemoveFile(filename);
                        UpdateMRUMenu();
                    }
                }
            }
        }

        public void Notify(NotifyAction a, int x, int y)
        {
            Board board = QuizShow.Board;

            if (a == NotifyAction.ButtonClicked)
            {
                view.SetSelected(x, y);

                // check if it's the category:
                if (y < 0)
                {
                    EditMode = EditMode.Category;
                    SelectedCategory = board.Category[x];
                    lblWhat.Text = "Category " + (x + 1).ToString();
                    tbAnswer.Focus();
                }
                else
                {
                    EditMode = EditMode.Clue;
                    SelectedClue = board.Category[x].Clue[y];
                    lblWhat.Text = "\"" + board.Category[x].Name + "\" -> $"
                        + board.PointValues[y] + " Clue";
                    tbClue.Focus();
                    tbClue.SelectionStart = tbClue.Text.Length;
                    tbClue.SelectionLength = 0;
                }
            }
            else if (a == NotifyAction.DisplayBoard)
            {
                lblWhat.Text = "Nothing Selected - Click a category or clue to edit";
                EditMode = EditMode.None;
            }
            else if (a == NotifyAction.TrackButtonChanged)
            {
                if (SelectedCategory == null && SelectedClue == null)
                {
                    if (x >= 0)
                    {
                        if (y >= 0)
                        {
                            tbClue.Text = board.Category[x].Clue[y].Question;
                            tbAnswer.Text = board.Category[x].Clue[y].Answer;
                        }
                        else
                        {
                            tbClue.Text = String.Empty;
                            tbAnswer.Text = board.Category[x].Name;
                        }
                    }
                    else
                        tbClue.Text = tbAnswer.Text = String.Empty;
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!DoClose())
                e.Cancel = true;
        }

        private Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = null;
                if (value != null)
                    tbAnswer.Text = value.Name;
                selectedCategory = value;
            }
        }

        private Clue SelectedClue
        {
            get { return selectedClue; }
            set
            {
                selectedClue = null;
                if (value != null)
                {
                    tbClue.Text = value.Question;
                    tbAnswer.Text = value.Answer;
                }
                selectedClue = value;
            }
        }

        private void TreeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.Tag is Clue)        // clue
            {
                Clue c = (Clue)node.Tag;
                if (view != null && view.DisplayingBoard && !view.HasSelectedButton)
                    view.EditClue(c);
            }
        }

        private void UpdateMRUMenu()
        {
            // first remove old
            if (mruFileMenus != null)
            {
                foreach (ToolStripItem i in mruFileMenus)
                    fileMenu.DropDownItems.Remove(i);
            }

            // now insert the items
            int nItems = Math.Min(mruFiles.RecentFileCount, maxRecentFiles);
            mruFileMenus = new ToolStripItem[Math.Max(1, nItems)];
            if (mruFiles.RecentFileCount == 0)
            {
                ToolStripItem dummy = new ToolStripMenuItem("No Recent Files");
                dummy.Enabled = false;
                mruFileMenus[0] = dummy;
                fileMenu.DropDownItems.Insert(mruInsertIndex, dummy);
            }
            else
            {
                for (int i = 0; i < nItems; i++)
                {
                    string fileName = mruFiles.GetFile(i);
                    ToolStripMenuItem m = new ToolStripMenuItem();
                    m.Text = ((i<9)?"&":"") + (i+1).ToString() + " " + Path.GetFileName(fileName);
                    m.ToolTipText = fileName;
                    m.Tag = fileName;
                    m.Click += new EventHandler(MruFileClick);
                    mruFileMenus[i] = m;
                    fileMenu.DropDownItems.Insert(mruInsertIndex+i, m);
                }
            }
        }

        private void UpdateTitle()
        {
            string text = Application.ProductName;
            if (HasDocument)
                text = CurrentDocName + (IsDirty ? "*" : "") + " - " + text;
            Text = text;
        }

        private void UpdateTree()
        {
            tree.BeginUpdate();

            tree.Nodes.Clear();
            clueToNodeMap.Clear();
            categoryToNodeMap.Clear();

            if (!HasDocument)
            {
                tree.EndUpdate();
                return;
            }

            for (int cat = 0; cat < 6; cat++)
            {
                Category theCat = QuizShow.Board.Category[cat];
                TreeNode catNode = new TreeNode(theCat.Name);

                categoryToNodeMap.Add(theCat, catNode);
                theCat.PropertyChanged += new PropertyChangedEventHandler(WeakPropertyChanged);

                for (int clue = 0; clue < 5; clue++)
                {
                    Clue theClue = theCat.Clue[clue];
                    TreeNode clueNode = new TreeNode();
                    UpdateTreeNode(clueNode, theClue);

                    clueToNodeMap.Add(theClue, clueNode);
                    theClue.PropertyChanged += new PropertyChangedEventHandler(WeakPropertyChanged);

                    catNode.Nodes.Add(clueNode);
                }

                tree.Nodes.Add(catNode);
            }

            tree.ExpandAll();
            tree.Nodes[0].EnsureVisible();
            tree.EndUpdate();
        }

        private void UpdateTreeNode(TreeNode n, Clue c)
        {
            n.Text = c.Question.Replace('\n', ' ').Replace("\r", "") + "\n" + c.Answer;
            n.Tag = c;
            n.ToolTipText = n.Text;
        }

        private void UpdateUI()
        {
            fileSave.Enabled = fileSaveAs.Enabled = fileClose.Enabled =
                tbSave.Enabled = tbStart.Enabled = HasDocument;
        }

        private void WeakPropertyChanged(object s, PropertyChangedEventArgs e)
        {
            tree.LockUpdate();
            if (s is Category)
            {
                Category cat = (Category)s;
                TreeNode n = categoryToNodeMap[cat];
                tree.UpdatingNode = n;
                n.Text = cat.Name;
            }
            else if (s is Clue)
            {
                Clue clue = (Clue)s;
                TreeNode n = clueToNodeMap[clue];
                tree.UpdatingNode = n;
                UpdateTreeNode(n, clue);
            }
            tree.UnlockUpdate();
        }



        // .NET FRAMEWORK STUPIDITY: must be derivation of Control,
        // or else PerformClick won't be called...  What's the point, exactly?
        class EditDoneButton : Control, IButtonControl
        {
            DialogResult dr;
            MainWin parent;

            public EditDoneButton(MainWin parent)
            {
                this.parent = parent;
            }

            public DialogResult DialogResult
            {
                get { return dr; }
                set { dr = value; }
            }

            public void NotifyDefault(bool value) { }

            public void PerformClick()
            {
                parent.EditDoneClick();
            }
        }
    }
}