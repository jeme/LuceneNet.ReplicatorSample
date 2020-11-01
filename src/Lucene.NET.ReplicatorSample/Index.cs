using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Hosting;
using Directory = System.IO.Directory;

namespace Lucene.NET.ReplicatorSample
{
    public class Index
    {
        private const LuceneVersion VERSION = LuceneVersion.LUCENE_48;
        public static Index Instance = new Index();

        private IndexWriter writer;
        private LocalReplicator replicator;
        public ReplicationService ReplicatorService { get; private set; }

        public void Initialize(IWebHostEnvironment env)
        {
            IndexWriterConfig config = new IndexWriterConfig(VERSION, new StandardAnalyzer(VERSION, CharArraySet.EMPTY_SET));
            config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
            
            string path = Path.Combine(env.ContentRootPath, "APP_DATA", "INDEX");
            Directory.CreateDirectory(path);
            writer = new IndexWriter(FSDirectory.Open(path), config);

            replicator = new LocalReplicator();
            ReplicatorService = new ReplicationService(new Dictionary<string, IReplicator>
            {
                ["shard_name"] = replicator
            }, "/api/replicate");
        }

        public void Write(Term term, Document doc)
        {
            writer.UpdateDocument(term, doc);
        }

        public void Commit()
        {
            writer.Commit();
            replicator.Publish(new IndexRevision(writer));
        }
    }
}