using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.Entity.SqlServer;
using System.Threading;

namespace Runnymede.Website.Utils
{
    public static class DapperHelper
    {
        public static SqlConnection GetOpenConnection()
        {
            var connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connString);
            connection.Open();
            return connection;
        }

        public static async Task<SqlConnection> GetOpenConnectionAsync()
        {
            var connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connString);
            // Dapper uses ConfigureAwait(false) in their code. It is discussed at +https://code.google.com/p/dapper-dot-net/issues/detail?id=128
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        // ??? Is there an obvious performance gain in resetting the connection asynchronously?
        ////public static async Task<SqlConnection> GetOpenConnectionAsync()
        ////{
        ////    var connString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        ////    var connection = new SqlConnection(connString);
        ////    await connection.OpenAsync();
        ////    return connection;
        ////}

        public static IEnumerable<TFirst> Map<TFirst, TSecond, TKey>
                    (
                    this Dapper.SqlMapper.GridReader reader,
                    Func<TFirst, TKey> firstKey,
                    Func<TSecond, TKey> secondKey,
                    Action<TFirst, IEnumerable<TSecond>> addChildren
                    )
        {
            var first = reader.Read<TFirst>().ToList();

            var childMap = reader
                .Read<TSecond>()
                .GroupBy(s => secondKey(s))
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            foreach (var item in first)
            {
                IEnumerable<TSecond> children;
                if (childMap.TryGetValue(firstKey(item), out children))
                {
                    addChildren(item, children);
                }
            }

            return first;
        }

        /// <summary>
        /// Strip the beginning of the exception message which contains the name of the stored procedure and variable values.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static Exception StripException(SqlException ex)
        {
            var index = ex.Message.IndexOf(":: "); // Magic separator.
            return (index > 0) ? new Exception(ex.Message.Substring(index + 3)/*, ex*/) : ex;
        }

        public static int ExecuteResiliently(string sql, dynamic param = null, CommandType? commandType = null)
        {
            int rowsAffected = 0;

            try
            {
                var executionStrategy = new SqlAzureExecutionStrategy();
                executionStrategy.Execute(() =>
                {
                    using (var connection = GetOpenConnection())
                    {
                        //  Outside-initiated user transactions are not supported. See Limitations with Retrying Execution Strategies (EF6 onwards) +http://msdn.microsoft.com/en-us/data/dn307226
                        // +http://entityframework.codeplex.com/wikipage?title=Connection+Resiliency+Spec
                        // To find a workaround, search for SqlAzureExecutionStrategy in the project code.
                        rowsAffected = SqlMapper.Execute(connection, sql, param: param, transaction: null, commandTimeout: null, commandType: commandType);
                    }
                });
            }
            catch (SqlException ex)
            {
                throw StripException(ex);
            }

            return rowsAffected;
        }

        public static async Task<int> ExecuteResilientlyAsync(string sql, dynamic param = null, CommandType? commandType = null)
        {
            int rowsAffected = 0;

            try
            {
                var executionStrategy = new SqlAzureExecutionStrategy();
                await executionStrategy.ExecuteAsync(
                async () =>
                {
                    using (var connection = await GetOpenConnectionAsync())
                    {
                        //  Outside-initiated user transactions are not supported. See Limitations with Retrying Execution Strategies (EF6 onwards) +http://msdn.microsoft.com/en-us/data/dn307226
                        // +http://entityframework.codeplex.com/wikipage?title=Connection+Resiliency+Spec
                        // To find a workaround, search for SqlAzureExecutionStrategy in the project code.
                        // Example of using ExecuteAsync(). +https://code.google.com/p/dapper-dot-net/source/browse/DapperTests%20NET45/Tests.cs
                        rowsAffected = await SqlMapper.ExecuteAsync(connection, sql, param: param, transaction: null, commandTimeout: null, commandType: commandType);
                    }
                },
                    // Apparently, CancellationToken is not used. See +https://github.com/Icehunter/entityframework/blob/master/src/EntityFramework.SqlServer/DefaultSqlExecutionStrategy.cs            
                    new CancellationToken()
                );
            }
            catch (SqlException ex)
            {
                throw StripException(ex);
            }

            return rowsAffected;
        }

        public static IEnumerable<T> QueryResiliently<T>(string sql, dynamic param = null, CommandType? commandType = null)
        {
            IEnumerable<T> result = null;

            try
            {
                var executionStrategy = new SqlAzureExecutionStrategy();
                executionStrategy.Execute(() =>
                {
                    using (var connection = GetOpenConnection())
                    {
                        // QueryAsync is marked async in the source code, but is not in metadata. +https://code.google.com/p/dapper-dot-net/source/browse/Dapper+NET45/SqlMapperAsync.cs
                        result = SqlMapper.Query<T>(connection, sql, param: param, transaction: null, commandTimeout: null, commandType: commandType);
                    }
                });
            }
            catch (SqlException ex)
            {
                throw StripException(ex);
            }

            return result;
        }

        public static void QueryMultipleResiliently(string sql, dynamic param = null, CommandType? commandType = null, Action<Dapper.SqlMapper.GridReader> action = null)
        {
            try
            {
                var executionStrategy = new SqlAzureExecutionStrategy();
                executionStrategy.Execute(() =>
                {
                    using (var connection = GetOpenConnection())
                    {
                        var reader = SqlMapper.QueryMultiple(connection, sql, param: param, transaction: null, commandTimeout: null, commandType: commandType);

                        if (action != null)
                        {
                            action(reader);
                        }
                    }
                });
            }
            catch (SqlException ex)
            {
                throw StripException(ex);
            }
        }

        public static async Task<IEnumerable<T>> QueryResilientlyAsync<T>(string sql, dynamic param = null, CommandType? commandType = null)
        {
            IEnumerable<T> result = null;

            try
            {
                var executionStrategy = new SqlAzureExecutionStrategy();
                await executionStrategy.ExecuteAsync(
                    async () =>
                    {
                        using (var connection = GetOpenConnection())
                        {
                            // QueryAsync is marked async in source code, but is not in metadata. +https://code.google.com/p/dapper-dot-net/source/browse/Dapper+NET45/SqlMapperAsync.cs
                            result = await SqlMapper.QueryAsync<T>(connection, sql, param: param, transaction: null, commandTimeout: null, commandType: commandType);
                        }
                    },
                    // Apparently, CancellationToken is not used. See +https://github.com/Icehunter/entityframework/blob/master/src/EntityFramework.SqlServer/DefaultSqlExecutionStrategy.cs            
                    new CancellationToken()
                );
            }
            catch (SqlException ex)
            {
                throw StripException(ex);
            }

            return result;
        }


        /* +http://stackoverflow.com/questions/5962117/is-there-a-way-to-call-a-stored-procedure-with-dapper
        In the simple case you can do:

        var user = cnn.Query<User>("spGetUser", new {Id = 1}, commandType: CommandType.StoredProcedure).First();
          
        If you want something more fancy, you can do:

         var p = new DynamicParameters();
         p.Add("@a", 11);
         p.Add("@b", dbType: DbType.Int32, direction: ParameterDirection.Output);
         p.Add("@c", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

         cnn.Execute("spMagicProc", p, commandType: commandType.StoredProcedure); 

         int b = p.Get<int>("@b");
         int c = p.Get<int>("@c");  
         */

    }
}
