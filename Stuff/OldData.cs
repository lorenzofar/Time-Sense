using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stuff
{
    public sealed class OldData
    {
        public class Report
        {
            public int Id { get; set; }

            public string data { get; set; }
            public string utilizzo { get; set; }
            public string sblocchi { get; set; }
            public string percentuale { get; set; }
            public int percentuale_value { get; set; }
            public int fascia0 { get; set; }
            public int fascia1 { get; set; }
            public int fascia2 { get; set; }
            public int fascia3 { get; set; }
            public int fascia4 { get; set; }
            public int fascia5 { get; set; }
            public int fascia6 { get; set; }
            public int fascia7 { get; set; }
            public int fascia8 { get; set; }
            public int fascia9 { get; set; }
            public int fascia10 { get; set; }
            public int fascia11 { get; set; }
            public int fascia12 { get; set; }
            public int fascia13 { get; set; }
            public int fascia14 { get; set; }
            public int fascia15 { get; set; }
            public int fascia16 { get; set; }
            public int fascia17 { get; set; }
            public int fascia18 { get; set; }
            public int fascia19 { get; set; }
            public int fascia20 { get; set; }
            public int fascia21 { get; set; }
            public int fascia22 { get; set; }
            public int fascia23 { get; set; }
        }

        public class Timeline
        {
            public int Id { get; set; }

            public string data { get; set; }
            public string orario { get; set; }
            public string utilizzo { get; set; }
            public string sblocchi { get; set; }
            public string batteria { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
        }

        public class Report_oggi_time
        {
            public int Id { get; set; }

            public string fascia { get; set; }
            public int uso { get; set; }
            public int sblocchi { get; set; }
            public string data { get; set; }
        }
    }
}
