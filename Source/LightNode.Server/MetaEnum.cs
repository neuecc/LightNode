using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LightNode.Server
{
    internal class MetaEnum : IEnumerable<KeyValuePair<string, object>>
    {
        delegate bool TryParseDelegate(string x, out object result);

        static readonly Dictionary<Type, TryParseDelegate> enumConvertTypeDictionary = new Dictionary<Type, TryParseDelegate>(8)
        {
            {typeof(Byte) ,(string x, out object result) => { Byte @out; var success = Byte.TryParse(x, out @out); result = (object)@out; return success; }}, // byte
            {typeof(SByte) ,(string x, out object result) => { SByte @out; var success = SByte.TryParse(x, out @out); result = (object)@out; return success; }}, // sbyte
            {typeof(Int16) ,(string x, out object result) => { Int16 @out; var success = Int16.TryParse(x, out @out); result = (object)@out; return success; }}, // short
            {typeof(UInt16) ,(string x, out object result) => { UInt16 @out; var success = UInt16.TryParse(x, out @out); result = (object)@out; return success; }}, // ushort
            {typeof(Int32) ,(string x, out object result) => { Int32 @out; var success = Int32.TryParse(x, out @out); result = (object)@out; return success; }}, // int
            {typeof(UInt32) ,(string x, out object result) => { UInt32 @out; var success = UInt32.TryParse(x, out @out); result = (object)@out; return success; }}, // uint
            {typeof(Int64),(string x, out object result) => { Int64 @out; var success = Int64.TryParse(x, out @out); result = (object)@out; return success; }}, // long
            {typeof(UInt64) , (string x, out object result) => { UInt64 @out; var success = UInt64.TryParse(x, out @out); result = (object)@out; return success; }}, // ulong
        };

        public Type EnumType { get; private set; }
        public Type EnumUnderlyingType { get; private set; }
        public bool IsBitFlag { get; private set; }

        // Key = fieldName, Value = object
        readonly KeyValuePair<string, object>[] Fields;
        readonly Dictionary<string, KeyValuePair<string, object>> byFieldName;
        readonly ILookup<string, KeyValuePair<string, object>> byFieldNameIgnoreCase;
        readonly Dictionary<object, KeyValuePair<string, object>> byValue;
        readonly Dictionary<object, KeyValuePair<string, object>> byUnderlyingType;

        // bitflag operator
        readonly object defaultConst;
        readonly object defaultValue;
        readonly object allBits;
        readonly object usedBits;
        readonly object unusedBits;
        readonly Func<object, object, object> or;
        readonly Func<object, object, object> and;
        readonly Func<object, object, object> xor;
        readonly Func<object, object> not;

        internal MetaEnum(Type enumType)
        {
            if (!enumType.IsEnum) throw new ArgumentException("ArgumentType is not Enum:" + enumType.FullName);

            EnumType = enumType;

            Fields = Enum.GetValues(enumType)
                .Cast<object>()
                .Select(x => new KeyValuePair<string, object>(Enum.GetName(enumType, x), x))
                .ToArray();

            byFieldName = Fields.ToDictionary(x => x.Key);
            byFieldNameIgnoreCase = Fields.ToLookup(x => x.Key, StringComparer.OrdinalIgnoreCase);
            byValue = Fields.ToDictionary(x => x.Value);

            var underlyingType = enumType.GetEnumUnderlyingType();
            EnumUnderlyingType = underlyingType;
            byUnderlyingType = Fields.ToDictionary(x => Convert.ChangeType(x.Value, underlyingType));

            // bit flags
            IsBitFlag = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

            // bit flag function, Code is inspired from UnconstrainedMelody by Jon Skeet.
            var objectX = Expression.Parameter(typeof(object), "x");
            var objectY = Expression.Parameter(typeof(object), "y");
            var unboxX = Expression.Unbox(objectX, underlyingType);
            var unboxY = Expression.Unbox(objectY, underlyingType);

            or = Expression.Lambda<Func<object, object, object>>(Expression.Convert(Expression.Or(unboxX, unboxY), typeof(object)), objectX, objectY).Compile();
            and = Expression.Lambda<Func<object, object, object>>(Expression.Convert(Expression.And(unboxX, unboxY), typeof(object)), objectX, objectY).Compile();
            xor = Expression.Lambda<Func<object, object, object>>(Expression.Convert(Expression.ExclusiveOr(unboxX, unboxY), typeof(object)), objectX, objectY).Compile();
            not = Expression.Lambda<Func<object, object>>(Expression.Convert(Expression.Not(unboxX), typeof(object)), objectX).Compile();

            defaultConst = Activator.CreateInstance(underlyingType);
            defaultValue = Enum.ToObject(enumType, defaultConst);

            usedBits = defaultConst;
            foreach (var item in Enum.GetValues(enumType))
            {
                usedBits = or(usedBits, item);
            }

            allBits = not(defaultConst);
            unusedBits = and(allBits, not(usedBits));
        }

        bool IsValidFlag(object flags)
        {
            return and(flags, unusedBits).Equals(defaultConst);
        }

        public bool IsDefined(object o, bool ignoreCase = false)
        {
            object result;
            return TryParse(o, ignoreCase, out result);
        }

        public bool TryParse(object value, out object result)
        {
            return TryParse(value, false, out result);
        }

        public bool TryParse(object value, bool ignoreCase, out object result)
        {
            if (value == null)
            {
                result = null;
                return false;
            }
            if (value.GetType() == EnumType)
            {
                if (IsBitFlag)
                {
                    if (IsValidFlag(value))
                    {
                        result = value;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    KeyValuePair<string, object> meta;
                    if (byValue.TryGetValue(value, out meta))
                    {
                        result = meta.Value;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }

            var name = value as string;
            if (name != null)
            {
                if (name.Length == 0)
                {
                    result = null;
                    return false;
                }

                if (char.IsDigit(name[0]) || name[0] == '-' || name[0] == '+')
                {
                    // Character is Value String
                    object primitiveValue;
                    if (enumConvertTypeDictionary[EnumUnderlyingType](name, out primitiveValue))
                    {
                        value = primitiveValue;
                        goto FromValueType;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }

                if (ignoreCase)
                {
                    var r = byFieldNameIgnoreCase[name].FirstOrDefault();
                    if (r.Key == null)
                    {
                        result = null;
                        return false;
                    }
                    else
                    {
                        result = r.Value;
                        return true;
                    }
                }
                else
                {
                    KeyValuePair<string, object> r;
                    if (byFieldName.TryGetValue(name, out r))
                    {
                        result = r.Value;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }
        FromValueType:
            if (IsBitFlag)
            {
                if (IsValidFlag(value))
                {
                    result = Enum.ToObject(EnumType, value);
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            else
            {
                KeyValuePair<string, object> enumValue;
                if (byUnderlyingType.TryGetValue(value, out enumValue))
                {
                    result = enumValue.Value;
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Fields.AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}