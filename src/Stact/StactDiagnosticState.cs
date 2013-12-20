using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Stact
{
    public static class StactDiagnosticState
    {
        private static long _alphaMessages;
        private static long _alphaGCed;
        private static long _betaMessages;
        private static long _betaGCed;


        public static long AlphaMessages
        {
            get { return System.Threading.Interlocked.Read(ref _alphaMessages); }
        }

        public static long AlphaGCed
        {
            get { return System.Threading.Interlocked.Read(ref _alphaGCed); }
        }

        public static long BetaMessages
        {
            get { return System.Threading.Interlocked.Read(ref _betaMessages); }
        }

        public static long BetaGCed
        {
            get { return System.Threading.Interlocked.Read(ref _betaGCed); }
        }

        public static void SetAlphaMessagesInMemory(long value)
        {
            System.Threading.Interlocked.Exchange(ref _alphaMessages, value);
        }

        public static void SetBetaMessagesInMemory(long value)
        {
            System.Threading.Interlocked.Exchange(ref _betaMessages, value);
        }

        public static void IncrementAlphMessagesGCed()
        {
            System.Threading.Interlocked.Increment(ref _alphaGCed);
        }

        public static void IncrementBetaMessagesGCed()
        {
            System.Threading.Interlocked.Increment(ref _betaGCed);
        }

    }
}