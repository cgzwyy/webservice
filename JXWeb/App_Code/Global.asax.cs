using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Global 的摘要说明
/// </summary>
namespace H9000.DataInterFace
{
    public class Global : System.Web.HttpApplication
    {
        public Global()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        protected void Application_Start(Object sender, EventArgs e)
        {
            string applicationPath = Context.Server.MapPath(Context.Request.ApplicationPath);
            ConfigManager.OnApplicationStart(applicationPath);
        }
    }
}
