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
using MyGames.Properties;

namespace MyDatabase
{
    public partial class DisplayInit : Form
    {   //connexion DB
        MySqlConnection ConnectionToGetPlatform;
        MySqlConnection ConnectionToDisplayGames;
        MySqlConnection ConnectionToAddNewGame;
        MySqlConnection ConnectionForGameInfo;
        MySqlConnection ConnectionForDLCInfo;
        MySqlConnection ConnectionHardware;
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
        public string strSerie;
        public string strCover;
        public string strDlcCover;
        public string strCoverDLC;
        public string strCoverType;
        public string strPlatformSort = "title";
        public string strbtSearch = "";
        public string strTooltip = "Test";
        public string strDefaultSearch = "Tapez votre recherche";
        public string strSearchResults = "";
        public string iDLC;
        public bool bDLC;
        //interface
        bool bInitDone = false;
        bool bGameWithDLC = false;
        public PictureBox[] pbGameCover;
        public PictureBox[] pbDlcCover;
        public PictureBox pbGameCoverZoom;
        public PictureBox pbGameCoverMax;
        public PictureBox pbGameSelected;
        public PictureBox pbEditIcon;
        public ComboBox cbPlatformList;
        public ComboBox SortList;
        public Button btSearch;
        public Button btAddNewGame;
        public Button btCancel;
        public TextBox NewGameTitle;
        public Label bigCoverTitle;
        public Label lbDeveloper;
        public Label lbPublisher;
        public Label lbYear;
        public Label lbGenre;
        public Label lbSerie;
        public Label lbDLC;
        public Label lbSearchResult;
        public CheckBox chbTitle;
        public CheckBox chbDev;
        public CheckBox chbPublisher;
        public CheckBox chbGenre;
        public CheckBox chbSerie;
        public TextBox EditBox;
        public TextBox tbSearch;
        public RadioButton buttonAlphaSort;
        public RadioButton buttonNumSort;
        public Label lbStatistics;
        public ToolTip ttSearch;
        public ToolTip ttButtonAdd;
        List<int> iListHardwareIndex = new List<int>();
       //var
        int iGameTableSize;
        int iDlcTableSize;
        int iNumPlatform;
        int iNbDisplayedGames = 0;
        int iNbDisplayedDlc = 0;
        int iNbPlatformDLC = 0;
        float fWidthLibrary;
        float fScaleFactor = 1.05f;
        float fFormWidth;
        int iInfoWidth = 400;
        int iCoverOriginalX;
        int iCoverOriginalY;
        int iCoverOriginalWidth;
        int iCoverOriginalHeight;
        int iSelectedCoverPosX;
        int iSelectedCoverPosY;
        int iSelectedCoverWidth;
        int iSelectedCoverSize = 10; //taille du feedback de sélection
        int iMarginX = 15; //espace entre les jaquettes
        int iMarginY = 15; //espace entre les étagères
        int iLeftMarginBorder; //espace entre le bord gauche de l'appli et la 1ere jaquette
        int iMargeXPictureBoxImage; //espace entre gauche de l'image et la gauche de la picturebox
        int iMargeYPictureBoxImage; //espace entre haut de l'image et le haut de la picturebox
        int iCoverIndex = 0;
        int iCoverID;
        bool bAlreadyClick = false;
        bool bSearchMode = false;
        bool bSearchInProgress = false;
        object[,] iHardwareArray;
        int iItemSelected = 1;
        public string strSearchTitle ;
        public string strSearchDev;
        public string strSearchPublisher;
        public string strSearchGenre;
        public string strSearchSerie;
        //taille des covers
        static int iBoxArtDvdY = 183;
        static int iShelveHeight = 195;
        //saved data
        public string strPlatformFilter = Settings.Default.Platform_Name;
        int iHardwareSelected = Settings.Default.Platform_Index;
        int iSelectedCoverIndex = Settings.Default.Game_Index;

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public DisplayInit()
        {
            Console.WriteLine(iHardwareSelected);
            this.WindowState = FormWindowState.Maximized;
            this.Icon = new Icon("D:/Documents/GitHub/GameLibrary/MyGames/appicon.ico");
            this.Text = "My Games";
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.MinimumSize = new Size(620, 500); //TODO = marge = boite largeur max + marges
            this.Resize += new System.EventHandler(this.Refresh);
            fFormWidth = this.Width;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);
            //config file pour les accès à la DB
            XDocument doc = XDocument.Load("config.xml");
            strServer = doc.Root.Element("server").Value;
            strUser = doc.Root.Element("user").Value;
            strPassword = doc.Root.Element("password").Value;
            strDatabase = doc.Root.Element("database").Value;
            strSoftwareTable = doc.Root.Element("table").Value;
            strHardwareTable = doc.Root.Element("table1").Value;

            //connexion pour récupérer les plateformes de la DB
            ConnectionToGetPlatform = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            ConnectionToGetPlatform.Open();
            MySqlCommand cmd2 = ConnectionToGetPlatform.CreateCommand();
            cmd2.CommandText = "SELECT COUNT(*) FROM (SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform) AS numplatform"; //num platforms
            iNumPlatform = Convert.ToInt32(cmd2.ExecuteScalar());
            cmd2.CommandText = "SELECT platform, COUNT(*) FROM " + strSoftwareTable + " GROUP BY platform"; //platform list
            MySqlDataReader platforms = cmd2.ExecuteReader();

            //combo box header pour filtrer par plateforme
            cbPlatformList = new ComboBox();
            cbPlatformList.FlatStyle = FlatStyle.System;
            cbPlatformList.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPlatformList.Location = new Point(0, 0);
            cbPlatformList.Font = new Font("Ubuntu Bold", 14);

            //label pour afficher les résulats de la recherche à la place de la combobox filtre
            lbSearchResult = new Label();
            lbSearchResult.Location = new Point(0, 0);
            lbSearchResult.TextAlign = ContentAlignment.MiddleCenter;
            lbSearchResult.Font = new Font("Ubuntu Bold", 14);

            //boucle nombre de platformes + 1 pour tout afficher
            int k = 0;
            
            while (k < iNumPlatform)
            {
                //if (k == 0)
                //{
                //    cbPlatformList.Items.Insert(k, "All Platforms");
                //}
                //else
                //{
                    platforms.Read();
                    string strPlatformName = GetPlatformsName(platforms, 0);
                    cbPlatformList.Items.Insert(k, strPlatformName);
                //}
                k = k + 1;
            }
            
            platforms.Close();
            ConnectionToGetPlatform.Close();

            cbPlatformList.SelectedIndex = iHardwareSelected;
            cbPlatformList.SelectedIndexChanged += new System.EventHandler(cbPlatformList_SelectedIndexChanges);
            cbPlatformList.DrawMode = DrawMode.OwnerDrawFixed;
            cbPlatformList.DrawItem += new DrawItemEventHandler(cbPlatformList_DrawItem);
            cbPlatformList.DropDownClosed += new EventHandler(cbPlatformList_DropDownClosed);
            this.Controls.Add(cbPlatformList);
            this.Controls.Add(lbSearchResult);

            ConnectionHardware = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            ConnectionHardware.Open();
            MySqlCommand cmdHardware = ConnectionHardware.CreateCommand();
            cmdHardware.CommandText = "SELECT COUNT(*) FROM (SELECT hardware_id, COUNT(*) FROM " + strHardwareTable + " GROUP BY hardware_id) AS numplatform";
            int iTabSize = Convert.ToInt32(cmdHardware.ExecuteScalar());
            iHardwareArray = new object[iTabSize, 11];
            MySqlDataReader fullbasehardware = cmdHardware.ExecuteReader();

            fullbasehardware.Read();
            fullbasehardware.Close();

            cmdHardware.CommandText = "SELECT * FROM " + strHardwareTable + " ORDER BY name";
            MySqlDataReader fullbasehardware2 = cmdHardware.ExecuteReader();
            
