using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerAPI.Models
{
    public class Customer
    {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string City { get; set; }
    }
}