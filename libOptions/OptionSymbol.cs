namespace libOptions
{
    public static class OptionSymbol
    {
        public static string Encode(string sUnder,string sOpType, int nExpDate,decimal dStrike)
        { 
            string sExpDate = (nExpDate-20000000).ToString();
            string sStrike = (dStrike*1000).ToString("00000000");
            return sUnder+sExpDate+sOpType.ToUpper().Substring(0,1)+sStrike;
        }

        public static bool Decode(string sSymbol,out string sUnder,out string sOpType, out int nExpDate,out decimal dStrike)
        {
            int nUnderSize = sSymbol.Length - 15;
            sUnder = sSymbol.Substring(0, nUnderSize);
            nExpDate = 20000000 + int.Parse(sSymbol.Substring(nUnderSize, 6));
            sOpType = sSymbol.Substring(nUnderSize + 6, 1);
            dStrike = decimal.Parse(sSymbol.Substring(nUnderSize + 6 + 1, 5)) + decimal.Parse(sSymbol.Substring(nUnderSize + 6 + 1 + 5, 3)) /1000.0m;
            return true;
        }
    }
}
