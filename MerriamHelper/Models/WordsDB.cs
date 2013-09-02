using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace WordHelp.Models
{
    public class WordsDB
    {
        public DbSet<Word> Words { get; set; }
        public String Path { get; set; }
        
    }
}