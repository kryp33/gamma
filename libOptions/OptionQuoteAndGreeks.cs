using System;
using libCommon;

namespace libOptions
{
    public class OptionQuoteAndGreeks
    {
        public OptionQuote OptionQuote { get; set; }
        public Greeks Greeks { get; set; }
        public OptionQuoteAndGreeks(OptionQuote optionQuote)
        {
            OptionQuote = optionQuote;
            Greeks = fnGreeks();
        }

        private Greeks fnGreeks()
        {
            double dIv;
            bool isCall = OptionQuote.Option.OpType == AOption.EOpType.Call;
            DateTime dtTradeDate = Date.Decode(OptionQuote.TradeDate);
            DateTime dtExpDate = Date.Decode(OptionQuote.Option.ExpDate);
            double dT = Convert.ToDouble(OptionQuote.Option.TimeToExp(dtTradeDate, dtExpDate));
            double dRfr = fnRiskFreeRate(dtTradeDate, dtExpDate);

            int implVolNewton = BlackSholes.ImplVol_Newton(
                Convert.ToDouble(OptionQuote.Mid),
                Convert.ToDouble(OptionQuote.UnderPx),
                Convert.ToDouble(OptionQuote.Option.Strike),
                dT,
                dRfr,
                0,
                isCall,
                out dIv);
            if (implVolNewton != 0) return null;

            double dTheoPx;
            var gg = new Greeks();
            int nRet =
                BlackSholes.BlackSholesCals(
                    Convert.ToDouble(OptionQuote.UnderPx),
                    Convert.ToDouble(OptionQuote.Option.Strike),
                    dT,
                    dRfr,
                    0,
                    isCall,
                    dIv,
                    out dTheoPx,
                    gg);
            return nRet != 0 ? null : gg;
        }
        private double fnRiskFreeRate(DateTime dtTradeDate, DateTime dtExpDate)
        {
            return 0.005;
        }
    }
}