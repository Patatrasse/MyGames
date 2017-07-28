using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;

namespace MyDatabase
{
    public partial class DisplayInit : Form
    {   //connexion DB
        MySqlConnection MyConnection;
        MySqlConnection MyConnection2;
        MySqlConnection MyConnection3;
        MySqlConnection MyConnection4;
        MySqlConnection MyConnectionHardware;
        public string strServer;
        public string strUser;
        public string strPassword;
        public string strDatabase;
        public string strSoftwareTable;
        public string strHardwareTable;
        //infos sur le jeu
        public string strID;
        public string strTitle;
        public string strPlatform;
        public string strDeveloper;
        public string strPublisher;
        public string strReleaseYear;
        public string strCover;
        public string strCoverType;
        public string strPlatformFilter = "IS NOT NULL";
        public string strPlatformSort = "title";
        public string strbuttonSearch = "";
        public string strGameToAdd = "";
        public string iDLC;
        public bool bDLC;
        //interface
        bool bInitDone = false;
        public PictureBox[] GameCover;
        public PictureBox GameCoverZoom;
        public PictureBox bigCover;
        public PictureBox Hover;
        public ComboBox PlatformList;
        public ComboBox SortList;
        public Button buttonSearch;
        public Button buttonAddNewGame;
        public TextBox NewGameTitle;
        public Label bigCoverTitle;
        public Label bigCoverDev;
        public Label bigCoverPublisher;
        public Label bigCoverYear;
        public Label bigCoverGenre;
        public RadioButton buttonAlphaSort;
        public RadioButton buttonNumSort;
       //var
        int iTableSize;
        int iNumPlatform;
        int iNbDisplayedGames;
        float fWidthLibrary;
        float fScaleFactor = 1.05f;
        float fFormWidth;
        int iInfoWidth = 400;
        int iCoverOriginalX;
        int iCoverOriginalY;
        int iCoverOriginalWidth;
        int iCoverOriginalHeight;
        int iMarginX = 12;
        int iMarginY = 12;
        int iSelectedCoverIndex = 0;
        int[,] iBoxSizeArray;
        //taille des covers
        static int iBoxArtDvdY = 183;
        static int iShelveHeight = 195;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public DisplayInit()
        {
            this.WindowState = FormWindowState.Maximized;
            this.Icon = new Icon("D:/Documents/GitHub/GameLibrary/MyGames/appicon.ico");
            this.Text = "My Games";
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.MinimumSize = new Size(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width / 2);
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Resize += new System.EventHandler(this.Refresh);  
            fFormWidth = this.Width;
            
            //config file pour les accès à la DB
            XDocument doc = XDocument.Load("config.xml");
            strServer = doc.Root.Element("server").Value;
            strUser = doc.Root.Element("user").Value;
            strPassword = doc.Root.Element("password").Value;
            strDatabase = doc.Root.Element("database").Value;
            strSoftwareTable = doc.Root.Element("table").Value;
            strHardwareTable = doc.Root.Element("table1").Value;

            //récupération des plateformes de la DB TODO
            MyConnection = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection.Open();
            MySqlCommand cmd2 = MyConnection.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(*) FROM (SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform) AS numplatform"; //num platforms
            iNumPlatform = Convert.ToInt32(cmd2.ExecuteScalar());
            cmd2.CommandText = "SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform"; //platform list
            MySqlDataReader platforms = cmd2.ExecuteReader();

            //Header
            PlatformList = new ComboBox();
            PlatformList.FlatStyle = FlatStyle.System;
            PlatformList.DropDownStyle = ComboBoxStyle.DropDownList;
            PlatformList.Location = new Point(0, 0);
            PlatformList.Font = new Font("Arial Bold", 14);

            //buttonAlphaSort = new RadioButton();
            //buttonAlphaSort.Location = new Point(PlatformList.Width, 0);
            //buttonAlphaSort.Appearance = Appearance.Button;
            //buttonAlphaSort.FlatStyle = FlatStyle.Flat;
            //buttonAlphaSort.FlatAppearance.BorderSize = 0;
            //buttonAlphaSort.Height = 32;
            //buttonAlphaSort.Width = 32;
            //buttonAlphaSort.BackgroundImage = Image.FromFile("D:/Documents/GitHub/GameLibrary/MyGames/36672.png");
            //buttonAlphaSort.BackgroundImageLayout = ImageLayout.Zoom;

            //buttonNumSort = new RadioButton();
            //buttonNumSort.Location = new Point(PlatformList.Width + buttonAlphaSort.Width, 0);
            //buttonNumSort.Appearance = Appearance.Button;
            //buttonNumSort.FlatStyle = FlatStyle.Flat;
            //buttonNumSort.FlatAppearance.BorderSize = 0;
            //buttonNumSort.Height = 32;
            //buttonNumSort.Width = 32;
            //buttonNumSort.BackgroundImage = Image.FromFile("D:/Documents/GitHub/GameLibrary/MyGames/37170.png");
            //buttonNumSort.BackgroundImageLayout = ImageLayout.Zoom;

            //buttonSearch = new Button();
            //buttonSearch.Click += new EventHandler(buttonSearch_Click);
            //buttonSearch.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/MyGames/39308.png");
            //buttonSearch.BackColor = Color.Transparent;
            //buttonSearch.ForeColor = Color.Transparent;
            //buttonSearch.FlatStyle = FlatStyle.Flat;
            //buttonSearch.FlatAppearance.BorderSize = 0;
            //buttonSearch.Height = 32;
            //buttonSearch.Width = 32;
            //buttonSearch.Location = new Point(buttonNumSort.Location.X + buttonNumSort.Width, 0);

            //buttonAddNewGame = new Button();
            //buttonAddNewGame.Click += new EventHandler(buttonAddNewGame_Click);
            //buttonAddNewGame.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/MyGames/36780.png");
            //buttonAddNewGame.BackColor = Color.Transparent;
            //buttonAddNewGame.ForeColor = Color.Transparent;
            //buttonAddNewGame.FlatStyle = FlatStyle.Flat;
            //buttonAddNewGame.FlatAppearance.BorderSize = 0;
            //buttonAddNewGame.Height = 32;
            //buttonAddNewGame.Width = 32;
            //buttonAddNewGame.Location = new Point(buttonSearch.Location.X + buttonSearch.Width, 0);
            
            //boucle nombre de platform + 1 pour tout afficher
            int k = 0;
            while (k < iNumPlatform + 1)
            {
                if (k == 0)
                {
                    PlatformList.Items.Insert(k, "All Platforms");
                }
                else
                {
                    platforms.Read();
                    string strPlatformName = GetPlatformsList(platforms, 0);
                    PlatformList.Items.Insert(k, strPlatformName);
                }
                k = k + 1;
            }
            
            platforms.Close();
            MyConnection.Close();
            PlatformList.SelectedIndex = 0;
            PlatformList.SelectedIndexChanged += new System.EventHandler(PlatformList_SelectedIndexChanges);
            PlatformList.DrawMode = DrawMode.OwnerDrawFixed;
            PlatformList.DrawItem += new DrawItemEventHandler(PlatformList_DrawItem);
            PlatformList.DropDownClosed += new EventHandler(PlatformList_DropDownClosed);
            this.Controls.Add(PlatformList);

            MyConnectionHardware = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnectionHardware.Open();
            MySqlCommand cmdhardware = MyConnectionHardware.CreateCommand();
            cmdhardware.CommandText = "SELECT COUNT(*) FROM (SELECT hardware_id, COUNT(*) FROM " + strHardwareTable + " GROUP BY hardware_id) AS numplatform";
            int iTabSize = Convert.ToInt32(cmdhardware.ExecuteScalar());
            iBoxSizeArray = new int[iTabSize, 2];
            MySqlDataReader fullbasehardware = cmdhardware.ExecuteReader();

            fullbasehardware.Read();
            fullbasehardware.Close();

            cmdhardware.CommandText = "SELECT * FROM " + strHardwareTable + " ORDER BY 'hardware_id'";
            MySqlDataReader fullbasehardware2 = cmdhardware.ExecuteReader();
            
            k = 0;
            int iCoverWidth;
            int iCoverHeight;
            while (fullbasehardware2.Read())
            { 
                
             iCoverWidth = GetBoxeSizeX(fullbasehardware2);
             iCoverHeight =   GetBoxeSizeY(fullbasehardware2);
                
                iBoxSizeArray[k,0] = iCoverWidth;
                iBoxSizeArray[k,1] = iCoverHeight;
            k= k+1;
            }
            
            fullbasehardware2.Close();
            MyConnectionHardware.Close();

            DisplayLibrary(strPlatformFilter, bInitDone);



            ////récupération des plateformes de la DB TODO
            //MyConnection = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            //MyConnection.Open();
            //MySqlCommand cmd2 = MyConnection.CreateCommand();
            //cmd2.CommandText = "SELECT COUNT(*) FROM (SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform) AS numplatform"; //num platforms
            //iNumPlatform = Convert.ToInt32(cmd2.ExecuteScalar());
            //cmd2.CommandText = "SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform"; //platform list
            //MySqlDataReader platforms = cmd2.ExecuteReader();
        }

