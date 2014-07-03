using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HtmlAgilityPack;
using libCommon;

namespace libOptions
{
    public class OptionQuoteDownload
    {
        private enum CallFields { Symbol = 0, Last, Change, Bid, Ask, Volume, OpenInt, Strike };
        private enum PutFields { Strike = 7, Symbol = 8, Last, Change, Bid, Ask, Volume, OpenInt };
        private enum StackFields { Strike = 0, Symbol = 1, Last, Change, Bid, Ask, Volume, OpenInt };

        static private int _nThread;

        public List<OptionQuote> GetPriceForOptionChain(string sYhoUnder, int nExpYear, int nExpMonth, AOption.ESecType eSecType)
        {
            int nNow = Interlocked.Increment(ref _nThread);
            Console.WriteLine("Start thread #{0}", nNow);
            var ret = new List<OptionQuote>();
            var sExp = string.Format("{0}-{1:00}", nExpYear, nExpMonth);
            var sUrl = string.Format("http://finance.yahoo.com/q/os?s={0}&m={1}", sYhoUnder, sExp);

            int nTradeDate = Date.Today();
            int nTradeTime = Date.Now();
            if (nTradeTime > 160000) nTradeTime = 160000;

            try
            {
                var webGet = new HtmlWeb();
                var doc = webGet.Load(sUrl);

                var und = doc.DocumentNode.Descendants("span")
                    .Where(x => x.Attributes.Contains("class"))
                    .First(x => x.Attributes["class"].Value.Contains("time_rtq_ticker"));
                var sPx = und.SelectSingleNode("span").InnerText.Replace(",", "");

                decimal? dUnderPx = null;
                decimal dP;
                if (decimal.TryParse(sPx, out dP)) dUnderPx = dP;

                var tbls = doc.DocumentNode.Descendants("table")
                    .Where(x => x.Attributes.Contains("class"))
                    .First(x => x.Attributes["class"].Value.Contains("yfnc_datamodoutline1"));
                var trs = tbls.SelectNodes("tr/td/table/tr").Skip(2);

                foreach (var tr in trs)
                {
                    var cols = tr.SelectNodes("td");

                    var sCall = cols[(int)CallFields.Symbol].InnerText;
                    var sPut = cols[(int)PutFields.Symbol].InnerText;

                    var call = AOption.CreateFromYahooSymbol(sCall, eSecType);
                    var put = AOption.CreateFromYahooSymbol(sPut, eSecType);

                    var callQ = new OptionQuote(call);
                    var putQ = new OptionQuote(put);

                    if (decimal.TryParse(cols[(int) CallFields.Last].InnerText.Replace(",", ""),out dP)) callQ.Last = dP;
                    if (decimal.TryParse(cols[(int)PutFields.Last].InnerText.Replace(",", ""), out dP)) putQ.Last = dP;
                    if (decimal.TryParse(cols[(int)CallFields.Bid].InnerText.Replace(",", ""), out dP)) callQ.Bid = dP;
                    if (decimal.TryParse(cols[(int)PutFields.Bid].InnerText.Replace(",", ""),  out dP)) putQ.Bid = dP;
                    if (decimal.TryParse(cols[(int)CallFields.Ask].InnerText.Replace(",", ""), out dP)) callQ.Ask = dP;
                    if (decimal.TryParse(cols[(int)PutFields.Ask].InnerText.Replace(",", ""),  out dP)) putQ.Ask = dP;
                    int nV;
                    if (int.TryParse(cols[(int)CallFields.Volume].InnerText.Replace(",", ""), out nV)) callQ.Volume = nV;
                    if (int.TryParse(cols[(int)PutFields.Volume].InnerText.Replace(",", ""), out nV)) putQ.Volume = nV;
                    if (int.TryParse(cols[(int)CallFields.OpenInt].InnerText.Replace(",", ""), out nV)) callQ.OpenInt = nV;
                    if (int.TryParse(cols[(int)PutFields.OpenInt].InnerText.Replace(",", ""), out nV)) putQ.OpenInt = nV;

                    callQ.UnderPx = dUnderPx;
                    putQ.UnderPx = dUnderPx;

                    callQ.TradeDate = nTradeDate;
                    callQ.TradeTime = nTradeTime;
                    putQ.TradeDate = nTradeDate;
                    putQ.TradeTime = nTradeTime;

                    ret.Add(callQ);
                    ret.Add(putQ);
                }
                ret = ret.GroupBy(x => x.Option.YhoSymbol).Select(y => y.First()).ToList();
                nNow = Interlocked.Decrement(ref _nThread);
                Console.WriteLine("End thread #{0}", nNow);
                return ret;
            }
            catch (Exception)
            {
                Console.WriteLine("Error: {0} {1} {2}", sYhoUnder, nExpYear, nExpMonth);
                throw new OptionQuoteDownloadException
                {
                    YahoUnder = sYhoUnder,
                    ExpYear = nExpYear,
                    ExpMonth = nExpMonth,
                    SecType = eSecType
                };
            }
        }

