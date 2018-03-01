using JQL;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace JQLTests
{
    [TestFixture]
    class JQLTests
    {
        const string MAP_FILE = "SourceClass.map";
        string mapPath;
        SourceClass sourceTest;
        SourceClass[] sourceMultipleTest;

        [OneTimeSetUp]
        public void Setup()
        {
            sourceTest = new SourceClass();
            sourceTest.Name = "SourceTest";
            sourceTest.Alias = new List<string>();
            sourceTest.Alias.Add("SourceTestA");
            sourceTest.Alias.Add("SourceTestB");
            sourceTest.Lucky = new SourceSubClass()
            {
                LuckyNumbers = new List<int>()
            };
            sourceTest.Lucky.LuckyNumbers.Add(7);
            sourceTest.Lucky.LuckyNumbers.Add(11);
            sourceTest.BirthDate = DateTime.Now.AddYears(-40);

            sourceMultipleTest = new SourceClass[2];
            sourceMultipleTest[0] = sourceTest;
            sourceMultipleTest[1] = sourceTest;

            mapPath = Path.Combine(TestContext.CurrentContext.TestDirectory, MAP_FILE);

            GenerateMap();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(mapPath);
        }

        [Test]
        public void ShouldParseClass()
        {
            var converter = new DataConvert<TestClass>(mapPath);
            var output = converter.Parse(sourceTest);
            Assert.AreEqual(output.Name, "SourceTest");
            Assert.AreEqual(output.TestSubClass.SubName, "SourceTestA");
            Assert.AreEqual(output.Numbers.Contains(7), true);
            Assert.AreEqual(output.Numbers.Contains(11), true);
        }

        [Test]
        public void ShouldParseThrowErrorList()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), new TestDelegate(throwsExceptionForArray));
        }

        private void throwsExceptionForList()
        {
            var converter = new DataConvert<List<TestClass>>(mapPath);
            var testClassCollection = new List<SourceClass>();
            testClassCollection.Add(sourceTest);
            converter.Parse(testClassCollection);
        }

        [Test]
        public void ShouldThrowErrorParseArray()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), new TestDelegate(throwsExceptionForArray));
        }

        private void throwsExceptionForArray()
        {
            var converter = new DataConvert<TestClass[]>(mapPath);
            var testClassArray = new SourceClass[1];
            testClassArray[0] = sourceTest;
            converter.Parse(testClassArray);
        }

        [Test]
        public void ShouldParseJson()
        {
            var json = JsonConvert.SerializeObject(sourceTest);
            var converter = new DataConvert<TestClass>(mapPath);
            var output = converter.Parse(json);
            Assert.AreEqual(output.Name, "SourceTest");
            Assert.AreEqual(output.TestSubClass.SubName, "SourceTestA");
            Assert.AreEqual(output.Numbers.Contains(7), true);
            Assert.AreEqual(output.Numbers.Contains(11), true);
        }

        [Test]
        public void ShouldParsesSingleFromCollection()
        {
            var json = JsonConvert.SerializeObject(sourceMultipleTest);
            var map = GenerateMapMultiple();
            var converter = new DataConvert<TestClass>(map, false);
            var output = converter.Parse(json);
            Assert.AreEqual(output.Name, "SourceTest");
            Assert.AreEqual(output.TestSubClass.SubName, "SourceTestA");
            Assert.AreEqual(output.Numbers.Contains(7), true);
            Assert.AreEqual(output.Numbers.Contains(11), true);

        }

        private void GenerateMap()
        {
            var result = string.Empty;
            var maps = new List<Map>();
            maps.Add(new Map("Name", PropertyType.StringType, "Name"));
            maps.Add(new Map("TestSubClass.SubName", PropertyType.StringType, "Alias[0]"));
            maps.Add(new Map("Numbers", PropertyType.IntTypeList, "Lucky.LuckyNumbers"));
            maps.Add(new Map("BirthDate", PropertyType.DateTimeType, "BirthDate"));

            result = JsonConvert.SerializeObject(maps.ToArray());

            File.WriteAllText(mapPath, result);
        }

        private string GenerateMapMultiple()
        {
            var result = string.Empty;
            var maps = new List<Map>();
            maps.Add(new Map("Name", PropertyType.StringType, "[0].Name"));
            maps.Add(new Map("TestSubClass.SubName", PropertyType.StringType, "[0].Alias[0]"));
            maps.Add(new Map("Numbers", PropertyType.IntTypeList, "[0].Lucky.LuckyNumbers"));
            maps.Add(new Map("BirthDate", PropertyType.DateTimeType, "[0].BirthDate"));

            result = JsonConvert.SerializeObject(maps.ToArray());

            return result;
        }

        public class TestClass
        {
            public string Name { get; set; }
            public List<int> Numbers { get; set; }
            public SubClass TestSubClass { get; set; }
            public DateTime BirthDate { get; set; }
        }

        public class SubClass
        {
            public int SubInt { get; set; }
            public string SubName { get; set; }
        }

        public class SourceClass
        {
            public string Name { get; set; }
            public List<string> Alias { get; set; }
            public SourceSubClass Lucky { get; set; }
            public DateTime BirthDate { get; set; }
        }

        public class SourceSubClass
        {
            public List<int> LuckyNumbers { get; set; }
        }
    }
}
