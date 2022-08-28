using System.Threading.Tasks;

namespace CTrader.Strategies
{
    public interface ITradeStrategy
    {
        float FiatFunds { get; }

        float CoinFunds { get; } 

        Task Process();
    }

}
