using System;
using System.Windows.Forms;
using PlayingCards;

namespace PokerSolitaire
{
    public partial class OptionsDlg : Form
    {
        #region Properties
        private CardBacks _cardBack = CardBacks.Blue;
        public CardBacks CardBack { get { return _cardBack; } set { _cardBack = value; } }

        private PlayingCardImage _images = null;
        public PlayingCardImage Images { set { _images = value; } }
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
        #endregion
    }
}
