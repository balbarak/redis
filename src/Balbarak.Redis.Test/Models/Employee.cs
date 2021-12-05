using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Balbarak.Redis.Test.Models
{
    internal class Employee : CompareableObject
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();


        public static Employee Create()
        {
            Employee employee = new Employee()
            {
                FirstName = "Bader",
                LastName = "Smith",
                Age = 2
            };

            employee.Data.Add("Key", "1");
            employee.Data.Add("Key2", "2");
            employee.Data.Add("Key3", "3");

            return employee;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
            yield return Age;
            //yield return Data;
        }
    }
}
