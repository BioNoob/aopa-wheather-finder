using System;
using HtmlAgilityPack;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace ConsoleApp1
{
    public class location
    {
        public location(double lat,double lon)
        {
            Lat = lat;
            Lon = lon;
        }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public static location operator -(location lc, location lc2)
        {
            lc.Lat = Math.Abs(lc.Lat - lc2.Lat);
            lc.Lon = Math.Abs(lc.Lon - lc2.Lon);
            return lc;
        }
        public static bool operator >(location lc, location lc2)
        {
            if (lc.Lat > lc2.Lat && lc.Lon > lc2.Lon)
                return true;
            else
                return false;
        }
        public static bool operator <(location lc, location lc2)
        {
            if (lc.Lat < lc2.Lat && lc.Lon < lc2.Lon)
                return true;
            else
                return false;
        }
    }
    /*
     * https://maps.aopa.ru/export/exportFormRequest/?exportType=standart&exportAll%5B%5D=airport&exportAll%5B%5D=vert&exportFormat=csv&csv_options%5Bcharset%5D=utf8&csv_options%5Bdata%5D=objects_data&f%5B%5D=name_ru&f%5B%5D=index&f%5B%5D=kta_lon&f%5B%5D=kta_lat&api_key=7380-9xJ8zG
     */
    class Program
    {
        public static string Searcher(location cur)
        {
            Dictionary<string, location> data_import = new Dictionary<string, location>();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"aopa-points-export.csv");
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    double lat = 0;
                    double lon = 0;
                    lat = Convert.ToDouble(values[2], CultureInfo.InvariantCulture);
                    lon = Convert.ToDouble(values[1], CultureInfo.InvariantCulture);
                    data_import.Add(values[0], new location(lat, lon));
                }
            }
            double step = 10;
            double step_step = 1;
            while (data_import.Count > 5)
            {
                //data_import = data_import.Where(t => Math.Abs(t.Value.Lat - cur.Lat) < step && Math.Abs(t.Value.Lon - cur.Lon) < step).ToDictionary(t => t.Key, t => t.Value);
                data_import = data_import.Where(t => (CalcDist(t.Value, cur)/1000) < step).ToDictionary(t => t.Key, t => t.Value);
                if (step == 1)
                {
                    step_step = 0.05;
                    step -= step_step;
                }
                else if (step <= 0.05)
                {
                    step_step = 0.005;
                    step -= step_step;
                }
                else
                {
                    step -= step_step;
                }
            }
            double dist = double.MaxValue;
            string minkey = "";
            foreach (var item in data_import)
            {
                double buf = CalcDist(item.Value, cur);
                Debug.WriteLine(buf + @"\" + item.Key);
                if (buf < dist)
                {
                    dist = buf;
                    minkey = item.Key;
                }
            }
            return minkey;
        }
        static void Main(string[] args)
        {
            location lc = new location(55.854354, 37.445411);
            string key = Searcher(lc);
            kek(key);

        }
        static double CalcDist(location a1,location a2)
        {
            double lat1 = a1.Lat * Math.PI / 180;
            double lat2 = a2.Lat * Math.PI / 180;
            double long1 = a1.Lon * Math.PI / 180;
            double long2 = a2.Lon * Math.PI / 180;

            // косинусы и синусы широт и разницы долгот
            double cl1 = Math.Cos(lat1);
            double cl2 = Math.Cos(lat2);
            double sl1 = Math.Sin(lat1);
            double sl2 = Math.Sin(lat2);
            double delta = long2 - long1;
            double cdelta = Math.Cos(delta);
            double sdelta = Math.Sin(delta);

            // вычисления длины большого круга
            double y = Math.Sqrt(Math.Pow(cl2 * sdelta, 2) + Math.Pow(cl1 * sl2 - sl1 * cl2 * cdelta, 2));
            double x = sl1 * sl2 + cl1 * cl2 * cdelta;
            //
            double ad = Math.Atan2(y, x);
            double dist = ad * 6372795;
            return dist;
        }
        static void kek(string key)
        {
            var url = $"http://meteocenter.asia/?m=gcc&p={key}";
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            //Encoding.RegisterProvider()
            // var t = Encoding.GetEncodings();
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            web.OverrideEncoding = Encoding.GetEncoding("windows-1251");
            var doc = web.Load(url);
            List<windout> lw = ParseAllTables(doc);
            lw = lw.Where(t => t.DateFormat >= DateTime.Now && t.DateFormat <= DateTime.Now.AddDays(1)).ToList();
            foreach (var item in lw)
            {
                Console.Write($"{item.Date} ;");
                foreach (var itemW in item._wind)
                {
                    Console.Write($"{itemW}");
                }
                Console.WriteLine();
            }

        }
        private static List<windout> ParseAllTables(HtmlDocument doc)
        {
            var rows = doc.DocumentNode.Descendants("tr");
            Regex rgT = new Regex("^[0-9]{2}$");
            Regex rgD = new Regex(@"^([0-9]{2}\.){2}[0-9]+$");
            Regex selc = new Regex(@"(?<Hi>(\([0-9]+.м\))+).(?<asim>[0-9]{3})\/(?<speed>[0-9]{1,3}).(?<temp>(\-)?[0-9]{1,2})");
            windout ww;
            List<windout> outlist = new List<windout>();
            foreach (var row in rows.Skip(1))
            {
                ww = new windout();
                var row_rw = row.Descendants("td");
                string Hour = string.Empty;
                string Data = string.Empty;
                string Wind = string.Empty;
                string temper = string.Empty;
                string asim = string.Empty;
                foreach (var cell in row_rw)
                {
                    var text = cell.InnerText;
                    if (rgT.IsMatch(text) && Hour == string.Empty)
                    {
                        Hour = text;
                        continue;
                    }
                    else if (rgD.IsMatch(text) && Data == string.Empty)
                    {
                        Data = text;
                        continue;
                    }
                    else if (text.Contains("...") && text.Contains("FL"))
                    {
                        Wind = text.Trim().Replace("...", "").Trim();
                        List<Winder> wr = new List<Winder>(); 
                        foreach (Match item in selc.Matches(Wind))
                        {
                            Winder wind = new Winder();
                            wind.Asim = item.Groups["asim"].Value;
                            wind.Wind = item.Groups["speed"].Value;
                            wind.Temp = item.Groups["temp"].Value;
                            wind.hi = item.Groups["Hi"].Value;
                            wr.Add(wind);
                        }
                        ww._wind = wr;
                        continue;
                    }
                }
                if (Hour != string.Empty && Data != string.Empty && Wind != string.Empty)
                {
                    ww.Date = Hour + ";" + Data;
                    outlist.Add(ww);
                }
            }
            return outlist;
        }
        public class Winder
        {
            public Winder()
            {

            }
            public override string ToString()
            {
                return $"{hi}. Ветер:{Asim}° - {Wind}км/ч. Температура {Temp}";
            }
            private double _asim;
            private double _temp;
            private double _wind;
            public string hi { get; set; }
            public string Wind
            {
                get
                {
                    return _wind.ToString(CultureInfo.InvariantCulture);
                }
                set
                {
                    double t = 0;
                    double.TryParse(value, out t);
                    _wind = t;
                }
            }
            public string Asim
            {
                get
                {
                    return _asim.ToString(CultureInfo.InvariantCulture);
                }
                set
                {
                    double t = 0;
                    double.TryParse(value, out t);
                    _asim = t;
                }
            }
            public string Temp
            {
                get
                {
                    if(_temp > 0)
                    {
                        return "+" + _temp.ToString(CultureInfo.InvariantCulture);
                    }
                    else
                        return _temp.ToString(CultureInfo.InvariantCulture);
                }
                set
                {
                    double t = 0;
                    double.TryParse(value, out t);
                    _temp = t;
                }
            }
        }
        public class windout
        {
            public windout()
            {
                _wind = new List<Winder>();
            }
            private DateTime _dt;
            public List<Winder> _wind { get; set; }

            public DateTime DateFormat
            {
                get
                {
                    return _dt;
                }
            }
            public string Date
            {
                get
                {
                    return _dt.ToString("HH:mm dd.MMMM.yyyy");
                }
                set
                {
                    //_dt = new DateTime();
                    var q = value.Split(';');
                    _dt = Convert.ToDateTime(q[1]);
                    double x = 0;
                    double.TryParse(q[0],out x);
                    _dt = _dt.AddHours(x);
                }
            }
        }
    }
}
