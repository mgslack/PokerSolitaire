using System.Collections.Generic;
using PlayingCards;

/*
 * Engine to do the Poker scoring and chosing for the Poker Solitaire game.
 * Scores for hands are (using American scoring):
 *  Royal Flush      100
 *  Straight Flush   75
 *  Four of a Kind   50
 *  Full House       25
 *  Flush            20
 *  Straight         15
 *  Three of a Kind  10
 *  Two Pair         5
 *  One Pair         2
 *  
 * The hinting is fairly basic.  Will select pos if can get straight flush
 * with hand, can get 4 of a kind, can get full house, flush, straight, 3-of-kind
 * or pair(s).  If filling up hands, will pick the spot that may get one of those
 * combinations if cards line up properly.
 * 
 * Notes: 2021-12-27
 * In an attempt to get better hinting, added a new hint method to the engine
 * to see if I can get auto-played game scoring closer to 100+ points.  Have two
 * methods built in now, one uses each hand can fill in on game board and scores
 * with current card (original method).  Second scores each card position on game
 * board and scores the sums each hand that card can belong to.
 * Original method: 25 games, scores 26 - 125 points, avg score/game = 54.49 pts.
 * Slot method: 25 games, scores 8 - 101 points, avg score/game = 58.24 pts.
 * Neither method does a real good job, though the original had more games over
 * 100 points (2) versus only one over 100 point game for slot scoring.  The slot
 * method ends up with wider variance in scores, scewed on toward the small end,
 * but higher average score.  Thinking of other methods to determine hint and will
 * add them as developed, and will add option to main game to select between them.
 * Seems like scoring each position should lead to larger scores, and it sort of
 * does, but the upper bound is lower than original hand scoring.  The by slot
 * method should be better, but the by hand method gives higher bound scores.
 * 
 * Author: Michael G. Slack
 * Date Written: 2021-12-13
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: 2021-12-26 - Had wrong score value for royal flush, leftover from
 *                       different score values for hands.
 *          2021-12-27 - Added a different hint selection routine rather than
 *                       original created.  New routine selects based on score
 *                       of each card position on board rather than on each hand
 *                       on board with new card added to it.  Left both methods
 *                       in, setup a property to select the method to use.
 *          2021-12-28 - Added ability in main form to configure and control the
 *                       hint method used.
 *          2022-01-13 - Added MAX_NUM_CARDS const to replace default max const
 *                       from card hand.
 * 
 */
namespace PokerSolitaire
{
    public enum HintMethod { ByHand, BySlot };

    class PokerEng
    {
        #region Constants
        public const int MAX_NUM_CARDS = 5; // max num cards in a hand

        private const int NUM_CARD_VALS = 13;
        private const int MAX_SLOT_HAND = 4;
        private const int ROYAL_FLUSH_PTS = 100;
        private const int STRAIGHT_FLUSH_PTS = 75;
        private const int FOUR_KIND_PTS = 50;
        private const int FULL_HOUSE_PTS = 25;
        private const int FLUSH_PTS = 20;
        private const int STRAIGHT_PTS = 15;
        private const int THREE_KIND_PTS = 10;
        private const int TWO_PAIR_PTS = 5;
        private const int PAIR_PTS = 2;
        private const int PARTIAL_FLUSH_PTS = 4;
        private const int PARTIAL_STRAIGHT_PTS = 3;
        #endregion

        #region Private statics
        private static readonly int[,] slotToHandIdx = { { 0, 5, 10, -1 }, { 0, 6, -1, -1 },
                                                         { 0, 7, -1, -1 }, { 0, 8, -1, -1 },
                                                         { 0, 9, 11, -1 }, { 1, 5, -1, -1 },
                                                         { 1, 6, 10, -1 }, { 1, 7, -1, -1 },
                                                         { 1, 8, 11, -1 }, { 1, 9, -1, -1 },
                                                         { 2, 5, -1, -1 }, { 2, 6, -1, -1 },
                                                         { 2, 7, 10, 11 }, { 2, 8, -1, -1 },
                                                         { 2, 9, -1, -1 }, { 3, 5, -1, -1 },
                                                         { 3, 6, 10, -1 }, { 3, 7, -1, -1 },
                                                         { 3, 8, 11, -1 }, { 3, 9, -1, -1 },
                                                         { 4, 5, 11, -1 }, { 4, 6, -1, -1 },
                                                         { 4, 7, -1, -1 }, { 4, 8, -1, -1 },
                                                         { 4, 9, 10, -1 } };
        #endregion

