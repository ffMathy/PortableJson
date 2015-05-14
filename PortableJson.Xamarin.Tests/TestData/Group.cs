using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableJson.Xamarin.Tests.TestData
{
    class Group
    {
        public List<Person> Persons { get; set; }

        public string Title { get; set; }

        public bool IsActive { get; set; }
    }
}
