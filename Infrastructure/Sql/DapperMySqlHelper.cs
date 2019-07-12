using Dapper;
using Infrastructure.Config;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Infrastructure.Sql
{
    public static class IEnumerableExt
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
        {
            var props = typeof(T).GetProperties();
            var dt = new DataTable();
            Type targetType;
            NullableConverter nullableConvert;
            List<DataColumn> cols = new List<DataColumn>();
            foreach (var p in props)
            {
                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    nullableConvert = new NullableConverter(p.PropertyType);
                    targetType = nullableConvert.UnderlyingType;
                    cols.Add(new DataColumn(p.Name, targetType));
                }
                else
                {
                    cols.Add(new DataColumn(p.Name, p.PropertyType));
                }
            }
            dt.Columns.AddRange(cols.ToArray());

            //list.ForEach((l) => {
            //    List<object> objs = new List<object>();
            //    objs.AddRange(ps.Select(p => p.GetValue(l, null)));
            //    dt.Rows.Add(objs.ToArray());
            //});

            if (collection.Count() > 0)
            {
                for (int i = 0; i < collection.Count(); i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in props)
                    {
                        object obj = pi.GetValue(collection.ElementAt(i), null);
                        tempList.Add(obj);
                    }
                    object[] array = tempList.ToArray();
                    dt.LoadDataRow(array, true);
                }
            }
            return dt;
        }
    }

    public class DapperMySqlHelper
    {
        private ServerConfig _config;
        public DapperMySqlHelper(ConfigService service)
        {
            _config = service.config;
        }

        /// <summary>
        /// get the connection
        /// </summary>
        /// <returns></returns>
        private MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection(_config.mysql.connstring);

            if (conn.State == ConnectionState.Closed)
                conn.Open();
            return conn;
        }

        /// <summary>
        /// 返回DATASET
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataSet getDateWithSql(string sqlText, object param = null)
        {
            //MySqlConnection conn = new MySqlConnection(conStr);
            var conn = OpenConnection();
            MySqlCommand cmd = new MySqlCommand(sqlText, conn);
            cmd.CommandType = CommandType.Text;
            if (param != null)
            {
                cmd.Parameters.Add(new MySqlParameter("@d", MySqlDbType.String) { Value = "" });
                cmd.Parameters.Add(param);
            }

            MySqlDataAdapter adp = new MySqlDataAdapter();
            adp.SelectCommand = cmd;
            DataSet ds = new DataSet();
            adp.Fill(ds);
            ds.Clone();
            conn.Close();
            return ds;
        }

        #region yma
        #region //私有方法
        /// <summary>
        /// 合并连个相同的对象（对象值不可重复）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objUpdate"></param>
        /// <param name="objWhere"></param>
        /// <returns></returns>
        private T CombineModel<T>(T obj1, T obj2)
        {
            T t = default(T);
            //获取T类中所有属性
            PropertyInfo[] tPropertyInfo = typeof(T).GetProperties(System.Reflection.BindingFlags.Instance
                                                                    | System.Reflection.BindingFlags.Public
                                                                    | System.Reflection.BindingFlags.NonPublic);
            //用反射自动创建泛型对象
            t = System.Activator.CreateInstance<T>();

            if (obj1.GetType().Name == obj2.GetType().Name)
            {
                foreach (PropertyInfo pInfo in tPropertyInfo)
                {
                    object valUpdate = obj1.GetType().GetProperty(pInfo.Name).GetValue(obj1, null);
                    object valWhere = obj2.GetType().GetProperty(pInfo.Name).GetValue(obj2, null);
                    if (valUpdate != null || valWhere != null)
                    {
                        var ppValue = (valUpdate == null) ? valWhere : valUpdate;
                        pInfo.SetValue((object)t, ppValue);
                    }

                }
            }
            return t;
        }
        #endregion

        /// <summary>
        /// 新增数据(成功返回0)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Insert<T>(T entity)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                string sql = ModelPropUtils.ModelToInsertSql(entity, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                var res = conn.Execute(sql, entity);
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;

        }
        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Insert<T>(T entity, out string result)
        {
            result = "0";
            int idenId = 0;
            var conn = OpenConnection();
            try
            {
                string sql = ModelPropUtils.ModelToInsertSql(entity, out result);
                if (!result.Equals("0"))
                {
                    result = "=>" + sql;
                    return 0;
                }

                //conn.Open();
                sql += "SELECT @@identity;";
                idenId = conn.ExecuteScalar<int>(sql, entity);
                //result = res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return idenId;

        }
        /// <summary>
        /// 批量新增数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string BatchInsert<T>(List<T> entitys)
        {
            string result = "";
            var conn = OpenConnection();
            IDbTransaction trans = null;
            try
            {
                string sql = ModelPropUtils.ModelToInsertSqllist(entitys, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                trans = conn.BeginTransaction();
                var res = conn.Execute(sql, entitys, trans);
                trans.Commit();
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;

        }
        
        /// <summary>
        /// Replace数据(成功返回0)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Replace<T>(T entity)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                string sql = ModelPropUtils.ModelToReplaceSql(entity, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                var res = conn.Execute(sql, entity);
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;

        }
        /// <summary>
        /// 批量Replace数据(成功返回0)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string BatchReplace<T>(List<T> entitys)
        {
            string result = "";
            var conn = OpenConnection();
            IDbTransaction trans = null;
            try
            {
                string sql = ModelPropUtils.ModelToReplaceSqllist(entitys, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                trans = conn.BeginTransaction();
                var res = conn.Execute(sql, entitys, trans);
                trans.Commit();
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;

        }

        /// <summary>
        /// 更新数据-条件：主键
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <returns></returns>
        public string Update<T>(T entityToUpdate)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                string sql = ModelPropUtils.ModelToUpDateSqlOfKey(entityToUpdate, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                var res = conn.Execute(sql, entityToUpdate, null, null, null);
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        /// <summary>
        /// 更新数据-条件
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <param name="entityOfWhere"></param>
        /// <param name="LoptIsAnd">逻辑运算符是否为“and”</param>
        /// <returns></returns>
        public string Update<T>(T entityToUpdate, T entityOfWhere, bool LoptIsAnd)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                /*string sql = ModelPropUtils.ModelToUpDateSql(entityToUpdate, entityOfWhere, LoptIsAnd, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                var res = conn.Execute(sql, CombineModel<T>(entityToUpdate, entityOfWhere), null, null, null);
                result = "0";// res.ToString();
                conn.Close();*/

                string sql = ModelPropUtils.ModelToUpDateSql(entityToUpdate, entityOfWhere, LoptIsAnd, out result);
                if (result.Equals("0"))
                {
                    //conn.Open();
                    var res = conn.Execute(sql, CombineModel<T>(entityToUpdate, entityOfWhere), null, null, null);
                }
                result = "0";// res.ToString();
                conn.Close();
            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        /// <summary>
        /// 更新数据-条件
        /// </summary>
        /// <param name="entityToUpdate"></param>
        /// <param name="entityOfWhere"></param>
        /// <param name="LoptIsAnd">逻辑运算符是否为“and”</param>
        /// <returns></returns>
        public string Update<T>(string sqlStr, T entityOfWhere)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                //conn.Open();
                var res = conn.Execute(sqlStr, entityOfWhere, null, null, null);
                if (res > 0)
                    result = "0";// res.ToString();
                else
                    result = "0,";
                conn.Close();
            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityOfWhere"></param>
        /// <param name="LoptIsAnd"></param>
        /// <returns></returns>
        public string Delete<T>(T entityOfWhere, bool LoptIsAnd)
        {
            string result = "";
            var conn = OpenConnection();
            try
            {
                string sql = ModelPropUtils.ModelToDeleteSql(entityOfWhere, LoptIsAnd, out result);
                if (!result.Equals("0"))
                    return result;

                //conn.Open();
                var res = conn.Execute(sql, entityOfWhere, null, null, null);
                result = "0";// res.ToString();
                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return result;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(string sql, object parameters, out string result)
        {
            result = "0";
            IEnumerable<T> res = null;
            var conn = OpenConnection();
            try
            {
                //string sql = ModelPropUtils.ModelToDeleteSql(entityOfWhere, LoptIsAnd, out result);
                //if (!result.Equals("0"))
                //    return result;

                //conn.Open();

                res = conn.Query<T>(sql, parameters);

                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return res;
        }

        /// <summary>
        /// 获取数据-多个返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public (IEnumerable<T1>, IEnumerable<T2>) GetMultipleList<T1, T2>(string sql, object parameters, out string result)
        {
            result = "0";
            IEnumerable<T1> t1 = null;
            IEnumerable<T2> t2 = null;
            //List<IEnumerable<object>> ieer = new List<IEnumerable<object>>();
            var conn = OpenConnection();
            try
            {
                //conn.Open();

                var multi = conn.QueryMultiple(sql, parameters);
                if (!multi.IsConsumed)
                {
                    t1 = multi.Read<T1>();
                    t2 = multi.Read<T2>();
                }

                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return (t1, t2);
        }

        /// <summary>
        /// 获取数据-多个返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public (IEnumerable<T1>, IEnumerable<T2>, IEnumerable<T3>) GetMultipleList<T1, T2, T3>(string sql, object parameters, out string result)
        {
            result = "0";
            IEnumerable<T1> t1 = null;
            IEnumerable<T2> t2 = null;
            IEnumerable<T3> t3 = null;
            var conn = OpenConnection();
            try
            {
                //conn.Open();

                var multi = conn.QueryMultiple(sql, parameters);
                if (!multi.IsConsumed)
                {
                    t1 = multi.Read<T1>();
                    t2 = multi.Read<T2>();
                    t3 = multi.Read<T3>();
                }

                conn.Close();

            }
            catch (Exception ex)
            {
                result = $"ex=>{ex.ToString()}";
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }

            return (t1, t2, t3);
        }
        #endregion
    }
}