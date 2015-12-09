using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http.OData.Query;

namespace Runnymede.Website.Utils
{
    public class DapperODataUtils
    {
    }

    //public static class ODataQueryOptionExtensions
    //{
    //    public static IQueryable ApplyTo(this ODataQueryOptions query/*, ISession session*/)
    //    {
    //        string from = "from " + query.Context.ElementClrType.Name + " $it" + Environment.NewLine;

    //        // convert $filter to HQL where clause.
    //        WhereClause where = ToFilterQuery(query.Filter);

    //        // convert $orderby to HQL orderby clause.
    //        string orderBy = ToOrderByQuery(query.OrderBy);

    //        // create a query using the where clause and the orderby clause.
    //        string queryString = from + where.Clause + orderBy;
    //        IQueryable hQuery = null;// = session.CreateQuery(queryString);
    //        for (int i = 0; i < where.PositionalParameters.Length; i++)
    //        {
    //            //hQuery.SetParameter(i, where.PositionalParameters[i]);
    //        }

    //        // Apply $skip.
    //        hQuery = hQuery.Apply(query.Skip);

    //        // Apply $top.
    //        hQuery = hQuery.Apply(query.Top);

    //        return hQuery;
    //    }

    //    private static IQueryable Apply(this IQueryable query, TopQueryOption topQuery)
    //    {
    //        if (topQuery != null)
    //        {
    //            //query = query.SetMaxResults(topQuery.Value);
    //        }

    //        return query;
    //    }

    //    private static IQueryable Apply(this IQueryable query, SkipQueryOption skipQuery)
    //    {
    //        if (skipQuery != null)
    //        {
    //            //query = query.SetFirstResult(skipQuery.Value);
    //        }

    //        return query;
    //    }

    //    private static string ToOrderByQuery(OrderByQueryOption orderByQuery)
    //    {
    //        return null;// NHibernateOrderByBinder.BindOrderByQueryOption(orderByQuery);
    //    }

    //    private static WhereClause ToFilterQuery(FilterQueryOption filterQuery)
    //    {
    //        return null;// NHibernateFilterBinder.BindFilterQueryOption(filterQuery);
    //    }
    //}

    //public class WhereClause
    //{
    //    public string Clause { get; set; }

    //    public object[] PositionalParameters { get; set; }
    //}
}