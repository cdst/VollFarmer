using System.Collections.Generic;
using System.ComponentModel;
using log4net;
using Loki;
using Loki.Common;
using Loki.Game;
using Newtonsoft.Json;

namespace VollFarmer
{
    /// <summary>Settings for the Dev tab. </summary>
    public class VollFarmerSettings : JsonSettings
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();

        private static VollFarmerSettings _instance;

        /// <summary>The current instance for this class. </summary>
        public static VollFarmerSettings Instance
        {
            get
            {
                return _instance ?? (_instance = new VollFarmerSettings());
            }
        }

        /// <summary>The default ctor. Will use the settings path "VollFarmer".</summary>
        public VollFarmerSettings()
            : base(GetSettingsFilePath(Configuration.Instance.Name, string.Format("{0}.json", "VollFarmer")))
        {
        }

        private string _BossVoll;
        private string _vollFarmerDifficulty;


        /// <summary>The grind zone area the bot will travel to for grinding.</summary>
        [DefaultValue("The Dried Lake")]
        public string VollFarmer1Name
        {
            get
            {
                return _BossVoll;
            }
            set
            {
                if (value.Equals(_BossVoll))
                {
                    return;
                }
                _BossVoll = value;
            }
        }


        /// <summary>The grind zone difficulty.</summary>
        [DefaultValue("Merciless")]
        public string VollFarmerDifficulty
        {
            get
            {
                return _vollFarmerDifficulty;
            }
            set
            {
                if (value.Equals(_vollFarmerDifficulty))
                {
                    return;
                }
                _vollFarmerDifficulty = value;
                NotifyPropertyChanged(() => VollFarmerDifficulty);
            }
        }
        /// <summary>
        /// Returns the current grind zone id based on the name and difficulty.
        /// </summary>
        [JsonIgnore]
        public string VollFarmer1Id
        {
            get
            {
                return LokiPoe.GetZoneId(VollFarmerDifficulty, VollFarmer1Name);
            }
        }

        private List<string> _allGrindZoneDifficulty;

        /// <summary> </summary>
        [JsonIgnore]
        public List<string> AllGrindZoneDifficulty
        {
            get
            {
                return _allGrindZoneDifficulty ?? (_allGrindZoneDifficulty = new List<string>
                {
                    "Normal",
                    "Cruel",
                    "Merciless"
                });
            }
        }

        private List<string> _allGrindZoneNames;

        /// <summary> </summary>
        [JsonIgnore]
        public List<string> AllGrindZoneNames
        {
            get
            {
                return _allGrindZoneNames ?? (_allGrindZoneNames = new List<string>
                {
                    "The Dried Lake",

                });
            }
        }
    }
}