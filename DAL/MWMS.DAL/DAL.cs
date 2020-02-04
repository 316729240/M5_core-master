using MWMS.Helper.Extensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace MWMS.DAL
{
    public class DAL{
            public static TableHandle M(string table)
            {
                return new TableHandle(table);
        }
        public static List<Dictionary<string, object>> ExecuteReader(string sql, Dictionary<string, object> p)
        {
            return SqlHelper.Sql.ExecuteList(sql, GetParameter(p));
        }
        public static object ExecuteNonQuery(string sql, Dictionary<string,object> p)
        {
            return SqlHelper.Sql.ExecuteNonQuery(sql, GetParameter(p));
        }
        static MySqlParameter[] GetParameter(Dictionary<string, object> p)
            {
                MySqlParameter[] _p = new MySqlParameter[p.Count];
                int i1 = 0;
                foreach (var  value in p)
                {
                    _p[i1] = new MySqlParameter(value.Key.ToString(), value.Value);
                    i1++;
                }
                return _p;
            }
    }
}