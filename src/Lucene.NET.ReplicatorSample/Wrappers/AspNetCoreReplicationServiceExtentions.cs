using Lucene.Net.Replicator.Http;
using Microsoft.AspNetCore.Http;

namespace Lucene.NET.ReplicatorSample.Wrappers
{
    public static class AspNetCoreReplicationServiceExtentions
    {
        // Optionally, provide a extension method for calling the perform method directly using the specific request
        // and response objects from AspNetCore
        public static void Perform(this ReplicationService self, HttpRequest request, HttpResponse response)
            => self.Perform(
                new AspNetCoreReplicationRequest(request),
                new AspNetCoreReplicationResponse(response));
    }
}