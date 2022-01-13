# PokerSolitaire
The game of poker solitaire (or poker squares)

Game was inspired by a game played years ago that was created by Roger Smith Jr. for OS/2 back in 1989.  Didn't have
source and hadn't seen it running in a long time, but remembered hint and auto-play functionalities.  This version of
the game includes both.  Uses American scoring rules (higher hand has higher points).

Game requires PlayingCards, GameStatistics and CustomMessageBox assembly's.  Edited and compiled in VS 2019.

One minor issue discovered while developing this game.  The drag-n-drop pointer created of the card image is not created
with the auto-scaled size of the image grabbing to drag.  If the game is ran on a monitor with 100% scaling, the mouse
pointer image is the correct size, if running on monitor with larger scaling (say 125%), the image is still at 100% -
about 25% smaller than normal.  It plays Ok, but would be better if I could resize to the scaling used by the form.
Could not find this scaling factor (other than for the primary screen/monitor).  Played around with it for a while, but
still haven't found the answer.  Let me know if someone has a solution (have two monitors, the laptops and external - laptop
running at 125% scaling, external is 100%).  Hopefully a solution that doesn't limit program to running on Windows 10 or higher.

Updated to include a different hint method to try to bring up the scores of auto-play games and give better hints on where to
play a card.  The original hint method picked the hand in the board where the new card gave the biggest score for that hand.
The additional method changed to score each card slot based on the score of that slot of every hand the slot can participate
in.  Neither of the two methods does real well (average game scores for auto-play are in the mid to upper 50's for both
methods).  The 'By Hand' method does seem to provide higher upper bounds of scoring, but a lower average score.  Need to
determine if there is a better method than the two currently available.
