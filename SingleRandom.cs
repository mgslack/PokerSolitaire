﻿using System;

/*
 * Singleton random class, as found on Social MSDN.
 *  Title: Sharing one Random object across entire application
 *  Answer posted on: Sunday, December 05, 2010 9:32 PM
 *
 */
namespace PokerSolitaire
{
    class SingleRandom : Random
    {
        static SingleRandom _Instance;
        public static SingleRandom Instance {
            get {
                if (_Instance == null) _Instance = new SingleRandom();
                return _Instance;
            }
        }

        private SingleRandom() { }
    }
}
