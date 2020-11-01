using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Util;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Replicator;
using Lucene.Net.Replicator.Http;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = System.IO.Directory;

namespace Lucene.NET.ReplicatorSample.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:53000";
            Indices.Instance.AddIndex(url, "IndexA");
            Indices.Instance.AddIndex(url, "IndexB");
            Indices.Instance.AddIndex(url, "IndexC");

            while (true)
            {
                string input = Console.ReadLine();

                if(input != null && input.ToUpper() == "EXIT")
                    return;

                try
                {
                    Console.WriteLine(Indices.Instance.Lookup("IndexA").Search(input));
                    Console.WriteLine(Indices.Instance.Lookup("IndexB").Search(input));
                    Console.WriteLine(Indices.Instance.Lookup("IndexC").Search(input));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }

    public class Indices
    {
        public static Indices Instance = new Indices();

        private readonly Dictionary<string, Index> indices = new Dictionary<string, Index>();
        private readonly Dictionary<string, ReplicationClient> replicators = new Dictionary<string, ReplicationClient>();

        public Index Lookup(string name)
        {
            return indices.TryGetValue(name, out Index value) ? value : null;
        }

        public Index AddIndex(string url, string name)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "APP_DATA", "Indices", name);
            string temp = Path.Combine(Directory.GetCurrentDirectory(), "APP_DATA", "temp", name);
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(temp);

            IReplicator replicator = new HttpReplicator($"{url}/api/replicate/{name}");
            FSDirectory directory = FSDirectory.Open(path);
            Index index = new Index(directory, name);

            indices.Add(name, index);

            ReplicationClient client = new ReplicationClient(replicator, new IndexReplicationHandler(directory, index.UpdateIndex), new PerSessionDirectoryFactory(temp));
            client.UpdateNow();
            client.StartUpdateThread(5000, "Replicator Thread");
            replicators.Add(name, client);
            return index;
        }
    }

    public class Index
    {
        private const LuceneVersion VERSION = LuceneVersion.LUCENE_48;

        private readonly string name;
        private readonly FSDirectory directory;
        private readonly QueryParser parser = new QueryParser(VERSION, "content", new StandardAnalyzer(VERSION, CharArraySet.EMPTY_SET));
        private SearcherManager searcherManager;

        public Index(FSDirectory fsDirectory, string name)
        {
            this.directory = fsDirectory;
            this.name = name;
        }

        public bool? UpdateIndex()
        {
            searcherManager ??= new SearcherManager(directory, new SearcherFactory());
            searcherManager.MaybeRefresh();
            return true;
        }

        public string Search(string input)
        {
            Query query = parser.Parse(input);
            IndexSearcher searcher = searcherManager.Acquire();

            TopDocs results = searcher.Search(query, 5);

            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Search for: {query} in index '{name}' resulted in a total of {results.TotalHits} hits.");
            resultBuilder.AppendLine($"============================== RESULTS ==============================");
            foreach (string result in results.ScoreDocs
                .Select(doc => searcher.Doc(doc.Doc))
                .Select(doc =>
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"id: {doc.GetField("id").GetStringValue()}");
                    builder.AppendLine($" - last updated: {doc.GetField("updated").GetStringValue()} by {doc.GetField("by").GetStringValue()}");
                    builder.AppendLine($" - {doc.GetField("content").GetStringValue()}");
                    return builder.ToString();
                }))
            {
                resultBuilder.AppendLine(result);
                resultBuilder.AppendLine();
            }
            searcherManager.Release(searcher);
            return resultBuilder.ToString();
        }
    }
}
