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
        D = 3,
    }

    [Flags]
    public enum MyFlagEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 4
    }

    [TestClass]
    public class MetaEnumTest
    {
        [TestMethod]
        public void IsDefined()
        {
            var meta = new MetaEnum(typeof(MyEnum));

            meta.IsDefined(MyEnum.A).IsTrue();
            meta.IsDefined(MyEnum.C).IsTrue();
            meta.IsDefined(MyFlagEnum.A).IsFalse();

            meta.IsDefined((int)MyEnum.A).IsTrue();
            meta.IsDefined((uint)MyEnum.A).IsFalse();

            meta.IsDefined("a").IsTrue();
        }

        [TestMethod]
        public void TryParse()
        {
            var meta = new MetaEnum(typeof(MyEnum));

            object result;
            meta.TryParse("a", true, out result).IsTrue();
            result.Is(MyEnum.A);
        }
    }
}
