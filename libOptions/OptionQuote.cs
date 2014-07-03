namespace libOptions
{
    public class OptionQuote
    {
        public AOption Option { get; private set; }
        public OptionQuote(AOption eo)
        {
            Option = eo;
        }

        public object GetValue(string sFieldName)
        {
            switch (sFieldName)
            {
                case "Last": return Last;
                case "Bid": return Bid;
                case "Ask": return Ask;
                case "Mid": return Mid;
                case "UnderPx": return UnderPx;
                case "Volume": return Volume;
                case "OpenInt": return OpenInt;
                default: return null;
            }
        }

        public decimal? Last     { get; set; }
        public decimal? Bid      { get; set; }
        public decimal? Ask      { get; set; }
        public decimal? Mid      { get {return  0.5m * (Bid + Ask); } }
        public decimal? UnderPx  { get; set; }
        public int? Volume       { get; set; }
        public int? OpenInt      { get; set; }

        public int TradeDate    { get; set; }
        public int TradeTime    { get;set;  }
    }
}
