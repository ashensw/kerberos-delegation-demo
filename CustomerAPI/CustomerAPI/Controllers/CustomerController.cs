using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using CustomerAPI.Models;

namespace CustomerAPI.Controllers
{
    public class CustomerController : ApiController
    {
        public List<Customer> GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();
      
            string path = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "Resources\\Customers.xml");

            XDocument doc = XDocument.Load(path);
            foreach (XElement element in doc.Descendants("DocumentElement")
                .Descendants("Customer"))
            {
                Customer customer = new Customer();

                customer.CustomerID = element.Element("CustomerID").Value;
                customer.CompanyName = element.Element("CompanyName").Value;
                customer.City = element.Element("City").Value;

                customers.Add(customer);
            }

            return customers;
        }

        public Customer GetCustomer(int id)
        {
            Customer customer = new Customer();

            string path = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), "Resources\\Customers.xml");
            XDocument doc = XDocument.Load(path);

            XElement element = doc.Element("DocumentElement")
                                .Elements("Customer").Elements("CustomerID").
                                SingleOrDefault(x => x.Value == id.ToString());

            if (element != null)
            {
                XElement parent = element.Parent;

                customer.CustomerID =
                        parent.Element("CustomerID").Value;
                customer.CompanyName =
                        parent.Element("CompanyName").Value;
                customer.City =
                        parent.Element("City").Value;

                return customer;
            }
            else
            {
                throw new HttpResponseException
                    (new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}
