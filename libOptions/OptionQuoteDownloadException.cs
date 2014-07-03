using System;

namespace libOptions
{
    public class OptionQuoteDownloadException : Exception
    {
        public string YahoUnder { get; set; }
        public int ExpYear { get; set; }
        public int ExpMonth { get; set; }
        public AOption.ESecType SecType { get; set; }
    }
}