        public void PlatformList_DropDownClosed(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        public void DisplayLibrary(string strPlatformFilter, bool bInitDone)
        {
            PlatformList.Width = (int)fFormWidth;
            //Accès base de jeux
            MyConnection2 = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection2.Open();
            MySqlCommand cmd = MyConnection2.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM " + strSoftwareTable + " WHERE platform " + strPlatformFilter;
            iTableSize = Convert.ToInt32(cmd.ExecuteScalar());
            if (strbuttonSearch == "")
            {
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE platform " + strPlatformFilter + " ORDER BY " + strPlatformSort;
            }
            else 
            {
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE " + 
                "title LIKE " + strbuttonSearch + " OR " + 
                "serie LIKE " + strbuttonSearch + " OR " + 
                "developer LIKE " + strbuttonSearch + " OR " + 
                "publisher LIKE  " + strbuttonSearch + " OR " + 
                "genre LIKE " + strbuttonSearch + " OR " + 
                "subgenre LIKE " + strbuttonSearch + " OR " +
                "serie LIKE " + strbuttonSearch + " ORDER BY " + strPlatformSort;
            }
            MySqlDataReader fullbase = cmd.ExecuteReader();

            int i = 0;
            int j = 0;
            int iGameWidth;
            int iTotalGameWidth = 0;
            int iShelveTop;

            iNbDisplayedGames = 0;
            
            //Panels settings          
            GameList = new Panel();
            GameList.Location = new Point(0, PlatformList.Height);
            GameList.Height = this.ClientSize.Height - PlatformList.Height;
            GameList.VerticalScroll.Maximum = 0;
            GameList.AutoScroll = false;
            GameList.VerticalScroll.Visible = false;
            GameList.AutoScroll = true;
            //GameList.BackColor = Color.Red;
            Controls.Add(GameList);

            GameInfo = new Panel();
            GameInfo.MinimumSize = new Size(iInfoWidth, Screen.PrimaryScreen.Bounds.Height / 2);
            
            GameInfo.Width = iInfoWidth;
            fWidthLibrary = this.Width - GameInfo.Width;
            GameList.Width = (int)fWidthLibrary;
            GameInfo.Location = new Point(GameList.Width, PlatformList.Height);
            GameInfo.Height = GameList.Height;
            GameInfo.AutoScroll = false;
            GameInfo.BackColor = Color.Blue;
           
            this.Controls.Add(GameInfo);
         
            // Placement des jeux dans un panel           
            GameCover = new PictureBox[iTableSize];
            iShelveTop = 0;
            int iLibraryRightMargin = (int)fWidthLibrary;
            int iLibraryRightMarginTemp = 0;
            while (fullbase.Read())
            {
                strCover = GetCover(fullbase);
                strCoverType = GetPlatform(fullbase);
                iDLC = GetDLC(fullbase);
                bDLC = false;
                if (iDLC != "")
                {
                    bDLC =true;
                }

                GameCover[i] = new PictureBox();
                GameCover[i].SizeMode = PictureBoxSizeMode.Zoom;

                //récupération des tailles des boîtes
                GameCover[i].Width = iBoxSizeArray[GetPlatformIndex(fullbase)-1,0];
                GameCover[i].Height = iBoxSizeArray[GetPlatformIndex(fullbase)-1,1];
                
                

                //conversion des images manquantes
                if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg"))
                {
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");

                }
                else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg"))
                {
                    var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg");
                    CreateImage(new_mini, GameCover[i].Width, GameCover[i].Height, strCover);
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");
                }
                else
                {
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                }

                iGameWidth = GameCover[i].Size.Width;
 
                if (bDLC == false)
                {
                    // change shelve
                    if (iGameWidth + iMarginX*2 > fWidthLibrary - iTotalGameWidth)
                    {
                        iLibraryRightMarginTemp = (int)fWidthLibrary - iTotalGameWidth;
                        if (iLibraryRightMarginTemp < iLibraryRightMargin && iLibraryRightMarginTemp > iMarginX)
                        {
                            iLibraryRightMargin = iLibraryRightMarginTemp;
                        }

                        iTotalGameWidth = 0;
                        j++;
                        iShelveTop = iShelveHeight + iShelveTop;               
                    }
                    if (iTotalGameWidth == 0)
                    {
                       GameCover[i].Left = iMarginX;
                    }
                    else
                    {
                        GameCover[i].Left = GameCover[i - 1].Location.X + GameCover[i - 1].Width + iMarginX;
                    }

                    iNbDisplayedGames = iNbDisplayedGames + 1;
                    iTotalGameWidth = iTotalGameWidth + iGameWidth + iMarginX;

                    GameCover[i].Top = iShelveTop + (iShelveHeight - GameCover[i].Height);
                    GameCover[i].Name = GetCover(fullbase);
                    GameCover[i].Show();

                    GameCover[i].MouseEnter += new System.EventHandler(this.GameCover_Enter);
                    GameCover[i].MouseLeave += new System.EventHandler(this.GameCover_Leave);
                    GameList.Controls.Add(GameCover[i]);

                    i = i + 1;
                }
            }
            
            if (j != 0)
            {
            GameList.Location = new Point(iLibraryRightMargin / 2 - iMarginX, PlatformList.Height);
            GameList.Width = GameList.Width - iLibraryRightMargin / 2 + iMarginX;
            }
            fullbase.Close();
            MyConnection2.Close();
        }

        public void RemoveGames()
        {
            GameList.Dispose();
            GameList = null;
            GameInfo.Dispose();
            GameInfo = null;
            fFormWidth = this.Width;
            DisplayLibrary(strPlatformFilter, bInitDone); 
        }
 
        private void PlatformList_SelectedIndexChanges(object sender, System.EventArgs e)
        {
            var value = (ComboBox)sender;
            if (PlatformList.Text == "All Platforms")
            {
                strPlatformFilter = "IS NOT NULL";
            }
            else
            {
                strPlatformFilter = " = '" + PlatformList.Text + "'";
            }

            if (PlatformList.Text == "All Platforms")
            {
                iShelveHeight = iBoxArtDvdY + iMarginY;
            }
            else
            {
                iShelveHeight = iBoxSizeArray[PlatformList.SelectedIndex - 1, 1] + iMarginY;
            }
            
            PlatformList.Text = strPlatformFilter;
            strbuttonSearch = "";
            RemoveGames();
        }

        private void SortList_SelectedIndexChanges(object sender, System.EventArgs e)
        {
            var value = (ComboBox)sender;
            if (SortList.Text == "A->B")
            {
                strPlatformSort = "title";
            }
            else
            {
                strPlatformSort = "release_year";
            }
            SortList.Text = strPlatformSort;
            RemoveGames();
        }

        private void buttonAddNewGame_Click(object sender, System.EventArgs e)
        {
            NewGameTitle = new TextBox();
            NewGameTitle.KeyDown += new KeyEventHandler(buttonAddNewGame_Validation);
            NewGameTitle.Location = new Point(PlatformList.Width + SortList.Width + buttonSearch.Width + buttonAddNewGame.Width, 0);
            NewGameTitle.Text = "Title";
            //.Controls.Add(NewGameTitle);

        }

        private void buttonAddNewGame_Validation(object sender, KeyEventArgs e)
        {
            var text = (TextBox)sender;
            if (e.KeyCode == Keys.Enter)
            {
                strGameToAdd = text.Text;
                MyConnection3 = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
                MyConnection3.Open();
                MySqlCommand cmd = MyConnection3.CreateCommand();
                cmd.CommandText = "INSERT INTO games.test (title) VALUES ('"+strGameToAdd+"');";
                cmd.ExecuteNonQuery();
                MyConnection3.Close();
                NewGameTitle.Dispose();
                NewGameTitle = null;
            }
        }

        private void buttonSearch_Click(object sender, System.EventArgs e)
        {
            strbuttonSearch = "'%"+"Plate-forme"+"%'";
            strPlatformFilter = "IS NOT NULL";
            RemoveGames();
        }

        private void GameCover_Enter(object sender, System.EventArgs e)
        {    
            var cover = (PictureBox)sender;
            strCover = cover.Name;

            iSelectedCoverIndex = Int32.Parse(strCover);
            Console.WriteLine(cover.TabIndex);
            iCoverOriginalX = cover.Location.X;
            iCoverOriginalY = cover.Location.Y;
            iCoverOriginalWidth = cover.Size.Width;
            iCoverOriginalHeight = cover.Size.Height;
            
            int CoverNewWidth = (int)(cover.Width * fScaleFactor);
            int CoverNewHeight = (int)(cover.Height * fScaleFactor);

            int CoverNewLocX = (int)(iCoverOriginalX - ((CoverNewWidth - iCoverOriginalWidth) / 2));
            int CoverNewLocY = (int)(iCoverOriginalY - ((CoverNewHeight - iCoverOriginalHeight) / 2));
            
            GameCoverZoom = new PictureBox();
            GameCoverZoom.SizeMode = PictureBoxSizeMode.Zoom;
            GameCoverZoom.MouseLeave += new System.EventHandler(this.GameCoverZoom_Leave);
            GameCoverZoom.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GameCoverZoom_Click);
            try
            {
                GameCoverZoom.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + strCover + ".jpg");
            }
            catch
            {
                GameCoverZoom.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + "0" + ".jpg");
            }
            GameCoverZoom.Height = CoverNewHeight;
            GameCoverZoom.Width = CoverNewWidth;
            ///GameCoverZoom.BackColor = Color.Black;
            GameCoverZoom.Location = new Point(CoverNewLocX, CoverNewLocY);
            GameList.Controls.Add(GameCoverZoom);
            GameCoverZoom.BringToFront();
        }

        private void GameCover_Leave(object sender, System.EventArgs e)
        {
            var cover = (PictureBox)sender;
            cover.Height = iCoverOriginalHeight;
            cover.Width = iCoverOriginalWidth;
            cover.Location = new Point(iCoverOriginalX, iCoverOriginalY);     
        }

        private void GameCoverZoom_Leave(object sender, System.EventArgs e)
        {
            //antibug
            if (GameCoverZoom == null)
            {
                return;
            }
            else
            {
                GameCoverZoom.Dispose();
                // GameCoverZoom = null;
            }
        }

        bool bAlreadyClick = false;
        public void GameCoverZoom_Click(object sender, System.EventArgs e)
        {
            
            float fBigCoverBottom;
            float fBigCoverTop;
            if (bAlreadyClick == true)
            {
                Remove_bigCover(GameInfo);
            }

            var zoomCover = (PictureBox)sender;
            bigCover = new PictureBox();
            bigCoverTitle = new Label();
            bigCoverDev = new Label();
            bigCoverPublisher = new Label();
            bigCoverYear = new Label();
            bigCoverGenre = new Label();
            
            MyConnection4 = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection4.Open();
            MySqlCommand cmd = MyConnection4.CreateCommand();
            cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE game_id = " + strCover ;
            MySqlDataReader gameinfo = cmd.ExecuteReader();
            gameinfo.Read();

            bigCover.Width = GameInfo.Width;
            bigCover.Height = zoomCover.Height * 2;
            fBigCoverBottom = GameInfo.Height / 2;
            fBigCoverTop = iBoxArtDvdY*2.20f - bigCover.Height;
            bigCover.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + strCover + ".jpg");
            bigCover.Location = new Point(0, (int)fBigCoverTop);
            bigCover.SizeMode = PictureBoxSizeMode.Zoom;
            bigCover.BackColor = Color.AntiqueWhite;

            bigCoverTitle.Text = "Titre : " + GetTitle(gameinfo);
            bigCoverTitle.Location = new Point(0, bigCover.Bottom + 10);
            bigCoverTitle.Width = GameInfo.Width;
            bigCoverTitle.BackColor = Color.Aqua;

            bigCoverDev.Text = "Développeur : " + GetDeveloper(gameinfo);
            bigCoverDev.Location = new Point(0, bigCoverTitle.Bottom + 10);
            bigCoverDev.Width = GameInfo.Width;
            bigCoverDev.BackColor = Color.Aquamarine;

            bigCoverPublisher.Text = "Éditeur : " + GetPublisher(gameinfo);
            bigCoverPublisher.Location = new Point(0, bigCoverDev.Bottom + 10);
            bigCoverPublisher.Width = GameInfo.Width;
            bigCoverPublisher.BackColor = Color.Azure;

            bigCoverYear.Text = "Sortie : " + GetReleaseYear(gameinfo);
            bigCoverYear.Location = new Point(0, bigCoverPublisher.Bottom + 10);
            bigCoverYear.Width = GameInfo.Width;
            bigCoverYear.BackColor = Color.Beige;

            if (GetGenre2(gameinfo) == "")
                {
                    bigCoverGenre.Text = "Genre : " + GetGenre(gameinfo);
                }
            else
                {
                    bigCoverGenre.Text = "Genre : " + GetGenre(gameinfo) + "/" + GetGenre2(gameinfo);
                }
            bigCoverGenre.Location = new Point(0, bigCoverYear.Bottom + 10);
            bigCoverGenre.Width = GameInfo.Width;
            bigCoverGenre.BackColor = Color.Bisque;

            GameInfo.Controls.Add(bigCover);
            GameInfo.Controls.Add(bigCoverTitle);
            GameInfo.Controls.Add(bigCoverDev);
            GameInfo.Controls.Add(bigCoverPublisher);
            GameInfo.Controls.Add(bigCoverYear);
            GameInfo.Controls.Add(bigCoverGenre);
   

            bAlreadyClick = true;
            MyConnection4.Close();
        }
        
        public void Remove_bigCover(Panel GameInfo)
        {
            bigCover.Dispose();
            bigCover = null;
            bigCoverTitle.Dispose();
            bigCoverTitle = null;
            bigCoverDev.Dispose();
            bigCoverDev = null;
            bigCoverPublisher.Dispose();
            bigCoverPublisher = null;
            bigCoverYear.Dispose();
            bigCoverYear = null;
            bigCoverGenre.Dispose();
            bigCoverGenre = null;
        }

        //Resize is always called when ResizeEnd is called, so add a flag to detect end of resize
        bool bResizeInProgress = false;
        private void Refresh(object sender, System.EventArgs e)
        {
            if (bResizeInProgress || bInitDone == false)
            {
                bInitDone = true;
                return;
            }
            else
            {
                RemoveGames();
            }
        }

        public string GetLocalized(MySqlDataReader fullbase, int iColumn)
        {
            if (fullbase.IsDBNull(iColumn))
            {
                // antibug null string
                return "";
            }
            else
            {
                return fullbase.GetString(iColumn);
            }
        }

        public string GetPlatformsList(MySqlDataReader platforms, int iColumn)
        {
            return platforms.GetString(iColumn);
        }

        public string GetID(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 0);
        }

        public string GetTitle(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 1);
        }

        public string GetPlatform(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 2);
        }

        public int GetPlatformIndex(MySqlDataReader fullbase)
        {
            return Int32.Parse(GetLocalized(fullbase, 3));
        }

        public string GetDeveloper(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 4);
        }

        public string GetPublisher(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 5);
        }

        public string GetReleaseYear(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 6);
        }

        public string GetGenre(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 7);
        }

        public string GetGenre2(MySqlDataReader gameinfo)
        {
            return GetLocalized(gameinfo, 8);
        }

        public string GetCover(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 15);
        }
        
        public string GetDLC(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 14);
        }

        public int GetBoxeSizeX(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 4));
        }
        
        public int GetBoxeSizeY(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 5));
        }

        // Allow Combo Box to center aligned
        private void PlatformList_DrawItem(object sender, DrawItemEventArgs e)
        {
            // By using Sender, one method could handle multiple ComboBoxes
            ComboBox cbx = sender as ComboBox;
            if (cbx != null)
            {
                // Always draw the background
                e.DrawBackground();

                // Drawing one of the items?
                if (e.Index >= 0)
                {
                    // Set the string alignment.  Choices are Center, Near and Far
                    StringFormat sf = new StringFormat();
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;

                    // Set the Brush to ComboBox ForeColor to maintain any ComboBox color settings
                    // Assumes Brush is solid
                    Brush brush = new SolidBrush(cbx.ForeColor);

                    // If drawing highlighted selection, change brush
                    if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        brush = SystemBrushes.HighlightText;

                    // Draw the string                   
                    e.Graphics.DrawString(cbx.Items[e.Index].ToString(), cbx.Font, brush, e.Bounds, sf);
                }
            }
        }
        
        //create missing cover
        public static Bitmap CreateImage(Image image, int width, int height, string name)
        {
            var miniCoverArea = new Rectangle(0, 0, width, height);
            var miniCover = new Bitmap(width, height);

            miniCover.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            
            using (var graphics = Graphics.FromImage(miniCover))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, miniCoverArea, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            miniCover.Save(@"D:\Documents\GitHub\GameLibrary\covers_mini\" + name + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            var zoomCoverArea = new Rectangle(0, 0, width * 4, height * 4);
            var zoomCover = new Bitmap(width * 4, height * 4);

            zoomCover.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            
            using (var graphics = Graphics.FromImage(zoomCover))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, zoomCoverArea, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            zoomCover.Save(@"D:\Documents\GitHub\GameLibrary\covers_zoom\" + name + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            return miniCover;
        }
    }
}
