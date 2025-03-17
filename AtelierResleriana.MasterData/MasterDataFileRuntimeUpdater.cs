using System.Collections;
using System.Reflection;
using System.Text.Json.Nodes;

namespace AtelierResleriana.MasterData
{
    public class MasterDataFileRuntimeUpdater
    {
        public Func<object, object, bool>? ShouldUpdate { get; set; }

        public MasterDataFileRuntimeUpdater() : this(new Options()) { }

        public MasterDataFileRuntimeUpdater(Options options)
        {
            ShouldUpdate = options.ShouldUpdate;
        }

        public void UpdateEntities(object baseMasterData, JsonNode updateMasterData)
        {
            if (baseMasterData is IList baseList && updateMasterData is JsonArray updateArray)
            {
                var updateMap = new Dictionary<long, JsonObject>();
                foreach (JsonNode? node in updateArray)
                {
                    if (node is JsonObject obj && obj["id"] != null)
                    {
                        // Handle various numeric types for ID
                        long id;
                        if (obj["id"] is JsonValue idValue)
                        {
                            try
                            {
                                if (idValue.TryGetValue<long>(out var longId))
                                    id = longId;
                                else if (idValue.TryGetValue<int>(out var intId))
                                    id = intId;
                                else if (idValue.TryGetValue<ushort>(out var ushortId))
                                    id = ushortId;
                                else if (idValue.TryGetValue<uint>(out var uintId))
                                    id = uintId;
                                else
                                    continue;

                                updateMap[id] = obj;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }

                foreach (var item in baseList)
                {
                    if (item is IDictionary<object, object> dict && dict.ContainsKey("id"))
                    {
                        // Handle dictionary-based objects (MessagePack)
                        var idObj = dict["id"];
                        long id = Convert.ToInt64(idObj);
                        if (updateMap.TryGetValue(id, out var updateObj))
                        {
                            UpdateDictionary(dict, updateObj);
                        }
                    }
                    else
                    {
                        // Handle regular objects with properties
                        var idProp = item.GetType().GetProperty("id", BindingFlags.Public | BindingFlags.Instance);
                        if (idProp != null)
                        {
                            var idObj = idProp.GetValue(item);
                            if (idObj != null)
                            {
                                long id = Convert.ToInt64(idObj);
                                if (updateMap.TryGetValue(id, out var updateObj))
                                {
                                    UpdateObject(item, updateObj);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateDictionary(IDictionary<object, object> baseDict, JsonObject updateObject)
        {
            foreach (var kvp in updateObject.AsObject())
            {
                if (!baseDict.ContainsKey(kvp.Key))
                {
                    continue;
                }

                if (kvp.Value is JsonValue jsonValue)
                {
                    var baseValue = baseDict[kvp.Key];
                    var value = GetTypedValue(jsonValue, baseValue?.GetType() ?? typeof(object));
                    if (value != null && ShouldUpdateValue(baseValue, value))
                    {
                        baseDict[kvp.Key] = value;
                    }
                }
                else if (kvp.Value is JsonObject updateValueObj && baseDict[kvp.Key] is IDictionary<object, object> baseValueDict)
                {
                    UpdateDictionary(baseValueDict, updateValueObj);
                }
                else if (kvp.Value is JsonArray updateArray && baseDict[kvp.Key] is IList baseList)
                {
                    UpdateList(baseList, updateArray, baseList.GetType());
                }
            }
        }

        private void UpdateObject(object baseObject, JsonObject updateObject)
        {
            var properties = baseObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var kvp in updateObject.AsObject())
            {
                var prop = properties.FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
                if (prop == null || !prop.CanWrite) continue;

                if (kvp.Value is JsonValue jsonValue)
                {
                    var value = GetTypedValue(jsonValue, prop.PropertyType);
                    if (value != null && ShouldUpdateValue(prop.GetValue(baseObject), value))
                    {
                        prop.SetValue(baseObject, value);
                    }
                }
                else if (kvp.Value is JsonObject nestedObject)
                {
                    var nestedValue = prop.GetValue(baseObject);
                    if (nestedValue != null)
                    {
                        UpdateObject(nestedValue, nestedObject);
                    }
                }
                else if (kvp.Value is JsonArray jsonArray && prop.GetValue(baseObject) is IList baseList)
                {
                    UpdateList(baseList, jsonArray, prop.PropertyType);
                }
            }
        }

        private void UpdateList(IList baseList, JsonArray updateArray, Type propertyType)
        {
            if (baseList.Count != updateArray.Count) return;

            var elementType = propertyType.IsArray
                ? propertyType.GetElementType()
                : propertyType.GetGenericArguments().FirstOrDefault();

            if (elementType == null) return;

            for (int i = 0; i < baseList.Count; i++)
            {
                var baseItem = baseList[i];
                var updateItem = updateArray[i];

                if (updateItem is JsonObject updateObj && baseItem != null)
                {
                    if (baseItem is IDictionary<object, object> baseDict)
                        UpdateDictionary(baseDict, updateObj);
                    else
                        UpdateObject(baseItem, updateObj);
                }
                else if (updateItem is JsonValue jsonValue)
                {
                    var value = GetTypedValue(jsonValue, elementType);
                    if (value != null && ShouldUpdateValue(baseItem, value))
                    {
                        baseList[i] = value;
                    }
                }
            }
        }

        private bool ShouldUpdateValue(object? baseValue, object? updateValue)
        {
            if (baseValue == null || updateValue == null)
            {
                return false;
            }

            if (baseValue.GetType() != updateValue.GetType())
            {
                return false;
            }

            if (ShouldUpdate != null)
            {
                return ShouldUpdate(baseValue, updateValue);
            }

            return true;
        }

        private object? GetTypedValue(JsonValue value, Type targetType)
        {
            try
            {
                if (targetType == typeof(string))
                    return value.GetValue<string>();
                if (targetType == typeof(int))
                    return value.GetValue<int>();
                if (targetType == typeof(uint))
                    return value.GetValue<uint>();
                if (targetType == typeof(short))
                    return value.GetValue<short>();
                if (targetType == typeof(ushort))
                    return value.GetValue<ushort>();
                if (targetType == typeof(long))
                    return value.GetValue<long>();
                if (targetType == typeof(ulong))
                    return value.GetValue<ulong>();
                if (targetType == typeof(double))
                    return value.GetValue<double>();
                if (targetType == typeof(float))
                    return (float)value.GetValue<double>();
                if (targetType == typeof(bool))
                    return value.GetValue<bool>();
                if (targetType == typeof(DateTime))
                    return DateTime.Parse(value.GetValue<string>());
                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value.GetValue<string>());
            }
            catch
            {
                return null;
            }

            return null;
        }

        public class Options
        {
            public Func<object, object, bool>? ShouldUpdate { get; set; }
        }
    }
}