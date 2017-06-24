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
    public partial class DisplayInit : Form
    {   //connexion DB
        MySqlConnection MyConnection;
        MySqlConnection MyConnection2;
        MySqlConnection MyConnection3;
        public string strServer;
        public string strUser;
        public string strPassword;
        public string strDatabase;
        public string strSoftwareTable;
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
        public string strSearch = "";
        public string strGameToAdd = "";
        //interface
        bool bInitDone = false;
        public PictureBox[] GameCover;
        public PictureBox GameCoverZoom;
        public Bitmap destImage;
        public PictureBox bigCover;
        public ComboBox PlatformList;
        public ComboBox SortList;
        public Button Search;
        public Button AddNewGame;
        public TextBox NewGameTitle;
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
        //taille des covers
        static int iBoxArtDvdX = 130;
        static int iBoxArtDvdY = 183;
        static int iBoxArtBrayX = 130;
        static int iBoxArtBrayY = 149;
        static int iBoxArtIosX = 100;
        static int iBoxArtIosY = 100;
        static int iBoxArtPsOneX = 125;
        static int iBoxArtPsOneY = 125;
        static int iBoxArtN64X = 180;
        static int iBoxArtN64Y = 120;
        static int iBoxArtSnesX = 180;
        static int iBoxArtSnesY = 120;
        static int iBoxArtNesX = 127;
        static int iBoxArtNesY = 138;
        static int iBoxArtGameBoyX = 127;
        static int iBoxArtGameBoyY = 138;
        static int iBoxArtNintendoDsX = 127;
        static int iBoxArtNintendoDsY = 101;
        static int iBoxArtDreamcastX = 120;
        static int iBoxArtDreamcastY = 120;
        static int iBoxArtMegadriveX = 140;
        static int iBoxArtMegadriveY = 190;
        static int iShelveHeight = iBoxArtDvdY + 20;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public DisplayInit()
        {         
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.MinimumSize = new Size(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width / 2);
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.WindowState = FormWindowState.Maximized;
            this.Resize += new System.EventHandler(this.Refresh);
            
            fFormWidth = this.Width;
            
            // read config file
            XDocument doc = XDocument.Load("config.xml");
            strServer = doc.Root.Element("server").Value;
            strUser = doc.Root.Element("user").Value;
            strPassword = doc.Root.Element("password").Value;
            strDatabase = doc.Root.Element("database").Value;
            strSoftwareTable = doc.Root.Element("table").Value;

            // récupération des plateformes de la DB
            MyConnection = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection.Open();
            MySqlCommand cmd2 = MyConnection.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(*) FROM (SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform) AS numplatform"; //num platforms
            iNumPlatform = Convert.ToInt32(cmd2.ExecuteScalar());
            cmd2.CommandText = "SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform"; //platform list
            MySqlDataReader platforms = cmd2.ExecuteReader();

            //Header contains interaction
            Header = new Panel();
            PlatformList = new ComboBox();
            PlatformList.SelectedIndexChanged += new System.EventHandler(PlatformList_SelectedIndexChanges);
            PlatformList.DropDownStyle = ComboBoxStyle.DropDownList;
            PlatformList.Location = new Point(0, 0);

            SortList = new ComboBox();
            SortList.SelectedIndexChanged += new System.EventHandler(SortList_SelectedIndexChanges);
            SortList.DropDownStyle = ComboBoxStyle.DropDownList;
            SortList.Location = new Point(PlatformList.Width, 0);

            Search = new Button();
            Search.Click += new EventHandler(Search_Click);
            Search.Text = "Search";
            Search.BackColor = Button.DefaultBackColor;
            Search.Location = new Point(PlatformList.Width + SortList.Width, 0);

            AddNewGame = new Button();
            AddNewGame.Click += new EventHandler(AddNewGame_Click);
            AddNewGame.Text = "Add";
            AddNewGame.BackColor = Button.DefaultBackColor;
            AddNewGame.Location = new Point(PlatformList.Width + SortList.Width + Search.Width, 0);

            Header.Location = new Point(0, 0);
            Header.Width = this.Width;
            Header.Height = PlatformList.Height;
            Header.BackColor = Color.Green;
            
            //boucle nombre de platform + 1 pour tout afficher
            int k = 0;
            while (k < iNumPlatform + 1)
            {
                if (k == 0)
                {
                    PlatformList.Items.Insert(k, "All");
                }
                else
                {
                    platforms.Read();
                    string strPlatformName = GetPlatformsList(platforms, 0);
                    PlatformList.Items.Insert(k, strPlatformName);
                }
                k = k + 1;
            }
            SortList.Items.Insert(0, "A->B");
            SortList.Items.Insert(1, "0->1");
            
            platforms.Close();
            MyConnection.Close();

            Header.Controls.Add(PlatformList);
            Header.Controls.Add(SortList);
            Header.Controls.Add(Search);
            Header.Controls.Add(AddNewGame);

            DisplayLibrary(strPlatformFilter, bInitDone);
            this.Controls.Add(Header);         
        }
   
        public void DisplayLibrary(string strPlatformFilter, bool bInitDone)
        {          
            // Accès base de jeux
            MyConnection2 = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            MyConnection2.Open();
            MySqlCommand cmd = MyConnection2.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM " + strSoftwareTable + " WHERE platform " + strPlatformFilter;
            iTableSize = Convert.ToInt32(cmd.ExecuteScalar());
            if (strSearch == "")
            {
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE platform " + strPlatformFilter + " ORDER BY " + strPlatformSort;
            }
            else 
            {
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE " + 
                "title LIKE " + strSearch + " OR " + 
                "serie LIKE " + strSearch + " OR " + 
                "developer LIKE " + strSearch + " OR " + 
                "publisher LIKE  " + strSearch + " OR " + 
                "genre LIKE " + strSearch + " OR " + 
                "subgenre LIKE " + strSearch + " OR " +
                "serie LIKE " + strSearch + " ORDER BY " + strPlatformSort;
            }
            MySqlDataReader fullbase = cmd.ExecuteReader();

            int i = 0;
            int j = 0;
            int iGameWidth;
            int iTotalGameWidth = 0;
            int iShelveTop;
            int iMarginX = 12;
            iNbDisplayedGames = 0;
            
            //Panels settings          
            GameList = new Panel();
            GameList.Location = new Point(0, PlatformList.Height);
            
            Header.Width = this.Width;
            GameList.Height = this.ClientSize.Height - PlatformList.Height;
            GameList.AutoScroll = false;
            GameList.HorizontalScroll.Enabled = false;
            GameList.HorizontalScroll.Visible = false;
            GameList.HorizontalScroll.Maximum = 0;
            GameList.AutoScroll = true;
            GameList.BackColor = Color.Red;
            Controls.Add(GameList);

            GameInfo = new Panel();
            GameInfo.MinimumSize = new Size(iInfoWidth, Screen.PrimaryScreen.Bounds.Height / 2);
            GameInfo.Location = new Point((int)fFormWidth - GameInfo.Width, PlatformList.Height);
            GameInfo.Width = iInfoWidth;
            fWidthLibrary = this.Width - GameInfo.Width;
            GameList.Width = (int)fWidthLibrary;
            GameInfo.Height = GameList.Height;
            GameInfo.AutoScroll = false;
            GameInfo.BackColor = Color.Blue;
           
            this.Controls.Add(GameInfo);
         
            // Placement des jeux dans un panel           
            GameCover = new PictureBox[iTableSize];
            iShelveTop = GameList.Location.Y;
            while (fullbase.Read())
            {
                strCover = GetCover(fullbase);
                strCoverType = GetPlatform(fullbase);
                GameCover[i] = new PictureBox();
                GameCover[i].SizeMode = PictureBoxSizeMode.Zoom;

                //récupération des tailles des boîtes
                switch (strCoverType)
                {
                    case "Dreamcast":
                        GameCover[i].Width = iBoxArtDreamcastX;
                        GameCover[i].Height = iBoxArtDreamcastY;
                        break;
                    case "Game Boy":
                        GameCover[i].Width = iBoxArtGameBoyX;
                        GameCover[i].Height = iBoxArtGameBoyY;
                        break;
                    case "Game Boy Advance":
                        GameCover[i].Width = iBoxArtGameBoyX;
                        GameCover[i].Height = iBoxArtGameBoyY;
                        break;
                    case "Game Boy Color":
                        GameCover[i].Width = iBoxArtGameBoyX;
                        GameCover[i].Height = iBoxArtGameBoyY;
                        break;
                    case "Gamecube":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "iOS":
                        GameCover[i].Width = iBoxArtIosX;
                        GameCover[i].Height = iBoxArtIosY;
                        break;
                    case "Megadrive":
                        GameCover[i].Width = iBoxArtMegadriveX;
                        GameCover[i].Height = iBoxArtMegadriveY;
                        break;
                    case "Nintendo 64":
                        GameCover[i].Width = iBoxArtN64X;
                        GameCover[i].Height = iBoxArtN64Y;
                        break;
                    case "Nes":
                        GameCover[i].Width = iBoxArtNesX;
                        GameCover[i].Height = iBoxArtNesY;
                        break;
                    case "Nintendo 3DS":
                        GameCover[i].Width = iBoxArtNintendoDsX;
                        GameCover[i].Height = iBoxArtNintendoDsY;
                        break;
                    case "Nintendo DS":
                        GameCover[i].Width = iBoxArtNintendoDsX;
                        GameCover[i].Height = iBoxArtNintendoDsY;
                        break;
                    case "PC":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "Playstation":
                        GameCover[i].Width = iBoxArtPsOneX;
                        GameCover[i].Height = iBoxArtPsOneY;
                        break;
                    case "Playstation 2":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "Playstation 3":
                        GameCover[i].Width = iBoxArtBrayX;
                        GameCover[i].Height = iBoxArtBrayY;
                        break;
                    case "Playstation 4":
                        GameCover[i].Width = iBoxArtBrayX;
                        GameCover[i].Height = iBoxArtBrayY;
                        break;
                    case "Super Nintendo":
                        GameCover[i].Width = iBoxArtSnesX;
                        GameCover[i].Height = iBoxArtSnesY;
                        break;
                    case "Wii":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "Wii U":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "Xbox":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                    case "Xbox 360":
                        GameCover[i].Width = iBoxArtDvdX;
                        GameCover[i].Height = iBoxArtDvdY;
                        break;
                }

                //conversion des images manquantes
                if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg"))
                {
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");

                }
                else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg"))
                {
                    var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg");
                    ResizeImage(new_mini, GameCover[i].Width, GameCover[i].Height, strCover);
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");
                }
                else
                {
                    GameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                }

                iGameWidth = GameCover[i].Size.Width;

                // change shelve
                if (iGameWidth + iMarginX > fWidthLibrary - iTotalGameWidth - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth)
                {
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

            fullbase.Close();
            MyConnection2.Close();

        }

        public void RemoveGames()
        {
            GameList.Dispose();
            GameList = null;
            GameInfo.Dispose();
            GameInfo = null;
            DisplayLibrary(strPlatformFilter, bInitDone); 
        }
 
        private void PlatformList_SelectedIndexChanges(object sender, System.EventArgs e)
        {
            var value = (ComboBox)sender;
            if (PlatformList.Text == "All")
            {
                strPlatformFilter = "IS NOT NULL";
            }
            else
            {
                strPlatformFilter = " = '" + PlatformList.Text + "'";
            }
            PlatformList.Text = strPlatformFilter;
            strSearch = "";
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

        private void AddNewGame_Click(object sender, System.EventArgs e)
        {
            NewGameTitle = new TextBox();
            NewGameTitle.KeyDown += new KeyEventHandler(AddNewGame_Validation);
            NewGameTitle.Location = new Point(PlatformList.Width + SortList.Width + Search.Width + AddNewGame.Width, 0);
            NewGameTitle.Text = "Title";
            Header.Controls.Add(NewGameTitle);

        }

        private void AddNewGame_Validation(object sender, KeyEventArgs e)
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

        private void Search_Click(object sender, System.EventArgs e)
        {
            strSearch = "'%"+"Plate-forme"+"%'";
            strPlatformFilter = "IS NOT NULL";
            RemoveGames();
        }

        private void GameCover_Enter(object sender, System.EventArgs e)
        {    
            var cover = (PictureBox)sender;
            strCover = cover.Name;
            iCoverOriginalX = cover.Location.X;
            iCoverOriginalY = cover.Location.Y;
            iCoverOriginalWidth = cover.Size.Width;
            iCoverOriginalHeight = cover.Size.Height;
            
            int CoverNewWidth = (int)(cover.Width * fScaleFactor);
            int CoverNewHeight = (int)(cover.Height * fScaleFactor);

            int CoverNewLocX = (int)(iCoverOriginalX - ((CoverNewWidth - iCoverOriginalWidth) / 2));
            int CoverNewLocY = (int)(iCoverOriginalY - ((CoverNewHeight - iCoverOriginalHeight) / 2));

            //cover.Height = (int)(cover.Height * fScaleFactor);
            //cover.Width = (int)(cover.Width * fScaleFactor);
            //cover.Location = new Point(CoverNewLocX, CoverNewLocY);
            
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
            GameCoverZoom.Dispose();
            GameCoverZoom = null;
        }
        
        bool bAlreadyClick = false;
        private void GameCoverZoom_Click(object sender, System.EventArgs e)
        {
            if (bAlreadyClick == true)
            {
                Remove_bigCover(GameInfo);
            }
            var zoomCover = (PictureBox)sender;
            bigCover = new PictureBox();
            //bigCover.BackColor = Color.Black;
            bigCover.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + strCover + ".jpg");
            bigCover.Location = new Point(0, 0);
            bigCover.Width = GameInfo.Width;
            bigCover.Height = zoomCover.Height * 2 ;
            bigCover.SizeMode = PictureBoxSizeMode.Zoom;
            GameInfo.Controls.Add(bigCover);
            bAlreadyClick = true;           
        }
        public void Remove_bigCover(Panel GameInfo)
        {
            bigCover.Dispose();
            bigCover = null;
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
                fFormWidth = this.Width;
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

        public string GetID(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 0);
        }

        public string GetTitle(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 1);
        }

        public string GetPlatform(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 2);
        }

        public string GetDeveloper(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 4);
        }

        public string GetPublisher(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 5);
        }

        public string GetReleaseYear(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 6);
        }

        public string GetGenre(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 7);
        }

        public string GetCover(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 15);
        }

        public static Bitmap ResizeImage(Image image, int width, int height, string name)
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
