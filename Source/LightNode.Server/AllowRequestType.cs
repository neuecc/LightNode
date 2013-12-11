using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class AllowRequestType
    {
        internal static bool IsAllowType(Type targetType)
        {
            return GetConverter(targetType) != null || GetArrayConverter(targetType) != null;
        }

        internal static Func<string, object> GetConverter(Type targetType)
        {
            if (targetType == typeof(string))
            {
                return (string x) => (object)x;
            }
            else if (targetType == typeof(DateTime) || targetType == typeof(Nullable<DateTime>))
            {
                return (string x) => (object)Convert.ToDateTime(x);
            }
            else if (targetType == typeof(DateTimeOffset) || targetType == typeof(Nullable<DateTimeOffset>))
            {
                return (string x) => (object)DateTimeOffset.Parse(x);
            }
            else if (targetType == typeof(Boolean) || targetType == typeof(Nullable<Boolean>))
            {
                return (string x) => (object)Convert.ToBoolean(x);
            }
            else if (targetType == typeof(Decimal) || targetType == typeof(Nullable<Decimal>))
            {
                return (string x) => (object)Convert.ToDecimal(x);
            }
            else if (targetType == typeof(Char) || targetType == typeof(Nullable<Char>))
            {
                return (string x) => (object)Convert.ToChar(x);
            }
            else if (targetType == typeof(TimeSpan) || targetType == typeof(Nullable<TimeSpan>))
            {
                return (string x) => (object)TimeSpan.Parse(x);
            }
            else if (targetType == typeof(Int16) || targetType == typeof(Nullable<Int16>))
            {
                return (string x) => (object)Convert.ToInt16(x);
            }
            else if (targetType == typeof(Int32) || targetType == typeof(Nullable<Int32>))
            {
                return (string x) => (object)Convert.ToInt32(x);
            }
            else if (targetType == typeof(Int64) || targetType == typeof(Nullable<Int64>))
            {
                return (string x) => (object)Convert.ToInt64(x);
            }
            else if (targetType == typeof(UInt16) || targetType == typeof(Nullable<UInt16>))
            {
                return (string x) => (object)Convert.ToUInt16(x);
            }
            else if (targetType == typeof(UInt16) || targetType == typeof(Nullable<UInt16>))
            {
                return (string x) => (object)Convert.ToUInt16(x);
            }
            else if (targetType == typeof(UInt32) || targetType == typeof(Nullable<UInt32>))
            {
                return (string x) => (object)Convert.ToUInt32(x);
            }
            else if (targetType == typeof(UInt64) || targetType == typeof(Nullable<UInt64>))
            {
                return (string x) => (object)Convert.ToUInt64(x);
            }
            else if (targetType == typeof(Single) || targetType == typeof(Nullable<Single>))
            {
                return (string x) => (object)Convert.ToSingle(x);
            }
            else if (targetType == typeof(Double) || targetType == typeof(Nullable<Double>))
            {
                return (string x) => (object)Convert.ToDouble(x);
            }
            else if (targetType == typeof(Double) || targetType == typeof(Nullable<Double>))
            {
                return (string x) => (object)Convert.ToDouble(x);
            }
            else if (targetType == typeof(SByte) || targetType == typeof(Nullable<SByte>))
            {
                return (string x) => (object)Convert.ToSByte(x);
            }
            else if (targetType == typeof(Byte) || targetType == typeof(Nullable<Byte>))
            {
                return (string x) => (object)Convert.ToByte(x);
            }
            else
            {
                // targetType is not supported
                return null;
            }
        }

        internal static Func<IEnumerable<string>, object> GetArrayConverter(Type targetType)
        {
            if (targetType == typeof(string[]))
            {
                return (IEnumerable<string> xs) => (object)xs.ToArray();
            }
            else if (targetType == typeof(DateTime[]) || targetType == typeof(Nullable<DateTime>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDateTime(x)).ToArray();
            }
            else if (targetType == typeof(DateTimeOffset[]) || targetType == typeof(Nullable<DateTimeOffset>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => DateTimeOffset.Parse(x)).ToArray();
            }
            else if (targetType == typeof(Boolean[]) || targetType == typeof(Nullable<Boolean>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToBoolean(x)).ToArray();
            }
            else if (targetType == typeof(Decimal[]) || targetType == typeof(Nullable<Decimal>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDecimal(x)).ToArray();
            }
            else if (targetType == typeof(Char[]) || targetType == typeof(Nullable<Char>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToChar(x)).ToArray();
            }
            else if (targetType == typeof(TimeSpan[]) || targetType == typeof(Nullable<TimeSpan>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => TimeSpan.Parse(x)).ToArray();
            }
            else if (targetType == typeof(Int16[]) || targetType == typeof(Nullable<Int16>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt16(x)).ToArray();
            }
            else if (targetType == typeof(Int32[]) || targetType == typeof(Nullable<Int32>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt32(x)).ToArray();
            }
            else if (targetType == typeof(Int64[]) || targetType == typeof(Nullable<Int64>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToInt64(x)).ToArray();
            }
            else if (targetType == typeof(UInt16[]) || targetType == typeof(Nullable<UInt16>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt16(x)).ToArray();
            }
            else if (targetType == typeof(UInt16[]) || targetType == typeof(Nullable<UInt16>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt16(x)).ToArray();
            }
            else if (targetType == typeof(UInt32[]) || targetType == typeof(Nullable<UInt32>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt32(x)).ToArray();
            }
            else if (targetType == typeof(UInt64[]) || targetType == typeof(Nullable<UInt64>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToUInt64(x)).ToArray();
            }
            else if (targetType == typeof(Single[]) || targetType == typeof(Nullable<Single>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSingle(x)).ToArray();
            }
            else if (targetType == typeof(Double[]) || targetType == typeof(Nullable<Double>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDouble(x)).ToArray();
            }
            else if (targetType == typeof(Double[]) || targetType == typeof(Nullable<Double>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToDouble(x)).ToArray();
            }
            else if (targetType == typeof(SByte[]) || targetType == typeof(Nullable<SByte>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToSByte(x)).ToArray();
            }
            else if (targetType == typeof(Byte[]) || targetType == typeof(Nullable<Byte>[]))
            {
                return (IEnumerable<string> xs) => (object)xs.Select(x => Convert.ToByte(x)).ToArray();
            }
            else
            {
                // targetType is not supported
                return null;
            }
        }
    }
}