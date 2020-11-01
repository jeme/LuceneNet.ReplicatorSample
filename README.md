# LuceneNet.ReplicatorSample

This provides a very basic and crude example of how to use the Lucene.NET Replicator with multiple indices.
To play with the project, download it, start the server (AspNetCore) and wait untill it has indexed on it's first turn (there should be a segemnts_{NUMBER} file in each of the 3 index directories)
Then you should be able to start the client and begin to run Queries against the replicated indexes.
E.g. enter "*:*" to do a MatchAll search (This can verify that it has replicated and that data has come across)

Note that this should only be seen as an inspiration, the Quality of the code does not even come close to anything I would recommend, but I hope it can make you get started with the Lucene.NET replicator.
