using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Database
{
    public sealed class Hour
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int hour { get; set; }
        public int usage { get; set; }
        public int unlocks { get; set; }
        public string date { get; set; }
    }
}
