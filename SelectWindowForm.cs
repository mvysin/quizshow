using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jeopardy
{
    class SelectWindowForm : Form
    {
        ScreenConfigurationPanel scrs;
        TableLayoutPanel tbl;
        RadioButton rbFull, rbChoose, rbManual;
        ComboBox cxFPres, cxFPlay, cxCWhich, cxCDisp;
        Label lblFPres, lblFPlay, lblCWhich, lblCDisp, lblFNotAvail;
        Button btnStart, btnCancel;

        Screen presenterScreen = null, playerScreen = null;

        public SelectWindowForm()
        {
            DialogResult = DialogResult.Cancel;
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            Text = "Jeopardy | Select Window Layout";
            ClientSize = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            tbl = new TableLayoutPanel();
            rbFull = new RadioButton();
            rbChoose = new RadioButton();
            rbManual = new RadioButton();
            lblFPres = new Label();
            cxFPres = new ComboBox();
            lblFPlay = new Label();
            cxFPlay = new ComboBox();
            lblFNotAvail = new Label();
            lblCWhich = new Label();
            cxCWhich = new ComboBox();
            lblCDisp = new Label();
            cxCDisp = new ComboBox();
            btnStart = new Button();
            btnCancel = new Button();


            scrs = new ScreenConfigurationPanel();
            scrs.Location = new Point(0, 0);
            scrs.Size = new Size(ClientSize.Width, 150);
            Controls.Add(scrs);

            tbl.Location = new Point(20, 170);
            tbl.Size = new Size(ClientSize.Width - 40, ClientSize.Height - 215);
            tbl.ColumnCount = 5;
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
            tbl.RowStyles.Add(new RowStyle());
            tbl.RowStyles.Add(new RowStyle());
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            tbl.RowStyles.Add(new RowStyle());
            tbl.RowStyles.Add(new RowStyle());
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            tbl.RowStyles.Add(new RowStyle());
            Controls.Add(tbl);


            rbFull.Location = new Point(20, 170);
            rbFull.AutoSize = true;
            rbFull.Text = "Full : Display the board and the presenter view as follows:";
            rbFull.CheckedChanged += new EventHandler(TypeChanged);
            rbFull.Checked = true;
            tbl.Controls.Add(rbFull, 0, 0);
            tbl.SetColumnSpan(rbFull, 5);

            lblFPres.Text = "Presenter View:";
            lblFPres.Dock = DockStyle.Right;
            lblFPres.TextAlign = ContentAlignment.MiddleLeft;
            lblFPres.AutoSize = true;
            tbl.Controls.Add(lblFPres, 1, 1);

            cxFPres.DropDownStyle = ComboBoxStyle.DropDownList;
            cxFPres.AutoSize = true;
            cxFPres.Size = new Size(150, cxFPres.PreferredHeight);
            cxFPres.SelectedValueChanged += new EventHandler(ComboChanged);
            tbl.Controls.Add(cxFPres, 2, 1);

            lblFPlay.Text = "Player View:";
            lblFPlay.Dock = DockStyle.Right;
            lblFPlay.TextAlign = ContentAlignment.MiddleLeft;
            lblFPlay.AutoSize = true;
            tbl.Controls.Add(lblFPlay, 3, 1);

            cxFPlay.DropDownStyle = ComboBoxStyle.DropDownList;
            cxFPlay.AutoSize = true;
            cxFPlay.Size = new Size(150, cxFPlay.PreferredHeight);
            cxFPlay.SelectedValueChanged += new EventHandler(ComboChanged);
            tbl.Controls.Add(cxFPlay, 4, 1);

            lblFNotAvail.Text = "Full view is not available.  Connect to a projector or television first.";
            lblFNotAvail.TextAlign = ContentAlignment.MiddleCenter;
            lblFNotAvail.Visible = false;
            Controls.Add(lblFNotAvail);


            rbChoose.Location = new Point(20, 230);
            rbChoose.AutoSize = true;
            rbChoose.Text = "Let Me Choose:";
            rbChoose.CheckedChanged += new EventHandler(TypeChanged);
            tbl.Controls.Add(rbChoose, 0, 3);
            tbl.SetColumnSpan(rbChoose, 5);

            lblCWhich.Text = "Display only:";
            lblCWhich.Dock = DockStyle.Right;
            lblCWhich.TextAlign = ContentAlignment.MiddleLeft;
            lblCWhich.AutoSize = true;
            tbl.Controls.Add(lblCWhich, 1, 4);

            cxCWhich.DropDownStyle = ComboBoxStyle.DropDownList;
            cxCWhich.AutoSize = true;
            cxCWhich.Items.Add("Presenter View");
            cxCWhich.Items.Add("Player View");
            cxCWhich.SelectedIndex = 0;
            cxCWhich.Size = new Size(150, cxCWhich.PreferredHeight);
            cxCWhich.SelectedValueChanged += new EventHandler(ComboChanged);
            tbl.Controls.Add(cxCWhich, 2, 4);

            lblCDisp.Text = "on display:";
            lblCDisp.Dock = DockStyle.Right;
            lblCDisp.TextAlign = ContentAlignment.MiddleLeft;
            lblCDisp.AutoSize = true;
            tbl.Controls.Add(lblCDisp, 3, 4);

            cxCDisp.DropDownStyle = ComboBoxStyle.DropDownList;
            cxCDisp.AutoSize = true;
            cxCDisp.Size = new Size(150, cxCDisp.PreferredHeight);
            cxCDisp.SelectedValueChanged += new EventHandler(ComboChanged);
            tbl.Controls.Add(cxCDisp, 4, 4);


            rbManual.Location = new Point(20, 290);
            rbManual.AutoSize = true;
            rbManual.Text = "Manual: Just give me two windows, I'll put them where I want!";
            rbManual.CheckedChanged += new EventHandler(TypeChanged);
            tbl.Controls.Add(rbManual, 0, 6);
            tbl.SetColumnSpan(rbManual, 5);


            btnStart.Location = new Point(ClientSize.Width - 200, ClientSize.Height - 45);
            btnStart.Size = new Size(80, 25);
            btnStart.Text = "Start Game";
            btnStart.Click += new EventHandler(ButtonClick);
            Controls.Add(btnStart);
            AcceptButton = btnStart;

            btnCancel.Location = new Point(ClientSize.Width - 100, ClientSize.Height - 45);
            btnCancel.Size = new Size(80, 25);
            btnCancel.Text = "Cancel";
            btnCancel.Click += new EventHandler(ButtonClick);
            Controls.Add(btnCancel);
            CancelButton = btnCancel;

            ResumeLayout();
            CenterToScreen();
            Recalc();

            lblFNotAvail.Location = lblFPres.Location + new Size(tbl.Location);
            lblFNotAvail.Size = Rectangle.Union(new Rectangle(lblFPres.Location, lblFPres.Size),
                new Rectangle(cxFPlay.Location, cxFPlay.Size)).Size;
            lblFNotAvail.BringToFront();
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            if (sender == btnStart)
            {
                if (rbFull.Checked)
                {
                    presenterScreen = ((ScreenInfo)cxFPres.SelectedItem).Screen;
                    playerScreen = ((ScreenInfo)cxFPlay.SelectedItem).Screen;
                }
                else if (rbChoose.Checked)
                {
                    playerScreen = presenterScreen = null;
                    if ((string)cxCWhich.SelectedItem == "Player View")
                        playerScreen = ((ScreenInfo)cxCDisp.SelectedItem).Screen;
                    else
                        presenterScreen = ((ScreenInfo)cxCDisp.SelectedItem).Screen;
                }
                else if (rbManual.Checked)
                {
                    presenterScreen = playerScreen = null;
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            else if (sender == btnCancel)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void ComboChanged(object sender, EventArgs e)
        {
            TypeChanged(sender, e);
        }

        public Screen PlayerScreen
        {
            get { return playerScreen; }
        }

        public Screen PresenterScreen
        {
            get { return presenterScreen; }
        }

        private void Recalc()
        {
            ICollection<ScreenInfo> scrs = this.scrs.Screens;

            if (scrs.Count == 1)
            {
                if (rbFull.Checked)
                    rbChoose.Checked = true;
                cxFPlay.Enabled = cxFPres.Enabled = rbFull.Enabled = false;
                cxFPlay.Visible = cxFPres.Visible = lblFPlay.Visible = lblFPres.Visible = false;
                lblFNotAvail.Visible = true;
            }
            else
            {
                cxFPlay.Enabled = cxFPres.Enabled = rbFull.Enabled = true;
                cxFPlay.Visible = cxFPres.Visible = lblFPlay.Visible = lblFPres.Visible = true;
                lblFNotAvail.Visible = false;
            }

            ScreenInfo primary = null, secondary = null;

            foreach (ScreenInfo si in scrs)
            {
                cxFPres.Items.Add(si);
                cxFPlay.Items.Add(si);
                cxCDisp.Items.Add(si);

                if (si.Screen.Primary)
                    primary = si;
                else if (secondary == null ||
                         (secondary.Screen.Bounds.Width >= si.Screen.Bounds.Width &&
                          secondary.Screen.Bounds.Height > si.Screen.Bounds.Height))
                    secondary = si;
            }

            if (primary != null)
            {
                cxFPres.SelectedItem = primary;
                cxCDisp.SelectedItem = primary;
            }
            if (secondary != null)
                cxFPlay.SelectedItem = secondary;
        }

        private void TypeChanged(object sender, EventArgs e)
        {
            lblFPlay.Enabled = lblFPres.Enabled = cxFPlay.Enabled = cxFPres.Enabled = rbFull.Checked;
            lblCDisp.Enabled = lblCWhich.Enabled = cxCDisp.Enabled = cxCWhich.Enabled = rbChoose.Checked;
            btnStart.Enabled = (rbFull.Checked && (cxFPlay.SelectedItem != cxFPres.SelectedItem)) ||
                    rbChoose.Checked ||
                    rbManual.Checked;
        }
    }
}