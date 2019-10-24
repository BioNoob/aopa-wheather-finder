using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App1
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        //        Class1 cles;
        public MainPage()
        {
            InitializeComponent();
            gpp = new ResponsedData();
            gpp.temperature = 10;
            gpp.icon = "rain";
            gpp.precipIntensity = 0.0089f;
            gpp.precipProbability = 0.9f;
            gpp.precipType = "rain";
            gpp.temperature = 66.1f;
            gpp.humidity = 0.83f;
            gpp.pressure = 1010.34f;
            gpp.windSpeed = 5.59f;
            gpp.windGust = 12.03f;
            gpp.windBearing = 246;
            gpp.cloudCover = 0.7f;
            gpp.visibility = 9.84f;
            Place = "Москов";
            Time = DateTime.Now.ToShortDateString();
            Val = gpp.getParams();
            BindingContext = this;
        }

        public ResponsedData gpp { get; set; }
        public string Place { get; set; }
        public string Time { get; set; }
        public Dictionary<string, string> Val { get; set; }
    }
    public class ResponsedData
    {

        public DateTime duration { get; set; }
        public float temperature { get; set; }
        public float pressure { get; set; }
        private string _source;

        public float windSpeed { get; set; }
        public float windGust { get; set; }
        public float cloudCover { get; set; }
        public float windBearing { get; set; }
        public float humidity { get; set; }
        public string precipType { get; set; }
        public float precipIntensity { get; set; }
        public float precipProbability { get; set; }
        public float visibility { get; set; }

        public Dictionary<string, string> getParams()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("temperature", string.Format("+{0:#0.# C°}", temperature));
            dic.Add("pressure", string.Format("{0:#0 hPa}", pressure));
            dic.Add("windSpeed", string.Format("{0:#0.# м/с}", windSpeed));
            dic.Add("windGust", string.Format("{0:#0.# м/с}", windGust));
            dic.Add("humidity", string.Format(CultureInfo.InvariantCulture, "{0:P1}", humidity));
            dic.Add("precipIntensity", string.Format("{0:#0.# ?}", precipIntensity));
            dic.Add("visibility", string.Format("{0:#0.# км}", visibility));
            dic.Add("precipProbability", string.Format(CultureInfo.InvariantCulture, "{0:P1}", precipProbability));
            dic.Add("windBearing", string.Format("{0:#0 °}", windBearing));
            return dic;
        }
        public ResponsedData()
        {

        }
        public string icon
        {
            get { return _source; }
            set
            {
                switch (value)
                {
                    case "clear-day":
                    case "clear-night":
                    case "partly-cloudy-day":
                    case "partly-cloudy-night":
                        _source = value.Replace('-', '_') + ".png";
                        break;
                    case "rain":
                    case "snow":
                    case "sleet":
                    case "wind":
                    case "fog":
                    case "cloudy":
                        _source = value + ".png";
                        break;
                    default:
                        _source = "def.png";
                        break;
                }
            }
        }
    }
}
