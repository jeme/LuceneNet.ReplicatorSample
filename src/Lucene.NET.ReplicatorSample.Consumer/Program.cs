using System;
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
            Index.Instance.Initialize(url);

            while (true)
            {
                string input = Console.ReadLine();

                if(input != null && input.ToUpper() == "EXIT")
                    return;

                try
                {
                    Console.WriteLine(Index.Instance.Search(input));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
    }

    public class Index
    {
        private const LuceneVersion VERSION = LuceneVersion.LUCENE_48;
        public static Index Instance = new Index();

        private FSDirectory directory;
        private SearcherManager searcherManager;
        private QueryParser parser = new QueryParser(VERSION, "content", new StandardAnalyzer(VERSION, CharArraySet.EMPTY_SET));

        public bool? UpdateIndex()
        {
            searcherManager ??= new SearcherManager(directory, new SearcherFactory());
            searcherManager.MaybeRefresh();
            return true;
        }

        public void Initialize(string url)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "APP_DATA", "INDEX");
            string temp = Path.Combine(Directory.GetCurrentDirectory(), "APP_DATA", "TEMP");
            Directory.CreateDirectory(path);
            Directory.CreateDirectory(temp);
            
            IReplicator replicator = new HttpReplicator($"{url}/api/replicate/shard_name");
            directory = FSDirectory.Open(path);
            ReplicationClient client = new ReplicationClient(replicator, new IndexReplicationHandler(directory, UpdateIndex), new PerSessionDirectoryFactory(temp));

            client.UpdateNow();
            client.StartUpdateThread(5000, "Replicator Thread");
        }

        public string Search(string input)
        {
            Query query = parser.Parse(input);
            IndexSearcher searcher = searcherManager.Acquire();

            TopDocs results = searcher.Search(query, 5);

            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Search for: {query} resulted in a total of {results.TotalHits} hits.");
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
