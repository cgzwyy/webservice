using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using WebDataSource;
using System.Data.Odbc;
using System.Data;
using System.Collections.Specialized;
using System.Threading;
using System.IO;
using System.Collections;
using MySql.Data.MySqlClient;
// using System.Text.StringBuilder;

namespace H9000.DataInterFace
{/// <summary>
    /// H9000Service 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class H9000Service : System.Web.Services.WebService
    {
        public static DataSource ds;
        private static MySqlConnection cn;
        private static MySqlCommand cmd;
        private static MySqlDataAdapter MyAdapter;
        private static string MyConnectionStr = "Database=xopensdb;Server="+ConfigManager.DBIP+";Uid=h9000;Pwd=ems;charset=utf8";
        private static string MyDisDataConnectionStr = "Database=xopenshdb;Server=" + ConfigManager.DBIP + ";Uid=h9000;Pwd=ems;charset=utf8";


        private void checkCn(string myconnection)
        {
            try
            {
                cn = new MySqlConnection(myconnection);
                cmd = cn.CreateCommand();
                MyAdapter = new MySqlDataAdapter(cmd);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "连接数据库失败！");
            }
        }

        private DataSet ExecSql(String sql)
        {
            DataSet Rds = new DataSet();
            try
            {
                if (cn.State.GetHashCode() == 0)
                {
                    cn.Open();
                }
                if (sql != null)
                {
                    cmd.CommandText = sql;
                }
                MyAdapter.SelectCommand = cmd;
                MyAdapter.Fill(Rds);
                MyAdapter.Dispose();
                cmd.Dispose();
                return Rds;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "执行查询失败！");
            }
        }

        private static int DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
            int datediff = 0;
            TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
            TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
            TimeSpan ts = ts1.Subtract(ts2).Duration();
            datediff = ts.Days;
            return datediff;
        }


        [WebMethod(Description = "登录验证")]
        public string isUser(string username, string password)
        {
            WriteLogFile("调用方法isUser，传入参数username的值为：" + username + ",password的值为：" + password);
            if (String.IsNullOrEmpty(username) == true)   //判断传入参数是否为空
            {
                WriteLogFile("调用方法isUser失败，传入参数为空");
                return "用户名为空";
            }
            else 
            {
                DataSet Rds = new DataSet();
                try {
                    checkCn(MyConnectionStr);
                    string sql = "select rights,ranges,levels from appUser where trim(username)='" + username + "' and trim(password)='";
                    if (String.IsNullOrEmpty(password))
                    {
                        sql += "'";
                    }
                    else {
                        sql += password + "'";
                    }
                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法isUser失败，没有找到该用户数据！");
                        return "用户名或密码错误";

                    }
                    else {

                        WriteLogFile("调用方法isUser成功，获取到的数据结果为：" + Rds.Tables[0].Rows[0][0].ToString().Trim());
                        StringBuilder json = new StringBuilder();

                        json.Append("{\"rights\":\"" + Rds.Tables[0].Rows[0][0].ToString().Trim() + "\",\"ranges\":\"" + Rds.Tables[0].Rows[0][1].ToString().Trim()
                            + "\",\"level\":\"" + Rds.Tables[0].Rows[0][2].ToString().Trim() + "\"}");

                        return json.ToString();
                    }  

                }
                catch (Exception err) {
                    GC.Collect();
                    return "获取信息失败" + err.Message;
                }
                finally {
                    cn.Close();
                    cn.Dispose();
                }
            }
        }

        [WebMethod(Description = "获取用户列表信息")]
        public string getUserInfo()
        {
            WriteLogFile("调用方法getUserInfo");

            DataSet Rds = new DataSet();
            try
            {
                checkCn(MyConnectionStr);
                string sql = "select * from appUser";
                //SQL做成
                Rds = ExecSql(sql);

                if (Rds.Tables[0].Rows.Count < 1)
                {
                    WriteLogFile("调用方法getUserInfo失败，没有找到用户数据！");
                    return "未找到数据";

                }
                StringBuilder json = new StringBuilder();

                json.Append("{\"userlist\":[");

                for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        json.Append("{\"userName\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\",\"userPassword\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"userRights\":\""
                            + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"userRange\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\",\"userLevel\":\"" + Rds.Tables[0].Rows[i][4].ToString().Trim() + "\"}");
                    }
                    else {
                        json.Append(",{\"userName\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\",\"userPassword\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"userRights\":\""
                            + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"userRange\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\",\"userLevel\":\"" + Rds.Tables[0].Rows[i][4].ToString().Trim() + "\"}");
                    }
                    
                }

                json.Append("]}");

                WriteLogFile("调用方法getUserInfo成功，获取到的数据结果为：" + json);

                return json.ToString();

            }
            catch (Exception err)
            {
                GC.Collect();
                return "获取信息失败" + err.Message;
            }
            finally
            {
                cn.Close();
                cn.Dispose();
            }
        }

        [WebMethod(Description = "登录验证")]
        public string addUser(string username, string password, string rights, string range, string level)
        {
            WriteLogFile("调用方法addUser，传入参数username的值为：" + username + ",password的值为：" + password);
            if (String.IsNullOrEmpty(username) == true || String.IsNullOrEmpty(rights) == true || String.IsNullOrEmpty(level) || String.IsNullOrEmpty(range))   //判断传入参数是否为空
            {
                WriteLogFile("调用方法addUser失败，传入参数为空");
                return "参数为空";
            }
            else
            {
                try
                {
                    checkCn(MyConnectionStr);
                    string sql = "insert into xopensdb.appUser values('" + username + "','";
                    if (String.IsNullOrEmpty(password))
                    {
                        sql += "',";
                    }
                    else {
                        sql += password + "',";
                    }
                    sql += "'" + rights + "','" + range + "'," + level + ")";
                    //SQL做成
                    ExecSql(sql);
                    return "true";
                  
                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取信息失败" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
            }
        }

        [WebMethod(Description = "登录验证")]
        public string deleteUser(string username)
        {
            WriteLogFile("调用方法deleteUser，传入参数username的值为：" + username);
            if (String.IsNullOrEmpty(username) == true )   //判断传入参数是否为空
            {
                WriteLogFile("调用方法deleteUser失败，传入参数为空");
                return "参数为空";
            }
            else
            {
                try
                {
                    checkCn(MyConnectionStr);
                    string sql = "delete from xopensdb.appUser where userName='" + username + "'";                  
                    //SQL做成
                    ExecSql(sql);
                    return "true";

                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取信息失败" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
            }
        }

        [WebMethod(Description = "登录验证")]
        public string updateUser(string username, string password, string rights, string range, string level)
        {
            WriteLogFile("调用方法updateUser，传入参数username的值为：" + username);
            if (String.IsNullOrEmpty(username) == true || String.IsNullOrEmpty(rights) == true || String.IsNullOrEmpty(level) || String.IsNullOrEmpty(range))   //判断传入参数是否为空
            {
                WriteLogFile("调用方法updateUser失败，传入参数为空");
                return "参数为空";
            }
            else
            {
                try
                {
                    checkCn(MyConnectionStr);
                    string sql = "update xopensdb.appUser set password='";
                    if (String.IsNullOrEmpty(password))
                    {
                        sql += "',";
                    }
                    else
                    {
                        sql += password + "',";
                    }
                    sql += " rights='" + rights + "', ranges='" + range + "', levels=" + level + " where userName='" + username + "'";
                    WriteLogFile(sql);
                    //SQL做成
                    ExecSql(sql);
                    return "true";

                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取信息失败" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
            }
        }

        [WebMethod(Description = "传入电厂ID，获取该电厂对应的数据")]
        public string GetStationInfo(string Str_StationCode)
        {
            WriteLogFile("调用方法GetStationInfo，传入参数Str_StationCode的值为：" + Str_StationCode);
            if (String.IsNullOrEmpty(Str_StationCode) == true)
            {
                WriteLogFile("调用方法GetStationInfo失败，传入参数为空");
                return "请输入场站ID";
            }
            else
            {
                DataSet Rds = new DataSet();
                try
                {
                    //DB连接设定
                    checkCn(MyConnectionStr);
                    string sql = "SELECT a.场站名称,a.表示名称,b.data FROM 手机表示配置表 a join webservice b on a.遥测遥信代码=b.sname where 1=1 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";
                    if (String.IsNullOrEmpty(Str_StationCode) == false)
                    {
                        sql += " AND RTRIM(场站代码) = '" + Str_StationCode + "'";
                    }

                    sql += " AND trim(分类项目)='stationInfo' AND 风机编号='0'";

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetStationInfo失败，没有找到配置数据！");
                        return "没有找到GetStationInfo配置数据！";

                    }
                    StringBuilder json = new StringBuilder();

                    json.Append("{\"场站名称\":\"" + Rds.Tables[0].Rows[0][0].ToString().Trim() + "\"");

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        json.Append(",\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"");

                    }

                    //获取每台风机详细信息
                    string sql2 = "SELECT a.风机编号,a.表示名称,b.data FROM 手机表示配置表 a join webservice b on a.遥测遥信代码=b.sname where 1=1 ";
                    if (String.IsNullOrEmpty(Str_StationCode) == false)
                    {
                        sql2 += " and RTRIM(场站代码) = '" + Str_StationCode + "'";
                    }
                    sql2 += " AND trim(分类项目)='WTGSInfo' AND trim(风机编号)<>'0' order by 风机编号";
                    //SQL做成
                    DataSet Rds2 = ExecSql(sql2);

                    if (Rds2.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetWTGSInfo失败，没有找到配置数据！");
                        return "没有找到GetWTGSInfo配置数据！";

                    }

                    string old_num = Rds2.Tables[0].Rows[0][0].ToString();
                    string new_num = Rds2.Tables[0].Rows[0][0].ToString();

                    json.Append(",\"list\":[{\"编号\":\"" + new_num.Trim() + "\"");

                    for (int i = 0; i < Rds2.Tables[0].Rows.Count; i++)
                    {
                        new_num = Rds2.Tables[0].Rows[i][0].ToString();
                        if (new_num == old_num)
                        {
                            json.Append(",\"" + Rds2.Tables[0].Rows[i][1].ToString().Trim() + "\":\"" + Rds2.Tables[0].Rows[i][2].ToString().Trim() + "\"");
                        }
                        else
                        {
                            json.Append("},{\"编号\":\"" + new_num.Trim() + "\"");
                            json.Append(",\"" + Rds2.Tables[0].Rows[i][1].ToString().Trim() + "\":\"" + Rds2.Tables[0].Rows[i][2].ToString().Trim() + "\"");
                            old_num = new_num;
                        }

                    }
                    json.Append("}]}");

                    WriteLogFile("调用方法GetStationInfo成功，获取到的数据结果为：" + json);

                    return json.ToString();
                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取厂站信息错误" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
            }

        }


        [WebMethod(Description = "传入时间、两个遥测代码，获取实时功率、预测功率数据")]
        public String GetStationP(string sdate, string pcode, string forecase_pcode)
        {
            WriteLogFile("调用方法GetStationP，传入参数的值为：" + sdate + "  " + pcode + "  " + forecase_pcode);
            //传入日期和有功功率代码不能为空
            if (String.IsNullOrEmpty(sdate) == true || String.IsNullOrEmpty(pcode) == true || String.IsNullOrEmpty(forecase_pcode) == true)
            {
                WriteLogFile("调用方法GetStationP失败，传入参数为空");
                return "请输入参数";
            }
            else
            {
                DateTime date1 = Convert.ToDateTime("1970-01-01").AddDays(Convert.ToDouble(sdate));
                string tablename = "data" + date1.ToString("yyyyMM");

                int yesterday = Convert.ToInt16(sdate) - 1;
         
                DataSet Rds = new DataSet();
                try
                {
                    //DB连接设定
                    checkCn(MyDisDataConnectionStr);
                    string sql = "SELECT sdate,time,data from " + tablename + " where flag=1 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";
                    
                    sql += " AND RTRIM(sname) = '" + pcode + "'";

                    sql += " AND (trim(sdate)=" + sdate + " or (trim(sdate)=" + yesterday + " and time=1440) )";

                    sql += " order by sdate,time";

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetStationP失败，没有找到配置数据！");
                        return "没有找到GetStationP配置数据！";

                    }
                    StringBuilder json = new StringBuilder();
                    int maxtime = 0;

                    json.Append("{\"pcode\":[");
                    int oldtime = 0;
                    int newtime = 0;

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            json.Append("{\"time\":\"0\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = 0;
                        }
                        else 
                        {
                            json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = Convert.ToInt16(Rds.Tables[0].Rows[i][1].ToString());
                        }
                        if (maxtime < newtime - oldtime) {
                            maxtime = newtime - oldtime;
                        }

                        oldtime = newtime;
                        
                    }

                    json.Append("]");

                    
                    sql = "SELECT sdate,time,data from " + tablename + " where flag=1 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                    sql += " AND RTRIM(sname) = '" + forecase_pcode + "'";

                    sql += " AND (trim(sdate)=" + sdate + " or (trim(sdate)=" + yesterday + " and time=1440) )";

                    sql += " order by sdate,time";

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetStationP失败，没有找到配置数据！");
                        return "没有找到GetStationP配置数据！";

                    }
                    
                    json.Append(",\"forecast_pcode\":[");

                    oldtime = 0;
                    newtime = 0;

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            json.Append("{\"time\":\"0\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = 0;
                        }
                        else
                        {
                            json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = Convert.ToInt16(Rds.Tables[0].Rows[i][1].ToString());
                        }
                        if (maxtime < newtime - oldtime)
                        {
                            maxtime = newtime - oldtime;
                        }

                        oldtime = newtime;

                    }

                    json.Append("]");
                    //json.Append(",\"maxtime\":\"" + maxtime +"\"}");
                    
                    json.Append("}");

                    return json.ToString();
                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取厂站信息错误" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
                 
            }

        }

        [WebMethod(Description = "传入时间、两个遥测代码，获取实时功率、预测功率数据")]
        public String GetStationP2(string sdate, string station_name)
        {
            WriteLogFile("调用方法GetStationP2，传入参数的值为：" + sdate + "  " + station_name );
            //传入日期和有功功率代码不能为空
            if (String.IsNullOrEmpty(sdate) == true || String.IsNullOrEmpty(station_name) == true)
            {
                WriteLogFile("调用方法GetStationP2失败，传入参数为空");
                return "请输入参数";
            }
            else
            {
                DateTime Day1970 = new DateTime(1970, 1, 1);
                DateTime DayChart = Convert.ToDateTime(sdate);
                int days = DateDiff(Day1970, DayChart);

                string tablename = "data" + DayChart.ToString("yyyyMM");

                int yesterday = days - 1;
                
                /*
                return days + "   " + tablename + "    " + yesterday;              

                DateTime date1 = Convert.ToDateTime("1970-01-01").AddDays(Convert.ToDouble(sdate));
                string tablename = "data" + date1.ToString("yyyyMM");

                int yesterday = Convert.ToInt16(sdate) - 1;
                 * */

                DataSet Rds = new DataSet();
                try
                {
                    //DB连接设定
                    checkCn(MyDisDataConnectionStr);
                    string sql = "SELECT sdate,time,data from xopenshdb." + tablename + " a,xopensdb.手机表示配置表 b where a.sname=b.遥测遥信代码 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                    sql += " and b.表示名称='有功功率'  AND RTRIM(b.场站代码) = '" + station_name + "'";

                    sql += " AND (trim(a.sdate)=" + days + " or (trim(a.sdate)=" + yesterday + " and a.time=1440) )";

                    sql += " order by a.sdate,a.time";

                    WriteLogFile(sql);

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetStationP2失败，没有找到配置数据！");
                        return "没有找到GetStationP2配置数据！";

                    }
                    StringBuilder json = new StringBuilder();
                    int maxtime = 0;

                    json.Append("{\"pcode\":[");
                    int oldtime = 0;
                    int newtime = 0;

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            json.Append("{\"time\":\"0\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = 0;
                        }
                        else
                        {
                            json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = Convert.ToInt16(Rds.Tables[0].Rows[i][1].ToString());
                        }
                        if (maxtime < newtime - oldtime)
                        {
                            maxtime = newtime - oldtime;
                        }

                        oldtime = newtime;

                    }

                    json.Append("]");


                    sql = "SELECT sdate,time,data from xopenshdb." + tablename + " a,xopensdb.手机表示配置表 b where a.sname=b.遥测遥信代码 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                    sql += " and b.表示名称='预测功率'  AND RTRIM(b.场站代码) = '" + station_name + "'";

                    sql += " AND (trim(a.sdate)=" + days + " or (trim(a.sdate)=" + yesterday + " and a.time=1440) )";

                    sql += " order by a.sdate,a.time";

                    WriteLogFile(sql);

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetStationP失败，没有找到配置数据！");
                        return "没有找到GetStationP配置数据！";

                    }

                    json.Append(",\"forecast_pcode\":[");

                    oldtime = 0;
                    newtime = 0;

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            json.Append("{\"time\":\"0\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = 0;
                        }
                        else
                        {
                            json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            newtime = Convert.ToInt16(Rds.Tables[0].Rows[i][1].ToString());
                        }
                        if (maxtime < newtime - oldtime)
                        {
                            maxtime = newtime - oldtime;
                        }

                        oldtime = newtime;

                    }

                    json.Append("]");
                    //json.Append(",\"maxtime\":\"" + maxtime +"\"}");

                    json.Append("}");

                    return json.ToString();
                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取厂站信息错误" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }
                
            }

        }

        [WebMethod(Description = "传入时间、两个遥测代码，获取实时功率、预测功率数据")]
        public String GetStationP3(string sdate, string time, string station_name)
        {
            WriteLogFile("调用方法GetStationP3，传入参数的值为：" + sdate + "  " + time + "    " + station_name);
            //传入日期和有功功率代码不能为空
            if (String.IsNullOrEmpty(sdate) == true || String.IsNullOrEmpty(station_name) == true || String.IsNullOrEmpty(time))
            {
                WriteLogFile("调用方法GetStationP3失败，传入参数为空");
                return "请输入参数";
            }
            else
            {
                if (time.Equals("0"))
                {
                    return GetStationP2(sdate, station_name);
                }
                else {
                    DateTime Day1970 = new DateTime(1970, 1, 1);
                    DateTime DayChart = Convert.ToDateTime(sdate);
                    int days = DateDiff(Day1970, DayChart);

                    string tablename = "data" + DayChart.ToString("yyyyMM");

                    int yesterday = days - 1;

                    /*
                    return days + "   " + tablename + "    " + yesterday;              

                    DateTime date1 = Convert.ToDateTime("1970-01-01").AddDays(Convert.ToDouble(sdate));
                    string tablename = "data" + date1.ToString("yyyyMM");

                    int yesterday = Convert.ToInt16(sdate) - 1;
                     * */

                    DataSet Rds = new DataSet();
                    try
                    {
                        //DB连接设定
                        checkCn(MyDisDataConnectionStr);
                        string sql = "SELECT sdate,time,data from xopenshdb." + tablename + " a,xopensdb.手机表示配置表 b where a.sname=b.遥测遥信代码 ";
                        //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                        sql += " and b.表示名称='有功功率'  AND RTRIM(b.场站代码) = '" + station_name + "'";

                        sql += " AND trim(a.sdate)=" + days + " and trim(a.time)>" + time;

                        sql += " order by a.sdate,a.time";

                        WriteLogFile(sql);

                        //SQL做成
                        Rds = ExecSql(sql);

                        if (Rds.Tables[0].Rows.Count < 1)
                        {
                            WriteLogFile("调用方法GetStationP3失败，没有找到配置数据！");
                            return "没有找到GetStationP3配置数据！";

                        }
                        StringBuilder json = new StringBuilder();

                        json.Append("{\"pcode\":[");

                        for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                json.Append("{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            }
                            else
                            {
                                json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            }


                        }

                        json.Append("]");


                        sql = "SELECT sdate,time,data from xopenshdb." + tablename + " a,xopensdb.手机表示配置表 b where a.sname=b.遥测遥信代码 ";
                        //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                        sql += " and b.表示名称='预测功率'  AND RTRIM(b.场站代码) = '" + station_name + "'";

                        sql += " AND trim(a.sdate)=" + days + " and trim(a.time)>" + time;

                        sql += " order by a.sdate,a.time";

                        WriteLogFile(sql);

                        //SQL做成
                        Rds = ExecSql(sql);

                        if (Rds.Tables[0].Rows.Count < 1)
                        {
                            WriteLogFile("调用方法GetStationP3失败，没有找到配置数据！");
                            return "没有找到GetStationP3配置数据！";

                        }

                        json.Append(",\"forecast_pcode\":[");

                        for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                        {
                            if (i == 0)
                            {
                                json.Append("{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");
                            }
                            else
                            {
                                json.Append(",{\"time\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"data\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\"}");

                            }

                        }

                        json.Append("]");

                        json.Append("}");

                        return json.ToString();
                    }
                    catch (Exception err)
                    {
                        GC.Collect();
                        return "获取厂站信息错误" + err.Message;
                    }
                    finally
                    {
                        cn.Close();
                        cn.Dispose();
                    }
                }                

            }

        }


        [WebMethod(Description = "传入日期、时间，获取最新的alarm数据")]
        public String GetAlarmData(string sdate, string stime, string ranges, string level)
        {
            WriteLogFile("调用方法GetAlarmData，传入参数的值为：" + sdate + "  " + stime);
            //传入日期、时间不能为空
            if (String.IsNullOrEmpty(sdate) == true || String.IsNullOrEmpty(stime) == true)
            {
                WriteLogFile("调用方法GetAlarmData失败，传入参数为空");
                return "请输入参数";
            }
            else
            {

                DataSet Rds = new DataSet();
                try
                {
                    //DB连接设定
                    checkCn(MyDisDataConnectionStr);
                    string sql = "select if(bb.名称 is null,'系统',bb.名称) 场站,aa.类型名,aa.年月日,aa.时分秒毫秒,aa.事件文字描述 from (SELECT a.事件对象组名,b.类型名,a.年月日,a.时分秒毫秒,a.事件文字描述 from xopenshdb.历史事项表 a , xopensdb.事项类型表 b  where  a.事件类型=b.类型号 ";
                    //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                    sql += " AND trim(a.年月日) >= " + sdate ;

                    sql += " AND trim(a.时分秒毫秒) > " + stime;

                    sql += " )aa left outer join xopensdb.厂站参数表 bb on aa.事件对象组名=bb.编号";

                    if (!String.IsNullOrEmpty(ranges)) {

                        sql = "select * from (" + sql;

                        if (level.Equals("1"))
                        {
                            sql += " )tmp where 场站 in ('系统','" + ranges.Replace(",", "','") + "')";
                        }
                        else {
                            sql += " )tmp where 场站 in ('" + ranges.Replace(",", "','") + "')";
                        }
                        
                    }

                    sql += " order by 年月日,时分秒毫秒";

                    WriteLogFile(sql);

                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法GetAlarmData失败，没有找到配置数据！");
                        return "没有找到GetAlarmData配置数据！";

                    }
                    StringBuilder json = new StringBuilder();
                    json.Append("{\"AlarmData\":[");

                    for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            json.Append("{\"场站\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\"");
                            json.Append(",\"类型名\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"日期\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"时间\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\",\"事项\":\"" + Rds.Tables[0].Rows[i][4].ToString().Trim() + "\"}");
                        }
                        else
                        {
                            json.Append(",{\"场站\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\"");
                            json.Append(",\"类型名\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"日期\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"时间\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\",\"事项\":\"" + Rds.Tables[0].Rows[i][4].ToString().Trim() + "\"}");
                            
                        }                        

                    }                    
                    json.Append("]}");

                    return json.ToString();
                }
                catch (Exception err)
                {
                    GC.Collect();
                    return "获取厂站信息错误" + err.Message;
                }
                finally
                {
                    cn.Close();
                    cn.Dispose();
                }

            }

        }

        [WebMethod(Description = "获取最新的10条alarm数据")]
        public String GetAlarmDataTop10()
        {
            WriteLogFile("调用方法GetAlarmDataTop10");
            DataSet Rds = new DataSet();
            try
            {
                //DB连接设定
                checkCn(MyDisDataConnectionStr);
                string sql = "SELECT b.类型名,a.年月日,a.时分秒毫秒,a.事件文字描述 from xopenshdb.历史事项表 a , xopensdb.事项类型表 b  where  a.事件类型=b.类型号 ";
                //string sql = "SELECT 厂站代码,表示名称,遥测遥信代码 FROM H9000.手机表示配置表 ";

                sql += " order by a.年月日 desc,a.时分秒毫秒 desc limit 10";

                //SQL做成
                Rds = ExecSql(sql);

                if (Rds.Tables[0].Rows.Count < 1)
                {
                    WriteLogFile("调用方法GetAlarmDataTop10失败，没有找到配置数据！");
                    return "没有找到GetAlarmDataTop10配置数据！";

                }
                StringBuilder json = new StringBuilder();
                json.Append("{\"AlarmData\":[");

                for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        json.Append("{\"类型名\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\",\"日期\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"时间\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"事项\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\"}");
                    }
                    else
                    {
                        json.Append(",{\"类型名\":\"" + Rds.Tables[0].Rows[i][0].ToString().Trim() + "\",\"日期\":\"" + Rds.Tables[0].Rows[i][1].ToString().Trim() + "\",\"时间\":\"" + Rds.Tables[0].Rows[i][2].ToString().Trim() + "\",\"事项\":\"" + Rds.Tables[0].Rows[i][3].ToString().Trim() + "\"}");

                    }

                }
                json.Append("]}");

                return json.ToString();
            }
            catch (Exception err)
            {
                GC.Collect();
                return "获取GetAlarmDataTop10信息错误" + err.Message;
            }
            finally
            {
                cn.Close();
                cn.Dispose();
            }

        }

        [WebMethod(Description = "获取场站列表")]
        public string GetStationName(String ranges)
        {
            WriteLogFile("调用getStationName()");
            DataSet Rds = new DataSet();
            try
            {
                //DB连接设定
                checkCn(MyConnectionStr);
                //string sql = "SELECT sdate,time,data from " + tablename + " where flag=1 ";
                string sqls = "SELECT distinct 场站名称 FROM xopensdb.手机表示配置表 where 分类项目='PowerInfo'";
                if (!String.IsNullOrEmpty(ranges)) {
                    sqls += " and 场站名称 in ('" + ranges.Replace(",", "','") + "')";
                }
                WriteLogFile(sqls);
                byte[] bytes = Encoding.UTF8.GetBytes(sqls);
                string sql = Encoding.UTF8.GetString(bytes);
                //string sql = "SELECT distinct 场站名称 FROM H9000.手机表示配置表 ";


                //SQL做成
                Rds = ExecSql(sql);

                if (Rds.Tables[0].Rows.Count < 1)
                {
                    WriteLogFile("调用方法getStationName失败，没有找到配置数据！");
                    return "没有找到getStationName配置数据！";

                }
                StringBuilder json = new StringBuilder();


                for (int i = 0; i < Rds.Tables[0].Rows.Count; i++)
                {
                    if (i == 0)
                    {
                        json.Append( Rds.Tables[0].Rows[i][0].ToString().Trim() );
                    }
                    else
                    {
                        json.Append("," + Rds.Tables[0].Rows[i][0].ToString().Trim());
                    }                    
                }             
               
                return json.ToString();
            }
            catch (Exception err)
            {
                GC.Collect();
                return "获取厂站信息错误" + err.Message;
            }
            finally
            {
                cn.Close();
                cn.Dispose();
            }
        }

        [WebMethod(Description = "test")]
        public string test()
        {
            for (int i = 0; i <= 1440; i++) {
                WriteLogFile(Convert.ToDouble(i)/60 + "");
            }
            return "test";
        }


        private void WriteLogFile(String input)
        {
            ///指定日志文件的目录
            string fname = "E:\\webService\\LogFile.txt";
            /**/
            ///定义文件信息对象

            FileInfo finfo = new FileInfo(fname);

            if (!finfo.Exists)
            {
                FileStream fs;
                fs = File.Create(fname);
                fs.Close();
                finfo = new FileInfo(fname);
            }

            using (FileStream fs = finfo.OpenWrite())
            {
                /**/
                ///根据上面创建的文件流创建写数据流
                StreamWriter w = new StreamWriter(fs);

                /**/
                ///设置写数据流的起始位置为文件流的末尾
                w.BaseStream.Seek(0, SeekOrigin.End);

                /**/
                ///写入“Log Entry : ”
                w.Write("\n\rLog Entry : ");

                /**/
                ///写入当前系统时间并换行
                w.Write("{0} {1} \n\r", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());

                /**/
                ///写入日志内容并换行
                w.Write(input + "\n\r");

                /**/
                ///写入------------------------------------“并换行
                //w.Write("------------------------------------\n\r");

                /**/
                ///清空缓冲区内容，并把缓冲区内容写入基础流
                w.Flush();

                /**/
                ///关闭写数据流
                w.Close();
            }
        }
    }
}

