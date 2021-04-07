using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XCentium.Models
{
    public class FormModel
    {
        public string UserInputUrl { get; set; }
        public List<string> Images { get; set; }
        public List<WordCount> Words { get; set; }
        public int TotalWordCount { get; set; }
        public string ErrorText { get; set; }
    }

    public class WordCount
    {
        public string Word { get; set; }
        public int Count { get; set; }
    }
}