            //tableau pour récupérer les infos sur les palteformes
            k = 0;
            int iHardwareID;
            string strHardwareName;
            string strManufacturer;
            int iReleaseDate;
            int iCoverWidth;
            int iCoverHeight;
            int iCoverColorRed;
            int iCoverColorGreen;
            int iCoverColorBlue;                     
            string strOnlineService;
            string strMedia;
            while (fullbasehardware2.Read())
            {
                iHardwareID =  GetHardwareID(fullbasehardware2);
                strHardwareName = GetHardwareName(fullbasehardware2); ;
                strManufacturer = GetManufacturer(fullbasehardware2);
                iReleaseDate = GetPlatformRelease(fullbasehardware2);             
                iCoverWidth = GetBoxeSizeX(fullbasehardware2);
                iCoverHeight =   GetBoxeSizeY(fullbasehardware2);
                iCoverColorRed = GetHardwareColorRed(fullbasehardware2);
                iCoverColorGreen = GetHardwareColorGreen(fullbasehardware2);
                iCoverColorBlue = GetHardwareColorBlue(fullbasehardware2);        
                strOnlineService = GetOnlineService(fullbasehardware2);
                strMedia = GetMedia(fullbasehardware2);
                
                iHardwareArray[k, 0] = iHardwareID;
                iHardwareArray[k, 1] = strHardwareName;
                iHardwareArray[k, 2] = strManufacturer;
                iHardwareArray[k, 3] = iReleaseDate;
                iHardwareArray[k, 4] = iCoverWidth;
                iHardwareArray[k, 5] = iCoverHeight;
                iHardwareArray[k, 6] = iCoverColorRed;
                iHardwareArray[k, 7] = iCoverColorGreen;
                iHardwareArray[k, 8] = iCoverColorBlue;
                iHardwareArray[k, 9] = strOnlineService;
                iHardwareArray[k, 10] = strMedia;
                iListHardwareIndex.Add(iHardwareID);
                k = k + 1;
            }
            
            fullbasehardware2.Close();
            ConnectionHardware.Close();

            //panel pour afficher des info sur l'affichage + options
            plFooter = new Panel();
            plFooter.Height = cbPlatformList.Height;
            plFooter.BackColor = Color.GhostWhite;
            this.Controls.Add(plFooter);

            btSearch = new Button();
            btSearch.Click += new EventHandler(btSearch_Click);
            btSearch.BackgroundImage = Image.FromFile("D:\\Documents\\GitHub\\GameLibrary\\MyGames\\icon_search32.png");
            btSearch.BackgroundImageLayout = ImageLayout.Zoom;
            btSearch.Height = plFooter.Height;
            btSearch.Width = btSearch.Height;
            btSearch.Location = new Point(0, 0);
            btSearch.BackColor = Color.GhostWhite;
            btSearch.ForeColor = Color.GhostWhite;
            btSearch.FlatStyle = FlatStyle.Flat;
            btSearch.FlatAppearance.BorderSize = 0;
            btSearch.Text = "";

            btAddNewGame = new Button();
            btAddNewGame.Click += new EventHandler(btAddNewGame_Click);
            btAddNewGame.BackgroundImage = Image.FromFile("D:\\Documents\\GitHub\\GameLibrary\\MyGames\\icon_add32.png");
            btAddNewGame.BackgroundImageLayout = ImageLayout.Zoom;
            btAddNewGame.Height = plFooter.Height;
            btAddNewGame.Width = btSearch.Height;
            btAddNewGame.Location = new Point(btSearch.Width, 0);
            btAddNewGame.BackColor = Color.GhostWhite;
            btAddNewGame.ForeColor = Color.GhostWhite;
            btAddNewGame.FlatStyle = FlatStyle.Flat;
            btAddNewGame.FlatAppearance.BorderSize = 0;
            btAddNewGame.Tag = "Add new game";

            btCancel = new Button();
            btCancel.Click += new EventHandler(btCancel_Click);
            btCancel.BackgroundImage = Image.FromFile("D:\\Documents\\GitHub\\GameLibrary\\MyGames\\icon_cancel32.png");
            btCancel.BackgroundImageLayout = ImageLayout.Zoom;
            btCancel.Height = plFooter.Height;
            btCancel.Width = btCancel.Height;
            btCancel.Location = new Point(0, 0);
            btCancel.BackColor = Color.GhostWhite;
            btCancel.ForeColor = Color.GhostWhite;
            btCancel.FlatStyle = FlatStyle.Flat;
            btCancel.FlatAppearance.BorderSize = 0;
            btCancel.Text = "";

            tbSearch = new TextBox();
            tbSearch.KeyDown += new KeyEventHandler(tbSearch_Validation);
            tbSearch.Font = new Font("Ubuntu", 8, FontStyle.Regular);
            tbSearch.Location = new Point(btSearch.Width, 0);
            tbSearch.Width = (plFooter.Width - btSearch.Width) / 6;
            tbSearch.MinimumSize = new Size(175, 0);

            chbTitle = new CheckBox();
            chbDev = new CheckBox();
            chbPublisher = new CheckBox();
            chbGenre = new CheckBox();
            chbSerie = new CheckBox();
            chbTitle.Checked = true;
            chbGenre.Checked = true;
            chbSerie.Checked = true;
            chbPublisher.Checked = true;
            chbDev.Checked = true;

            DisplayLibrary(strPlatformFilter, bInitDone, iCoverIndex, iCoverID);
        }

        public void cbPlatformList_DropDownClosed(object sender, EventArgs e)
        {
            this.ActiveControl = null;
        }

