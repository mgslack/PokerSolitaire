using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using CustomMessageBox;
using GameStatistics;
using PlayingCards;

/*
 * Partial class defining the main window for for the Poker Solitaire game.  Inspired
 * on a idea of a game created by Roger Smith Jr. for OS/2 in 1989.
 * - No code, just designed with same features.
 * 
 * Author: Michael Slack
 * Date Written: 2021-12-13
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: yyyy-mm-dd - xxxx.
 * 
 */
namespace PokerSolitaire
{
    public partial class MainWin : Form
    {
        #region Constants
        private const string HTML_HELP_FILE = "PokerSolitaire_help.html";
        private const int MAX_CARDS = 25;
        private const int MAX_HANDS = 12;
        private const CardPlaceholders CARD_TO_PLAY_PH = CardPlaceholders.RedX;
        private const CardPlaceholders HINT_PH = CardPlaceholders.GreenCircle;
        private const CardPlaceholders PLACE_HOLDER = CardPlaceholders.Gray;
        #endregion

        #region Registry Name and Key Consts
        private const string REG_NAME = @"HKEY_CURRENT_USER\Software\Slack and Associates\Games\PokerSolitaire";
        private const string REG_KEY1 = "PosX";
        private const string REG_KEY2 = "PosY";
        private const string REG_KEY3 = "CardBack";
        private const string REG_CS_AUTOWIN = "Highest Autoplay Score";
        #endregion

        #region Private statics
        private static readonly int[,] handIdx = { { 0,  1,  2,  3,  4  }, { 5,  6,  7,  8,  9  },
                                                   { 10, 11, 12, 13, 14 }, { 15, 16, 17, 18, 19 },
                                                   { 20, 21, 22, 23, 24 }, { 0,  5,  10, 15, 20 },
                                                   { 1,  6,  11, 16, 21 }, { 2,  7,  12, 17, 22 },
                                                   { 3,  8,  13, 18, 23 }, { 4,  9,  14, 19, 24 },
                                                   { 0,  6,  12, 18, 24 }, { 4,  8,  12, 16, 20 } };
        #endregion

        #region Private vars
        private CardBacks cardBack = CardBacks.Blue;
        private CardDeck cards = new CardDeck();
        private PlayingCard[] hands = new PlayingCard[MAX_CARDS];
        private PictureBox[] handDisp = new PictureBox[MAX_CARDS];
        private PlayingCardImage images = new PlayingCardImage();
        private Statistics stats = new Statistics(REG_NAME);
        private int currentCardNum = MAX_CARDS, hintLocation = -1, curScore = 0;
        private bool autoPlayed = false;
        private PlayingCard currentCard = PlayingCard.EMPTY_CARD;
        private PokerEng pokerEng = new PokerEng();
        #endregion

        #region Drag-n-Drop Fields (variables)
        private bool dragStarting = false;
        private bool useCustomCursors = false;
        private Cursor cardMoveCursor;
        private Cursor cardNoneCursor;
        #endregion

        // --------------------------------------------------------------------

        #region Private methods
        private void LoadFromRegistry()
        {
            int winX = -1, winY = -1, cardB = (int)CardBacks.Blue;

            try
            {
                winX = (int)Registry.GetValue(REG_NAME, REG_KEY1, winX);
                winY = (int)Registry.GetValue(REG_NAME, REG_KEY2, winY);
                cardB = (int)Registry.GetValue(REG_NAME, REG_KEY3, cardB);
            }
            catch (Exception ex) { /* ignore, go with defaults */ }

            if ((winX != -1) && (winY != -1)) this.SetDesktopLocation(winX, winY);
            if (Enum.IsDefined(typeof(CardBacks), cardB)) cardBack = (CardBacks)cardB;
        }

        private void SetupContextMenu()
        {
            ContextMenu mnu = new ContextMenu();
            MenuItem mnuStats = new MenuItem("Game Statistics");
            MenuItem mnuOptions = new MenuItem("Options");
            MenuItem sep = new MenuItem("-");
            MenuItem mnuAbout = new MenuItem("About");

            mnuStats.Click += new EventHandler(MnuStats_Click);
            mnuOptions.Click += new EventHandler(MnuOptions_Click);
            mnuAbout.Click += new EventHandler(MnuAbout_Click);
            mnu.MenuItems.AddRange(new MenuItem[] { mnuStats, mnuOptions, sep, mnuAbout });
            this.ContextMenu = mnu;
        }

        private void SetDDEventHandlers(PictureBox pb)
        {
            pb.AllowDrop = true;
            pb.MouseDown += XX_MouseDown;
            pb.GiveFeedback += XX_GiveFeedback;
            pb.DragDrop += XX_DragDrop;
            pb.DragEnter += XX_DragEnter;
            pb.QueryContinueDrag += XX_QueryContinueDrag;
        }

