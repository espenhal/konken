using System;
using NUnit.Framework;
using web.Code;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine(Calculations.ConvertValueToDouble(990));
            Console.WriteLine(Calculations.ConvertValueToDouble(1000));
            Console.WriteLine(Calculations.ConvertValueToDouble(1001));
            Console.WriteLine(Calculations.ConvertValueToDouble(1053));
            
            Assert.Pass();
        }
    }
}