        #region Properties
        private HintMethod _hintMethod = HintMethod.ByHand;
        public HintMethod HintMethod { get => _hintMethod; set => _hintMethod = value; }
        #endregion

        // --------------------------------------------------------------------

        #region Private Scoring methods
        private bool HaveFlush(CardHand hand)
        {
            bool have = true;

            for (int i = CardHand.NEXT; i < MAX_NUM_CARDS; i++)
                if (hand.CardAt(i-1).Suit != hand.CardAt(i).Suit)
                {
                    have = false; i = MAX_NUM_CARDS;
                }

            return have;
        }

        private bool RoyalStraight(CardHand hand)
        {
            int idx = CardHand.FIRST;

            return hand.CardAt(idx++).CardValue == CardValue.King &&
                hand.CardAt(idx++).CardValue == CardValue.Queen &&
                hand.CardAt(idx++).CardValue == CardValue.Jack &&
                hand.CardAt(idx++).CardValue == CardValue.Ten &&
                hand.CardAt(idx).CardValue == CardValue.Ace;
        }

        private bool Straight(CardHand hand)
        {
            bool sFlg = true;
            int cVal = hand.CardAt(CardHand.FIRST).GetCardPointValue();

            for (int i = CardHand.NEXT; i < MAX_NUM_CARDS; i++)
            {
                cVal--;
                if (cVal != hand.CardAt(i).GetCardPointValue())
                {
                    sFlg = false; i = MAX_NUM_CARDS;
                }
            }

            return sFlg;
        }

        private bool HaveStraight(CardHand hand)
        {
            return RoyalStraight(hand) || Straight(hand);
        }

        private int HaveMultiples(CardHand hand)
        {
            int[] chkCnts = new int[NUM_CARD_VALS];
            int score = 0;
            bool havePair1 = false, havePair2 = false, haveThree = false, haveFour = false;

            for (int i = 0; i < NUM_CARD_VALS; i++) chkCnts[i] = 0;

            for (int i = CardHand.FIRST; i < MAX_NUM_CARDS; i++)
            {
                int crdValIdx = hand.CardAt(i).GetCardPointValue() - 1;
                if (crdValIdx >= 0) chkCnts[crdValIdx]++;
            }

            for (int i = 0; i < NUM_CARD_VALS; i++)
            {
                if (chkCnts[i] == 2)
                {
                    if (havePair1) havePair2 = true; else havePair1 = true; // can have up to 2
                }
                else if (chkCnts[i] == 3)
                {
                    haveThree = true; // can only be one
                }
                else if (chkCnts[i] == 4)
                {
                    haveFour = true; // can only be one
                }
            }

            if (havePair1)
            {
                if (havePair2)
                    score = TWO_PAIR_PTS;
                else if (haveThree)
                    score = FULL_HOUSE_PTS;
                else
                    score = PAIR_PTS;
            }
            else if (haveThree)
            {
                score = THREE_KIND_PTS;
            }
            else if (haveFour)
            {
                score = FOUR_KIND_PTS;
            }

            return score;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).
         */
        private bool HavePartialFlush(CardHand hand)
        {
            bool have = true;
            Suit suit = hand.CardAt(CardHand.FIRST).Suit;

            for (int i = CardHand.NEXT; i < hand.CurNumCardsInHand; i++)
            {
                if (hand.CardAt(i).Suit != suit) have = false;
            }

            return have;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).
         * - sorted in largest to smallest card (king downto ace).
         */
        private bool HavePartialStraight(CardHand hand)
        {
            int bVal = hand.CardAt(CardHand.FIRST).GetCardPointValue();
            int eVal = hand.CardAt(hand.CurNumCardsInHand - 1).GetCardPointValue();
            int numCards = hand.CurNumCardsInHand;
            bool have = (bVal - eVal) < MAX_NUM_CARDS;

            if (!have)
            {
                // check for royal straight...
                if (eVal == 1 && bVal >= 10)
                {
                    if (numCards == 2)
                    {
                        have = true;
                    }
                    else
                    {
                        int mVal = hand.CardAt(CardHand.NEXT).GetCardPointValue();
                        if (mVal >= 10)
                        {
                            // potential royal straight, ignore trailing ace in next loop
                            have = true; numCards--;
                        }
                    }
                }
            }

            if (have)
            {
                if (hand.CurNumCardsInHand > 2)
                {
                    for (int i = CardHand.NEXT; i < numCards; i++)
                        if (bVal > hand.CardAt(i).GetCardPointValue())
                            bVal = hand.CardAt(i).GetCardPointValue();
                        else
                            have = false;
                }
            }

            return have;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).
         */
        private int ScorePartialHand(CardHand hand)
        {
            int score = 0;

            // only score partial hands, fulls hands already scored
            if (hand.CurNumCardsInHand > 1 && hand.CurNumCardsInHand < MAX_NUM_CARDS)
            {
                if (HavePartialFlush(hand)) score = PARTIAL_FLUSH_PTS * hand.CurNumCardsInHand;
                if (HavePartialStraight(hand)) score += PARTIAL_STRAIGHT_PTS * hand.CurNumCardsInHand;
            }

            return score;
        }
        #endregion

