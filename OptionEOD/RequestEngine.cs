using System;
using System.Collections.Generic;
using System.Linq;
using libCommon;
using libOptions;

namespace OptionEOD
{
    public class RequestEngine
    {
        private int TradeDate { get; set; }
        private int TradeTime { get; set; }
        private string ListName  { get; set; }

        private readonly List<OptionError> _optionErrorsList;
        private readonly int _fwdMonths;

        public AOption.ESecType SecType { get; set; }
        public List<Tuple<string, DateTime>> Requests { get; private set; }

        PriceEntities PriceEntities { get; set; }
        public int Attempt { get; set; }
        
        
        public RequestEngine(string sList,int nFwd=4)
        {
            ListName = sList;
            _fwdMonths = nFwd;
            TradeDate = Date.Today();
            TradeTime = Date.Now();

            PriceEntities = new PriceEntities();

            SecType = GetSecType();
            _optionErrorsList = ErrorList().ToList();
            Attempt = _optionErrorsList.Any() ?_optionErrorsList.Select(v => v.dldAttempt).Max():0;
            Requests = _optionErrorsList.Any() ? MakeErrorRequestList() : MakeNewRequestList();
        }

        IEnumerable<OptionError> ErrorList()
        {
            return PriceEntities.OptionErrors.Where(v => v.tradeDate == TradeDate && v.groupName == ListName && v.success == 0);
        }

        List<Tuple<string, DateTime>> MakeErrorRequestList()
        {
            return _optionErrorsList.Select(v => new Tuple<string, DateTime>(v.underSymbol, new DateTime(v.expYear, v.expMonth, 1))).ToList();
        }

        List<Tuple<string, DateTime>> MakeNewRequestList()
        {
            using (var db = new SymsEntities())
            {
                var ddExps = new List<DateTime>();
                var dtNow = DateTime.Today;

               IEnumerable<string> syms = db.GroupSymbols.Where(v => v.groupName == ListName).Select(v => v.symbol);

                for (var i = 0; i < _fwdMonths; ++i)
                {
                    ddExps.Add(dtNow);
                    dtNow = dtNow.AddMonths(1);
                }
                return (from s in syms from exp in ddExps select new Tuple<string, DateTime>(s, exp)).ToList();
            }
        }

        AOption.ESecType GetSecType()
        {
            using (var db = new SymsEntities())
            {
                var secType =
                    (db.GroupParams.Where(v => v.groupName == ListName && v.paramName == "SEC_TYPE")
                        .Select(v => v.paramValue)).Single();
                return (secType == "Equity") ? AOption.ESecType.Equity : AOption.ESecType.Index;
            }
        }

        public int HandleNewErrors(List<OptionQuoteDownloadException> newErrors)
        {
                if (_optionErrorsList.Any())
                {
                    

                    var mapNewErr = new Dictionary<Tuple<string, int, int>, OptionQuoteDownloadException>();
                    foreach (var newErr in newErrors)
                        mapNewErr[new Tuple<string, int, int>(newErr.YahoUnder,newErr.ExpYear,newErr.ExpMonth)] = newErr;

                    foreach (var e in _optionErrorsList)
                    {
                        e.dldAttempt += 1;
                        e.lastAttDate = TradeDate;
                        e.lastAttTime = TradeTime;
                        if (!mapNewErr.ContainsKey(new Tuple<string, int, int>(e.underSymbol, e.expYear, e.expMonth)))
                            e.success = 1;
                    }
                    PriceEntities.SaveChanges();
                    //foreach (var er in newErrors)
                    //{
                    //    var newErr =
                    //        _optionErrorsList.Single(
                    //            v =>
                    //                (v.underSymbol == er.YahoUnder && v.expYear == er.ExpYear &&
                    //                 v.expMonth == er.ExpMonth));
                    //    newErr.success = 0;
                    //}
                }
                else
                {
                    foreach (var er in newErrors)
                    {
                        var oe = new OptionError
                        {
                            underSymbol = er.YahoUnder,
                            tradeDate = Date.Today(),
                            expYear = er.ExpYear,
                            expMonth = er.ExpMonth,
                            success = 0,
                            dldAttempt = 1,
                            lastAttDate = TradeDate,
                            lastAttTime = TradeTime,
                            groupName = ListName
                        };
                        PriceEntities.OptionErrors.Add(oe);
                    }
                }
                return PriceEntities.SaveChanges();
        }
    }
}
