using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CompareFunction.Model;
using MySql.Data.MySqlClient;

namespace CompareFunction
{
    public class QueryCompareNew
    {
        public ConnectDB db = new ConnectDB();
        public QueryCompareNew()
        {
        }

        public List<Survey> SelectSurvey()
        {
            // string query = "SELECT survey.uid, survey.survey_date, survey.month, survey.customer_name, service.question FROM used_services service  LEFT JOIN  surveys survey on survey.uid = service.survey_uid GROUP BY customer_name;";
            string query = "SELECT survey.uid, survey.survey_date, survey.month, survey.customer_name, service.question FROM used_services service  LEFT JOIN  surveys survey on survey.uid = service.survey_uid";
            var surveys = new List<Survey>();

            //Open connection
            if (db.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, db.con);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    surveys.Add(new Survey
                    {
                        SurveyUId = Encoding.UTF8.GetString((byte[])dataReader["uid"]),
                        SurveyDate = Convert.ToDateTime(dataReader["survey_date"]),
                        Month = ChangeFormatYear(dataReader["month"].ToString()),
                        CustomerName = dataReader["customer_name"].ToString(),
                        Services = dataReader["question"].ToString()
                    });
                }
                foreach (var item in surveys)
                {
                    System.Console.WriteLine($"{item.CustomerName}  {item.Services} {item.SurveyDate} {item.Month}");
                }

                var newSurveys = surveys.GroupBy(it => it.CustomerName)
                .SelectMany(it => it.GroupBy(i => i.SurveyDate))
                .Select(i => new Survey
                {
                    SurveyUId = i.FirstOrDefault().SurveyUId,
                    CustomerName = i.FirstOrDefault().CustomerName,
                    Month = i.FirstOrDefault().Month,
                    SurveyDate = i.Key,
                    Services = string.Join(',', i.Select(s => s.Services)),
                }).ToList();

                // Display data
                foreach (var item in newSurveys)
                {
                    System.Console.WriteLine($"{item.CustomerName}  {item.Services} {item.SurveyDate} {item.Month}");
                }

                //close Data Reader
                dataReader.Close();
                //close Connection
                db.CloseConnection();
                return newSurveys;
            }
            else
            {
                return new List<Survey>();
            }
        }

        public void CompareSurvey()
        {
            var surveys = SelectSurvey();
            var dataCompares = new List<Compare>();

            var dataCompare = surveys.GroupBy(it => it.CustomerName)
            .Select(it =>
            {
                var compare = new List<Compare>();
                if (it.Count() == 1)
                {
                    compare.Add(new Compare
                    {
                        SurveyUId = it.FirstOrDefault().SurveyUId,
                        CustomerName = it.FirstOrDefault().CustomerName,
                        Month = it.FirstOrDefault().Month,
                        CompareDate = it.FirstOrDefault().SurveyDate,
                        Services = it.FirstOrDefault().Services,
                        DiffDate = 0,
                        DateH1 = it.FirstOrDefault().SurveyDate,
                        DateH2 = it.FirstOrDefault().SurveyDate,
                    });
                    return new Compared
                    {
                        CustomerName = it.Key,
                        ListCompare = compare
                    };
                }
                else
                {
                    var h2 = it.OrderByDescending(date => date.SurveyDate).FirstOrDefault();
                    var services = h2.Services.Split(',');

                    var latesData = it.OrderByDescending(date => date.SurveyDate).Skip(1)
                        .Where(o => services.Any(i => o.Services.Contains(i)))
                        .Select(it => new
                        {
                            SurveyUId = it.SurveyUId,
                            CustomerName = it.CustomerName,
                            Month = FormatDateAndYear(it.Month),
                            MonthString = it.Month,
                            SurveyDate = it.SurveyDate,
                            Services = it.Services,
                            DiffDate = MonthDifference(FormatDateAndYear(h2.Month), FormatDateAndYear(it.Month))
                        })
                        .Where(it => it.DiffDate >= 6).Take(2).ToList();

                    if (!latesData.Any())
                    {
                        return new Compared
                        {
                            CustomerName = "",
                            ListCompare = new List<Compare>(),
                        };
                    }
                    else
                    {
                        compare.Add(new Compare
                        {
                            SurveyUId = latesData.FirstOrDefault().SurveyUId,
                            CustomerName = latesData.FirstOrDefault().CustomerName,
                            Month = latesData.FirstOrDefault().MonthString,
                            CompareDate = latesData.FirstOrDefault().SurveyDate,
                            Services = latesData.FirstOrDefault().Services,
                            DiffDate = latesData.FirstOrDefault().DiffDate,
                            DateH1 = latesData.LastOrDefault().SurveyDate,
                            DateH2 = latesData.FirstOrDefault().SurveyDate,
                            Group = "H1"
                        });
                        compare.Add(new Compare
                        {
                            SurveyUId = h2.SurveyUId,
                            CustomerName = h2.CustomerName,
                            Month = h2.Month,
                            CompareDate = h2.SurveyDate,
                            Services = h2.Services,
                            Group = "H2",
                            DateH1 = latesData.FirstOrDefault().SurveyDate,
                            DateH2 = h2.SurveyDate,
                            DiffDate = MonthDifference(FormatDateAndYear(h2.Month), latesData.FirstOrDefault().Month)
                        });
                        return new Compared
                        {
                            CustomerName = it.Key,
                            ListCompare = compare
                        };
                    }
                }
            })
            .ToList();

            var sql = new StringBuilder("INSERT INTO compares (survey_uid, customer_name, compare_date, month, h2, h1, diff_month, grouped) VALUES ");
            foreach (var item in dataCompare)
            {
                if (item.CustomerName != "")
                {
                    foreach (var compare in item.ListCompare)
                    {
                        sql.AppendFormat($"('{compare.SurveyUId}', '{compare.CustomerName}', '{compare.CompareDate.ToString("yyyy-MM-dd")}', '{compare.Month}', '{compare.DateH2.ToString("yyyy-MM-dd")}', '{compare.DateH1.ToString("yyyy-MM-dd")}', '{compare.DiffDate}', '{compare.Group}'), ");
                    }
                }
            }

            sql.ToString().TrimEnd(',', ' ');
            var sqlCommand = $"{sql.ToString().TrimEnd(',', ' ')};";

            System.Console.WriteLine(sqlCommand);

            if (db.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(sqlCommand, db.con);
                cmd.ExecuteNonQuery();
            }
            db.CloseConnection();

            // foreach (var item in dataCompare)
            // {
            //     System.Console.WriteLine($"{item.CustomerName}  {item.}");
            //     System.Console.WriteLine($"{item.CompareDate.ToString("yyyy-MM-dd")} { item.DateH2.ToString("yyyy-MM-dd")} { item.DateH1.ToString("yyyy-MM-dd")}");
            //     System.Console.WriteLine($"{item.DiffDate}");
            // }
        }

        public int MonthDifference(DateTime H2Date, DateTime H1Date)
        {
            return (H2Date.Month - H1Date.Month) + 12 * (H2Date.Year - H1Date.Year);
        }

        public string ChangeFormatYear(string monthyear)
        {
            if (monthyear.Contains("19"))
            {
                var date = monthyear.Split('-');
                var newYear = date[1].Replace(date[1], "2019");
                var newMY = $"{date[0]}-{newYear}";
                return newMY;
            }
            return monthyear;
        }

        public DateTime FormatDateAndYear(string my)
        {
            return DateTime.ParseExact(Convert.ToDateTime(my).ToString("yyyy-MM"), "yyyy-MM", CultureInfo.InvariantCulture);
        }
    }
}