        // --------------------------------------------------------------------

        #region Private Hint/Auto-play methods
        /*
         * Used to determine where to play next card (hint or auto-play).  Hand method.
         */
        private int[] CalcAllScores(PlayingCard[] hands, int[,] handIdx, int maxHands, PlayingCard currentCard)
        {
            int[] scores = new int[maxHands];
            int multiplier = 1;
            CardHand chkHand = new CardHand(MAX_NUM_CARDS); // 5 cards, sorted order (largest to smallest, K - A)

            for (int i = 0; i < maxHands; i++)
            {
                for (int j = CardHand.FIRST; j < MAX_NUM_CARDS; j++)
                {
                    PlayingCard crd = hands[handIdx[i, j]];
                    if (crd != PlayingCard.EMPTY_CARD) chkHand.Add(crd);
                }
                if (chkHand.CurNumCardsInHand < MAX_NUM_CARDS)
                    chkHand.Add(currentCard);
                else
                    multiplier = -1;
                scores[i] = ScoreHand(chkHand, true) * multiplier; // previous full hands get scored, but negated
                if (scores[i] >= 0) scores[i] += ScorePartialHand(chkHand);
                chkHand.RemoveAll(); multiplier = 1;
            }

            return scores;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).  Hand method.
         */
        private int SelectHandWithHighestScore(int[] scores, int maxHands)
        {
            int hand = -1, score = 0;
            
            for (int i = 0; i < maxHands; i++)
            {
                if (scores[i] > score)
                {
                    hand = i; score = scores[i];
                }
            }
            // only hand, or more than one?
            if (hand >= 0)
            {
                List<int> list = new List<int> { hand };
                
                for (int i = 0; i < maxHands; i++)
                    if (score == scores[i] && hand != i) list.Add(i);
                if (list.Count > 1)
                {
                    // randomly select one (more than one)
                    int idx = SingleRandom.Instance.Next(list.Count);
                    hand = list[idx];
                }
            }

            return hand;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).  Hand and Slot method.
         */
        private int PickRandomHandSlot(PlayingCard[] hands, int maxCards)
        {
            int slot = -1;

            while (slot == -1)
            {
                int pick = SingleRandom.Instance.Next(maxCards);
                if (hands[pick] == PlayingCard.EMPTY_CARD) slot = pick;
            }

            return slot;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).
         * Original hint method.  Adds card to each hand on game board (12 of them) and determines
         * highest score of the hand with the card added to it.  Returns a slot of the highest
         * scoring hand as the hint index.
         */
        private int GetHintByHand(PlayingCard[] hands, int maxCards, int[,] handIdx, int maxHands,
            PlayingCard currentCard)
        {
            int hintIdx;
            int[] scores = CalcAllScores(hands, handIdx, maxHands, currentCard);
            int hand = SelectHandWithHighestScore(scores, maxHands);

            if (hand < 0)
            {
                hintIdx = PickRandomHandSlot(hands, maxCards);
            }
            else
            {
                PlayingCard[] hnd = new PlayingCard[MAX_NUM_CARDS];
                for (int i = 0; i < MAX_NUM_CARDS; i++)
                {
                    hnd[i] = hands[handIdx[hand, i]];
                }
                // pick random slot in highest point hand that is open
                hintIdx = handIdx[hand, PickRandomHandSlot(hnd, MAX_NUM_CARDS)];
            }

            return hintIdx;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).  Slot method.
         */
        private int[] CalcAllSlotScores(PlayingCard[] hands, int maxCards, int[,] handIdx,
            PlayingCard currentCard)
        {
            int[] scores = new int[maxCards];
            CardHand chkHand = new CardHand(MAX_NUM_CARDS); // 5 cards, sorted order (largest to smallest, K - A)

            // determine score for each slot
            for (int i = 0; i < maxCards; i++)
            {
                scores[i] = -1;
                // sum scores of each hand contained in slot, if slot is empty
                if (hands[i] == PlayingCard.EMPTY_CARD)
                {
                    for (int j = 0; j < MAX_SLOT_HAND; j++)
                    {
                        int hndIdx = slotToHandIdx[i, j];
                        if (hndIdx >= 0)
                        {
                            for (int k = CardHand.FIRST; k < MAX_NUM_CARDS; k++)
                            {
                                PlayingCard crd = hands[handIdx[hndIdx, k]];
                                if (crd != PlayingCard.EMPTY_CARD) chkHand.Add(crd);
                            }
                            if (chkHand.CurNumCardsInHand < MAX_NUM_CARDS)
                            {
                                if (scores[i] < 0) scores[i] = 0;
                                chkHand.Add(currentCard);
                                scores[i] += ScoreHand(chkHand, true) + ScorePartialHand(chkHand);
                                chkHand.RemoveAll();
                            }
                        }
                    }
                }
            }

            return scores;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).  Slot method.
         */
        private int SelectSlotWithHighestScore(int[] scores, int maxCards)
        {
            int slot = -1, score = 0;

            for (int i = 0; i < maxCards; i++)
            {
                if (scores[i] > score)
                {
                    score = scores[i]; slot = i;
                }
            }
            // if have slot, only one?
            if (slot >= 0)
            {
                List<int> list = new List<int> { slot };

                for (int i = 0; i < maxCards; i++)
                    if (i != slot && scores[i] == score) list.Add(i);
                if (list.Count > 1)
                {
                    // randomly select one (more than one)
                    int idx = SingleRandom.Instance.Next(list.Count);
                    slot = list[idx];
                }
            }

            return slot;
        }

