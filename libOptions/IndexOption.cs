using System;
using libCommon;

namespace libOptions
{
    public class IndexOption : AOption
    {
        public IndexOption()
        {
            SecType = ESecType.Index;
        }

        public override string IbSymbol
        {
            get
            {
                //TODO: Something fishy
                int nExpIB = Date.Encode(Date.Decode(ExpDate).AddDays(-1)); //They use 3rd Friday
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
                return Underlying + sExpDate + sOpType + sStrike;
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
            switch (sStdSymbol.ToUpper())
            {
                case ".DJI": case "^DJI": case "DJI": case "DJX":  case "^DJX": return "^DJX";
                case ".SPX": case "^SPX": case "SPX": case "^GSPC":case "GSPC": return "^SPXPM";
                case ".OEX": case "^OEX": case "OEX":                           return "^OEX";
                case ".NDX": case "^NDX": case "NDX":                           return "^NDX";
                case ".RUT": case "^RUT": case "RUT":                           return "^RUT";
                default:                                                        return null;
            }
        }

        public static AOption CreateFromYahooSymbol(string sYho)
        {
            int nUnderSize = sYho.Length - 15;
            try
            {
                //RUT140124C00189000
                var eo = new IndexOption();
                string sYhoRoot = sYho.Substring(0, nUnderSize);

                string sSymbol = sYho.Substring(0, nUnderSize);
                switch (sSymbol)
                {
                    case "RUTQ":
                        eo.Underlying = "^RUT";
                        eo.SubType = "RUTQ"; //Expired in end of quater New since Apr 04 2014
                        break;
                    default:
                        eo.Underlying = "^"+sSymbol;
                        break;
                }
                eo.Multiplier = 100;
                eo.ExpDate = 20000000 + int.Parse(sYho.Substring(nUnderSize, 6));
                eo.OpType = sYho.Substring(nUnderSize + 6, 1) == "C" ? EOpType.Call : EOpType.Put;
                eo.Strike = decimal.Parse(sYho.Substring(nUnderSize + 6 + 1, 5)) + decimal.Parse(sYho.Substring(nUnderSize + 6 + 1 + 5, 3)) / 1000.0m;
                return eo;
            }
            catch { return null; }
        }

        public override decimal TimeToExp(DateTime dtTradeDate, DateTime dtExpDate)
        {
            //Check calcs... according to exp time
            DateTime mid = new DateTime(dtTradeDate.Year, dtTradeDate.Month, dtTradeDate.Day, 23, 59, 59); //midnight
            TimeSpan tsCur = mid - dtTradeDate;
            decimal dCurrDay = tsCur.Hours * 60 + tsCur.Minutes + 1;
            const decimal dSettDay = 900.0m; // Assuming that option expirin Sutturday at 3:00 pm.(p11 of VIX spec)
            decimal dOthers = (dtExpDate - mid).Days * 24 * 60;
            return (dCurrDay + dSettDay + dOthers) / 525600.0m;      //Mins in year
        }
    }
}