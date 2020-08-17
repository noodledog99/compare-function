using System;
using System.Globalization;
using System.Text;
using CompareFunction.Model;
using MySql.Data.MySqlClient;
using NodaTime;

namespace CompareFunction
{
    class Program
    {
        static void Main(string[] args)
        {
            var query = new QueryCompareNew();
            query.CompareSurvey();
        }
    }
}