        private void SetupComponents()
        {
            handDisp[0] = pbCard1; handDisp[1] = pbCard2; handDisp[2] = pbCard3;
            handDisp[3] = pbCard4; handDisp[4] = pbCard5; handDisp[5] = pbCard6;
            handDisp[6] = pbCard7; handDisp[7] = pbCard8; handDisp[8] = pbCard9;
            handDisp[9] = pbCard10; handDisp[10] = pbCard11; handDisp[11] = pbCard12;
            handDisp[12] = pbCard13; handDisp[13] = pbCard14; handDisp[14] = pbCard15;
            handDisp[15] = pbCard16; handDisp[16] = pbCard17; handDisp[17] = pbCard18;
            handDisp[18] = pbCard19; handDisp[19] = pbCard20; handDisp[20] = pbCard21;
            handDisp[21] = pbCard22; handDisp[22] = pbCard23; handDisp[23] = pbCard24;
            handDisp[24] = pbCard25;
            pbCardDeck.Image = images.GetCardBackImage(cardBack);
            SetDDEventHandlers(pbCardDeck);
            pbCardToPlay.Image = images.GetCardPlaceholderImage(CARD_TO_PLAY_PH);
            SetDDEventHandlers(pbCardToPlay);
            for (int i = 0; i < MAX_CARDS; i++) SetDDEventHandlers(handDisp[i]);
        }

        private void InitializeHands()
        {
            for (int i = 0; i < MAX_CARDS; i++)
            {
                handDisp[i].Image = images.GetCardPlaceholderImage(PLACE_HOLDER);
                hands[i] = PlayingCard.EMPTY_CARD;
            }
        }

        private void DealCard()
        {
            currentCard = cards.GetNextCard();
            pbCardToPlay.Image = images.GetCardImage(currentCard);
            currentCardNum++;
        }

        private void ScoreHands()
        {
            CardHand chkHand = new CardHand(); // sorted hand (largest to smalles), 5 cards in hand

            curScore = 0;
            for (int i = 0; i < MAX_HANDS; i++)
            {
                for (int j = CardHand.FIRST; j < CardHand.DEF_MAX_NBR_CARDS; j++)
                {
                    PlayingCard crd = hands[handIdx[i, j]];
                    if (crd != PlayingCard.EMPTY_CARD) chkHand.Add(crd);
                }
                curScore += pokerEng.ScoreHand(chkHand, false);
                chkHand.RemoveAll();
            }

            lblScore.Text = "" + curScore;
        }

        private void FinishGame()
        {
            if (!autoPlayed)
            {
                MsgBox.Show(this, "You have placed all cards, your score is: " + curScore, this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxIcon.Information);
                stats.GameWon(curScore);
                lblHighScore.Text = "" + stats.HighestScore;
            }
            else
            {
                MsgBox.Show(this, "All cards have been played, auto-play score is: " + curScore, this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxIcon.Information);
                if (stats.CustomStatistic(REG_CS_AUTOWIN) < curScore)
                    stats.SetCustomStatistic(REG_CS_AUTOWIN, curScore);
                stats.GameDone();
            }
            BtnHint.Enabled = false;
            BtnAuto.Enabled = false;
        }

        private void PlaceCard(int loc)
        {
            hands[loc] = currentCard;
            handDisp[loc].Image = images.GetCardImage(currentCard);
            ScoreHands();
            if (currentCardNum < MAX_CARDS)
            {
                DealCard();
            }
            else
            {
                currentCard = PlayingCard.EMPTY_CARD;
                FinishGame();
            }
        }
        #endregion

        // --------------------------------------------------------------------

        public MainWin()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Form event handlers
        private void MainWin_Load(object sender, EventArgs e)
        {
            LoadFromRegistry();
            SetupContextMenu();
            SetupComponents();
            InitializeHands();
            stats.GameName = this.Text;
            lblHighScore.Text = "" + stats.HighestScore;
        }

        private void MainWin_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                Registry.SetValue(REG_NAME, REG_KEY1, this.Location.X);
                Registry.SetValue(REG_NAME, REG_KEY2, this.Location.Y);
            }
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            DialogResult res = DialogResult.Yes;

            if (currentCardNum < MAX_CARDS)
            {
                res = MsgBox.Show(this, "Game is currently started, quit and start new?", this.Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxIcon.Question);
                if (res == DialogResult.Yes) stats.GameDone();
            }

