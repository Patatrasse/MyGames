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
        //interface
        bool bInitDone = false;
        public PictureBox[] GameCover;
        public Bitmap destImage;
        public PictureBox bigCover;
        public ComboBox PlatformList;
        public ComboBox SortList;
       //var
        int iTableSize;
        int iNumPlatform;
        int iNbDisplayedGames;
        float fWidthLibrary;
        float fWidthInfos;
        float fScaleFactor = 1.1f;
        float fFormWidth;
        int iCoverOriginalX;
        int iCoverOriginalY;
        int iCoverOriginalWidth;
        int iCoverOriginalHeight;
        //taille des covers
        static int iShelveHeight = 150;
        static int iBoxArtDvdX = 130;
        static int iBoxArtDvdY = 183;
        static int iBoxArtBrayX = 130;
        static int iBoxArtBrayY = 149;
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
            //SortList.SelectedIndexChanged += new System.EventHandler(SortList_SelectedIndexChanges);
            SortList.DropDownStyle = ComboBoxStyle.DropDownList;
            SortList.Location = new Point(PlatformList.Width, 0);

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
            cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE platform " + strPlatformFilter + " ORDER by " + strPlatformSort;
            MySqlDataReader fullbase = cmd.ExecuteReader();

            int i = 0;
            int j = 0;
            int iGameWidth;
            int iTotalGameWidth = 0;
            int iShelveTop;
            int iMarginX = 5;
            iNbDisplayedGames = 0;
            fWidthLibrary = (float)(fFormWidth * 0.8);
            fWidthInfos = (float)(fFormWidth * 0.2);

            // Placement des jeux dans un panel
            GameList = new Panel();
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
                        GameCover[i].Width = iBoxArtDreamcastX / 2;
                        GameCover[i].Height = iBoxArtDreamcastY / 2;
                        break;
                    case "Game Boy":
                        GameCover[i].Width = iBoxArtGameBoyX / 2;
                        GameCover[i].Height = iBoxArtGameBoyY / 2;
                        break;
                    case "Game Boy Advance":
                        GameCover[i].Width = iBoxArtGameBoyX / 2;
                        GameCover[i].Height = iBoxArtGameBoyY / 2;
                        break;
                    case "Game Boy Color":
                        GameCover[i].Width = iBoxArtGameBoyX / 2;
                        GameCover[i].Height = iBoxArtGameBoyY / 2;
                        break;
                    case "Gamecube":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "iOS":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Megadrive":
                        GameCover[i].Width = iBoxArtMegadriveX / 2;
                        GameCover[i].Height = iBoxArtMegadriveY / 2;
                        break;
                    case "Nintendo 64":
                        GameCover[i].Width = iBoxArtN64X / 2;
                        GameCover[i].Height = iBoxArtN64Y / 2;
                        break;
                    case "Nes":
                        GameCover[i].Width = iBoxArtNesX / 2;
                        GameCover[i].Height = iBoxArtNesY / 2;
                        break;
                    case "Nintendo 3DS":
                        GameCover[i].Width = iBoxArtNintendoDsX / 2;
                        GameCover[i].Height = iBoxArtNintendoDsY / 2;
                        break;
                    case "Nintendo DS":
                        GameCover[i].Width = iBoxArtNintendoDsX / 2;
                        GameCover[i].Height = iBoxArtNintendoDsY / 2;
                        break;
                    case "PC":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Playstation":
                        GameCover[i].Width = iBoxArtPsOneX / 2;
                        GameCover[i].Height = iBoxArtPsOneY / 2;
                        break;
                    case "Playstation 2":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Playstation 3":
                        GameCover[i].Width = iBoxArtBrayX / 2;
                        GameCover[i].Height = iBoxArtBrayY / 2;
                        break;
                    case "Playstation 4":
                        GameCover[i].Width = iBoxArtBrayX / 2;
                        GameCover[i].Height = iBoxArtBrayY / 2;
                        break;
                    case "Super Nintendo":
                        GameCover[i].Width = iBoxArtSnesX / 2;
                        GameCover[i].Height = iBoxArtSnesY / 2;
                        break;
                    case "Wii":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Wii U":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Xbox":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
                        break;
                    case "Xbox 360":
                        GameCover[i].Width = iBoxArtDvdX / 2;
                        GameCover[i].Height = iBoxArtDvdY / 2;
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
                GameCover[i].MouseHover += new System.EventHandler(this.GameCover_Hover);
                GameCover[i].MouseClick += new System.Windows.Forms.MouseEventHandler(this.GameCover_Click);
                GameCover[i].MouseWheel += new System.Windows.Forms.MouseEventHandler(this.GameCover_Scroll);
                GameList.Controls.Add(GameCover[i]);

                i = i + 1;
            }

            fullbase.Close();
            MyConnection2.Close();

            //Panels settings          
            GameList.Location = new Point(0, PlatformList.Height);
            GameList.Width = (int)fWidthLibrary;
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
            GameInfo.Location = new Point((int)fWidthLibrary, PlatformList.Height);
            GameInfo.Width = (int)fWidthInfos;
            GameInfo.Height = GameList.Height;
            GameInfo.AutoScroll = false;
            GameInfo.BackColor = Color.Blue;
            this.Controls.Add(GameInfo);

        }

        public void RemoveGames()
        {
            GameList.Dispose();
            GameList = null;
            GameInfo.Dispose();
            GameInfo = null;
            DisplayLibrary(strPlatformFilter, bInitDone); 
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
            RemoveGames();
        }

        private void SortList_SelectedIndexChanges(object sender, System.EventArgs e)
        {
            var value = (ComboBox)sender;
            if (SortList.Text == "A->B")
            {
                strPlatformFilter = "IS NOT NULL";
            }
            else
            {
                strPlatformFilter = " = '" + PlatformList.Text + "'";
            }
            PlatformList.Text = strPlatformFilter;
            RemoveGames();
        }

        private void GameCover_Enter(object sender, System.EventArgs e)
        {
            var cover = (PictureBox)sender;
            iCoverOriginalX = cover.Location.X;
            iCoverOriginalY = cover.Location.Y;
            iCoverOriginalWidth = cover.Size.Width;
            iCoverOriginalHeight = cover.Size.Height;

            int CoverNewWidth = (int)(cover.Width * fScaleFactor);
            int CoverNewHeight = (int)(cover.Height * fScaleFactor);

            int CoverNewLocX = (int)(iCoverOriginalX - ((CoverNewWidth - iCoverOriginalWidth) / 2));
            int CoverNewLocY = (int)(iCoverOriginalY - ((CoverNewHeight - iCoverOriginalHeight) / 2));

            cover.Height = (int)(cover.Height * fScaleFactor);
            cover.Width = (int)(cover.Width * fScaleFactor);
            cover.Location = new Point(CoverNewLocX, CoverNewLocY);
        }

        private void GameCover_Hover(object sender, System.EventArgs e)
        {

        }

        private void GameCover_Leave(object sender, System.EventArgs e)
        {
                var cover = (PictureBox)sender;
                cover.Height = iCoverOriginalHeight;
                cover.Width = iCoverOriginalWidth;
                cover.Location = new Point(iCoverOriginalX, iCoverOriginalY);
        }
        
        private void GameCover_Scroll(object sender, System.EventArgs e)
        {

        }

        bool bAlreadyClick = false;
        private void GameCover_Click(object sender, System.EventArgs e)
        {
            if (bAlreadyClick == true)
            {
                Remove_bigCover(GameInfo);
            }
            var zoomCover = (PictureBox)sender;
            strCover = zoomCover.Name;
            bigCover = new PictureBox();
            bigCover.BackColor = Color.Black;
            bigCover.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + strCover + ".jpg");
            bigCover.Location = new Point(0, PlatformList.Height);
            bigCover.Width = GameInfo.Width;
            bigCover.Dock = DockStyle.Top;
            bigCover.Height = this.ClientSize.Height;
            bigCover.SizeMode = PictureBoxSizeMode.Normal;
            bigCover.BringToFront();
            bigCover.Show();
            GameInfo.Controls.Add(bigCover);
            bAlreadyClick = true;           
        }
        public void Remove_bigCover(Panel GameInfo)
        {
            GameInfo.Dispose();
            GameInfo = null;
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
