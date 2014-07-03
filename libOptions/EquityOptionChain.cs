using System.Collections.Generic;
using System.Data;

namespace libOptions
{
    public class EquityOptionChain
    {
        public static DataTable MakeTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("symbol", typeof(string));
            dt.Columns.Add("tradeDate", typeof(int));
            dt.Columns.Add("tradeTime", typeof(int));
            dt.Columns.Add("underlier", typeof(string));
            dt.Columns.Add("expDate", typeof(int));
            dt.Columns.Add("opType", typeof(string));
            dt.Columns.Add("strike", typeof(decimal));
            dt.Columns.Add("lastPx", typeof(decimal));
            dt.Columns.Add("bid", typeof(decimal));
            dt.Columns.Add("ask", typeof(decimal));
            dt.Columns.Add("volume", typeof(int));
            dt.Columns.Add("openInt", typeof(int));
            dt.Columns.Add("UnderPx", typeof(decimal));
            return dt;
        }

        public static DataTable ToQuoteTable(List<OptionQuote> qq,DataTable dt=null)
        {
            if (dt == null) dt = MakeTable();
            foreach (var q in qq)
            {
                DataRow row = dt.NewRow();
                row["symbol"]       = q.Option.YhoSymbol;
                row["tradeDate"]    = q.TradeDate;
                row["tradeTime"]    = q.TradeTime;
                row["underlier"]    = q.Option.Underlying;
                row["expDate"]      = q.Option.ExpDate;
                row["opType"]       = q.Option.OpType == AOption.EOpType.Call ? "C" : "P";
                row["strike"]       = q.Option.Strike;
                row["lastPx"]       = q.Last;
                row["bid"]          = q.Bid;
                row["ask"]          = q.Ask;
                row["volume"]       = q.Volume;
                row["openInt"]      = q.OpenInt;
                row["underPx"]      = q.UnderPx;
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}