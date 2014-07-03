using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using libCommon;

namespace libOptions
{
    public class OptionQuoteSaver
    {
        public string TableName { get; set; }
        public string ConnStr { get; set; }
        public OptionQuoteSaver(string sTableName)
        {
            TableName = sTableName;
            ConnStr = ConfigurationManager.ConnectionStrings[TableName].ConnectionString;
        }

        public void Save(List<OptionQuoteAndGreeks> listOptionQuoteAndGreekses)
        {
            using (var conn = new SqlConnection(ConnStr))
            {
                conn.Open();
                var dt = PrepareTable(conn);
                FillTable(listOptionQuoteAndGreekses, dt);
                BulkDelete.Delete(conn, TableName, dt, new[] {"symbol", "tradeDate"});
                BulkSave.Save(conn, TableName, dt);
                conn.Close();
            }
        }

        private DataTable PrepareTable(SqlConnection conn)
        {
            var cmd = new SqlCommand(string.Format("SET FMTONLY ON; SELECT * FROM {0} ; SET FMTONLY OFF;", TableName), conn);
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        private void FillTable(IEnumerable<OptionQuoteAndGreeks> opsList, DataTable dt)
        {
            foreach (var op in opsList)
            {
                DataRow row = dt.NewRow();
                
                row["symbol"] = op.OptionQuote.Option.YhoSymbol;
                row["tradeDate"] = op.OptionQuote.TradeDate;
                row["tradeTime"] = op.OptionQuote.TradeTime;
                row["underlier"] = op.OptionQuote.Option.Underlying;
                row["secType"] = op.OptionQuote.Option.SecType.ToString();
                row["subType"] = op.OptionQuote.Option.SubType;
                row["multiplier"] = op.OptionQuote.Option.Multiplier;
                row["expDate"] = op.OptionQuote.Option.ExpDate;
                row["opType"] = op.OptionQuote.Option.OpType == AOption.EOpType.Call ? "C" : "P";
                row["strike"] = op.OptionQuote.Option.Strike;
                if (op.OptionQuote.Last == null) row["lastPx"] = DBNull.Value; else row["lastPx"] = op.OptionQuote.Last;
                if (op.OptionQuote.Bid == null) row["bid"] = DBNull.Value; else row["bid"] = op.OptionQuote.Bid;
                if (op.OptionQuote.Ask == null) row["ask"] = DBNull.Value; else row["ask"] = op.OptionQuote.Ask;
                if (op.OptionQuote.Volume == null) row["volume"] = DBNull.Value; else row["volume"] = op.OptionQuote.Volume;
                if (op.OptionQuote.OpenInt == null) row["openInt"] = DBNull.Value; else row["openInt"] = op.OptionQuote.OpenInt;
                row["underPx"] = op.OptionQuote.UnderPx ?? op.OptionQuote.UnderPx;
                row["src"] = 1;
                if (op.Greeks != null)
                {
                    row["impVol"] = op.Greeks.IV;
                    row["delta"] = op.Greeks.Delta;
                    row["gamma"] = op.Greeks.Gamma;
                    row["theta"] = op.Greeks.Theta;
                    row["rho"] = op.Greeks.Rho;
                    row["vega"] = op.Greeks.Vega;
                }
                dt.Rows.Add(row);
            }
        }
    }
}
