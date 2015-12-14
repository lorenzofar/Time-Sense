using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Database
{
    [Table("timeline")]
    public sealed class Timeline
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string date { get; set; }
        public string time { get; set; }
        public double usage { get; set; }
        public int unlocks { get; set; }
        public int battery { get; set; }
        public string battery_status { get; set; }
        public string bluetooth_status { get; set; }
        public string wifi_status { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
