using System;
using System.Collections.Generic;
using System.Linq;

namespace LightNode.Server
{
    internal class MetaEnum : IEnumerable<KeyValuePair<string, object>>
    {
        public Type EnumType { get; private set; }
        public bool IsBitFlag { get; private set; }

        // Key = fieldName, Value = object
        readonly KeyValuePair<string, object>[] Fields;
        readonly Dictionary<string, KeyValuePair<string, object>> byFieldName;
        readonly ILookup<string, KeyValuePair<string, object>> byFieldNameIgnoreCase;
        readonly Dictionary<object, KeyValuePair<string, object>> byValue;
        readonly Dictionary<object, KeyValuePair<string, object>> byUnderlyingType;

        internal MetaEnum(Type enumType)
        {
            IsBitFlag = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();
            EnumType = enumType;

            Fields = Enum.GetValues(enumType)
                .Cast<object>()
                .Select(x => new KeyValuePair<string, object>(Enum.GetName(enumType, x), x))
                .ToArray();

            byFieldName = Fields.ToDictionary(x => x.Key);
            byFieldNameIgnoreCase = Fields.ToLookup(x => x.Key, StringComparer.OrdinalIgnoreCase);
            byValue = Fields.ToDictionary(x => x.Value);

            var underlyingType = enumType.GetEnumUnderlyingType();
            byUnderlyingType = Fields.ToDictionary(x => Convert.ChangeType(x.Value, underlyingType));
        }

        public bool IsDefined(object o, bool ignoreCase = false)
        {
            if (o == null) return false;

            var name = o as string;
            if (name != null)
            {
                if (ignoreCase)
                {
                    return byFieldNameIgnoreCase.Contains(name);
                }
                else
                {
                    return byFieldName.ContainsKey(name);
                }
            }
            else if (o.GetType() == EnumType)
            {
                return byValue.ContainsKey(o);
            }
            else
            {
                return byUnderlyingType.ContainsKey(o);
            }
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
                result = value;
                return true;
            }

            var name = value as string;
            if (name != null)
            {
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
            else
            {
                if (IsBitFlag)
                {
                    result = Enum.ToObject(EnumType, value);
                    return true;
                }

                KeyValuePair<string, object> r;
                if (byUnderlyingType.TryGetValue(name, out r))
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