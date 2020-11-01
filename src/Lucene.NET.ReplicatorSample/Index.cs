using System.Collections.Generic;
using System.Diagnostics;
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
    public class Indices
    {
        public static Indices Instance = new Indices();

        private readonly Dictionary<string, Index> indices = new Dictionary<string, Index>();
        private readonly Dictionary<string, IReplicator> replicators = new Dictionary<string, IReplicator>();

        public ReplicationService ReplicatorService { get; private set; }

        public void Initialize()
        {
            ReplicatorService = new ReplicationService(replicators, "/api/replicate");
        }

        public Index AddIndex(string name, IWebHostEnvironment env)
        {
            Index index = new Index(name, env);
            replicators.Add(name, index.Replicator);
            indices.Add(name, index);
            return index;
        }
    }

    public class Index
    {
        private const LuceneVersion VERSION = LuceneVersion.LUCENE_48;

        private readonly IndexWriter writer;
        public LocalReplicator Replicator { get; }

        public Index(string name, IWebHostEnvironment env)
        {
            IndexWriterConfig config = new IndexWriterConfig(VERSION, new StandardAnalyzer(VERSION, CharArraySet.EMPTY_SET));
            config.IndexDeletionPolicy = new SnapshotDeletionPolicy(config.IndexDeletionPolicy);
            
            string path = Path.Combine(env.ContentRootPath, "APP_DATA", "Indices", name);
            Directory.CreateDirectory(path);
            writer = new IndexWriter(FSDirectory.Open(path), config);

            Replicator = new LocalReplicator();
        }

        public void Write(Term term, Document doc)
        {
            writer.UpdateDocument(term, doc);
        }

        public void Commit()
        {
            writer.Commit();
            Replicator.Publish(new IndexRevision(writer));
        }
    }
}