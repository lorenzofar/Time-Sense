using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Database
{
    [Table("Report")]
    public sealed class Report
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string date { get; set; }
        public int usage { get; set; }
        public int unlocks { get; set; }
        public string note { get; set; }
    }
}
