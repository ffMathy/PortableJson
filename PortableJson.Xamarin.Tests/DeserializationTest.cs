using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortableJson.Xamarin.Tests.TestData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableJson.Xamarin.Tests
{
    [TestClass]
    public class DeserializationTest
    {
        [TestMethod]
        public void TestStringDeserialization()
        {
            Assert.AreEqual("hello world", JsonSerializationHelper.Deserialize<string>("\"hello world\""));
            Assert.AreEqual("hello Mr. \"officer\"!", JsonSerializationHelper.Deserialize<string>("\"hello Mr. \\\"officer\\\"!\""));
            Assert.AreEqual("The file is in C:\\mystuff", JsonSerializationHelper.Deserialize<string>("\"The file is in C:\\\\mystuff\""));

            Assert.AreEqual("null", JsonSerializationHelper.Deserialize<string>(null));
        }

        [TestMethod]
        public void TestNumericDeserialization()
        {
            Assert.AreEqual((double)1.337, JsonSerializationHelper.Deserialize<double>("1.337"));
            Assert.AreEqual((decimal)1.337, JsonSerializationHelper.Deserialize<decimal>("1.337"));
            Assert.AreEqual((float)1.337, JsonSerializationHelper.Deserialize<float>("1.337"));

            Assert.AreEqual((double)-1.337, JsonSerializationHelper.Deserialize<double>("-1.337"));
            Assert.AreEqual((decimal)-1.337, JsonSerializationHelper.Deserialize<decimal>("-1.337"));
            Assert.AreEqual((float)-1.337, JsonSerializationHelper.Deserialize<float>("-1.337"));

            Assert.AreEqual((double)0, JsonSerializationHelper.Deserialize<double>("0"));
            Assert.AreEqual((int)0, JsonSerializationHelper.Deserialize<int>("0"));

            Assert.AreEqual((int)1337, JsonSerializationHelper.Deserialize<int>("1337"));
            Assert.AreEqual((short)1337, JsonSerializationHelper.Deserialize<short>("1337"));
            Assert.AreEqual((long)1337, JsonSerializationHelper.Deserialize<long>("1337"));

            Assert.AreEqual((int)-1337, JsonSerializationHelper.Deserialize<int>("-1337"));
            Assert.AreEqual((short)-1337, JsonSerializationHelper.Deserialize<short>("-1337"));
            Assert.AreEqual((long)-1337, JsonSerializationHelper.Deserialize<long>("-1337"));
        }

        [TestMethod]
        public void TestObjectDeserialization()
        {
            var group = JsonSerializationHelper.Deserialize<Group>("{Persons:[{Name:\"Jens\",Age:1337},{Name:\"Ole\",Age:-10}],Title:\"My group\"}");
            Assert.IsNotNull(group);

            Assert.AreEqual("My group", group.Title);

            Assert.IsNotNull(group.Persons);
            Assert.AreEqual(2, group.Persons.Count);

            Assert.IsTrue(group.Persons.Any(p => p.Age == 1337 && p.Name == "Jens"));
            Assert.IsTrue(group.Persons.Any(p => p.Age == -10 && p.Name == "Ole"));
        }
    }
}
