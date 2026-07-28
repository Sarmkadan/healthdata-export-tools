using System;
using System.Threading;
using System.Threading.Tasks;

namespace HealthDataExport.Tools.Cache
{
    public class InMemoryCacheProvider
    {
        public async Task<object> GetAsync(object key, CancellationToken cancellationToken = default)
        {
            // existing code
        }
    }
}