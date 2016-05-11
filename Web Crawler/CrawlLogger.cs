using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Web_Crawler
{
    public class CrawlLogger
    {
        private BlockingCollection<string> _collection;

        public CrawlLogger(string file_path)
        {
            _collection = new BlockingCollection<string>(10000);
            var t = Task.Factory.StartNew(() =>
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(file_path))
                {
                    while (true)
                    {
                        var text = _collection.Take();
                        try {
                            file.WriteLine(text);
                            file.Flush();
                        }
                    catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }                        
                    }
                }

            }, TaskCreationOptions.LongRunning);

        }

        public void AddLog(string logItem)
        {
            _collection.Add(logItem);         
        }
    }
}