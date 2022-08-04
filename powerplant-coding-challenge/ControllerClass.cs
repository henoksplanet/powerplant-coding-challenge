using Newtonsoft.Json;

namespace LoadAPI
{


    public class LoadBalancing : Root
    {

        const string WIND = "windturbine";
        const string GASFIRED = "gasfired";
        const string TURBOJET = "turbojet";

        public Root? m_payload = new Root();
        List<Powerplant> m_powerplants = new List<Powerplant>(); //final sorted power plants

        public List<Powerplant> m_windPowerUnit = new List<Powerplant>();
        public List<Powerplant> m_gasFiredUnits = new List<Powerplant>();
        public List<Powerplant> m_turboJetUnits = new List<Powerplant>();
        List<PowerplantExtended> m_powerplantExtended = new List<PowerplantExtended>();


        public static int CompareWindPowerUnits(Powerplant pp1, Powerplant pp2)
        {
            return pp2.pmax.CompareTo(pp1.pmax); // increasing order sorting 
        }

        public int ComparegasFiredUnits(Powerplant pp1, Powerplant pp2)
        {
            double costEuro = m_payload.fuels.GasEuroMWh;

            if (costEuro / pp1.efficiency > costEuro / pp2.efficiency)
            {
                return 1;      //put it at the start
            }
            else if (costEuro / pp1.efficiency < costEuro / pp2.efficiency)
            {
                return -1;     // put it at the end
            }
            else if (costEuro / pp1.efficiency == costEuro / pp2.efficiency)  //case when both have equal efficiency
            {
                if (pp1.pmax > pp2.pmax)
                {
                    return -1;  // good thing for pp1 casuse pmax is good
                }
                else if (pp1.pmax < pp2.pmax)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0; // throw an exception or error;
            }

        }

        public int CompareTurboJetUnits(Powerplant pp1, Powerplant pp2)
        {
            double costEuro = m_payload.fuels.KerosineEuroMWh;

            if (costEuro / pp1.efficiency > costEuro / pp2.efficiency)
            {
                return 1;      //bad thing cause cost is more
            }
            else if (costEuro / pp1.efficiency < costEuro / pp2.efficiency)
            {
                return -1;
            }
            else if (costEuro / pp1.efficiency == costEuro / pp2.efficiency)  //case when both have equal efficiency
            {
                if (pp1.pmax > pp2.pmax)
                {
                    return -1;  // good thing for pp1 casuse pmax is good
                }
                else if (pp1.pmax < pp2.pmax)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0; // throw an exception or error;
            }

        }


        public LoadBalancing(Root payload)
        {
            m_payload = payload;

            foreach (Powerplant powerplant in m_payload.powerplants)
            {

                switch (powerplant.type)
                {
                    case WIND:
                        m_windPowerUnit.Add(powerplant);
                        break;
                    case GASFIRED:
                        m_gasFiredUnits.Add(powerplant);
                        break;
                    case TURBOJET:
                        m_turboJetUnits.Add(powerplant);
                        break;
                    default:
                        // throw exception;
                        break;
                }
            }

            Comparison<Powerplant> m_windTurbineComparer = new Comparison<Powerplant>(CompareWindPowerUnits);
            Comparison<Powerplant> m_gasFiredTurbineComparer = new Comparison<Powerplant>(ComparegasFiredUnits);
            Comparison<Powerplant> m_turboJetTurbineComparer = new Comparison<Powerplant>(CompareTurboJetUnits);

            m_windPowerUnit.Sort(m_windTurbineComparer);
            m_gasFiredUnits.Sort(m_gasFiredTurbineComparer);
            m_turboJetUnits.Sort(m_turboJetTurbineComparer);

            m_powerplants.AddRange(m_windPowerUnit);//appends to the end of the list so that all are put in increasing order
            m_powerplants.AddRange(m_gasFiredUnits);
            m_powerplants.AddRange(m_turboJetUnits);

            foreach (Powerplant powerplants in m_powerplants)
            {
                PowerplantExtended powerplantExtended_temp = JsonConvert.DeserializeObject<PowerplantExtended>(JsonConvert.SerializeObject(powerplants));
                m_powerplantExtended.Add(powerplantExtended_temp);
            }

        }


        public List<Powerplant> windTurbines
        {
            get
            {
                return m_windPowerUnit;
            }
            set
            {

                m_windPowerUnit = value;
            }

        }