        public void DisplayLibrary(string strPlatformFilter, bool bInitDone, int iCoverIndex, int iCoverID)
        {
            cbPlatformList.Width = (int)fFormWidth;
            lbSearchResult.Width = cbPlatformList.Width;
            lbSearchResult.Height = cbPlatformList.Height;
            plFooter.Width = (int)fFormWidth - iInfoWidth;
            //accès base de jeux
            ConnectionToDisplayGames = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            ConnectionToDisplayGames.Open();
            MySqlCommand cmd = ConnectionToDisplayGames.CreateCommand();

            //prise en compte de la recherche ou non
            if (bSearchMode != true)
            {
                cmd.CommandText = "SELECT COUNT(*) FROM " + strSoftwareTable + " WHERE platform IS NOT NULL";
                iGameTableSize = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE platform LIKE " + "'" +  strPlatformFilter + "'" + " ORDER BY " + strPlatformSort;
                
            }
            else
            {
                cmd.CommandText = "SELECT COUNT(*) FROM " + strSoftwareTable + " WHERE " +
                                  strSearchTitle + " OR " + strSearchDev + " OR " + strSearchPublisher + " OR " + strSearchGenre + " OR " + strSearchSerie + " ORDER BY " + strPlatformSort;

                iGameTableSize = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE " +
                                  strSearchTitle + " OR " + strSearchDev + " OR " + strSearchPublisher + " OR " + strSearchGenre + " OR " + strSearchSerie + " ORDER BY " + strPlatformSort;
                if (iGameTableSize == 0)
                {
                    lbSearchResult.Text = "\"" + tbSearch.Text + "\"" + " : " + iGameTableSize + " jeu trouvé";
                    cbPlatformList.Hide();
                    lbSearchResult.Show();
                    return;
                }
                if (iGameTableSize > 1)
                {
                    lbSearchResult.Text = "\"" + tbSearch.Text + "\"" + " : " + iGameTableSize + " jeux trouvés";
                }
                else
                {
                    lbSearchResult.Text = "\"" + tbSearch.Text + "\"" + " : " + iGameTableSize + " jeu trouvé";
                }
            }
            MySqlDataReader fullbase = cmd.ExecuteReader();

            int i = 0;
            int iGameWidth;
            int iTotalGameWidth = 0;
            int iShelveTop = 0;
            iNbDisplayedGames = 0;
            iNbPlatformDLC = 0;     
            //panel settings -> plGameList = panel de tous les jeux de la plateforme          
            plGameList = new Panel();
            plGameList.Location = new Point(0, cbPlatformList.Height);
            plGameList.VerticalScroll.Maximum = 0;
            plGameList.HorizontalScroll.Maximum = 0;
            plGameList.AutoScroll = false;
            plGameList.VerticalScroll.Visible = false;
            plGameList.AutoScroll = true;
            plGameList.BackColor = Color.White;
            plGameList.MouseDown += new MouseEventHandler(this.plGameList_Click);
            Controls.Add(plGameList);
            //panel settings -> plGameInfo = panel avec les infos sur jeu sélectionnée 
            plGameInfo = new Panel();
            plGameInfo.Width = iInfoWidth;
            plGameInfo.Height = Screen.PrimaryScreen.Bounds.Height - cbPlatformList.Height;                  
            plGameInfo.AutoScroll = false;
            plGameInfo.BackColor = Color.White;  
            Controls.Add(plGameInfo);
            fWidthLibrary = this.ClientSize.Width - plGameInfo.Width;  

            //placement des jaquettes dans plGameList         
            pbGameCover = new PictureBox[iGameTableSize];
            
            while (fullbase.Read())
            {
                strCover = GetCover(fullbase);
                strCoverType = GetPlatform(fullbase);
                iDLC = GetMainGame_ForDLC(fullbase);
                int iHardwareIndex = GetPlatformIndex(fullbase);
                iHardwareIndex = iListHardwareIndex.FindIndex(x => x==iHardwareIndex);

                bDLC = false;
                if (iDLC != "")
                {
                    bDLC =true;
                    iNbPlatformDLC = iNbPlatformDLC + 1;
                }
                //chaque jaquette est une Picturebox
                pbGameCover[i] = new PictureBox();
                pbGameCover[i].SizeMode = PictureBoxSizeMode.Zoom;
                //récupération des tailles des boîtes
                pbGameCover[i].Width = (int)iHardwareArray[iHardwareIndex, 4];
                pbGameCover[i].Height = (int)iHardwareArray[iHardwareIndex, 5];
                //conversion des images manquantes pour le zoom et la sélection et la jaquette par défaut -> TODO : chemins relatifs
                if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg"))
                {
                    pbGameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");

                }
                else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg"))
                {
                    var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + strCover + ".jpg");
                    CreateImage(new_mini, pbGameCover[i].Width, pbGameCover[i].Height, strCover);
                    pbGameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCover + ".jpg");
                }
                else
                {
                    if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg"))
                    {
                        pbGameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg");

                    }
                    else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg"))
                    {
                        var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                        CreateImage(new_mini, pbGameCover[i].Width, pbGameCover[i].Height, "0");
                        pbGameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg");
                    }                  
                    pbGameCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                }

