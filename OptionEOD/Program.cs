using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using libCommon;
using libOptions;

namespace OptionEOD
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MyLog.Info("Main::Start()");

                var parser = new CMDLineParser();
                var opList = parser.AddStringParameter("--List=", "PX_OPT_EQUITY|PX_OPT_INDEX", true);
                parser.Parse(args);

                var ssSymbolGroup = opList.isMatched ? opList.Value.ToString().Split('|') : new[] { "PX_OPT_EQUITY" };

                foreach (var sList in ssSymbolGroup)
                {
                    var req = new RequestEngine(sList, 4);
                    var nAtpt = req.Attempt;
                    var dlds = new BlockingCollection<List<OptionQuoteAndGreeks>>();
                    var tDb = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (!dlds.IsCompleted)
                            {
                                var data = dlds.Take();
                                var s = new OptionQuoteSaver("Options");
                                s.Save(data);
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // InvalidOperationException is thrown when Take is called after 
                            // queue.CompleteAdding(), this is signals that this class is being
                            // disposed, so we allow the thread to complete.
                        }
                    }, TaskCreationOptions.LongRunning);

                    AOption.ESecType eSecType = req.SecType;
                    var dlErrors = new ConcurrentQueue<OptionQuoteDownloadException>();
                    Parallel.ForEach(
                        req.Requests,
                        new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount},
                        arg =>
                        {
                            try
                            {
                                var od = new OptionQuoteDownload();
                                List<OptionQuote> listOptionQuotes;
                                if (nAtpt == 1)
                                    listOptionQuotes = od.GetPriceForOptionChainStack(arg.Item1, arg.Item2.Year,
                                        arg.Item2.Month, eSecType);
                                else
                                    listOptionQuotes = od.GetPriceForOptionChain(arg.Item1, arg.Item2.Year,
                                        arg.Item2.Month, eSecType);
                                if (listOptionQuotes != null && listOptionQuotes.Any())
                                {
                                    var listOptionQuoteAndGreekses = new List<OptionQuoteAndGreeks>();
                                    foreach (var q in listOptionQuotes)
                                        listOptionQuoteAndGreekses.Add(new OptionQuoteAndGreeks(q));
                                    dlds.Add(listOptionQuoteAndGreekses);
                                }
                            }
                            catch (OptionQuoteDownloadException ex)
                            {
                                dlErrors.Enqueue(ex);
                            }
                        });

                    dlds.CompleteAdding();
                    tDb.Wait();
                    req.HandleNewErrors(dlErrors.ToList());
                }
                MyLog.Info("Main::EndOK()");
            } 
            catch (Exception e)
            {
                MyLog.Error(e.ToString());
                throw;
            }
        }
    }
}
