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
 * The hinting is fairly basic.  Will select pos if can can get straight flush
 * with hand, can get 4 of a kind, can get full house, flush, straight, 3-of-kind
 * or pair(s).  If filling up hands, will pick the spot that may get one of those
 * combinations if cards line up properly.
 * 
 * Author: Michael G. Slack
 * Date Written: 2021-12-13
 * 
 * ----------------------------------------------------------------------------
 * 
 * Revised: 2021-12-26 - Had wrong score value for royal flush, leftover from
 *                       different score values for hands.
 * 
 */
namespace PokerSolitaire
{
    class PokerEng
    {
        #region Constants
        private const int NUM_CARD_VALS = 13;
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

        // --------------------------------------------------------------------

        #region Private methods
        private bool HaveFlush(CardHand hand)
        {
            bool have = true;

            for (int i = CardHand.NEXT; i < CardHand.DEF_MAX_NBR_CARDS; i++)
                if (hand.CardAt(i-1).Suit != hand.CardAt(i).Suit)
                {
                    have = false; i = CardHand.DEF_MAX_NBR_CARDS;
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

            for (int i = CardHand.NEXT; i < CardHand.DEF_MAX_NBR_CARDS; i++)
            {
                cVal--;
                if (cVal != hand.CardAt(i).GetCardPointValue())
                {
                    sFlg = false; i = CardHand.DEF_MAX_NBR_CARDS;
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

            for (int i = CardHand.FIRST; i < CardHand.DEF_MAX_NBR_CARDS; i++)
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
            bool have = (bVal - eVal) < CardHand.DEF_MAX_NBR_CARDS;

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
            if (hand.CurNumCardsInHand > 1 && hand.CurNumCardsInHand < CardHand.DEF_MAX_NBR_CARDS)
            {
                if (HavePartialFlush(hand)) score = PARTIAL_FLUSH_PTS * hand.CurNumCardsInHand;
                if (HavePartialStraight(hand)) score += PARTIAL_STRAIGHT_PTS * hand.CurNumCardsInHand;
            }

            return score;
        }

        /*
         * Used to determine where to play next card (hint or auto-play).
         */
        private int[] CalcAllScores(PlayingCard[] hands, int[,] handIdx, int maxHands, PlayingCard currentCard)
        {
            int[] scores = new int[maxHands];
            int multiplier = 1;
            CardHand chkHand = new CardHand(); // 5 cards, sorted order (largest to smallest, K - A)

            for (int i = 0; i < maxHands; i++)
            {
                for (int j = CardHand.FIRST; j < CardHand.DEF_MAX_NBR_CARDS; j++)
                {
                    PlayingCard crd = hands[handIdx[i, j]];
                    if (crd != PlayingCard.EMPTY_CARD) chkHand.Add(crd);
                }
                if (chkHand.CurNumCardsInHand < CardHand.DEF_MAX_NBR_CARDS)
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
         * Used to determine where to play next card (hint or auto-play).
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
            if (hand >= 0)
            {
                List<int> list = new List<int>
                {
                    hand
                };

                // only hand, or more than one?
                for (int i = 0; i < maxHands; i++)
                {
                    if (score == scores[i] && hand != i) list.Add(i);
                }
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
         * Used to determine where to play next card (hint or auto-play).
         */
        private int PickRandomSlot(PlayingCard[] hands, int maxCards)
        {
            int slot = -1;

            while (slot == -1)
            {
                int pick = SingleRandom.Instance.Next(maxCards);
                if (hands[pick] == PlayingCard.EMPTY_CARD) slot = pick;
            }

            return slot;
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

            if (scorePartialHands || hand.CurNumCardsInHand == CardHand.DEF_MAX_NBR_CARDS)
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
            int[] scores = CalcAllScores(hands, handIdx, maxHands, currentCard);
            int hand = SelectHandWithHighestScore(scores, maxHands);

            if (hand < 0)
            {
                hintIdx = PickRandomSlot(hands, maxCards);
            }
            else
            {
                PlayingCard[] hnd = new PlayingCard[CardHand.DEF_MAX_NBR_CARDS];
                for (int i = 0; i < CardHand.DEF_MAX_NBR_CARDS; i++)
                {
                    hnd[i] = hands[handIdx[hand, i]];
                }
                // pick random slot in highest point hand that is open
                hintIdx = handIdx[hand, PickRandomSlot(hnd, CardHand.DEF_MAX_NBR_CARDS)];
            }

            return hintIdx;
        }
        #endregion
    }
}
