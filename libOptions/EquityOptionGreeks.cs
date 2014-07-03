namespace libOptions
{
    public class EquityOptionGreeks:EquityOption
    {
        public EquityOptionGreeks(EquityOption eo)
        {
            Underlying=eo.Underlying;
            OpType=eo.OpType;   
            ExpDate=eo.ExpDate;
            Strike = eo.Strike;  
        }
        public Greeks Greeks { get; set; }
    }
}