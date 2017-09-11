namespace NGuava.Base
{
    public static class Int32Extensions
    {
        public static int NumberOfLeadingZeros(this int i)
        {
            // HD, Figure 5-6
            if (i == 0)
                return 32;
            int n = 1;
            if ((uint)i >> 16 == 0) { n += 16; i <<= 16; }
            if ((uint)i >> 24 == 0) { n += 8; i <<= 8; }
            if ((uint)i >> 28 == 0) { n += 4; i <<= 4; }
            if ((uint)i >> 30 == 0) { n += 2; i <<= 2; }
            n -= (int)((uint)i >> 31);
            return n;
        }
    }
}