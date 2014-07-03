using System;

namespace libOptions
{
    public class EquityOption:AOption
    {
        public EquityOption()
        {
            SecType = ESecType.Equity;
        }

        public override string IbSymbol
        {
            get
            {
                //TODO: Something fishy
                //int nExpIb = Date.Encode(Date.Decode(ExpDate).AddDays(-1)); //They use 3rd Friday
                string sExpDate = (ExpDate - 20000000).ToString();
                string sStrike = (Strike * 1000).ToString("00000000");
                string sOpType = OpType == EOpType.Call ? "C" : "P";
                return Underlying + sExpDate + sOpType + sStrike;
            }
        }

        public override string YhoSymbol
        {
            get
            {
                string sExpDate = (ExpDate - 20000000).ToString(); //They Use 3rd Suturday
                string sStrike = (Strike * 1000).ToString("00000000");
                string sOpType = OpType == EOpType.Call ? "C" : "P";
                string sMini = Multiplier == 100 ? "" : "7";
                return Underlying + sMini + sExpDate + sOpType + sStrike;
            }
        }
        public override string FidelitySymbol
        {
            get
            {
                string sExpDate = (ExpDate - 20000000).ToString(); //They Use 3rd Suturday
                string sStrike = Strike.ToString(); //Unknon decimal convention...
                string sOpType = OpType == EOpType.Call ? "C" : "P";
                return "-" + Underlying + sExpDate + sOpType + sStrike;
            }
        }

        public static string GetYahooUnderling(string sStdSymbol)
        {
            return sStdSymbol;
        }

        public static AOption CreateFromYahooSymbol(string sYho)
        {
            int nUnderSize = sYho.Length - 15;
            try
            {
                //SPY140124C00189000
                var eo = new EquityOption();
                string sSymbol = sYho.Substring(0, nUnderSize);
                if (sSymbol[sSymbol.Length - 1] == '7')
                {
                    eo.Underlying = sSymbol.Substring(0, sSymbol.Length - 1);
                    eo.Multiplier = 10;
                }
                else
                {
                    eo.Underlying = sSymbol;
                    eo.Multiplier = 100;
                }

                eo.ExpDate = 20000000 + int.Parse(sYho.Substring(nUnderSize, 6));
                eo.OpType = sYho.Substring(nUnderSize + 6, 1) == "C" ? EOpType.Call : EOpType.Put;
                eo.Strike = decimal.Parse(sYho.Substring(nUnderSize + 6 + 1, 5)) + decimal.Parse(sYho.Substring(nUnderSize + 6 + 1 + 5, 3)) / 1000.0m;
                return eo;
            }
            catch { return null; }
        }

        public override decimal TimeToExp(DateTime dtTradeDate, DateTime dtExpDate)
        {
            var mid = new DateTime(dtTradeDate.Year, dtTradeDate.Month, dtTradeDate.Day, 23, 59, 59); //midnight
            var tsCur = mid - dtTradeDate;
            decimal dCurrDay = tsCur.Hours * 60 + tsCur.Minutes + 1;
            const decimal dSettDay = 900.0m; // Assuming that option expirin Sutturday at 3:00 pm.(p11 of VIX spec)
            decimal dOthers = (dtExpDate - mid).Days * 24 * 60;
            return (dCurrDay + dSettDay + dOthers) / 525600.0m;      //Mins in year
        }

    }
}