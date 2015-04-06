using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PortableJson.Xamarin;

namespace PortableJson.Xamarin.Tests
{
    [TestClass]
    public class SerializationTest
    {
        [TestMethod]
        public void TestArraySerialization()
        {
            var list = new List<string>();
            list.Add("The quick brown fox");
            list.Add("Jumps over the");
            list.Add("Lazy dog");

            var json = JsonSerializationHelper.Serialize(list);

            Assert.AreEqual("[\"The quick brown fox\",\"Jumps over the\",\"Lazy dog\"]", json);
        }
    }
}
