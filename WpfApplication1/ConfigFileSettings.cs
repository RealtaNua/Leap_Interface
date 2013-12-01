using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;


namespace LeapTouchPoint
{
    public class ConfigFileSettings
    {
        public Dictionary<string,string> getDefaultConfigParameters()
        {
            Dictionary<string, string> configDic = new Dictionary<string,string>();
            
            configDic["fineSensitivitySlider"] = System.Configuration.ConfigurationManager.AppSettings["fineSensitivitySlider"];
            configDic["yAxisSlider"]           = System.Configuration.ConfigurationManager.AppSettings["yAxisSlider"];
            configDic["secondsBeforeLocking"]  = System.Configuration.ConfigurationManager.AppSettings["secondsBeforeLocking"];

            return configDic;
        }

        public void setConfigParameters(Dictionary<string,string> configDic)
        {
            UpdateSettings("fineSensitivitySlider", configDic["fineSensitivitySlider"]);
            UpdateSettings("yAxisSlider", configDic["yAxisSlider"]);
            UpdateSettings("secondsBeforeLocking", configDic["secondsBeforeLocking"]);
        }

        private void UpdateSettings(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("appSettings");
        }
    }


}
