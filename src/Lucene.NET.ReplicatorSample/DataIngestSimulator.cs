using System;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Documents;
using Lucene.Net.Index;

namespace Lucene.NET.ReplicatorSample
{
    public class DataIngestSimulator
    {
        private readonly Random rnd = new Random();
        private readonly Guid[] ids = Enumerable.Repeat(0, 500).Select(ï => Guid.NewGuid()).ToArray();
        private readonly string[] words = "car,dog,plane,a,and,or,field,cargo,passenger,mold,bread,fox,bird,the,for,woman,man,age,date,word,wild,sport,forest,jump,train".Split(',');
        private readonly string[] names = "Peter,Carl,James,Karen,Natalie,Mathias,John,Kate,Josh,Patrick,Natasha,Angelina,Craig".Split(',');

        public void Start()
        {
            Task.Factory.StartNew(DataLoop, TaskCreationOptions.LongRunning);
        }
        private async Task DataLoop()
        {
            while (true)
            {
                foreach ((Term term, Document doc)  in Enumerable
                    .Range(0, NextRandomUpdateCount())
                    .Select(CreateDocument))
                {
                    Index.Instance.Write(term, doc);
                }

                Index.Instance.Commit();
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }

        private (Term term, Document doc) CreateDocument(int ignored = 0)
        {
            Guid id = NextRandomId();
            Document doc = new Document();
            doc.Add(new StringField("id", id.ToString("N"), Field.Store.YES));
            doc.Add(new TextField("content", NextRandomParagraph(), Field.Store.YES));
            doc.Add(new StringField("by", NextRandomName(), Field.Store.YES));
            doc.Add(new StringField("updated", DateTime.Now.ToString("s"), Field.Store.YES));
            return (new Term("id", id.ToString("N")), doc);
        }

        private int NextRandomUpdateCount() => rnd.Next(25);
        private Guid NextRandomId() => ids[rnd.Next(500)];
        private string NextRandomWord(int ignored = 0) => words[rnd.Next(words.Length)];
        private string NextRandomName() => names[rnd.Next(names.Length)];

        private string NextRandomSentence(int min = 10, int max = 25)
        {
            return string.Join(' ', 
                Enumerable
                    .Repeat(0, rnd.Next(min, max))
                    .Select(NextRandomWord)
            ) + ".";
        }

        private string NextRandomParagraph()
        {
            return string.Join(' ',
                Enumerable
                    .Repeat(0, rnd.Next(1, 5))
                    .Select(i => NextRandomSentence(rnd.Next(7, 14), rnd.Next(21, 30)))
            );
        }
    }
}