        /*
         * Other method used to determine hint/auto-play position.  This method scores each slot
         * in the game board (25 slots) based on each hand the slot can be part of.  Sums the
         * scores for each of those hands and selects the slot with the highest score.
         */
        private int GetHintBySlot(PlayingCard[] hands, int maxCards, int[,] handIdx, PlayingCard currentCard)
        {
            int hintIdx;
            int[] scores = CalcAllSlotScores(hands, maxCards, handIdx, currentCard);
            int slot = SelectSlotWithHighestScore(scores, maxCards);

            if (slot < 0)
                hintIdx = PickRandomHandSlot(hands, maxCards);
            else
                hintIdx = slot;

            return hintIdx;
        }
        #endregion

        // --------------------------------------------------------------------

        #region Constructor
        public PokerEng()
        {
            // empty constructor
        }
        #endregion

        // --------------------------------------------------------------------

        #region Public methods
        public int ScoreHand(CardHand hand, bool scorePartialHands)
        {
            int score = 0;

            if (scorePartialHands || hand.CurNumCardsInHand == MAX_NUM_CARDS)
            {
                // check flush, rsf, sf
                if (HaveFlush(hand))
                {
                    if (RoyalStraight(hand))
                        score = ROYAL_FLUSH_PTS;
                    else if (Straight(hand))
                        score = STRAIGHT_FLUSH_PTS;
                    else
                        score = FLUSH_PTS;
                }
                else if (HaveStraight(hand))
                {
                    score = STRAIGHT_PTS;
                }
                else
                {
                    score = HaveMultiples(hand);
                }
            }

            return score;
        }

        public int GetHint(PlayingCard[] hands, int maxCards, int[,] handIdx, int maxHands, 
            PlayingCard currentCard)
        {
            int hintIdx;

            if (_hintMethod == HintMethod.BySlot)
                hintIdx = GetHintBySlot(hands, maxCards, handIdx, currentCard);
            else
                hintIdx = GetHintByHand(hands, maxCards, handIdx, maxHands, currentCard);
            
            return hintIdx;
        }
        #endregion
    }
}