                iGameWidth = pbGameCover[i].Size.Width;
                int iNbGamesPerRow;
                //calcul de la marge à gauche pour démarrer le placement des jaquettes -> valeur fixe pour "All platforms"
                if (bSearchMode == true)
                {
                    iLeftMarginBorder = iMarginX;
                }
                else
                {
                    iNbGamesPerRow = (int)fWidthLibrary / (iGameWidth + iMarginX);
                    iLeftMarginBorder = ((int)fWidthLibrary - (iNbGamesPerRow * (iGameWidth + iMarginX))) / 2 + iMarginX / 2;
                }
                //les DLC ne sont pas affichés
                if (bDLC == false)
                {
                    //change de ligne si la prochaine jaquette n'a pas de place
                    if (iGameWidth + iLeftMarginBorder > fWidthLibrary - iTotalGameWidth)
                    {
                        iTotalGameWidth = 0;
                        if (bSearchMode == true)
                        {
                            iShelveTop = iShelveHeight + iMarginY + iShelveTop; //iShelveHeight + iShelveTop;
                        }
                        else
                        {
                            iShelveTop = (int)iHardwareArray[cbPlatformList.SelectedIndex, 5] + iMarginY + iShelveTop; //iShelveHeight + iShelveTop;
                        }
                    }
                    //position 1ere jaquette de chaque ligne
                    if (iTotalGameWidth == 0)
                    {
                        pbGameCover[i].Left = iLeftMarginBorder;
                        iTotalGameWidth = iLeftMarginBorder;
                    }
                    //position des jaquettes suivantes
                    else
                    {
                        pbGameCover[i].Left = pbGameCover[i - 1].Location.X + pbGameCover[i - 1].Width + iMarginX;
                    }
                    // picturebox settings
                    if (bSearchMode == true)
                    {
                        pbGameCover[i].Top = iShelveTop + (iShelveHeight - pbGameCover[i].Height);
                    }
                    else
                    {
                        pbGameCover[i].Top = iShelveTop + iMarginY;// iShelveTop + (iShelveHeight - pbGameCover[i].Height);
                    }
                    
                    pbGameCover[i].Name = GetCover(fullbase);
                    pbGameCover[i].Show();
                    pbGameCover[i].MouseEnter += new System.EventHandler(this.GameCover_Enter);
                    pbGameCover[i].MouseLeave += new System.EventHandler(this.GameCover_Leave);
                    //autosélection de la jaquette
                    if (i == iSelectedCoverIndex)
                    {
                        iCoverID = Int32.Parse(pbGameCover[iSelectedCoverIndex].Name);
                        iCoverIndex = iSelectedCoverIndex;
                    }
                    plGameList.Controls.Add(pbGameCover[i]);

                    //mise à jour des valeurs
                    iNbDisplayedGames = iNbDisplayedGames + 1;
                    iTotalGameWidth = iTotalGameWidth + iGameWidth + iMarginX;
                    i = i + 1;
                }
            }
            //placement du feedback de sélection
            pbGameSelected = new PictureBox();
            pbGameSelected.Width = pbGameCover[iCoverIndex].Width + iSelectedCoverSize;
            pbGameSelected.Height = pbGameCover[iCoverIndex].Height + iSelectedCoverSize;
            pbGameSelected.Location = new Point(pbGameCover[iCoverIndex].Location.X - iSelectedCoverSize / 2, pbGameCover[iCoverIndex].Location.Y - iSelectedCoverSize / 2);            
            //couleur du feedback de sélection dépendant de la platforme
            if (bSearchMode == true)
            {
                pbGameSelected.BackColor = Color.Black;
            }
            else
            {
                pbGameSelected.BackColor = Color.FromArgb(255, (int)iHardwareArray[cbPlatformList.SelectedIndex, 6], (int)iHardwareArray[cbPlatformList.SelectedIndex, 7], (int)iHardwareArray[cbPlatformList.SelectedIndex, 8]);
            }
            plGameList.Controls.Add(pbGameSelected);
            fullbase.Close();
            ConnectionToDisplayGames.Close();

            tbSearch.Text = strDefaultSearch;

            chbTitle.Text = "Titre";
            chbDev.Text = "Développeur";
            chbPublisher.Text = "Éditeur";
            chbGenre.Text = "Genre";
            chbSerie.Text = "Série";

            int iCheckboxwidth;
            iCheckboxwidth = Math.Max(TextRenderer.MeasureText(chbTitle.Text, chbTitle.Font).Width,
                                Math.Max(TextRenderer.MeasureText(chbDev.Text, chbTitle.Font).Width,
                                Math.Max(TextRenderer.MeasureText(chbPublisher.Text, chbTitle.Font).Width, TextRenderer.MeasureText(chbGenre.Text, chbTitle.Font).Width)));
            chbTitle.Location = new Point(tbSearch.Right + 10, 0);
            
            chbTitle.Font = new Font("Ubuntu", 8, FontStyle.Regular);
            chbTitle.Width = iCheckboxwidth;

            chbGenre.Location = new Point(chbTitle.Right, 0);
            chbGenre.Font = chbTitle.Font;
            chbGenre.Width = iCheckboxwidth;

            chbSerie.Location = new Point(chbGenre.Right, 0);
            
            chbSerie.Font = chbTitle.Font;
            chbSerie.Width = iCheckboxwidth;

            chbPublisher.Location = new Point(chbSerie.Right, 0);
            
            chbPublisher.Font = chbTitle.Font;
            chbPublisher.Width = iCheckboxwidth;

            chbDev.Location = new Point(chbPublisher.Right, 0);
            
            chbDev.Font = chbTitle.Font;
            chbDev.Width = TextRenderer.MeasureText(chbDev.Text, chbTitle.Font).Width + 25;

            lbStatistics = new Label();
            if (iNbPlatformDLC == 0)
            {
                strDlcCover = "";
            }
            else
            {
                strDlcCover = " - DLC : " + iNbPlatformDLC;
            }
            //if (iHardwareSelected != 0)
            //{
            lbStatistics.Text = "Fabricant : " + iHardwareArray[iHardwareSelected, 2]
                                + " - Sortie : " + iHardwareArray[iHardwareSelected, 3] 
                                + " - Support : " + iHardwareArray[iHardwareSelected, 10]
                                + " - Services en ligne : " + iHardwareArray[iHardwareSelected, 9]
                                + " - Jeux : " + iNbDisplayedGames + strDlcCover;
            //}
            //else
            //{
            //    lbStatistics.Text = "Jeux : " + iNbDisplayedGames + " - DLC : " + iNbPlatformDLC;
            //}
            lbStatistics.Font = new Font("Ubuntu", 10, FontStyle.Regular);
            lbStatistics.TextAlign = ContentAlignment.MiddleLeft;
            lbStatistics.BackColor = Color.GhostWhite;
            lbStatistics.Location = new Point(btSearch.Width + btAddNewGame.Width, 0);
            lbStatistics.Height = plFooter.Height;
            SizeF stringSize = new SizeF();
            using (Graphics g = CreateGraphics())
            {
                stringSize = g.MeasureString(lbStatistics.Text, lbStatistics.Font);

            }
            lbStatistics.Width = (int)stringSize.Width + 10;
            
            ttSearch = new ToolTip();
            ttSearch.OwnerDraw = true;
            ttSearch.Draw += new DrawToolTipEventHandler(ToolTip_Draw);
            ttSearch.Popup += new PopupEventHandler(ToolTip_Popup);
            ttSearch.SetToolTip(this.btSearch, strTooltip);
            ttSearch.InitialDelay = 0;
            ttSearch.BackColor = Color.FloralWhite;
            ttSearch.Tag = "Chercher un jeu";

            ttButtonAdd = new ToolTip();
            ttButtonAdd.OwnerDraw = true;
            ttButtonAdd.Draw += new DrawToolTipEventHandler(ToolTip_Draw);
            ttButtonAdd.Popup += new PopupEventHandler(ToolTip_Popup);
            ttButtonAdd.SetToolTip(this.btAddNewGame, strTooltip);
            ttButtonAdd.InitialDelay = 0;
            ttButtonAdd.BackColor = Color.FloralWhite;
            ttButtonAdd.Tag = "Ajouter un jeu";

            plFooter.Controls.Add(btSearch);
            plFooter.Controls.Add(btCancel);
            plFooter.Controls.Add(tbSearch);
            plFooter.Controls.Add(btAddNewGame);
            plFooter.Controls.Add(lbStatistics);
            plFooter.Controls.Add(chbTitle);
            plFooter.Controls.Add(chbDev);
            plFooter.Controls.Add(chbPublisher);
            plFooter.Controls.Add(chbGenre);
            plFooter.Controls.Add(chbSerie);

            //Search state  
            if (bSearchMode == true)
            {
                cbPlatformList.Hide();
                lbSearchResult.Show();

                tbSearch.Show();       
                btCancel.Show();
                
                btAddNewGame.Hide();
                lbStatistics.Hide();
                if (plFooter.Width < btCancel.Width + tbSearch.Width + chbTitle.Width + chbDev.Width + chbPublisher.Width + chbGenre.Width + chbSerie.Width)
                {
                    chbTitle.Hide();
                    chbDev.Hide();
                    chbPublisher.Hide();
                    chbGenre.Hide();
                    chbSerie.Hide();
                }
                else
                {
                    chbTitle.Show();
                    chbDev.Show();
                    chbPublisher.Show();
                    chbGenre.Show();
                    chbSerie.Show();
                }
            }
            else
            {
                cbPlatformList.Show();
                lbSearchResult.Hide();

                tbSearch.Hide();              
                btCancel.Hide();
                chbTitle.Hide();
                chbDev.Hide();
                chbPublisher.Hide();
                chbGenre.Hide();
                chbSerie.Hide();

                btSearch.Show();
                btAddNewGame.Show();
                
                if (plFooter.Width < lbStatistics.Width + btSearch.Width + btAddNewGame.Width)
                {
                    lbStatistics.Hide();
                }
                else
                {
                    lbStatistics.Show();
                }      
            }
            DisplayInfo(iCoverID);
        }
        
        bool bPreviousGameHasDLC = false;
        bool bPreviousGameIsSerie = false;
        public void DisplayInfo(int iCoverID)
        {
            int bigCoverId;
            int iDlcTotalWidth = 0;
            int i = 0;
            //info to display when clicked
            pbGameCoverMax = new PictureBox();
            bigCoverTitle = new Label();
            lbDeveloper = new Label();
            lbPublisher = new Label();
            lbYear = new Label();
            lbGenre = new Label();

            ConnectionForGameInfo = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            ConnectionForGameInfo.Open();
            MySqlCommand GameData = ConnectionForGameInfo.CreateCommand();
            GameData.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE game_id = " + iCoverID;
            MySqlDataReader gameinfo = GameData.ExecuteReader();
            gameinfo.Read();
            bigCoverId = GetID(gameinfo);
            pbGameCoverMax.Width = plGameInfo.Width;
            pbGameCoverMax.Height = 500;

            if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_zoom/" + iCoverID + ".jpg"))
            {
                pbGameCoverMax.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + iCoverID + ".jpg");
            }
            else
            {
                pbGameCoverMax.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/0.jpg");
            }

            pbGameCoverMax.SizeMode = PictureBoxSizeMode.Zoom;
            OffsetOfImage(pbGameCoverMax);
            
            if (iMargeXPictureBoxImage < iMarginY)
            {
                
                pbGameCoverMax.MaximumSize = new Size(plGameInfo.Width - iMarginX * 2, pbGameCoverMax.Height);
                OffsetOfImage(pbGameCoverMax);
                pbGameCoverMax.Location = new Point((plGameInfo.Width - pbGameCoverMax.Width) / 2, -iMargeYPictureBoxImage + iMarginY);
            }
            else
            {
                pbGameCoverMax.MaximumSize = new Size();
                pbGameCoverMax.Location = new Point(0, -iMargeYPictureBoxImage + iMarginY);
            }
                //pbGameCoverMax.BackColor = Color.FromArgb(0, 55, 55, 55);
            
            bigCoverTitle.Text = GetTitle(gameinfo);
            bigCoverTitle.Font = new Font("Ubuntu", 24, FontStyle.Bold);            
            bigCoverTitle.Location = new Point(iMargeXPictureBoxImage + (plGameInfo.Width - pbGameCoverMax.Width) / 2, pbGameCoverMax.Bottom - iMargeYPictureBoxImage + iMarginY);
            bigCoverTitle.Width = plGameInfo.Width - iMargeXPictureBoxImage * 2;
            using (Graphics g = CreateGraphics())
            {
                bigCoverTitle.Height = (int)g.MeasureString(bigCoverTitle.Text, bigCoverTitle.Font, plGameInfo.Width - iMargeXPictureBoxImage).Height + 6;
                
            }
            bigCoverTitle.BorderStyle = BorderStyle.None;
            bigCoverTitle.MouseEnter += new EventHandler(bigCoverTitle_Enter);
            bigCoverTitle.MouseLeave += new EventHandler(bigCoverTitle_Leave);
            bigCoverTitle.Click += new EventHandler(bigCoverTitle_Click);

            strDeveloper = GetDeveloper(gameinfo);
            lbDeveloper.Text = "Développeur : " + strDeveloper;
            lbDeveloper.Font = new Font("Ubuntu", 12, FontStyle.Regular);
            lbDeveloper.Location = new Point(bigCoverTitle.Left + iMarginX, bigCoverTitle.Bottom + iMarginX - 6);
            lbDeveloper.Width = plGameInfo.Width - iMarginX;
            lbDeveloper.MouseEnter += new EventHandler(bigCoverTitle_Enter);
            lbDeveloper.MouseLeave += new EventHandler(bigCoverTitle_Leave);
            lbDeveloper.Click += new EventHandler(bigCoverTitle_Click);           
            //lbDeveloper.BackColor = Color.Aquamarine;

            strPublisher = GetPublisher(gameinfo);
            lbPublisher.Text = "Éditeur : " + strPublisher;
            lbPublisher.Font = new Font("Ubuntu", 12, FontStyle.Regular);
            lbPublisher.Location = new Point(bigCoverTitle.Left + iMarginX, lbDeveloper.Bottom);
            lbPublisher.Width = plGameInfo.Width - iMarginX;
            lbPublisher.MouseEnter += new EventHandler(bigCoverTitle_Enter);
            lbPublisher.MouseLeave += new EventHandler(bigCoverTitle_Leave);
            lbPublisher.Click += new EventHandler(bigCoverTitle_Click);
            //lbPublisher.BackColor = Color.Azure;

            strReleaseYear = GetReleaseYear(gameinfo);
            lbYear.Text = "Sortie : " + strReleaseYear;
            lbYear.Font = new Font("Ubuntu", 12, FontStyle.Regular);
            lbYear.Location = new Point(bigCoverTitle.Left + iMarginX, lbPublisher.Bottom);
            lbYear.Width = plGameInfo.Width - iMarginX;
            lbYear.MouseEnter += new EventHandler(bigCoverTitle_Enter);
            lbYear.MouseLeave += new EventHandler(bigCoverTitle_Leave);
            lbYear.Click += new EventHandler(bigCoverTitle_Click);
            //lbYear.BackColor = Color.Beige;

            if (GetGenre2(gameinfo) == "")
            {
                lbGenre.Text = "Genre : " + GetGenre(gameinfo);
            }
            else
            {
                lbGenre.Text = "Genre : " + GetGenre(gameinfo) + "/" + GetGenre2(gameinfo);
            }
            lbGenre.Font = new Font("Ubuntu", 12, FontStyle.Regular);
            lbGenre.Location = new Point(bigCoverTitle.Left + iMarginX, lbYear.Bottom);
            lbGenre.Width = plGameInfo.Width - iMarginX;
            lbGenre.MouseEnter += new EventHandler(bigCoverTitle_Enter);
            lbGenre.MouseLeave += new EventHandler(bigCoverTitle_Leave);
            lbGenre.Click += new EventHandler(bigCoverTitle_Click);
            //lbGenre.BackColor = Color.Bisque;

            strSerie = GetSerie(gameinfo);
            if (strSerie != "")
            {
                if (bPreviousGameIsSerie == true)
                {
                    plGameInfo.Controls.Remove(lbSerie);
                    lbSerie = null;
                }
                lbSerie = new Label();
                lbSerie.Text = "Série : " + strSerie;
                lbSerie.Font = new Font("Ubuntu", 12, FontStyle.Regular);
                lbSerie.Location = new Point(bigCoverTitle.Left + iMarginX, lbGenre.Bottom);
                lbSerie.Width = plGameInfo.Width - iMarginX;
                lbSerie.MouseEnter += new EventHandler(bigCoverTitle_Enter);
                lbSerie.MouseLeave += new EventHandler(bigCoverTitle_Leave);
                lbSerie.Click += new EventHandler(bigCoverTitle_Click);
                plGameInfo.Controls.Add(lbSerie);
                bPreviousGameIsSerie = true;
            }
            else
            {
                bPreviousGameIsSerie = false;
                plGameInfo.Controls.Remove(lbSerie);
                lbSerie = null;
            }
           
            //lbGenre.BackColor = Color.Bisque;
    
            if (HasDLC(gameinfo) == 1)
            {
                bGameWithDLC = true;
                if (bPreviousGameHasDLC == true)
                {
                    plGameInfo.Controls.Remove(lbDLC);    
                }
                lbDLC = new Label();
                plGameInfo.Controls.Add(lbDLC);
                lbDLC.Text = "DLC :";
                lbDLC.Font = new Font("Ubuntu", 12, FontStyle.Regular);
                if (bPreviousGameIsSerie == true)
                {
                    lbDLC.Location = new Point(bigCoverTitle.Left + iMarginX, lbSerie.Bottom);
                }
                else 
                {
                    lbDLC.Location = new Point(bigCoverTitle.Left + iMarginX, lbGenre.Bottom);
                }
                    lbDLC.Width = plGameInfo.Width - iMarginX;
                //lbDLC.BackColor = Color.Beige;
                bPreviousGameHasDLC = true;

                plDlcInfo = new Panel();
                plDlcInfo.Width = pbGameCoverMax.Width - iMarginX -  iMargeXPictureBoxImage * 2;
                plDlcInfo.Height = this.ClientSize.Height- lbDLC.Bottom - cbPlatformList.Height;
                plDlcInfo.Location = new Point(bigCoverTitle.Left + iMarginX, lbDLC.Bottom);
                plDlcInfo.VerticalScroll.Maximum = 0;
                plDlcInfo.HorizontalScroll.Maximum = 0;
                plDlcInfo.AutoScroll = false;
                plDlcInfo.VerticalScroll.Visible = false;
                plDlcInfo.AutoScroll = true;
                //plDlcInfo.BackColor = Color.DarkCyan;
            }
            else if (HasDLC(gameinfo) == 0)
            {
                bGameWithDLC = false;
                plGameInfo.Controls.Remove(lbDLC);
                bPreviousGameHasDLC = false;         
            }
            ConnectionForGameInfo.Close();
            
            //connection pour obtenir les infos sur les DLC du jeu sélectionné
            ConnectionForDLCInfo = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            ConnectionForDLCInfo.Open();
            MySqlCommand DLCData = ConnectionForDLCInfo.CreateCommand();
            DLCData.CommandText = "SELECT COUNT(*) FROM (SELECT game_id, COUNT(*) FROM software WHERE fkid_game = " + bigCoverId + " GROUP BY game_id) AS numplatform";
            iDlcTableSize = Convert.ToInt32(DLCData.ExecuteScalar());
            DLCData.CommandText = "SELECT * FROM " + strSoftwareTable + " WHERE fkid_game = " + iCoverID;
            MySqlDataReader dlcinfo = DLCData.ExecuteReader();

            //placement des covers des DLC dans un panel
            pbDlcCover = new PictureBox[iDlcTableSize];
            int iDlcShelveTop = 0;
            while (dlcinfo.Read())
            {
                strCoverDLC = GetCover(dlcinfo);
                int iDlcWidth;
                int iNbDlcPerRow;
                if (i == 0)
                {
                    iDlcShelveTop = 0;
                    iNbDisplayedDlc = 0;
                }
                //chaque jaquette est une Picturebox
                pbDlcCover[i] = new PictureBox();
                pbDlcCover[i].SizeMode = PictureBoxSizeMode.Zoom;
                //récupération des tailles des boîtes
                pbDlcCover[i].Width = (int)((int)iHardwareArray[GetPlatformIndex(dlcinfo) - 1, 4] / 1);
                pbDlcCover[i].Height = (int)((int)iHardwareArray[GetPlatformIndex(dlcinfo) - 1, 5] / 1);
                pbDlcCover[i].Tag = GetTitle(dlcinfo);
                //conversion des images manquantes pour le zoom et la sélection et la jaquette par défaut -> TODO : chemins relatifs
                if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCoverDLC + ".jpg"))
                {
                    pbDlcCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCoverDLC + ".jpg");

                }
                else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/" + strCoverDLC + ".jpg"))
                {
                    var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + strCoverDLC + ".jpg");
                    CreateImage(new_mini, pbDlcCover[i].Width, pbDlcCover[i].Height, strCoverDLC);
                    pbDlcCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/" + strCoverDLC + ".jpg");
                }
                else
                {
                    if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg"))
                    {
                        pbDlcCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg");

                    }
                    else if (File.Exists("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg"))
                    {
                        var new_mini = Bitmap.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                        CreateImage(new_mini, pbDlcCover[i].Width, pbDlcCover[i].Height, "0");
                        pbDlcCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_mini/0.jpg");
                    }
                    pbDlcCover[i].Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/0.jpg");
                }
                OffsetOfImage(pbDlcCover[i]);
                iDlcWidth = pbDlcCover[i].Size.Width;
                iNbDlcPerRow = plDlcInfo.Width / iDlcWidth;
                iLeftMarginBorder = lbDLC.Left;
 
                //change de ligne si la prochaine jaquette n'a pas de place
                if (iDlcWidth > plDlcInfo.Width - iDlcTotalWidth)
                {
                    iDlcTotalWidth = 0;
                    iDlcShelveTop = pbDlcCover[i - 1].Bottom + iMarginX;
                }
                //position 1ere jaquette de chaque ligne
                if (iDlcTotalWidth == 0)
                {
                    pbDlcCover[i].Left = -iMargeXPictureBoxImage / 2;
                    //pbDlcCover[i].BackColor = Color.Blue;
                }
                //position des jaquettes suivantes
                else
                {
                    pbDlcCover[i].Left = (int)((pbDlcCover[i - 1].Left) + (pbDlcCover[i - 1].Width) - iMargeXPictureBoxImage);
                    //pbDlcCover[i].BackColor = Color.Yellow;
                }
                // picturebox settings
                pbDlcCover[i].Top = iDlcShelveTop;
                pbDlcCover[i].Name = GetCover(dlcinfo);
                pbDlcCover[i].Show();
                pbDlcCover[i].BringToFront();
                pbDlcCover[i].MouseEnter += new System.EventHandler(this.pbDlcCover_Enter);
                pbDlcCover[i].MouseLeave += new System.EventHandler(this.pbDlcCover_Leave);
                plDlcInfo.Controls.Add(pbDlcCover[i]);
                //mise à jour des valeurs
                iNbDisplayedDlc = iNbDisplayedDlc + 1;
                iDlcTotalWidth = iDlcTotalWidth + iDlcWidth - iMargeXPictureBoxImage;
                i = i + 1;
            }
            ConnectionForDLCInfo.Close();
            //add info to panel
            plGameInfo.Controls.Add(bigCoverTitle);            
            plGameInfo.Controls.Add(lbDeveloper);
            plGameInfo.Controls.Add(lbPublisher);
            plGameInfo.Controls.Add(lbYear);
            plGameInfo.Controls.Add(lbGenre);

            if (bGameWithDLC == true)
            {
                plGameInfo.Controls.Add(plDlcInfo);
            }
            plGameInfo.Controls.Add(pbGameCoverMax);
         
            plGameInfo.Location = new Point(this.ClientSize.Width - plGameInfo.Width + 1, cbPlatformList.Height);
            plGameList.Width = this.ClientSize.Width - plGameInfo.Width;
            plGameList.Height = this.ClientSize.Height - cbPlatformList.Height - plFooter.Height;
            plFooter.Location = new Point(0, plGameList.Height + cbPlatformList.Height);

            bAlreadyClick = true;
        }

        public void RemoveGames(int iCoverIndex, int iCoverID)
        {
            if (plGameList != null)
            {
                plGameList.Dispose();
                plGameList = null;
            }
            if (plGameInfo != null)
            {
                plGameInfo.Dispose();
                plGameInfo = null;
            }
            if (lbStatistics != null)
            {
                lbStatistics.Dispose();
                lbStatistics = null;
            }
            fFormWidth = this.ClientSize.Width;
            if (bHasRefresh == false)
            {
                iCoverIndex = 0;
            }
            DisplayLibrary(strPlatformFilter, bInitDone, iCoverIndex, iCoverID);
        }
 
        private void cbPlatformList_SelectedIndexChanges(object sender, System.EventArgs e)
        {
            var value = (ComboBox)sender;
            if (cbPlatformList.Text == "All Platforms")
            {
                strPlatformFilter = "IS NOT NULL";
            }
            else
            {
                strPlatformFilter = cbPlatformList.Text;
            }

            if (cbPlatformList.Text == "All Platforms")
            {
                iShelveHeight = iBoxArtDvdY + iMarginY;
            }
            else
            {
                iShelveHeight = (int)iHardwareArray[cbPlatformList.SelectedIndex, 5] + iMarginY;
            }
            
            cbPlatformList.Text = strPlatformFilter;
            iHardwareSelected = cbPlatformList.SelectedIndex;
            iSelectedCoverIndex = 0;
            //strbtSearch = "";
            bSearchMode = false;

            RemoveGames(iCoverIndex, iCoverID);
        }

        private void plGameList_Click(object sender, MouseEventArgs e)
        {
            string MouseButton = "";
            switch (e.Button)
            {
                case (MouseButtons.Left):
                    MouseButton = "Left_Click";
                    break;
                case (MouseButtons.Middle):
                    MouseButton = "Middle_Click";
                    break;
                case (MouseButtons.Right):
                    MouseButton = "Right_Click";
                    System.Windows.Forms.ContextMenuStrip contextMenu1;
                    contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
                    
                    ToolStripItem item1 = contextMenu1.Items.Add("Sort by name");
                    item1.Click += new EventHandler(menuItem_Click);
                    item1.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    item1.Image = Bitmap.FromFile("D:\\Documents\\GitHub\\GameLibrary\\MyGames\\icon_sortalph.png");

                    ToolStripItem item2 = contextMenu1.Items.Add("Sort by date");
                    item2.Click += new EventHandler(menuItem_Click);
                    item2.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    item2.Image = Bitmap.FromFile("D:\\Documents\\GitHub\\GameLibrary\\MyGames\\icon_sortnum.png");
                    if (iItemSelected == 1)
                    {
                        ((ToolStripMenuItem)item1).Checked = true;
                        ((ToolStripMenuItem)item2).Checked = false;
                    }
                    else
                    {
                        ((ToolStripMenuItem)item1).Checked = false;
                        ((ToolStripMenuItem)item2).Checked = true;
                    }

                    plGameList.ContextMenuStrip = contextMenu1;   
                    break;
                case (MouseButtons.XButton1):
                    MouseButton = "XButton1_Click"; //Previous
                    break;
                case (MouseButtons.XButton2):
                    MouseButton = "XButton2_Click"; //Next
                    break;                 
        }
        Console.WriteLine(MouseButton);   
        }
        
         private void pbGameCoverZoom_Click(object sender, MouseEventArgs e)
        {
            string MouseButton = "";
            switch (e.Button)
            {
                case (MouseButtons.Left):
                    MouseButton = "Left_Click";
                    break;
                case (MouseButtons.Middle):
                    MouseButton = "Middle_Click";
                    break;
                case (MouseButtons.Right):
                    MouseButton = "Right_Click";
                    System.Windows.Forms.ContextMenuStrip contextMenu2;
                    contextMenu2 = new System.Windows.Forms.ContextMenuStrip();
                    System.Windows.Forms.MenuItem menuItem1;
                    contextMenu2.Items.Add("Edit***");
                    contextMenu2.Items.Add("Delete***");
                    plGameList.ContextMenuStrip = contextMenu2;
                    break;
                case (MouseButtons.XButton1):
                    MouseButton = "XButton1_Click"; //Previous
                    break;
                case (MouseButtons.XButton2):
                    MouseButton = "XButton2_Click"; //Next
                    break;
            }   
        }
       
        public void menuItem_Click(object sender, System.EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            if (clickedItem.Text == "Sort by name")
            {
                strPlatformSort = "title";
                iItemSelected = 1;
            }
            else
            {
                strPlatformSort = "release_year";
                iItemSelected =  2;
            }          
            RemoveGames(iCoverIndex, iCoverID);
        }

        private void btAddNewGame_Click(object sender, System.EventArgs e)
        {
            NewGameTitle = new TextBox();
            NewGameTitle.KeyDown += new KeyEventHandler(btAddNewGame_Validation);
            NewGameTitle.Location = new Point(cbPlatformList.Width + SortList.Width + btSearch.Width + btAddNewGame.Width, 0);
            NewGameTitle.Text = "Title";
        }

        private void btAddNewGame_Validation(object sender, KeyEventArgs e)
        {
            //var text = (TextBox)sender;
            //if (e.KeyCode == Keys.Enter)
            //{
            //    strGameToAdd = text.Text;
            //    ConnectionToAddNewGame = new MySqlConnection("Server=" + strServer + ";" + "Uid=" + strUser + ";" + "Pwd=" + strPassword + ";" + "Database=" + strDatabase + ";");
            //    ConnectionToAddNewGame.Open();
            //    MySqlCommand cmd = ConnectionToAddNewGame.CreateCommand();
            //    cmd.CommandText = "INSERT INTO games.test (title) VALUES ('"+strGameToAdd+"');";
            //    cmd.ExecuteNonQuery();
            //    ConnectionToAddNewGame.Close();
            //    NewGameTitle.Dispose();
            //    NewGameTitle = null;
            //}
        }

        private void btSearch_Click(object sender, System.EventArgs e)
        {
            //strbtSearch = "'%"+"Plate-forme"+"%'";
            //strPlatformFilter = "IS NOT NULL";
            bSearchMode = true;
            tbSearch.Show();
            tbSearch.Focus();            
            btCancel.Show();
            chbTitle.Show();
            chbDev.Show();
            chbPublisher.Show();
            chbGenre.Show();
            chbSerie.Show();
            btSearch.Hide();
            btAddNewGame.Hide();
            lbStatistics.Hide();
        }

        private void btCancel_Click(object sender, System.EventArgs e)
        {
            //strbtSearch = "'%"+"Plate-forme"+"%'";
            //strPlatformFilter = "IS NOT NULL";
            bSearchMode = false;
            RemoveGames(iCoverIndex, iCoverID);
            
            //bSearchMode = false;
            //tbSearch.Hide();
            //btCancel.Hide();
            //chbTitle.Hide();
            //chbDev.Hide();
            //chbPublisher.Hide();
            //chbGenre.Hide();
            //chbSerie.Hide();
            //btSearch.Show();
            //btAddNewGame.Show();
            //lbStatistics.Show();
            //if (plFooter.Width < lbStatistics.Width + btSearch.Width + btAddNewGame.Width)
            //{
            //    lbStatistics.Hide();
            //}
        }

        int CoverNewLocX;
        int CoverNewLocY;
        private void pbDlcCover_Enter(object sender, System.EventArgs e)
        {
            var name = (PictureBox)sender;

            lbDLC.Text = "DLC : " + name.Tag;
        }
        private void pbDlcCover_Leave(object sender, System.EventArgs e)
        {
            lbDLC.Text = "DLC : ";
        }
        private void GameCover_Enter(object sender, System.EventArgs e)
        {    
            var cover = (PictureBox)sender;
            strCover = cover.Name;
            iCoverIndex = cover.TabIndex;
            iCoverOriginalX = cover.Location.X;
            iCoverOriginalY = cover.Location.Y;
            iCoverOriginalWidth = cover.Size.Width;
            iCoverOriginalHeight = cover.Size.Height;

            iSelectedCoverPosX = iCoverOriginalX;
            iSelectedCoverPosY = iCoverOriginalY + iCoverOriginalHeight;
            iSelectedCoverWidth = iCoverOriginalWidth;

            int CoverNewWidth = (int)(cover.Width * fScaleFactor);
            int CoverNewHeight = (int)(cover.Height * fScaleFactor);

            CoverNewLocX = (int)(iCoverOriginalX - ((CoverNewWidth - iCoverOriginalWidth) / 2));
            CoverNewLocY = (int)(iCoverOriginalY - ((CoverNewHeight - iCoverOriginalHeight) / 2));
            
            pbGameCoverZoom = new PictureBox();
            pbGameCoverZoom.SizeMode = PictureBoxSizeMode.StretchImage;
            pbGameCoverZoom.MouseLeave += new System.EventHandler(this.GameCoverZoom_Leave);
            pbGameCoverZoom.MouseDown += new MouseEventHandler(this.pbGameCoverZoom_Click);
            pbGameCoverZoom.MouseDown += new MouseEventHandler(this.GameCoverZoom_Click);
            try
            {
                pbGameCoverZoom.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_zoom/" + strCover + ".jpg");
            }
            catch
            {
                pbGameCoverZoom.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/covers_original/" + "0" + ".jpg");
            }
            pbGameCoverZoom.Height = CoverNewHeight;
            pbGameCoverZoom.Width = CoverNewWidth;
            pbGameCoverZoom.BackColor = Color.FromArgb(255,255,255,255);
            pbGameCoverZoom.Location = new Point(CoverNewLocX, CoverNewLocY);
            plGameList.Controls.Add(pbGameCoverZoom);
            pbGameCoverZoom.BringToFront();

            //selector position management
            if (iSelectedCoverIndex == iCoverIndex)
            {
                pbGameSelected.Width = (int)((pbGameCover[iCoverIndex].Width + iSelectedCoverSize) * fScaleFactor);
                pbGameSelected.Height = (int)((pbGameCover[iCoverIndex].Height + iSelectedCoverSize) * fScaleFactor);
                pbGameSelected.Location = new Point(CoverNewLocX - iSelectedCoverSize / 2, CoverNewLocY - iSelectedCoverSize / 2);
            }
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
            if (pbGameCoverZoom == null)
            {
                return;
            }
            else
            {
                pbGameCoverZoom.Dispose();
                // pbGameCoverZoom = null;
            }
            //selector position management
            if (iSelectedCoverIndex == iCoverIndex)
            {
                pbGameSelected.Width = (int)(pbGameCover[iCoverIndex].Width + iSelectedCoverSize);
                pbGameSelected.Height = (int)(pbGameCover[iCoverIndex].Height + iSelectedCoverSize);
                pbGameSelected.Location = new Point(pbGameCover[iCoverIndex].Location.X - iSelectedCoverSize / 2, pbGameCover[iCoverIndex].Location.Y - iSelectedCoverSize / 2);               
            }        
        }
        
        public void GameCoverZoom_Click(object sender, System.EventArgs e)
        {
            iSelectedCoverIndex = iCoverIndex;
            var zoomCover = (PictureBox)sender;
            //iCoverID =  (zoomCover.TabIndex); TO DELETE
            if (bAlreadyClick == true)
            {
                Remove_bigCover(plGameInfo);
            }
            //selector position management
            pbGameSelected.Width = (int)((pbGameCover[iCoverIndex].Width + iSelectedCoverSize) * fScaleFactor);
            pbGameSelected.Height = (int)((pbGameCover[iCoverIndex].Height + iSelectedCoverSize) * fScaleFactor);
            pbGameSelected.Location = new Point(CoverNewLocX - iSelectedCoverSize / 2, CoverNewLocY - iSelectedCoverSize / 2);

            DisplayInfo(Int32.Parse(strCover));
        }

        public void Remove_bigCover(Panel plGameInfo)
        {
            pbGameCoverMax.Dispose();
            pbGameCoverMax = null;
            bigCoverTitle.Dispose();
            bigCoverTitle = null;
            lbDeveloper.Dispose();
            lbDeveloper = null;
            lbPublisher.Dispose();
            lbPublisher = null;
            lbYear.Dispose();
            lbYear = null;
            lbGenre.Dispose();
            lbGenre = null;
            if (bGameWithDLC == true)
            {
                plDlcInfo.Dispose();
                plDlcInfo = null;
            }
        }

        private void bigCoverTitle_Enter(object sender, System.EventArgs e)
        {
            var info = (Label)sender;
            pbEditIcon = new PictureBox();
            pbEditIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pbEditIcon.Image = Image.FromFile("D:/Documents/GitHub/GameLibrary/MyGames/icon_edit.png");
            pbEditIcon.Width = 12;
            pbEditIcon.Height = 12;
            pbEditIcon.Location = new Point(info.Location.X - pbEditIcon.Width, info.Location.Y + (info.Height - pbEditIcon.Height) / 2);
            //pbEditIcon.BackColor = Color.Green;

            plGameInfo.Controls.Add(pbEditIcon);
        
        }
        private void bigCoverTitle_Leave(object sender, System.EventArgs e)
        {
            pbEditIcon.Dispose();
            pbEditIcon = null;
        }
        
        private void tbSearch_Validation(object sender, KeyEventArgs e)
        {           
            if (e.KeyCode == Keys.Enter)
            {
                bSearchMode = true;
                strbtSearch = tbSearch.Text ;
                strDefaultSearch = strbtSearch;
                if(chbTitle.Checked == true)
                {
                    strSearchTitle = "title LIKE '%" + strbtSearch + "%'";
                }
                else
                {
                    strSearchTitle = "title LIKE ''";
                }
                if(chbDev.Checked == true)
                {
                    strSearchDev = "developer LIKE '%" + strbtSearch + "%'";
                }
                else
                {
                    strSearchDev = "developer LIKE ''";
                }
                if(chbPublisher.Checked == true)
                {
                    strSearchPublisher = "publisher LIKE '%" + strbtSearch + "%'";
                }
                else
                {
                    strSearchPublisher = "publisher LIKE ''";
                }
                if(chbGenre.Checked == true)
                {
                    strSearchGenre = "genre LIKE '%" + strbtSearch + "%' OR subgenre LIKE '%" + strbtSearch + "%'";
                }
                else
                {
                    strSearchGenre = "genre LIKE '' OR subgenre LIKE ''";
                }
                if(chbSerie.Checked == true)
                {
                    strSearchSerie = "serie LIKE '%" + strbtSearch + "%'";
                }
                else
                {
                    strSearchSerie = "serie LIKE ''";
                }

                RemoveGames(iCoverIndex, iCoverID);
                //bSearchInProgress = false;
                //suppression du son d'alerte windows
                e.Handled = true;
                e.SuppressKeyPress = true;               
            }
        }
        private void bigCoverTitle_Click(object sender, System.EventArgs e)
        {
          
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("Etes-vous certain de vouloir quitter ?", "Quitter", MessageBoxButtons.YesNo) == DialogResult.No)
            //    e.Cancel = true;
            if (bSearchMode == false)
            {
                Settings.Default.Platform_Index = iHardwareSelected;
                Settings.Default.Platform_Name = cbPlatformList.Text;
                Settings.Default.Game_Index = iSelectedCoverIndex;
                Settings.Default.Save();
                Settings.Default.Reload();
            }

        }
        //Resize is always called when ResizeEnd is called, so add a flag to detect end of resize
        bool bResizeInProgress = false;
        bool bHasRefresh = false;
        private void Refresh(object sender, System.EventArgs e)
        {
            if (bResizeInProgress || bInitDone == false)
            {
                bInitDone = true;
                return;
            }
            else
            {
                bHasRefresh = true;
                RemoveGames(iCoverIndex, iCoverID);
                bHasRefresh = false;
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
        public string GetPlatformsName(MySqlDataReader platforms, int iColumn)
        {
            return platforms.GetString(iColumn);
        }
        public int GetID(MySqlDataReader gameinfo)
        {
            return Int32.Parse(GetLocalized(gameinfo, 0));
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
        public string GetSerie(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 10);
        } 
        public string GetCover(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 15);
        }       
        public string GetMainGame_ForDLC(MySqlDataReader fullbase)
        {
            return GetLocalized(fullbase, 14);
        }
        public int HasDLC(MySqlDataReader fullbase)
        {
            return Int32.Parse(GetLocalized(fullbase, 16));
        }
        public int GetHardwareID(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 0));
        }
        public string GetHardwareName(MySqlDataReader fullbasehardware2)
        {
            return GetLocalized(fullbasehardware2, 1);
        }  
        public string GetManufacturer(MySqlDataReader fullbasehardware2)
        {
            return GetLocalized(fullbasehardware2, 2);
        }  
        public int GetPlatformRelease(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 3));
        }  
        public int GetBoxeSizeX(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 4));
        }    
        public int GetBoxeSizeY(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 5));
        }
        public int GetHardwareColorRed(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 6));
        }
        public int GetHardwareColorGreen(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 7));
        }
        public int GetHardwareColorBlue(MySqlDataReader fullbasehardware2)
        {
            return Int32.Parse(GetLocalized(fullbasehardware2, 8));
        }
        public string GetOnlineService(MySqlDataReader fullbasehardware2)
        {
            return GetLocalized(fullbasehardware2, 9);
        }  
        public string GetMedia(MySqlDataReader fullbasehardware2)
        {
            return GetLocalized(fullbasehardware2, 10);
        }  

        // Allow Combo Box to center aligned
        private void cbPlatformList_DrawItem(object sender, DrawItemEventArgs e)
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
        
        //donne les paramètres pour la position de l'image dans la picturebox
        public void OffsetOfImage(PictureBox pbox)
        {
            //calculer les taux d'étirement/compression de l'image
            float fXRatio = 1f;
            float fYRatio = 1f;

            if (pbox.SizeMode == PictureBoxSizeMode.Zoom)
            {
                float a = (float)pbox.Height / (float)pbox.Image.Height;
                float b = (float)pbox.Width / (float)pbox.Image.Width;
                fXRatio = Math.Min(a, b);
                fYRatio = fXRatio;
            }
            //calculer la taille de l'image affichée
            Size imgs = new Size((int)(pbox.Image.Width * fXRatio), (int)(pbox.Image.Height * fYRatio));
            //calculer les différences entre l'image et le picturebox
            iMargeXPictureBoxImage = (int)((pbox.Width - pbox.Image.Width * fXRatio) / 2);
            iMargeYPictureBoxImage = (int)((pbox.Height - pbox.Image.Height * fYRatio) / 2);

            ////min est le point minimum le plus haut à gauche de l'image
            //min.X = diffx;
            //min.Y = iMargeYPictureBoxImage;
            ////max est le point le plus bas à droite de l'image
            //max.X = (int)(diffx + imgs.Width);
            //max.Y = (int)(iMargeYPictureBoxImage + imgs.Height);
        }

        void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            var info = (ToolTip)sender;
            //string test;
            //test = (string)info.Tag;
            // on popup set the size of tool tip
            e.ToolTipSize = TextRenderer.MeasureText((string)info.Tag, new Font("Ubuntu", 8, FontStyle.Regular));
        }

        void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            var info = (ToolTip)sender;
            Font f = new Font("Ubuntu", 8, FontStyle.Regular);
            e.DrawBackground();
            e.Graphics.DrawString((string)info.Tag, f, Brushes.Black, new PointF(2, 2));
           
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