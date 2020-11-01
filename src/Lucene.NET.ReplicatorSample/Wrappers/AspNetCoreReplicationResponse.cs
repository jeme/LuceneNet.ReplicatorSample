using System.IO;
using Lucene.Net.Replicator.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Lucene.NET.ReplicatorSample.Wrappers
{
    public class AspNetCoreReplicationResponse : IReplicationResponse
    {
        private readonly HttpResponse response;

        // Inject the actual response object in the constructor.
        public AspNetCoreReplicationResponse(HttpResponse response)
            => this.response = response;

        // Getter and Setter for the http Status code, in case of failure the ReplicatorService will set this
        // Property.
        public int StatusCode
        {
            get => response.StatusCode;
            set => response.StatusCode = value;
        }

        // Return a stream where the ReplicatorService can write to for the response.
        // Depending on the action either the file or the sesssion token will be written to this stream.
        public Stream Body => response.Body;

        // Called when the ReplicatorService is done writing data to the response.
        // Here it is mapped to the flush method on the "body" stream on the response.
        public void Flush() => response.Body.Flush();
    }
}