using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGA.Models
{
    class MessageData
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime? EditDate { get; set; }
        public MessageAuthor Author { get; set; }
        public string Content { get; set; }

        public string Category { get; set; }
        public int Views { get; set; }
        public int TotalReactionsCount { get; set; }
        public List<Reaction> Reactions { get; set; }
    }
}
