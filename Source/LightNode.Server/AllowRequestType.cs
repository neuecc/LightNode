using System;
using System.Collections.Generic;
using System.Linq;

namespace LightNode.Server
{
    internal class AllowRequestType
    {
        public delegate bool TryParse(string x, out object result);

        static readonly Dictionary<Type, TryParse> convertTypeDictionary = new Dictionary<Type, TryParse>(33)
        {
            {typeof(string), (string x,out object result) => {result = (object)x; return true;}},
            {typeof(DateTime) ,(string x, out object result) => { DateTime @out; var success = DateTime.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<DateTime>),(string x, out object result) => { DateTime @out; result = DateTime.TryParse(x, out @out) ? (object)@out : null; return true; }},
            {typeof(DateTimeOffset) ,(string x, out object result) => { DateTimeOffset @out; var success = DateTimeOffset.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<DateTimeOffset>),(string x, out object result) => { DateTimeOffset @out; result = DateTimeOffset.TryParse(x, out @out) ? (object)@out : null; return true; }},
            {typeof(Boolean) ,(string x, out object result) => { Boolean @out; var success = Boolean.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Boolean>),(string x, out object result) => { Boolean @out; result = Boolean.TryParse(x, out @out) ? (object)@out : null; return true; }},
            {typeof(Decimal) ,(string x, out object result) => { Decimal @out; var success = Decimal.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Decimal>),(string x, out object result) => { Decimal @out; result =  Decimal.TryParse(x, out @out) ? (object)@out: null; return true; }},
            {typeof(Char) ,(string x, out object result) => { Char @out; var success = Char.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Char>),(string x, out object result) => { Char @out; result =  Char.TryParse(x, out @out) ? (object)@out : null; return true; }},
            {typeof(TimeSpan) ,(string x, out object result) => { TimeSpan @out; var success = TimeSpan.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<TimeSpan>),(string x, out object result) => { TimeSpan @out; result =  TimeSpan.TryParse(x, out @out)?(object)@out:null; return true; }},
            {typeof(Int16) ,(string x, out object result) => { Int16 @out; var success = Int16.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Int16>),(string x, out object result) => { Int16 @out; result =  Int16.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(Int32) ,(string x, out object result) => { Int32 @out; var success = Int32.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Int32>),(string x, out object result) => { Int32 @out; result =  Int32.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(Int64),(string x, out object result) => { Int64 @out; var success = Int64.TryParse(x, out @out); result = (object)@out; return success; }}, 
            {typeof(Nullable<Int64>),(string x, out object result) => { Int64 @out; result =  Int64.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(UInt16) ,(string x, out object result) => { UInt16 @out; var success = UInt16.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<UInt16>),(string x, out object result) => { UInt16 @out; result =  UInt16.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(UInt32) ,(string x, out object result) => { UInt32 @out; var success = UInt32.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<UInt32>),(string x, out object result) => { UInt32 @out; result =  UInt32.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(UInt64) , (string x, out object result) => { UInt64 @out; var success = UInt64.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<UInt64>), (string x, out object result) => { UInt64 @out; result =  UInt64.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(Single) ,(string x, out object result) => { Single @out; var success = Single.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Single>),(string x, out object result) => { Single @out; result =  Single.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(Double),(string x, out object result) => { Double @out; var success = Double.TryParse(x, out @out); result = (object)@out; return success; }}, 
            {typeof(Nullable<Double>),(string x, out object result) => { Double @out; result =  Double.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(SByte) ,(string x, out object result) => { SByte @out; var success = SByte.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<SByte>),(string x, out object result) => { SByte @out; result =  SByte.TryParse(x, out @out)? (object)@out:null; return true; }},
            {typeof(Byte) ,(string x, out object result) => { Byte @out; var success = Byte.TryParse(x, out @out); result = (object)@out; return success; }},
            {typeof(Nullable<Byte>),(string x, out object result) => { Byte @out; result =  Byte.TryParse(x, out @out)? (object)@out:null; return true; }},
        };

        static readonly Dictionary<Type, Func<IEnumerable<string>, object>> convertArrayTypeDictionary = new Dictionary<Type, Func<IEnumerable<string>, object>>(16)
        {
            // NOTE:unsupport byte[] because request message will be very large. instead of use Base64.
            {typeof(string[]), (IEnumerable<string> xs) => (object)xs.ToArray()},
            {typeof(DateTime[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    DateTime @out;
                    if(!DateTime.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new DateTime[0];
            }},
            {typeof(DateTimeOffset[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    DateTimeOffset @out;
                    if(!DateTimeOffset.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new DateTimeOffset[0];
            }},
            {typeof(Boolean[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Boolean @out;
                    if(!Boolean.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Boolean[0];
            }},
            {typeof(Decimal[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Decimal @out;
                    if(!Decimal.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Decimal[0];
            }},
            {typeof(Char[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Char @out;
                    if(!Char.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Char[0];
            }},
            {typeof(TimeSpan[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    TimeSpan @out;
                    if(!TimeSpan.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new TimeSpan[0];
            }},
            {typeof(Int16[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Int16 @out;
                    if(!Int16.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Int16[0];
            }},
            {typeof(Int32[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Int32 @out;
                    if(!Int32.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Int32[0];
            }},
            {typeof(Int64[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Int64 @out;
                    if(!Int64.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Int64[0];
            }},
            {typeof(UInt16[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    UInt16 @out;
                    if (!UInt16.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new UInt16[0];
            }},
            {typeof(UInt32[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    UInt32 @out;
                    if(!UInt32.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new UInt32[0];
            }},
            {typeof(UInt64[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    UInt64 @out;
                    if(!UInt64.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new UInt64[0];
            }},
            {typeof(Single[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Single @out;
                    if(!Single.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Single[0];
            }},
            {typeof(Double[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    Double @out;
                    if(!Double.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new Double[0];
            }},
            {typeof(SByte[]), (IEnumerable<string> xs) =>
            {
                bool success = true;
                var result = xs.Select(x =>
                {
                    SByte @out;
                    if(!SByte.TryParse(x, out @out)) success = false;
                    return @out;
                }).ToArray();
                return (success) ? (object)result : new SByte[0];
            }}
        };

        internal static bool IsAllowType(Type targetType)
        {
            return GetConverter(targetType) != null
                || GetArrayConverter(targetType) != null
                || targetType.IsEnum
                || (targetType.IsNullable() && targetType.GetGenericArguments().First().IsEnum);
        }

        internal static TryParse GetConverter(Type targetType)
        {
            TryParse f;
            var result = convertTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
            if (result != null) return result;


            if (targetType.IsEnum)
            {
                return new TryParse((string x, out object o) => AllowRequestType.TryParseEnum(targetType, x, out o));
            }
            else if (targetType.IsNullable())
            {
                var genArg = targetType.GetGenericArguments().First();
                if (genArg.IsEnum)
                {
                    return new TryParse((string x, out object o) => AllowRequestType.TryParseEnum(genArg, x, out o));
                }
            }

            return result;
        }

        internal static Func<IEnumerable<string>, object> GetArrayConverter(Type targetType)
        {
            Func<IEnumerable<string>, object> f;
            var result = convertArrayTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
            if (result != null) return result;

            var elemType = targetType.GetElementType();
            if (elemType.IsEnum)
            {
                // TODO:perforamnce improvement 
                return new Func<IEnumerable<string>, object>((IEnumerable<string> xs) =>
                {
                    var success = true;
                    var array = xs.Select(x =>
                    {
                        object @out;
                        if (!TryParseEnum(elemType, x, out @out))
                        {
                            success = false;
                        }

                        return (success)
                            ? Convert.ChangeType(@out, elemType)
                            : null;
                    })
                    .ToArray();

                    if (success)
                    {
                        var resultArray = Array.CreateInstance(elemType, array.Length);
                        Array.Copy(array, resultArray, resultArray.Length);
                        return resultArray;
                    }
                    else
                    {
                        return Array.CreateInstance(elemType, 0);
                    }
                });
            }

            return result;
        }

        static bool TryParseEnum(Type targetType, string value, out object result)
        {
            try
            {
                // TODO:very ugly way, must be perf improvement
                result = Enum.Parse(targetType, value, ignoreCase: true);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}