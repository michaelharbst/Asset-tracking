using Asset_tracking;

public readonly struct Price
{
    public decimal PriceUSD { get; }
    public Currency Currency { get; }

    public decimal PriceLocal
    {
        get
        {
            decimal fxrate = Currency switch //Hardcoded fx rates
            {
                Currency.USD => 1m,
                Currency.SEK => 10m,
                Currency.TRY => 41,
                _ => throw new ArgumentOutOfRangeException()
            };
            return fxrate * PriceUSD;
        }
    }

public Price(decimal priceUSD, Currency currency)
    {
        PriceUSD = priceUSD;
        Currency = currency;
    }
}