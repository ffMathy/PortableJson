using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PortableJson.Xamarin.Tests.TestData;
using System;

namespace PortableJson.Xamarin.Tests
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestArraySerialization()
        {
            var list = new List<object>();
            list.Add("The quick brown fox jumps over the lazy dog");
            list.Add(1337);
            list.Add(null);
            list.Add(new Person() { Name = "Foobar", Age = 1337 });

            var json = JsonSerializationHelper.Serialize(list);

            Assert.AreEqual("[\"The quick brown fox jumps over the lazy dog\",1337,null,{\"Id\":\"" + default(Guid) + "\",\"Name\":\"Foobar\",\"Age\":1337,\"IsAlive\":false}]", json);
        }

        [TestMethod]
        public void TestStringSerialization()
        {
            Assert.AreEqual("\"hello world\"", JsonSerializationHelper.Serialize("hello world"));
            Assert.AreEqual("\"hello Mr. \\\"officer\\\"!\"", JsonSerializationHelper.Serialize("hello Mr. \"officer\"!"));
            Assert.AreEqual("\"The file is in C:\\\\mystuff\"", JsonSerializationHelper.Serialize("The file is in C:\\mystuff"));

            Assert.AreEqual("null", JsonSerializationHelper.Serialize((string)null));
        }

        [TestMethod]
        public void TestBooleanSerialization()
        {
            Assert.AreEqual("true", JsonSerializationHelper.Serialize(true));
            Assert.AreEqual("false", JsonSerializationHelper.Serialize(false));
        }

        [TestMethod]
        public void TestNumericSerialization()
        {
            Assert.AreEqual("1.337", JsonSerializationHelper.Serialize((double)1.337));
            Assert.AreEqual("1.337", JsonSerializationHelper.Serialize((decimal)1.337));
            Assert.AreEqual("1.337", JsonSerializationHelper.Serialize((float)1.337));

            Assert.AreEqual("-1.337", JsonSerializationHelper.Serialize((double)-1.337));
            Assert.AreEqual("-1.337", JsonSerializationHelper.Serialize((decimal)-1.337));
            Assert.AreEqual("-1.337", JsonSerializationHelper.Serialize((float)-1.337));

            Assert.AreEqual("0", JsonSerializationHelper.Serialize((double)0));
            Assert.AreEqual("0", JsonSerializationHelper.Serialize((int)0));

            Assert.AreEqual("1337", JsonSerializationHelper.Serialize((int)1337));
            Assert.AreEqual("1337", JsonSerializationHelper.Serialize((short)1337));
            Assert.AreEqual("1337", JsonSerializationHelper.Serialize((long)1337));

            Assert.AreEqual("-1337", JsonSerializationHelper.Serialize((int)-1337));
            Assert.AreEqual("-1337", JsonSerializationHelper.Serialize((short)-1337));
            Assert.AreEqual("-1337", JsonSerializationHelper.Serialize((long)-1337));
        }

        [TestMethod]
        public void TestObjectSerialization()
        {
            var group = new Group();
            group.Title = "My group";
            group.IsActive = true;

            var persons = group.Persons = new List<Person>();
            persons.Add(new Person()
            {
                Name = "Jens",
                Age = 1337,
                IsAlive = true
            });
            persons.Add(new Person()
            {
                Name = "Ole",
                Age = -10,
                IsAlive = false
            });

            Assert.AreEqual("{\"Leader\":null,\"Persons\":[{\"Id\":\"" + default(Guid) + "\",\"Name\":\"Jens\",\"Age\":1337,\"IsAlive\":true},{\"Id\":\"" + default(Guid) + "\",\"Name\":\"Ole\",\"Age\":-10,\"IsAlive\":false}],\"Title\":\"My group\",\"IsActive\":true}", JsonSerializationHelper.Serialize(group));
        }
    }
}
