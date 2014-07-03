namespace libOptions
{
    public class Greeks
    {
        public double IV { get; set; }
        public double Delta { get; set; }
        public double Gamma {get;set;}
        public double Theta {get;set;}
        public double Rho {get;set;}
        public double Vega {get;set;}

        public object GetValue(string fieldName)
        {
            switch (fieldName)
            {
                case "IV":    return IV;
                case "Delta": return Delta;
                case "Gamma": return Gamma;
                case "Theta": return Theta;
                case "Rho":   return Rho;
                case "Vega":  return Vega;
                default: return null;
            }
        }
    }
}