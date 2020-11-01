using System.Linq;
using Lucene.Net.Replicator.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Lucene.NET.ReplicatorSample.Wrappers
{
    public class AspNetCoreReplicationRequest : IReplicationRequest
    {
        private readonly HttpRequest request;

        // Inject the actual request object in the constructor.
        public AspNetCoreReplicationRequest(HttpRequest request)
            => this.request = request;

        // Provide the full path relative to the host.
        // In the common case in AspNetCore this should just return the full path, so PathBase + Path are concatenated and returned.
        // 
        // The path expected by the ReplicatorService is {context}/{shard}/{action} where:
        //  - action may be Obtain, Release or Update
        //  - context is the same context that is provided to the ReplicatorService constructor and defaults to '/replicate'
        public string Path => request.PathBase + request.Path;

        // Return values for parameters used by the ReplicatorService
        // The ReplicatorService will call this with:
        // - version: The index revision
        // - sessionid: The ID of the session
        // - source: The source index for the files
        // - filename: The file name
        //
        // In this implementation a exception is thrown in the case that parameters are provided multiple times.
        public string QueryParam(string name) => request.Query[name].SingleOrDefault();
    }
}