        public List<OptionQuote> GetPriceForOptionChainStack(string sYhoUnder, int nExpYear, int nExpMonth, AOption.ESecType eSecType)
        {
            int nNow = Interlocked.Increment(ref _nThread);
            Console.WriteLine("Start thread #{0}", nNow);
            var ret = new List<OptionQuote>();
            var sExp = string.Format("{0}-{1:00}", nExpYear, nExpMonth);
            var sUrl = string.Format("http://finance.yahoo.com/q/op?s={0}&m={1}", sYhoUnder, sExp);

            int nTradeDate = Date.Today();
            int nTradeTime = Date.Now();
            if (nTradeTime > 160000) nTradeTime = 160000;

            try
            {
                var webGet = new HtmlWeb();
                var doc = webGet.Load(sUrl);

                var und = doc.DocumentNode.Descendants("span")
                    .Where(x => x.Attributes.Contains("class"))
                    .First(x => x.Attributes["class"].Value.Contains("time_rtq_ticker"));
                var sPx = und.SelectSingleNode("span").InnerText.Replace(",", "");

                decimal? dUnderPx = null;
                decimal dP;
                if (decimal.TryParse(sPx, out dP)) dUnderPx = dP;

                var tbls = doc.DocumentNode.Descendants("table")
                    .Where(x => x.Attributes.Contains("class"))
                    .Where(x => x.Attributes["class"].Value.Contains("yfnc_datamodoutline1"));

                var htmlNodes = tbls as IList<HtmlNode> ?? tbls.ToList();
                if (htmlNodes.Count() > 1)
                {
                    ret = ParseStack(htmlNodes[0], eSecType,dUnderPx,nTradeDate,nTradeTime);
                    ret.AddRange(ParseStack(htmlNodes[1], eSecType, dUnderPx, nTradeDate, nTradeTime));
                }
               
                ret = ret.GroupBy(x => x.Option.YhoSymbol).Select(y => y.First()).ToList();
                nNow = Interlocked.Decrement(ref _nThread);
                Console.WriteLine("End thread #{0}", nNow);
                return ret;
            }
            catch (Exception)
            {
                Console.WriteLine("Error: {0} {1} {2}", sYhoUnder, nExpYear, nExpMonth);
                throw new OptionQuoteDownloadException
                {
                    YahoUnder = sYhoUnder,
                    ExpYear = nExpYear,
                    ExpMonth = nExpMonth,
                    SecType = eSecType
                };
            }
        }

        private List<OptionQuote> ParseStack(HtmlNode tblNode, AOption.ESecType eSecType,
            decimal? dUnderPx, int nTradeDate, int nTradeTime)
        {
            var ret = new List<OptionQuote>();
            var trs = tblNode.SelectNodes("tr/td/table/tr").Skip(2);

            foreach (var tr in trs)
            {
                var cols = tr.SelectNodes("td");

                var sOp = cols[(int)StackFields.Symbol].InnerText;

                var op = AOption.CreateFromYahooSymbol(sOp, eSecType);
                var opQ = new OptionQuote(op);

                decimal dP;
                if (decimal.TryParse(cols[(int)StackFields.Last].InnerText.Replace(",", ""), out dP)) opQ.Last = dP;
                if (decimal.TryParse(cols[(int)StackFields.Bid].InnerText.Replace(",", ""), out dP)) opQ.Bid = dP;
                if (decimal.TryParse(cols[(int)StackFields.Ask].InnerText.Replace(",", ""), out dP)) opQ.Ask = dP;
                int nV;
                if (int.TryParse(cols[(int)StackFields.Volume].InnerText.Replace(",", ""), out nV)) opQ.Volume = nV;
                if (int.TryParse(cols[(int)StackFields.OpenInt].InnerText.Replace(",", ""), out nV)) opQ.OpenInt = nV;

                opQ.UnderPx = dUnderPx;

                opQ.TradeDate = nTradeDate;
                opQ.TradeTime = nTradeTime;

                ret.Add(opQ);
            }
            return ret;
        }
    }
}
