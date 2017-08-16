using System;

namespace GitSharpApi.Models
{
    public class Commit
    {
        public string Hash { get; set; }
        public string Author { get; set; }
        public DateTime AuthorDate { get; set; }
        public string Message { get; set; }
    }
}