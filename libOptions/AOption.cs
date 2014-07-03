using System;
using System.Collections.Generic;

namespace libOptions
{
    public abstract class AOption
    {
        public enum         ESecType                {Equity,Etf,Index,Future}
        public enum         EOpType                 { Call, Put }
        public string       Underlying              { get; set; }
        public ESecType     SecType                 { get; set; }
        public string       SubType                 { get; set; }

        public EOpType      OpType                  { get; set; }
        public int          ExpDate                 { get; set; } //saturday
        public decimal      Strike                  { get; set; }
        public decimal      Multiplier              { get; set; }

        public abstract string IbSymbol             { get; }
        public abstract string YhoSymbol            { get; }
        public abstract string FidelitySymbol       { get; }

        public static string GetYahooUnderling(string sStdSymbol, ESecType eSecType)
        {
            switch (eSecType)
            {
                case ESecType.Equity: return EquityOption.GetYahooUnderling(sStdSymbol);
                case ESecType.Index:  return IndexOption.GetYahooUnderling(sStdSymbol);
                default: return null;
            }
        }

        public static
            AOption CreateFromYahooSymbol(string sSymbol, ESecType eSecType)
        {
            switch (eSecType)
            {
                case ESecType.Equity: return EquityOption.CreateFromYahooSymbol(sSymbol);
                case ESecType.Index: return IndexOption.CreateFromYahooSymbol(sSymbol);
                default: return null;
            }
        }

        public abstract decimal TimeToExp(DateTime dtTradeDate, DateTime dtExpDate);

        public static  DateTime GetStdExpiration(int nYY, int nMM)
        {
            var dt = new DateTime(nYY, nMM, 1);
            var dtFirstFri = DateTime.MinValue;
            for (int i = 0; i < 10; ++i)
            {
                if (dt.DayOfWeek == DayOfWeek.Friday)
                {
                    dtFirstFri = dt;
                    break;
                }
                dt = dt.AddDays(1);
            }
            return dtFirstFri.AddDays(15);
        }

        public static  DateTime[] GetFwdExpirations(DateTime dtTradeDate, int nFwd)
        {
            var dt = new DateTime(dtTradeDate.Year, dtTradeDate.Month, 1);
            var exps = new List<DateTime>();
            for (int i = 0; i < nFwd + 1; ++i)
            {
                exps.Add(GetStdExpiration(dt.Year, dt.Month));
                dt = dt.AddMonths(1);
            }
            if (exps[0] <= dtTradeDate) exps.RemoveAt(0); //We past the expiration. It is set by saturday
            else exps.RemoveAt(exps.Count - 1);
            return exps.ToArray();
        }
    }
}