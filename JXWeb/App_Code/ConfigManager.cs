using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;
using System.Collections;
using Database;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using WebDataSource;





namespace H9000.DataInterFace
{
    /// <summary>
    /// ConfigManager 的摘要说明
    /// </summary>
    public class ConfigManager : MarshalByRefObject
    {

        public static string DBIP;

        enum settingType
        {
            String = 0,
            Int = 1,
            Float = 2,
            Boolean = 3,
            DBType = 4,
            Double = 5
        }
      

        public ConfigManager()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        public static void OnApplicationStart(string path)
        {
            // Set path to the application's root folder and configure the application
            InitializeApplication(ConfigurationManager.AppSettings);//ConfigurationSettings.AppSettings);
        }
        public static void InitializeApplication(NameValueCollection settings)
        {

            WebDataSource.ConfigManager.InitializeApplication(settings);

            DBIP = (string)settingParse(settings, "DBIP", settingType.String);

        }

        private static object settingParse(NameValueCollection settings, string settingName, settingType type)
        {
            string settingValue = settings[settingName];
            object ret = settingValue;
            switch (type)
            {
                case settingType.String:
                    ret = settingValue;
                    break;
                case settingType.Int:
                    try
                    {
                        ret = 0;
                        if (settingValue != null) ret = int.Parse(settingValue);
                    }
                    catch { return 0; }
                    break;
                case settingType.Float:
                    try
                    {
                        ret = 0.0f;
                        if (settingValue != null) ret = float.Parse(settingValue);
                    }
                    catch { return 0; }
                    break;
                case settingType.Boolean:
                    try
                    {
                        ret = false;
                        if (settingValue != null) ret = Convert.ToBoolean(settingValue);
                    }
                    catch { return false; }
                    break;
                case settingType.Double:
                    try
                    {
                        ret = (double)0;
                        if (settingValue != null) ret = double.Parse(settingValue);
                    }
                    catch { return (double)0; }
                    break;
                default:
                    ret = settingValue;
                    break;
            }
            return ret;
        }
    }
}
