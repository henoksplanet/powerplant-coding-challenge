using Newtonsoft.Json;

namespace LoadAPI
{

        public class Fuels
        {
            [JsonProperty("gas(euro/MWh)")]
            public double GasEuroMWh { get; set; }

            [JsonProperty("kerosine(euro/MWh)")]
            public double KerosineEuroMWh { get; set; }

            [JsonProperty("co2(euro/ton)")]
            public int Co2EuroTon { get; set; }

            [JsonProperty("wind(%)")]
            public int Wind { get; set; }
        }

        public class Powerplant
        {
            public string name { get; set; }
            public string type { get; set; }
            public double efficiency { get; set; }
            public double pmin { get; set; }
            public double pmax { get; set; }
        }

        public class Root
        {
            public int load { get; set; }
            public Fuels fuels { get; set; }
            public List<Powerplant>? powerplants { get; set; }
        }



        public class PayloadResponse
        {
            public string? name;
            public double p;

        }




    }
