using System;
using System.Collections.Generic;
using System.Linq;

namespace LightNode.Server
{
    internal class AllowRequestType
    {
        static readonly Dictionary<Type, Func<string, object>> convertTypeDictionary = new Dictionary<Type, Func<string, object>>(33)
        {
            {typeof(string), (string x) => (object)x},
            {typeof(DateTime) ,(string x) => (object)Convert.ToDateTime(x)},
            {typeof(Nullable<DateTime>),(string x) => (object)Convert.ToDateTime(x)},
            {typeof(DateTimeOffset) ,(string x) => (object)DateTimeOffset.Parse(x)},
            {typeof(Nullable<DateTimeOffset>),(string x) => (object)DateTimeOffset.Parse(x)},
            {typeof(Boolean) ,(string x) => (object)Convert.ToBoolean(x)},
            {typeof(Nullable<Boolean>),(string x) => (object)Convert.ToBoolean(x)},
            {typeof(Decimal) ,(string x) => (object)Convert.ToDecimal(x)},
            {typeof(Nullable<Decimal>),(string x) => (object)Convert.ToDecimal(x)},
            {typeof(Char) ,(string x) => (object)Convert.ToChar(x)},
            {typeof(Nullable<Char>),(string x) => (object)Convert.ToChar(x)},
            {typeof(TimeSpan) ,(string x) => (object)TimeSpan.Parse(x)},
            {typeof(Nullable<TimeSpan>),(string x) => (object)TimeSpan.Parse(x)},
            {typeof(Int16) ,(string x) => (object)Convert.ToInt16(x)},
            {typeof(Nullable<Int16>),(string x) => (object)Convert.ToInt16(x)},
            {typeof(Int32) ,(string x) => (object)Convert.ToInt32(x)},
            {typeof(Nullable<Int32>),(string x) => (object)Convert.ToInt32(x)},
            {typeof(Int64),(string x) => (object)Convert.ToInt64(x)}, 
            {typeof(Nullable<Int64>),(string x) => (object)Convert.ToInt64(x)},
            {typeof(UInt16) ,(string x) => (object)Convert.ToUInt16(x)},
            {typeof(Nullable<UInt16>),(string x) => (object)Convert.ToUInt16(x)},
            {typeof(UInt32) ,(string x) => (object)Convert.ToUInt32(x)},
            {typeof(Nullable<UInt32>),(string x) => (object)Convert.ToUInt32(x)},
            {typeof(UInt64) , (string x) => (object)Convert.ToUInt64(x)},
            {typeof(Nullable<UInt64>), (string x) => (object)Convert.ToUInt64(x)},
            {typeof(Single) ,(string x) => (object)Convert.ToSingle(x)},
            {typeof(Nullable<Single>),(string x) => (object)Convert.ToSingle(x)},
            {typeof(Double),(string x) => (object)Convert.ToDouble(x)}, 
            {typeof(Nullable<Double>),(string x) => (object)Convert.ToDouble(x)},
            {typeof(SByte) ,(string x) => (object)Convert.ToSByte(x)},
            {typeof(Nullable<SByte>),(string x) => (object)Convert.ToSByte(x)},
            {typeof(Byte) ,(string x) => (object)Convert.ToByte(x)},
            {typeof(Nullable<Byte>),(string x) => (object)Convert.ToByte(x)},
        };

        static readonly Dictionary<Type, Func<IEnumerable<string>, object>> convertArrayTypeDictionary = new Dictionary<Type, Func<IEnumerable<string>, object>>(31)
        {
            // NOTE:unsupport byte[] because request message will be very large. instead of use Base64.
            {typeof(string[]),(IEnumerable<string> xs) => (object)xs.ToArray()},
            {typeof(DateTime[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDateTime(x)).ToArray()},
            {typeof(Nullable<DateTime>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDateTime(x)).ToArray()},
            {typeof(DateTimeOffset[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => DateTimeOffset.Parse(x)).ToArray()},
            {typeof(Nullable<DateTimeOffset>[]),(IEnumerable<string> xs) => (object)xs.Select(x => DateTimeOffset.Parse(x)).ToArray()},
            {typeof(Boolean[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToBoolean(x)).ToArray()},
            {typeof(Nullable<Boolean>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToBoolean(x)).ToArray()},
            {typeof(Decimal[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDecimal(x)).ToArray()},
            {typeof(Nullable<Decimal>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDecimal(x)).ToArray()},
            {typeof(Char[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToChar(x)).ToArray()},
            {typeof(Nullable<Char>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToChar(x)).ToArray()},
            {typeof(TimeSpan[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => TimeSpan.Parse(x)).ToArray()},
            {typeof(Nullable<TimeSpan>[]),(IEnumerable<string> xs) => (object)xs.Select(x => TimeSpan.Parse(x)).ToArray()},
            {typeof(Int16[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt16(x)).ToArray()},
            {typeof(Nullable<Int16>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt16(x)).ToArray()},
            {typeof(Int32[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt32(x)).ToArray()},
            {typeof(Nullable<Int32>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt32(x)).ToArray()},
            {typeof(Int64[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt64(x)).ToArray()},
            {typeof(Nullable<Int64>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt64(x)).ToArray()},
            {typeof(UInt16[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt16(x)).ToArray()},
            {typeof(Nullable<UInt16>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt16(x)).ToArray()},
            {typeof(UInt32[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt32(x)).ToArray()},
            {typeof(Nullable<UInt32>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt32(x)).ToArray()},
            {typeof(UInt64[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt64(x)).ToArray()},
            {typeof(Nullable<UInt64>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt64(x)).ToArray()},
            {typeof(Single[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSingle(x)).ToArray()},
            {typeof(Nullable<Single>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSingle(x)).ToArray()},
            {typeof(Double[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDouble(x)).ToArray()},
            {typeof(Nullable<Double>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDouble(x)).ToArray()},
            {typeof(SByte[]) ,(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSByte(x)).ToArray()},
            {typeof(Nullable<SByte>[]),(IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSByte(x)).ToArray()}
        };

        internal static bool IsAllowType(Type targetType)
        {
            return GetConverter(targetType) != null || GetArrayConverter(targetType) != null;
        }

        internal static Func<string, object> GetConverter(Type targetType)
        {
            Func<string, object> f;
            return convertTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
        }

        internal static Func<IEnumerable<string>, object> GetArrayConverter(Type targetType)
        {
            Func<IEnumerable<string>, object> f;
            return convertArrayTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
        }
    }
}