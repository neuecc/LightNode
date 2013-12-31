using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server.Tests
{
    public enum MyEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 300,
    }

    [Flags]
    public enum MyFlagEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 4,
        // = 8
        E = 16
    }

    [TestClass]
    public class MetaEnumTest
    {
        public TestContext TestContext { get; set; }

        // parse pattern, string | string value | value | mismatchtype value | enum

        [TestMethod]
        [TestCase("B", false, true, MyEnum.B)] // parse, ignoreCase, parseSuccess, result
        [TestCase("b", false, false, null)]
        [TestCase("b", true, true, MyEnum.B)]
        [TestCase("bb", true, false, null)]
        [TestCase("3", false, false, null)]
        [TestCase("2", false, true, MyEnum.C)]
        [TestCase(null, false, false, null)]
        [TestCase("", false, false, null)]
        [TestCase("34hoge", false, false, null)]
        [TestCase(3, false, false, null)]
        [TestCase(2, false, true, MyEnum.C)]
        [TestCase(3u, false, false, null)]
        [TestCase(2u, false, false, null)]
        [TestCase(MyEnum.B, false, true, MyEnum.B)]
        [TestCase((MyEnum)100, false, false, null)]

        public void TryParse()
        {
            var meta = new MetaEnum(typeof(MyEnum));

            TestContext.Run((object parse, bool ignoreCase, bool success, object result) =>
            {
                object r;
                meta.TryParse(parse, ignoreCase, out r).Is(success);
                if (success)
                {
                    r.Is(result);
                }
            });
        }

        [TestMethod]
        [TestCase("B", false, true, MyFlagEnum.B)] // parse, ignoreCase, parseSuccess, result
        [TestCase("b", false, false, null)]
        [TestCase("b", true, true, MyFlagEnum.B)]
        [TestCase("10", false, false, null)]
        [TestCase("2", false, true, MyFlagEnum.C)]
        [TestCase("3", false, true, MyFlagEnum.B | MyFlagEnum.C)]
        [TestCase("20", false, true, MyFlagEnum.D | MyFlagEnum.E)]
        [TestCase(3, false, true, MyFlagEnum.B | MyFlagEnum.C)]
        [TestCase(2, false, true, MyFlagEnum.C)]
        [TestCase(10, false, false, null)]
        [TestCase(3u, false, false, null)]
        [TestCase(2u, false, false, null)]
        [TestCase(MyFlagEnum.B, false, true, MyFlagEnum.B)]
        [TestCase(MyFlagEnum.B | MyFlagEnum.C, false, true, MyFlagEnum.B | MyFlagEnum.C)]
        [TestCase((MyFlagEnum)100, false, false, null)]
        public void FlagParse()
        {
            var meta = new MetaEnum(typeof(MyFlagEnum));

            TestContext.Run((object parse, bool ignoreCase, bool success, object result) =>
            {
                object r;
                meta.TryParse(parse, ignoreCase, out r).Is(success);
                if (success)
                {
                    r.Is(result);
                }
            });

        }
    }
}
