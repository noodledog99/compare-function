using System;

namespace CompareFunction.Model
{
    public class Compare
    {
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string SurveyUId { get; set; }

        ///<summary>
        /// Survey Date
        ///</summary>
        public DateTime CompareDate { get; set; }
        public string Month { get; set; }
        public DateTime Month1{ get; set; }

        public DateTime DateH1 { get; set; }

        ///<summary>
        /// Max Date
        ///</summary>
        public DateTime DateH2 { get; set; }
        public int DiffDate { get; set; }

        ///<summary>
        /// H1 = Old survey data
        /// H2 = Current survey data
        /// NULL = No survey
        ///</summary>
        public string Group { get; set; }
        public string Services { get; set; }

    }
}