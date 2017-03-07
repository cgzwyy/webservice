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
        private static string MyConnectionStr = "Database=xopensdb;Server="+ConfigManager.DBIP+";Uid=h9000;Pwd=ems;charset=gbk";
        private static string MyDisDataConnectionStr = "Database=xopenshdb;Server=" + ConfigManager.DBIP + ";Uid=h9000;Pwd=ems;charset=gbk";


        private void checkCn()
        {
            try
            {
                cn = new MySqlConnection(MyConnectionStr);
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

        [WebMethod(Description = "登录验证")]
        public string isUser(string username, string password)
        {
            WriteLogFile("调用方法isUser，传入参数username的值为：" + username + ",password的值为：" + password);
            if (String.IsNullOrEmpty(username) == true || String.IsNullOrEmpty(password) == true)   //判断传入参数是否为空
            {
                WriteLogFile("调用方法isUser失败，传入参数为空");
                return "用户名或密码为空";
            }
            else 
            {
                DataSet Rds = new DataSet();
                try {
                    checkCn();
                    string sql = "select levels from appUser where trim(username)='" + username + "' and trim(password)='" + password + "'";
                    //SQL做成
                    Rds = ExecSql(sql);

                    if (Rds.Tables[0].Rows.Count < 1)
                    {
                        WriteLogFile("调用方法isUser失败，没有找到该用户数据！");
                        return "用户名或密码错误";

                    }
                    else {

                        WriteLogFile("调用方法isUser成功，获取到的数据结果为：" + Rds.Tables[0].Rows[0][0].ToString().Trim());

                        return Rds.Tables[0].Rows[0][0].ToString().Trim();
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
                    checkCn();
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

