using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Identity.Application.Common.Interfaces
{
    public interface ICachedQuery
    {
        string CacheKey { get; }
        string[] Tags { get; }
        TimeSpan Expiration { get; }
        public TimeSpan? LocalCacheExpiration { get; init; }

    }

    public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;
}
