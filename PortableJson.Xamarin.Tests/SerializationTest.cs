﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PortableJson.Xamarin.Tests.TestData;

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

            Assert.AreEqual("[\"The quick brown fox jumps over the lazy dog\",1337,null,{Name:\"Foobar\",Age:1337}]", json);
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

            var persons = group.Persons = new List<Person>();
            persons.Add(new Person()
            {
                Name = "Jens",
                Age = 1337
            });
            persons.Add(new Person()
            {
                Name = "Ole",
                Age = -10
            });

            Assert.AreEqual("{Persons:[{Name:\"Jens\",Age:1337},{Name:\"Ole\",Age:-10}],Title:\"My group\"}", JsonSerializationHelper.Serialize(group));
        }
    }
}
