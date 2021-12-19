using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ForestSetup
{
    class Form1 : Form
    {
        public const string CFILE = "config.ini";
        public const string TITLE = "Forest Shootout Setup";

        Control[] ctrls;
        Button bt_save;
        Button bt_about;

        FileConfigManager.FCM cfg;
        string[] data, val;

        byte[] type =
        {
            0, //"Name",
            1, //"MouseSens",
            1, //"ScopeSens",
            2, //"InvertMouse",
            3, //"FOV",
            2, //"AutoReload",
            2, //"AutoRespawn",
            1, //"Volume",
            3, //"MaxFPS",
            3, //"ClientRate",
            3, //"ServerRate",
            3, //"MaxPlayers",
            2, //"LimPing",
            3, //"MaxPing",
            2, //"OvrdGfx",
            4, //"Aniso",
            4, //"AALevel",
            4, //"BoneQual",
            4, //"Vsync",
            1, //"ShadowDist",
            2, //"OvrdTer",
            1, //"TerrainQual",
            3, //"TreeMesh",
            1, //"TreeFade",
            1, //"TreeBill",
            2, //"Grass",
            4, //"GrassDens",
            1, //"GrassDis",
            2 //"Wind"
        };
        string[] conf =
        {
            "Name",
            "MouseSens",
            "ScopeSens",
            "InvertMouse",
            "FOV",
            "AutoReload",
            "AutoRespawn",
            "Volume",
            "MaxFPS",
            "ClientRate",
            "ServerRate",
            "MaxPlayers",
            "LimPing",
            "MaxPing",
            "OvrdGfx",
            "Aniso",
            "AALevel",
            "BoneQual",
            "Vsync",
            "ShadowDist",
            "OvrdTer",
            "TerrainQual",
            "TreeMesh",
            "TreeFade",
            "TreeBill",
            "Grass",
            "GrassDens",
            "GrassDis",
            "Wind"
        };

        public Form1()
        {
            Application.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Text = TITLE;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ClientSize = new Size(480, 400);
            StartPosition = FormStartPosition.CenterScreen;
            //Config info
            string[] opt =
            {
                "Name",
                "Sensitivity",
                "Scope Sens.",
                "Invert Mouse",
                "FOV",
                "Auto Reload",
                "Auto Respawn",
                "Volume",
                "Max FPS",
                "Client Rate",
                "Server Rate",
                "Max Players",
                "Limit Ping",
                "Max Ping",
                "Graphics override",
                "Anisotropic filtering",
                "Antialiasing",
                "Animation Quality",
                "Vertical Sync",
                "Shadow Distance",
                "Terrain Override",
                "Terrain Quality",
                "Realtime Trees",
                "Tree Fade Length",
                "Tree Distance",
                "Grass",
                "Grass Density",
                "Grass Distance",
                "Wind"
            };
            double[,] min_max =
            {
                {0.1,100.0}, //"MouseSens",
                {0.1,100.0}, //"ScopeSens",
                {60,150}, //"FOV",
                {0,100}, //"Volume",
                {0,1000}, //"MaxFPS",
                {10,30}, //"ClientRate",
                {10,30}, //"ServerRate",
                {2,16}, //"MaxPlayers",
                {50,10000}, //"MaxPing",
                {40,500}, //"ShadowDist",
                {0,200}, //"TerrainQual",
                {20,500}, //"TreeMesh",
                {0,500}, //"TreeFade",
                {20,500}, //"TreeBill",
                {10,500} //"GrassDis",
            };
            string[,] cb_opt =
            {
                {"Off","Per Texture","On",null},
                {"Off","2x","4x","8x"},
                {"Low","Medium","High",null},
                {"Off","On","Half",null},
                {"Low","Medium","High","Very High"}
            };
            //Config load
            cfg = new FileConfigManager.FCM();
            cfg.ReadAllData(CFILE, out data, out val);
            //Control generation
            ctrls = new Control[opt.Length];
            Label tmp;
            CheckBoxEx ctmp = null;
            int i2, i3;
            int itmp;
            double dtmp;
            bool btmp;
            int nm_cnt = 0;
            int cb_cnt = 0;
            int ch_cnt = 0;
            for (int i = 0; i < opt.Length; i++)
            {
                tmp = new Label();
                tmp.Location = new Point(12 + 240 * (i / 15), 12 + 24 * (i % 15));
                tmp.Size = new Size(112, 24);
                tmp.Text = opt[i];
                Controls.Add(tmp);
                switch (type[i])
                {
                    case 0:
                        ctrls[i] = new TextBox();
                        ((TextBox)ctrls[i]).MaxLength = 16;
                        break;
                    case 1:
                        ctrls[i] = new NumericUpDown();
                        ((NumericUpDown)ctrls[i]).DecimalPlaces = 2;
                        ((NumericUpDown)ctrls[i]).Minimum = (decimal)min_max[nm_cnt, 0];
                        ((NumericUpDown)ctrls[i]).Maximum = (decimal)min_max[nm_cnt, 1];
                        nm_cnt++;
                        break;
                    case 2:
                        ch_cnt++;
                        if (ch_cnt > 7)
                        {
                            ctmp = null;
                            ctrls[i] = new CheckBox();
                        }
                        else if (ch_cnt > 3)
                        {
                            ctmp = new CheckBoxEx();
                            ctmp.ctrls = new List<Control>();
                            ctmp.CheckedChanged += ctmp_CheckedChanged;
                            ctrls[i] = ctmp;
                        }
                        else
                            ctrls[i] = new CheckBox();
                        break;
                    case 3:
                        ctrls[i] = new NumericUpDown();
                        ((NumericUpDown)ctrls[i]).DecimalPlaces = 0;
                        ((NumericUpDown)ctrls[i]).Minimum = (decimal)min_max[nm_cnt, 0];
                        ((NumericUpDown)ctrls[i]).Maximum = (decimal)min_max[nm_cnt, 1];
                        nm_cnt++;
                        break;
                    case 4:
                        ctrls[i] = new ComboBox();
                        ((ComboBox)ctrls[i]).DropDownStyle = ComboBoxStyle.DropDownList;
                        for (i3 = 0; i3 < cb_opt.GetLength(1); i3++)
                        {
                            if (cb_opt[cb_cnt, i3] == null) break;
                            ((ComboBox)ctrls[i]).Items.Add(cb_opt[cb_cnt, i3]);
                        }
                        cb_cnt++;
                        break;
                }
                ctrls[i].Location = new Point(tmp.Right + 12, tmp.Location.Y);
                ctrls[i].Size = new Size(96, 24);
                if (ctmp != null && !(ctrls[i] is CheckBoxEx))
                {
                    ctrls[i].Enabled = ctmp.Checked;
                    ctmp.ctrls.Add(ctrls[i]);
                }
                Controls.Add(ctrls[i]);
                for (i2 = 0; i2 < data.Length; i2++)
                {
                    if (data[i2] == conf[i])
                    {
                        switch (type[i])
                        {
                            case 0:
                                ctrls[i].Text = val[i2];
                                break;
                            case 1:
                                if (double.TryParse(val[i2], out dtmp))
                                {
                                    if ((decimal)dtmp < ((NumericUpDown)ctrls[i]).Minimum) dtmp = (double)((NumericUpDown)ctrls[i]).Minimum;
                                    else if ((decimal)dtmp > ((NumericUpDown)ctrls[i]).Maximum) dtmp = (double)((NumericUpDown)ctrls[i]).Maximum;
                                    ((NumericUpDown)ctrls[i]).Value = (decimal)dtmp;
                                }
                                break;
                            case 2:
                                if (bool.TryParse(val[i2], out btmp))
                                    ((CheckBox)ctrls[i]).Checked = btmp;
                                break;
                            case 3:
                                if (double.TryParse(val[i2], out dtmp))
                                {
                                    if ((decimal)dtmp < ((NumericUpDown)ctrls[i]).Minimum) dtmp = (double)((NumericUpDown)ctrls[i]).Minimum;
                                    else if ((decimal)dtmp > ((NumericUpDown)ctrls[i]).Maximum) dtmp = (double)((NumericUpDown)ctrls[i]).Maximum;
                                    ((NumericUpDown)ctrls[i]).Value = (decimal)dtmp;
                                }
                                break;
                            case 4:
                                if (int.TryParse(val[i2], out itmp))
                                    ((ComboBox)ctrls[i]).SelectedIndex = itmp;
                                break;
                        }
                    }
                }
            }
            //Buttons
            bt_about = new Button();
            bt_about.Text = "About";
            bt_about.Location = new Point(Width - 224, ClientSize.Height - 28);
            bt_about.Size = new Size(96, 24);
            bt_about.Click += bt_about_Click;
            Controls.Add(bt_about);
            bt_save = new Button();
            bt_save.Text = "Save";
            bt_save.Location = new Point(Width - 112, ClientSize.Height - 28);
            bt_save.Size = new Size(96, 24);
            bt_save.Click += bt_save_Click;
            Controls.Add(bt_save);
        }

        void ctmp_CheckedChanged(object sender, EventArgs e)
        {
            bool tmp = ((CheckBox)sender).Checked;
            foreach (Control c in ((CheckBoxEx)sender).ctrls)
                c.Enabled = tmp;
        }

        void bt_about_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Written in C# 2008 Express Edition by Kurtis",
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        void bt_save_Click(object sender, EventArgs e)
        {
            int i2;
            for (int i = 0; i < ctrls.Length; i++)
            {
                for (i2 = 0; i2 < data.Length; i2++)
                {
                    if (data[i2] == conf[i])
                    {
                        switch (type[i])
                        {
                            case 0:
                            case 1:
                            case 3:
                                val[i] = ctrls[i].Text;
                                break;
                            case 2:
                                val[i] = ((CheckBox)ctrls[i]).Checked.ToString();
                                break;
                            case 4:
                                val[i] = ((ComboBox)ctrls[i]).SelectedIndex.ToString();
                                break;
                        }
                    }
                }
            }
            cfg.ChangeAllData(CFILE, data, val);
            MessageBox.Show("Successfully saved.", TITLE,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    class CheckBoxEx : CheckBox
    {
        public List<Control> ctrls;
    }

    class Program
    {
        [STAThread]
        static void Main()
        {
            if (!File.Exists("config.ini"))
            {
                MessageBox.Show(Form1.CFILE + " is missing!\nA new file has been generated for the game.",
                    Form1.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
                StreamWriter sw = new StreamWriter(Form1.CFILE, false, Encoding.Default);
                sw.Write(Properties.Resources.config);
                sw.Close();
            }
            Application.Run(new Form1());
        }
    }
}