            if (res == DialogResult.Yes)
            {
                currentCardNum = 0;
                hintLocation = -1;
                curScore = 0; lblScore.Text = "0";
                autoPlayed = false;
                InitializeHands();
                cards.Shuffle();
                DealCard();
                BtnHint.Enabled = true;
                BtnAuto.Enabled = true;
                stats.StartGameNoGSS(true);
            }
        }

        private void BtnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetEntryAssembly();
            var asmLocation = Path.GetDirectoryName(asm.Location);
            var htmlPath = Path.Combine(asmLocation, HTML_HELP_FILE);

            try
            {
                Process.Start(htmlPath);
            }
            catch (Exception ex)
            {
                MsgBox.Show("Cannot load help: " + ex.Message, "Help Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxIcon.Error);
            }
        }

        private void BtnHint_Click(object sender, EventArgs e)
        {
            if (!autoPlayed)
            {
                hintLocation = pokerEng.GetHint(hands, MAX_CARDS, handIdx, MAX_HANDS, currentCard);
                if (hintLocation != -1 && hands[hintLocation] == PlayingCard.EMPTY_CARD)
                    handDisp[hintLocation].Image = images.GetCardPlaceholderImage(HINT_PH);
            }
        }

        private void BtnAuto_Click(object sender, EventArgs e)
        {
            bool goFlg = true;

            autoPlayed = true;
            do {
                if (currentCardNum >= MAX_CARDS) goFlg = false;
                int loc = pokerEng.GetHint(hands, MAX_CARDS, handIdx, MAX_HANDS, currentCard);
                PlaceCard(loc);
            } while (goFlg);
            pbCardToPlay.Image = images.GetCardPlaceholderImage(CARD_TO_PLAY_PH);
        }

        private void MnuStats_Click(object sender, EventArgs e)
        {
            stats.ShowStatistics(this);
        }

        private void MnuOptions_Click(object sender, EventArgs e)
        {
            OptionsDlg dlg = new OptionsDlg()
            {
                Images = images,
                CardBack = cardBack
            };

            if (dlg.ShowDialog(this) == DialogResult.OK && dlg.CardBack != cardBack)
            {
                cardBack = dlg.CardBack;
                pbCardDeck.Image = images.GetCardBackImage(cardBack);
                Registry.SetValue(REG_NAME, REG_KEY3, (int)cardBack, RegistryValueKind.DWord);
            }
            dlg.Dispose();
        }

        private void MnuAbout_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();

            about.ShowDialog(this);
            about.Dispose();
        }
        #endregion

        // --------------------------------------------------------------------

        #region Drag-n-Drop events/methods
        private void DragCreateCursors()
        {
            Bitmap card = (Bitmap)images.GetCardImage(currentCard).Clone();
            Bitmap nCard = (Bitmap)card.Clone();

            nCard.MakeTransparent(card.GetPixel(0, 0));
            cardMoveCursor = CursorUtil.CreateCursor(card, 35, 48);
            cardNoneCursor = CursorUtil.CreateCursor(nCard, 35, 48);
            useCustomCursors = ((cardMoveCursor != null) && (cardNoneCursor != null));
        }

        private void DragDisposeCursors()
        {
            useCustomCursors = false;
            if (cardMoveCursor != null) cardMoveCursor.Dispose();
            if (cardNoneCursor != null) cardNoneCursor.Dispose();
        }

        private void DragMoveComplete()
        {
            // don't need...
        }

        private void XX_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender == pbCardToPlay && currentCard != PlayingCard.EMPTY_CARD && !autoPlayed)
            {
                dragStarting = true;
                DragCreateCursors();
                if (DoDragDrop(pbCardToPlay.Image, DragDropEffects.Move) == DragDropEffects.Move)
                {
                    DragMoveComplete();
                }
                else
                {
                    DragDisposeCursors();
                    pbCardToPlay.Image = images.GetCardImage(currentCard);
                }
            }
        }

        private void XX_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (useCustomCursors)
            {
                // Sets the custom cursor based upon the effect.
                e.UseDefaultCursors = false;
                if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
                    Cursor.Current = cardMoveCursor;
                else
                    Cursor.Current = cardNoneCursor;
                if (dragStarting)
                {
                    dragStarting = false;
                    pbCardToPlay.Image = images.GetCardPlaceholderImage(CARD_TO_PLAY_PH);
                }
            }
            else
                e.UseDefaultCursors = true;
        }

        private bool CanDragTo(int currentLoc)
        {
            return (hands[currentLoc] == PlayingCard.EMPTY_CARD);
        }

        private void XX_DragEnter(object sender, DragEventArgs e)
        {
            if (sender is PictureBox && sender != pbCardToPlay && sender != pbCardDeck)
            {
                int loc = Convert.ToInt32((sender as PictureBox).Tag);
                if (CanDragTo(loc))
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void XX_DragDrop(object sender, DragEventArgs e)
        {
            if (sender is PictureBox && sender != pbCardToPlay && sender != pbCardDeck)
            {
                int loc = Convert.ToInt32((sender as PictureBox).Tag);

                if (hintLocation >= 0 && loc != hintLocation)
                {
                    handDisp[hintLocation].Image = images.GetCardPlaceholderImage(PLACE_HOLDER);
                    hintLocation = -1;
                }
                PlaceCard(loc);
            }
        }

        private void XX_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            // Cancel the drag if the mouse moves off the form.
            if ((Control.MousePosition.X < this.DesktopBounds.Left) ||
                (Control.MousePosition.X > this.DesktopBounds.Right) ||
                (Control.MousePosition.Y < this.DesktopBounds.Top) ||
                (Control.MousePosition.Y > this.DesktopBounds.Bottom))
            {
                e.Action = DragAction.Cancel;
            }
        }
        #endregion
    }
}
