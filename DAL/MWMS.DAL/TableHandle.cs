using MWMS.Helper;
using MWMS.Helper.Extensions;
using MWMS.SqlHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace MWMS.DAL
{
    /// <summary>
    /// 表操作类
    /// </summary>
    public class TableHandle
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string PrimaryKey = "id";
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        string _whrestr = "";
        string _fieldstr = "*";
        string _savefieldstr = "*";
        string _limitstr = "";
        string _orderstr = "";
        Dictionary<string, object> _p = new Dictionary<string, object>();
        string _sqlstr = "";
        List<string> _joinstr = new List<string>();
        public TableHandle()
        {

        }
        public TableHandle(string tableName)
        {
            TableName = tableName;
        }
        public Dictionary<string, object> Get(double id)
        {
            this._whrestr = PrimaryKey + " = @" + PrimaryKey;
            _p = new Dictionary<string, object>();
            _p.Add(PrimaryKey, id);
            this.BuildSql();
            return SqlHelper.Sql.ExecuteDictionary(_sqlstr, Sql.GetParameter(_p));
        }
        public TableHandle Join(string tableName,string where)
        {
            _joinstr.Add(" inner join "+tableName+" on "+where);
            return this;
        }
        public Dictionary<string, object> Find()
        {
            BuildSql();
            return SqlHelper.Sql.ExecuteDictionary(_sqlstr,GetParameter( _p));
            //return GetModel(_fieldstr, this.GetWhere(), _p, "");
        }
        public Dictionary<string,object> GetModel(double id,string fields)
        {
            Dictionary<string, object> p = new Dictionary<string, object>();
            p.Add(PrimaryKey, id);
            return GetModel(fields, PrimaryKey+"=@"+ PrimaryKey, p, "");
        }
        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p)
        {
            return GetModel(fields, where, p, "");
        }
        MySqlParameter [] GetParameter(Dictionary<string, object> p)
        {
            MySqlParameter[] _p = new MySqlParameter[p.Count];
            int i1 = 0;
            foreach (var value in p)
            {
                _p[i1] = new MySqlParameter(value.Key, value.Value);
                i1++;
            }
            return _p;
        }
        public Dictionary<string, object> GetModel(double id)
        {
            return GetModel(id,"*");
        }
        //获取where条件
        string GetWhere()
        {
            return _whrestr;
        }
        public TableHandle Field(string fieldstr)
        {
            this._fieldstr = fieldstr;
            return this;
        }
        public TableHandle Where(string wherestr, Hashtable p=null)
        {
            if (wherestr == "") return this;
            if (this._whrestr != "") this._whrestr += " and ";
            if (p != null)
            {
                foreach(DictionaryEntry item in p)
                {
                    _p.Add(item.Key.ToString(), item.Value);
                }
            }
            this._whrestr += wherestr;
            return this;
        }
        public TableHandle Where(object [,] wherearr)
        {
            if (this._whrestr != "") this._whrestr += " and ";
            //this._whrestr = "";
            //_p = new Dictionary<string, object>();
            for (int i=0;i< wherearr.Length/3; i++)
            {
                string key = wherearr[i, 0].ToString();
                if(i>0)this._whrestr += " and ";
                this._whrestr += key + wherearr[i, 1].ToString() + "@"+key.MD5() ;
                _p[key.MD5()] = wherearr[i, 2].ToString();
            }
            return this;
        }
        public TableHandle Where(Dictionary<string,object> wherestr)
        {
            if (this._whrestr != "") this._whrestr += " and ";
            this._whrestr = "";
            //_p = new Dictionary<string, object>();
            foreach (var item in wherestr)
            {
                string key = item.Key.MD5();
                if (this._whrestr != "") this._whrestr += " and ";
                if (item.Value!=null && item.Value.GetType().Name== "Object[]")
                {
                    object[] data = (object[])item.Value;
                    this._whrestr += item.Key + data[0]+"@" + key;
                    _p[key] = data[1];
                }
                else
                {
                    this._whrestr += item.Key + "=@" + key;
                    _p[key] = item.Value;

                }
            }
            return this;
        }
        public TableHandle Order(string orderstr)
        {
            if (orderstr != "")
            {
                _orderstr =" order by "+orderstr;
            }
            
            return this;
        }
        public TableHandle Pagination(int pageSize,int pageNo=1)
        {
            _limitstr = "limit " + (pageSize * (pageNo - 1)).ToString() + "," + pageSize.ToString();
            return this;
        }
        /// <summary>
        /// 记录数
        /// </summary>
        /// <returns>反回数据总数</returns>
        public int Count()
        {
            BuildSql(3);
            return SqlHelper.Sql.ExecuteDictionary(_sqlstr, GetParameter(_p))["c"].ToInt();
        }
        public int Delete()
        {
            this.BuildSql(2);
            return SqlHelper.Sql.ExecuteNonQuery(this._sqlstr, this._p);
        }
        /// <summary>
        /// 生成sql
        /// </summary>
        /// <param name="type">0 select 1 update 2 delete 3 count</param>
        void BuildSql(int type=0)
        {
            if (type == 0)
            {
                this._sqlstr = "select ";
                this._sqlstr += this._fieldstr;
                this._sqlstr += " from " + this.TableName;
                foreach (string item in _joinstr)
                {
                    this._sqlstr += item + " ";
                }
                if(this._whrestr!="") this._sqlstr += " where " + this._whrestr;
                if(this._orderstr!="") this._sqlstr += this._orderstr;
                this._sqlstr += " "+_limitstr;
            }
            else if (type == 1)
            {
                this._sqlstr = "update ";
                this._sqlstr += this.TableName;
                this._sqlstr += " set " + this._savefieldstr;
                this._sqlstr += " where " + this._whrestr;
            }
            else if (type == 2)
            {
                this._sqlstr = "delete ";
                this._sqlstr += " from " + this.TableName;
                foreach (string item in _joinstr)
                {
                    this._sqlstr += item + " ";
                }
                if (this._whrestr != "") this._sqlstr += " where " + this._whrestr;
            }
            else if (type == 3)
            {
                this._sqlstr = "select ";
                this._sqlstr += "count(1) c";
                this._sqlstr += " from " + this.TableName;
                foreach (string item in _joinstr)
                {
                    this._sqlstr += item + " ";
                }
                if (this._whrestr != "") this._sqlstr += " where " + this._whrestr;
            }
        }
        public List<Dictionary<string,object>> Select()
        {
            this.BuildSql();
            return SqlHelper.Sql.ExecuteList(this._sqlstr,GetParameter( this._p));
        }

        public Dictionary<string, object> GetModel(string fields, string where, Dictionary<string, object> p, string desc="")
        {
            if (TableName == "") throw new Exception("表名不能为空");
            Dictionary<string, object> model = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            string[] _fields = fields.Split(',');
            MySqlParameter[] _p = GetParameter(p);
            if (desc != "") desc = "order by "+desc;
            MySqlDataReader rs = SqlHelper.Sql.ExecuteReader("select " + fields + " from " + TableName + " where "+ where+" "+desc, _p);
            bool flag = false;
            if (rs.Read())
            {
                for (int i = 0; i < rs.FieldCount; i++)
                {
                    if (rs.IsDBNull(i))
                    {
                        model[rs.GetName(i)] = "";
                    }
                    else { 
                    model[rs.GetName(i)] = rs[rs.GetName(i)];
                    }
                }
                flag = true;
            }
            rs.Close();
            if (!flag) return null;
            return model;
        }
        public int Count(string where, Dictionary<string, object> p)
        {
            MySqlParameter[] _p = GetParameter(p);
            return int.Parse(Sql.ExecuteScalar("select count(1) from " + TableName + " where " + where, _p).ToString());
        }
        /// <summary>
        /// 保存模型到表中
        /// </summary>
        /// <param name="model">模型</param>
        public double Save(Dictionary<string, object> model)
        {
            if (TableName == "") throw new Exception("表名不能为空");
            bool updateFlag = model.ContainsKey(PrimaryKey);
            if (updateFlag && (double)(model[PrimaryKey])>0) {
                this.Where(new object[,] {
                    {"id","=",model[PrimaryKey]}
                });
                 Update(model);
            }else
            {
                 Append(model);
            }
            return (double)model[PrimaryKey];
        }
        public int Append(Dictionary<string, object> model)
        {
            return this.Insert(model);
        }
        public int Insert(Dictionary<string, object> model)
        {
            StringBuilder fieldstr = new StringBuilder();
            StringBuilder fieldstr2 = new StringBuilder();
            if (!model.ContainsKey(PrimaryKey) || (double)(model[PrimaryKey]) == 0) model[PrimaryKey] = double.Parse(Tools.GetId());
            foreach (var field in model)
            {
                if (fieldstr.Length > 0)
                {
                    fieldstr.Append(",");
                    fieldstr2.Append(",");
                }
                fieldstr.Append(field.Key);
                fieldstr2.Append("@" + field.Key);
            }
            _p = model;
            _sqlstr = "insert into  " + TableName + " (" + fieldstr.ToString() + ")values(" + fieldstr2.ToString() + ")";

            return Sql.ExecuteNonQuery("insert into  " + TableName + " (" + fieldstr.ToString() + ")values(" + fieldstr2.ToString() + ")", _p);
        }
        public int Update(Dictionary<string, object> model)
        {
            //StringBuilder fieldstr = new StringBuilder();
            _savefieldstr = "";
            foreach (var field in model)
            {
                if (_savefieldstr!="") _savefieldstr+=",";

                _savefieldstr+=field.Key + "=@" + field.Key.MD5();
                _p[field.Key.MD5()] = field.Value;
            }
            this.BuildSql(1);
            return Sql.ExecuteNonQuery(this._sqlstr,GetParameter( _p));
            //return (double)model[PrimaryKey];
        }
        public void Remove(double id)
        {
            this.Where(new object[,] {
                    {"id","=",id}
                }).Delete();
        }
    }
}
