﻿namespace Infrastructure.Adapters.StockQuotation.Exceptions
{
    internal class StockQuotationException : Exception
    {
        public StockQuotationException(string message) : base(message) { }
        public StockQuotationException(string message, Exception ex) : base(message, ex) { }
    }
}
