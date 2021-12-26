using System;
using System.Windows.Forms;
using PlayingCards;

/*
 * Defines the partial class used for the options dialog used by the Poker
 * Solitaire game.  Also defines the winning score enums used by the game.
 * Defined here because mainly used in the options dialog.
 * 
 * Author: Michael G. Slack
 * Written: 2021-12-15
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: 2021-12-26 - Added enum/options to set winning score.
 * 
 */
namespace PokerSolitaire
{
    public enum WinScores { Easy = 100, Normal = 150, Hard = 200 };

    public partial class OptionsDlg : Form
    {
        #region Properties
        private CardBacks _cardBack = CardBacks.Blue;
        public CardBacks CardBack { get { return _cardBack; } set { _cardBack = value; } }

        private PlayingCardImage _images = null;
        public PlayingCardImage Images { set { _images = value; } }

        private int _winScore = (int)WinScores.Normal;
        public int WinScore { get { return _winScore; } set { _winScore = value; } }
        #endregion

        // --------------------------------------------------------------------

        #region Private Methods
        private void LoadAndDisplayCardBacks()
        {
            int idx = 0;

            foreach (string name in Enum.GetNames(typeof(CardBacks)))
                cbImage.Items.Add(name);
            foreach (int val in Enum.GetValues(typeof(CardBacks)))
                if (val == (int)_cardBack) idx = (int)_cardBack - (int)CardBacks.Spheres;
            cbImage.SelectedIndex = idx;

            if (_images != null) pbCardBack.Image = _images.GetCardBackImage(_cardBack);
        }
        #endregion

        // --------------------------------------------------------------------

        public OptionsDlg()
        {
            InitializeComponent();
        }

        // --------------------------------------------------------------------

        #region Form event handlers
        private void OptionsDlg_Load(object sender, EventArgs e)
        {
            LoadAndDisplayCardBacks();
        }

        private void cbImage_SelectedIndexChanged(object sender, EventArgs e)
        {
            _cardBack = (CardBacks)(cbImage.SelectedIndex + (int)CardBacks.Spheres);
            if (_images != null) pbCardBack.Image = _images.GetCardBackImage(_cardBack);
        }

        private void RbEasy_CheckedChanged(object sender, EventArgs e)
        {
            _winScore = (int)WinScores.Easy;
        }

        private void RbNormal_CheckedChanged(object sender, EventArgs e)
        {
            _winScore = (int)WinScores.Normal;
        }

        private void RbHard_CheckedChanged(object sender, EventArgs e)
        {
            _winScore = (int)WinScores.Hard;
        }
        #endregion
    }
}
