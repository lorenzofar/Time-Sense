using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Database
{
    public sealed class AllowedContact
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string name { get; set; }
        public string number { get; set; }
    }
}
