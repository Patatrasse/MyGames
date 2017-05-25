using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;

namespace MyDatabase
{
    public partial class DisplayGames : Form
    {
        public string strServer;
        public string strUser;
        public string strPassword;
        public string strDatabase;
        public string strSoftwareTable;

        public string strID;
        public string strTitle;
        public string strPlatform;
        public string strDeveloper;
        public string strPublisher;
        public string strReleaseYear;
        public string strCover;

        public string[] strGameInfo;
        public TextBox[] GameInfoTextBox;
        public PictureBox[] GameCover;

        int iFormWidth;
        int iTableSize;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        MySqlConnection MyConnection;

        public DisplayGames()
        {
            //InitializeComponent();

            //AttachConsole(ATTACH_PARENT_PROCESS);

            // read config file
            XDocument doc = XDocument.Load("config.xml");
            strServer = doc.Root.Element("server").Value;
            strUser = doc.Root.Element("user").Value;
            strPassword = doc.Root.Element("password").Value;
            strDatabase = doc.Root.Element("database").Value;
            strSoftwareTable = doc.Root.Element("table").Value;

            // Access DB
            MyConnection = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection.Open();
            MySqlCommand cmd = MyConnection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM " + strSoftwareTable + " WHERE platform = 'Wii U'";
            iTableSize = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE platform = 'Wii U'";
            MySqlDataReader reader = cmd.ExecuteReader();

            // Display
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.WindowState = FormWindowState.Maximized;
            //this.ResizeEnd += new EventHandler(DisplayGames_Resize);

            iFormWidth = this.Width;

            GameCover = new PictureBox[iTableSize];
            int i = 0;
            int iGameWidth;
            int iTotalGameWidth = 0;
            int iGameHighest = 0;

            while (reader.Read())
            {
                strCover = GetCover(reader);
                GameCover[i] = new PictureBox();

                try
                {
                    GameCover[i].Image = Image.FromFile("./covers/" + strCover + ".png");
                    GameCover[i].SizeMode = PictureBoxSizeMode.AutoSize;
                    iGameWidth = GameCover[i].Size.Width;

                    if (i == 0)
                    {
                        GameCover[i].Location = new Point(5, 15);
                    }
                    else
                    {
                        if (iFormWidth - iTotalGameWidth < iGameWidth) // Changement de ligne
                        {
                            GameCover[i].Location = new Point(5, GameCover[i - 1].Location.Y + iGameHighest);
                            iTotalGameWidth = 0;
                            iGameHighest = 0;
                        }
                        else if (GameCover[i].Height < iGameHighest && GameCover[i - 1].Height == iGameHighest)
                        {
                            GameCover[i].Location = new Point(GameCover[i - 1].Location.X + GameCover[i - 1].Width, GameCover[i - 1].Location.Y + iGameHighest - GameCover[i].Height);
                        }
                        else if (GameCover[i].Height >= iGameHighest && GameCover[i - 1].Height == iGameHighest)
                        {
                            GameCover[i].Location = new Point(GameCover[i - 1].Location.X + GameCover[i - 1].Width, GameCover[i - 1].Location.Y + iGameHighest - GameCover[i].Height);
                        }
                        else if (GameCover[i].Height < iGameHighest)
                        {
                            GameCover[i].Location = new Point(GameCover[i - 1].Location.X + GameCover[i - 1].Width, GameCover[i - 1].Location.Y);
                        }
                        else
                        {
                            GameCover[i].Location = new Point(GameCover[i - 1].Location.X + GameCover[i - 1].Width, iGameHighest + GameCover[i - 1].Height);
                        }
                    }

                    this.Controls.Add(GameCover[i]);
                    GameCover[i].Show();

                    iTotalGameWidth = iTotalGameWidth + iGameWidth;
                    if (iGameHighest < GameCover[i].Height)
                    {
                        iGameHighest = GameCover[i].Height;
                    }
                    i = i + 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("<---Jaquette " + strCover + " non trouvée --->\r" + ex.Message);
                }
            }
            reader.Close();


            MyConnection.Close();

            this.AutoScroll = false;
            this.HorizontalScroll.Enabled = false;
            this.HorizontalScroll.Visible = false;
            this.HorizontalScroll.Maximum = 0;
            this.AutoScroll = true;
            this.Show();
        }

        public string GetLocalized(MySqlDataReader reader, int iLanguage)
        {
            if (reader.IsDBNull(iLanguage))
            {
                // antibug null string
                return "";
            }
            else
            {
                return reader.GetString(iLanguage);
            }
        }

        public string GetID(MySqlDataReader reader)
        {
            return GetLocalized(reader, 0);
        }

        public string GetTitle(MySqlDataReader reader)
        {
            return GetLocalized(reader, 1);
        }

        public string GetPlatform(MySqlDataReader reader)
        {
            return GetLocalized(reader, 2);
        }

        public string GetDeveloper(MySqlDataReader reader)
        {
            return GetLocalized(reader, 3);
        }

        public string GetPublisher(MySqlDataReader reader)
        {
            return GetLocalized(reader, 4);
        }

        public string GetReleaseYear(MySqlDataReader reader)
        {
            return GetLocalized(reader, 5);
        }

        public string GetGenre(MySqlDataReader reader)
        {
            return GetLocalized(reader, 6);
        }

        public string GetCover(MySqlDataReader reader)
        {
            return GetLocalized(reader, 14);
        }

        //private void DisplayGames_Resize(object sender, EventArgs e)
        //{
        //    InitializeComponent();

        //}


    }
}