        public List<Powerplant> gasFiredTurbines
        {
            get
            {
                return m_gasFiredUnits;
            }
            set
            {

                m_gasFiredUnits = value;
            }

        }

        public List<Powerplant> turboJetTurbines
        {
            get
            {
                return m_turboJetUnits;
            }
            set
            {

                m_windPowerUnit = value;
            }

        }

        public List<Powerplant> allTurbines
        {
            get
            {
                return m_powerplants;
            }
            set
            {

                m_powerplants = value;
            }

        }


        public List<PowerplantExtended> allTurbinesExtended
        {
            get
            {
                return m_powerplantExtended;
            }
            set
            {

                m_powerplantExtended = value;
            }

        }



        public class PowerplantExtended : Powerplant
        {
            public double p { get; set; } = 0;
        }


        //unit commitment logic

        public double loadDistribution()
        {
            foreach (PowerplantExtended powerplant in allTurbinesExtended)
            {
                add(powerplant);

                if (m_payload.load - loadMet() <= 0)
                {
                    break;
                }
            }

            if (loadMet() == m_payload.load)
            {
                return loadMet();
            }
            else
            {
                throw new Exception("load demand can't be mate!");
            }

        }

        public int add(PowerplantExtended powerplant)
        {
            if (loadMet() + powerplant.pmax <= m_payload.load)
            {
                ((PowerplantExtended)allTurbinesExtended.Where(s => powerplant.name == s.name).ToList()[0]).p = powerplant.pmax;
 
                return 0;
            }

            if (m_payload.load - loadMet() >= powerplant.pmin && m_payload.load - loadMet() <= powerplant.pmax && m_payload.load - loadMet() >= 0 && loadMet() + powerplant.pmax >= m_payload.load) //far right situation
            {

                ((PowerplantExtended)allTurbinesExtended.Where(s => powerplant.name == s.name).ToList()[0]).p = m_payload.load - loadMet();  //fills the difference with the last power plant 

                return 0;
            }

            if (m_payload.load - loadMet() < powerplant.pmin) //adjusting backwards
            {
                //first make it excess

                ((PowerplantExtended)allTurbinesExtended.Where(s => powerplant.name == s.name).ToList()[0]).p = powerplant.pmin;
                int currentIndex = allTurbinesExtended.IndexOf(powerplant);

                if (currentIndex - 1 < 0)
                {
                    throw new InvalidOperationException("No unit commitment coombination can be determined for the load/payload combination");

                }
                if ((allTurbinesExtended[currentIndex - 1].p + m_payload.load - loadMet()) >= allTurbinesExtended[currentIndex - 1].pmin)  //just one step back
                {
                    allTurbinesExtended[currentIndex - 1].p += m_payload.load - loadMet(); //decrement from previous
                    return 0;
                }

                for (int i = 1; i < allTurbinesExtended.Count; i++)   //if we have to go back further back
                {
                    if (currentIndex - i < 0 || currentIndex - (i + 1) < 0)
                    {
                        throw new InvalidOperationException("No unit commitment coombination can be determined for the load/payload combination");
                        break;

                    }
                    if ((allTurbinesExtended[currentIndex - i].p + m_payload.load - loadMet()) < allTurbinesExtended[currentIndex - i].pmin)
                    {
                        allTurbinesExtended[currentIndex - i].p = allTurbinesExtended[currentIndex - 1].pmin;
                        allTurbinesExtended[currentIndex - (i + 1)].p += m_payload.load - loadMet();
 
                    }
                    if (loadMet() == m_payload.load)
                    {
                        break;
                        return 0;
                    }
                }


            }

            return 0;
        }


        public bool validateAllTurbineLoad()  // validates if all Pmin/Pmax rules are respected
        {
            foreach (PowerplantExtended powerplantExtended in allTurbinesExtended)
            {
                if (powerplantExtended.p < powerplantExtended.pmin && powerplantExtended.p > powerplantExtended.pmax)
                {
                    return false;
                }
            }
            return true;
        }

        public double loadMet()
        {

            double loadTot_tmp = 0;
            foreach (PowerplantExtended powerplantExtended in allTurbinesExtended)
            {
                loadTot_tmp += powerplantExtended.p;
            }
            return loadTot_tmp;
        }
    }


}
