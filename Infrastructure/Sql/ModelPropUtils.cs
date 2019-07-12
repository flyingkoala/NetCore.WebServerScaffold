using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Infrastructure.Sql
{
    public class ModelPropUtils
    {
        private static readonly IDictionary<Type, string> TableNames = new Dictionary<Type, string>();
        private static readonly IDictionary<string, string> ColumnNames = new Dictionary<string, string>();

        private static string Encapsulate(string databaseword) => $"`{databaseword}`";//mysql
        private static string GetTableName(object entity)
        {
            string tableName;
            var type = entity.GetType();
            if (TableNames.TryGetValue(type, out tableName))
                return tableName;

            tableName = Encapsulate(type.Name);//type.GetDefaultMembers()
            var tableattr = type.GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(TableAttribute).Name) as dynamic;
            if (tableattr != null)
            {
                tableName = Encapsulate(tableattr.Name);
                try
                {
                    if (!string.IsNullOrEmpty(tableattr.Schema))
                    {
                        string schemaName = Encapsulate(tableattr.Schema);
                        tableName = $"{schemaName}.{tableName}";
                    }
                }
                catch (RuntimeBinderException)
                {
                    //Schema doesn't exist on this attribute.
                }
            }
            return tableName;
        }

        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句(SQLParamer)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="result">值为0表示组装成功</param>
        /// <returns></returns>
        public static string ModelToInsertSql<T>(T obj, out string result)
        {
            result = "0";
            System.Reflection.PropertyInfo[] PropS = obj.GetType().GetProperties();

            StringBuilder builder = new StringBuilder();
            builder.Append($"insert into  {GetTableName(obj)} ( ");

            List<string> requiredList = new List<string>();//必选且值为空的字段
            int FiledCount = 0;//第一次计算出共有多少个字段赋了值
            StringBuilder builderValues = new StringBuilder();
            object propattr;
            for (int i = 0; i < PropS.Length; i++)
            {
                //判断必选字段是否符合要求
                propattr = PropS[i].GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(RequiredAttribute).Name);
                if (propattr != null && PropS[i].GetValue(obj, null) == null)
                    requiredList.Add(PropS[i].Name);

                if (PropS[i].GetValue(obj, null) != null)
                {
                    FiledCount++;
                    builder.Append($"`{PropS[i].Name}`, ");
                    builderValues.Append($"@{PropS[i].Name}, ");//PropS[i].GetValue(obj, null).ToString()
                }
            }
            if (requiredList.Count > 0) result = $"error: 必选字段=>({string.Join(",", requiredList.ToArray())})";
            if (FiledCount == 0) result = "error: 插入数据不可以全部为空";//无需要更新的字段

            if (builder.ToString().EndsWith(", "))
                builder.Remove(builder.Length - 2, 2);
            if (builderValues.ToString().EndsWith(", "))
                builderValues.Remove(builderValues.Length - 2, 2);

            builder.Append(") values (");
            builder.Append(builderValues);
            builder.Append(");");

            return builder.ToString();
        }

        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句(SQLParamer)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="result">值为0表示组装成功</param>
        /// <returns></returns>
        public static string ModelToReplaceSql<T>(T obj, out string result)
        {
            result = "0";
            System.Reflection.PropertyInfo[] PropS = obj.GetType().GetProperties();

            StringBuilder builder = new StringBuilder();
            builder.Append($"replace into  {GetTableName(obj)} ( ");//根据主键和唯一约束

            List<string> requiredList = new List<string>();//必选且值为空的字段
            int FiledCount = 0;//第一次计算出共有多少个字段赋了值
            StringBuilder builderValues = new StringBuilder();
            object propattr;
            for (int i = 0; i < PropS.Length; i++)
            {
                //判断必选字段是否符合要求
                propattr = PropS[i].GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(RequiredAttribute).Name);
                if (propattr != null && PropS[i].GetValue(obj, null) == null)
                    requiredList.Add(PropS[i].Name);

                if (PropS[i].GetValue(obj, null) != null)
                {
                    FiledCount++;
                    builder.Append($"`{PropS[i].Name}`, ");
                    builderValues.Append($"@{PropS[i].Name}, ");//PropS[i].GetValue(obj, null).ToString()
                }
            }
            if (requiredList.Count > 0) result = $"error: 必选字段=>({string.Join(",", requiredList.ToArray())})";
            if (FiledCount == 0) result = "error: 插入数据不可以全部为空";//无需要更新的字段

            if (builder.ToString().EndsWith(", "))
                builder.Remove(builder.Length - 2, 2);
            if (builderValues.ToString().EndsWith(", "))
                builderValues.Remove(builderValues.Length - 2, 2);

            builder.Append(") values (");
            builder.Append(builderValues);
            builder.Append(");");

            return builder.ToString();
        }


        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句(SQLParamer)
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="result">值为0表示组装成功</param>
        /// <returns></returns>
        public static string ModelToInsertSqllist<T>(List<T> objs, out string result)
        {
            result = "0";
            StringBuilder builder = new StringBuilder();
            if (objs == null || objs.Count() == 0)
            {
                result = "object[] 为空";
            }
            else
            {
                string res = "";
                string oneSql = "";
                StringBuilder builderStr = new StringBuilder();
                //foreach (object obj in objs)
                //{
                oneSql = ModelToInsertSql(objs[0], out res);
                if (res.Equals("0"))
                    builder.Append(oneSql);
                else
                    builderStr.Append(res);
                //}
                if (!string.IsNullOrEmpty(builderStr.ToString()))
                    result = builderStr.ToString();
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句(SQLParamer)
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="result">值为0表示组装成功</param>
        /// <returns></returns>
        public static string ModelToReplaceSqllist<T>(List<T> objs, out string result)
        {
            result = "0";
            StringBuilder builder = new StringBuilder();
            if (objs == null || objs.Count() == 0)
            {
                result = "object[] 为空";
            }
            else
            {
                string res = "";
                string oneSql = "";
                StringBuilder builderStr = new StringBuilder();
                //foreach (object obj in objs)
                //{
                oneSql = ModelToReplaceSql(objs[0], out res);
                if (res.Equals("0"))
                    builder.Append(oneSql);
                else
                    builderStr.Append(res);
                //}
                if (!string.IsNullOrEmpty(builderStr.ToString()))
                    result = builderStr.ToString();
            }

            return builder.ToString();
        }



        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句
        /// </summary>
        /// <param name="obj">条件为[keyAttribute]</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string ModelToUpDateSqlOfKey<T>(T obj, out string result)
        {
            result = "0";
            StringBuilder builder = new StringBuilder();
            builder.Append("update  " + GetTableName(obj) + " set ");

            System.Reflection.PropertyInfo[] PropS = obj.GetType().GetProperties();

            StringBuilder builderUpdate = new StringBuilder();
            StringBuilder builderWhere = new StringBuilder();
            builderWhere.Append(" where ");
            int FiledCount = 0;//第一次计算出共有多少个字段赋了值
            bool isHaveKey = false;
            for (int i = 0; i < PropS.Length; i++)
            {
                var propAttr = PropS[i].GetCustomAttributes(true).SingleOrDefault(attr => attr.GetType().Name == typeof(KeyAttribute).Name) as dynamic;
                object propValue = PropS[i].GetValue(obj, null);
                if (propValue != null)
                {
                    if (propAttr != null)
                    {
                        isHaveKey = true;
                        builderWhere.Append($"{PropS[i].Name} = @{PropS[i].Name}");
                    }
                    else
                    {
                        FiledCount++;
                        builderUpdate.Append($"{PropS[i].Name} = @{PropS[i].Name}");
                        if (i < PropS.Length - 1)
                            builderUpdate.AppendFormat(", ");
                    }
                }
            }
            if (FiledCount == 0) result = "error: 无需要更新的字段";//无需要更新的字段
            if (!isHaveKey) result = "error: KeyAttribute标签的字段为条件，不可为空";

            if (builderUpdate.ToString().EndsWith(", "))
                builderUpdate.Remove(builderUpdate.Length - 2, 2);
            builder.Append(builderUpdate.ToString());
            builder.Append(builderWhere.ToString());
            builder.Append(";");

            return builder.ToString();
        }

        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="objWhere"></param>
        /// <param name="LoptIsAnd"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string ModelToUpDateSql<T>(T obj, T objWhere, bool LoptIsAnd, out string result)
        {
            result = "0";
            StringBuilder builder = new StringBuilder();
            builder.Append("update  " + GetTableName(obj) + " set ");

            System.Reflection.PropertyInfo[] PropS = obj.GetType().GetProperties();

            StringBuilder builderUpdate = new StringBuilder();
            int FiledCount = 0;//第一次计算出共有多少个字段赋了值
            for (int i = 0; i < PropS.Length; i++)
            {
                if (PropS[i].GetValue(obj, null) != null)
                {
                    FiledCount++;

                    //如果string是-1，则清空值
                    if (PropS[i].PropertyType.FullName == typeof(System.String).FullName
                        && PropS[i].GetValue(obj, null).Equals("-1"))
                    {
                        builderUpdate.Append($"{PropS[i].Name} = NULL");
                    }//如果int是-999，则清空值
                    else if (PropS[i].PropertyType.FullName == typeof(System.Int32?).FullName
                        && PropS[i].GetValue(obj, null).ToString().Equals("-999"))
                    {
                        builderUpdate.Append($"{PropS[i].Name} = NULL");
                    }
                    else
                    {
                        builderUpdate.Append($"{PropS[i].Name} = @{PropS[i].Name}");
                    }
                    if (i < PropS.Length - 1)
                        builderUpdate.AppendFormat(", ");
                }
            }
            if (FiledCount == 0) result = "error: 无需要更新的字段";//无需要更新的字段

            if (builderUpdate.ToString().EndsWith(", "))
                builderUpdate.Remove(builderUpdate.Length - 2, 2);
            builder.Append(builderUpdate.ToString());

            if (objWhere != null)
            {
                builder.Append(" where ");
                System.Reflection.PropertyInfo[] wherePropS = objWhere.GetType().GetProperties();
                if (wherePropS != null)
                {
                    for (int j = 0; j < wherePropS.Length; j++)
                    {
                        var propValue = wherePropS[j].GetValue(objWhere, null);
                        if (propValue != null)
                        {
                            builder.Append($"{wherePropS[j].Name} = @{wherePropS[j].Name} ");
                            if (j < wherePropS.Length - 1)
                            {
                                if (LoptIsAnd)
                                    builder.Append("and ");
                                else
                                    builder.Append(" or ");
                            }
                        }
                    }
                    if (builder.ToString().EndsWith("and ") || builder.ToString().EndsWith("or "))
                        builder.Remove(builder.ToString().Length - 4, 4);
                }
            }

            return builder.ToString();
        }



        /// <summary>
        /// 将对象的属性分解，并组装成SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objWhere"></param>
        /// <param name="LoptIsAnd"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string ModelToDeleteSql<T>(T objWhere, bool LoptIsAnd, out string result)
        {
            result = "0";
            StringBuilder builder = new StringBuilder();
            builder.Append("delete from " + GetTableName(objWhere) + " ");

            PropertyInfo[] propWhere = objWhere.GetType().GetProperties();
            if (propWhere != null)
            {
                builder.Append("where ");
                for (int i = 0; i < propWhere.Length; i++)
                {
                    var pValue = propWhere[i].GetValue(objWhere, null);
                    if (pValue != null)
                    {
                        builder.Append($"{propWhere[i].Name} = @{propWhere[i].Name} ");
                        if (i < propWhere.Length - 1)
                        {
                            if (LoptIsAnd)
                                builder.Append("and ");
                            else
                                builder.Append(" or ");
                        }
                    }
                }
                if (builder.ToString().EndsWith("and ") || builder.ToString().EndsWith("or "))
                    builder.Remove(builder.ToString().Length - 4, 4);
            }

            return builder.ToString();
        }


    }
}
