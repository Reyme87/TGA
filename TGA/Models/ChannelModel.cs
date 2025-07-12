using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGA.Models
{
    class ChannelModel
    {
        private long _id;
        private string _name;
        private string _author;

        public long Id
        {
            get => _id;
            set => _id = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Author
        {
            get => _author;
            set => _author = value;
        }

        public ChannelModel(long id, string name, string author)
        {
            Id = id;
            Name = name;
            Author = author;
        }
    }
}
