using System;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LightNode.Server
{
    internal static class TypeBinder
    {
        public delegate bool TryParse(string x, out object result);

        readonly static ConcurrentDictionary<Type, MetaEnum> metaEnumCache = new ConcurrentDictionary<Type, MetaEnum>();
        readonly static ConcurrentDictionary<Type, Func<int, Array>> arrayInitCache = new ConcurrentDictionary<Type, Func<int, Array>>();

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

        static readonly Dictionary<Type, Func<IList<string>, object>> convertArrayTypeDictionary = new Dictionary<Type, Func<IList<string>, object>>(16)
        {
            // NOTE:unsupport byte[] because request message will be very large. use Base64 instead of byte[].
            {typeof(string[]), (IList<string> xs) => (object)xs.ToArray()},
            {typeof(DateTime[]), (IList<string> xs) =>
            {
                var result = new DateTime[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    DateTime @out;
                    if (DateTime.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new DateTime[0];
                }
                return result;
            }},
            {typeof(DateTimeOffset[]), (IList<string> xs) =>
            {
                var result = new DateTimeOffset[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    DateTimeOffset @out;
                    if (DateTimeOffset.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new DateTimeOffset[0];
                }
                return result;
            }},
            {typeof(Boolean[]), (IList<string> xs) =>
            {
                var result = new Boolean[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Boolean @out;
                    if (Boolean.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Boolean[0];
                }
                return result;
            }},
            {typeof(Decimal[]), (IList<string> xs) =>
            {
                var result = new Decimal[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Decimal @out;
                    if (Decimal.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Decimal[0];
                }
                return result;
            }},
            {typeof(Char[]), (IList<string> xs) =>
            {
                var result = new Char[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Char @out;
                    if (Char.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Char[0];
                }
                return result;
            }},
            {typeof(TimeSpan[]), (IList<string> xs) =>
            {
                var result = new TimeSpan[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    TimeSpan @out;
                    if (TimeSpan.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new TimeSpan[0];
                }
                return result;
            }},
            {typeof(Int16[]), (IList<string> xs) =>
            {
                var result = new Int16[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Int16 @out;
                    if (Int16.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Int16[0];
                }
                return result;
            }},
            {typeof(Int32[]), (IList<string> xs) =>
            {
                var result = new Int32[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Int32 @out;
                    if (Int32.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Int32[0];
                }
                return result;
            }},
            {typeof(Int64[]), (IList<string> xs) =>
            {
                var result = new Int64[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Int64 @out;
                    if (Int64.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Int64[0];
                }
                return result;
            }},
            {typeof(UInt16[]), (IList<string> xs) =>
            {
                var result = new UInt16[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    UInt16 @out;
                    if (UInt16.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new UInt16[0];
                }
                return result;
            }},
            {typeof(UInt32[]), (IList<string> xs) =>
            {
                var result = new UInt32[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    UInt32 @out;
                    if (UInt32.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new UInt32[0];
                }
                return result;
            }},
            {typeof(UInt64[]), (IList<string> xs) =>
            {
                var result = new UInt64[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    UInt64 @out;
                    if (UInt64.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new UInt64[0];
                }
                return result;
            }},
            {typeof(Single[]), (IList<string> xs) =>
            {
                var result = new Single[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Single @out;
                    if (Single.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Single[0];
                }
                return result;
            }},
            {typeof(Double[]), (IList<string> xs) =>
            {
                var result = new Double[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    Double @out;
                    if (Double.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new Double[0];
                }
                return result;
            }},
            {typeof(SByte[]), (IList<string> xs) =>
            {
                var result = new SByte[xs.Count];
                for (int i = 0; i < xs.Count; i++)
                {
                    SByte @out;
                    if (SByte.TryParse(xs[i], out @out)) result[i] = @out;
                    else return new SByte[0];
                }
                return result;
            }}
        };

        internal static bool IsAllowType(Type targetType)
        {
            return GetConverter(targetType, false) != null
                || GetArrayConverter(targetType, false) != null;
        }

        internal static TryParse GetConverter(Type targetType, bool enumStrictParse)
        {
            TryParse f;
            var result = convertTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
            if (result != null) return result;

            if (targetType.IsEnum())
            {
                var meta = metaEnumCache.GetOrAdd(targetType, x => new MetaEnum(x));
                return (enumStrictParse)
                    ? new TryParse((string x, out object o) => meta.TryParseStrict(x, out o))
                    : new TryParse((string x, out object o) => meta.TryParse(x, true, out o));
            }
            else if (targetType.IsNullable())
            {
                var genArg = targetType.GetGenericArguments()[0];
                if (genArg.IsEnum())
                {
                    var meta = metaEnumCache.GetOrAdd(genArg, x => new MetaEnum(x));
                    return (enumStrictParse)
                        ? new TryParse((string x, out object o) => meta.TryParseStrict(x, out o))
                        : new TryParse((string x, out object o) => meta.TryParse(x, true, out o));
                }
            }

            return result;
        }

        internal static Func<IList<string>, object> GetArrayConverter(Type targetType, bool enumStrictParse)
        {
            if (!targetType.IsArray) return null;

            Func<IList<string>, object> f;
            var result = convertArrayTypeDictionary.TryGetValue(targetType, out f)
                ? f
                : null;
            if (result != null) return result;

            var elemType = targetType.GetElementType();
            if (elemType == null) return null;
            if (elemType.IsEnum())
            {
                var tryParse = GetConverter(elemType, enumStrictParse);

                var arrayInitializer = arrayInitCache.GetOrAdd(elemType, _type =>
                {
                    var length = Expression.Parameter(typeof(int), "length");
                    return Expression.Lambda<Func<int, Array>>(Expression.NewArrayBounds(_type, length), length).Compile();
                });

                return new Func<IList<string>, object>((IList<string> xs) =>
                {
                    var array = new object[xs.Count];
                    for (int i = 0; i < xs.Count; i++)
                    {
                        object @out;
                        if (tryParse(xs[i], out @out)) array[i] = @out;
                        else return arrayInitializer(0);
                    }

                    var resultArray = arrayInitializer(array.Length);
                    Array.Copy(array, resultArray, array.Length);
                    return resultArray;
                });
            }

            return result;
        }
    }
}