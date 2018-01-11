using System.Threading.Tasks;

namespace PoolObserver.Common.Models
{
    public class Subscriber
    {
        public string TokenId { get; set; }
        public object PoolManager { get; set; }
        public Task Observer { get; set; }
        public object Update { get; set; }
    }
}
