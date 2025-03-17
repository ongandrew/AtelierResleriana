using System.Text.Json.Nodes;

namespace AtelierResleriana.MasterData
{
    public class MasterDataFileUpdater
    {
        public Func<object, object, bool>? ShouldUpdate { get; set; }

        public MasterDataFileUpdater() : this(new Options()) { }

        public MasterDataFileUpdater(Options options)
        {
            ShouldUpdate = options.ShouldUpdate;
        }

        public void UpdateEntities(JsonNode baseMasterDataFile, JsonNode updateMasterDataFile)
        {
            if (baseMasterDataFile is JsonArray baseArray && updateMasterDataFile is JsonArray updateArray)
            {
                IDictionary<int, JsonObject> updateMap = new Dictionary<int, JsonObject>();
                foreach (JsonNode? jsonNode in updateArray)
                {
                    if (jsonNode is JsonObject jsonObject &&
                        jsonObject["id"] is JsonValue idValue &&
                        idValue.TryGetValue<int>(out int id))
                    {
                        updateMap[id] = jsonObject;
                    }
                }
                foreach (JsonNode? baseNode in baseArray)
                {
                    if (baseNode is JsonObject baseObject &&
                        baseObject["id"] is JsonValue baseIdValue &&
                        baseIdValue.TryGetValue<int>(out int baseId) &&
                        updateMap.TryGetValue(baseId, out JsonObject? updateObject))
                    {
                        UpdateEntity(baseObject, updateObject);
                    }
                }
            }
        }

        public void UpdateEntity(JsonObject baseObject, JsonObject updateObject)
        {
            foreach (var property in updateObject)
            {
                if (!baseObject.ContainsKey(property.Key))
                {
                    continue;
                }

                if (property.Value is JsonObject updateValueObj && baseObject[property.Key] is JsonObject baseValueObj)
                {
                    UpdateEntity(baseValueObj, updateValueObj);
                }
                else if (property.Value is JsonArray updateValueArray && baseObject[property.Key] is JsonArray baseValueArray)
                {
                    UpdateArrays(baseValueArray, updateValueArray);
                }
                else if (ShouldUpdateValue(baseObject[property.Key], property.Value))
                {
                    baseObject[property.Key] = CloneNode(property.Value);
                }
            }
        }

        private void UpdateArrays(JsonArray baseArray, JsonArray updateArray)
        {
            if (baseArray.Count != updateArray.Count)
            {
                return;
            }

            for (int i = 0; i < baseArray.Count; i++)
            {
                if (baseArray[i] is JsonObject baseObj && updateArray[i] is JsonObject updateObj)
                {
                    UpdateEntity(baseObj, updateObj);
                }
                else if (baseArray[i] is JsonArray nestedBaseArray && updateArray[i] is JsonArray nestedUpdateArray)
                {
                    UpdateArrays(nestedBaseArray, nestedUpdateArray);
                }
                else if (ShouldUpdateValue(baseArray[i], updateArray[i]))
                {
                    baseArray[i] = CloneNode(updateArray[i]);
                }
            }
        }

        private bool ShouldUpdateValue(JsonNode? baseValue, JsonNode? updateValue)
        {
            // Only handle primitive values
            if (baseValue is not JsonValue baseJsonValue || updateValue is not JsonValue updateJsonValue)
            {
                return false;
            }

            try
            {
                object? baseObj = GetPrimitiveValue(baseJsonValue);
                object? updateObj = GetPrimitiveValue(updateJsonValue);

                if (baseObj == null || updateObj == null)
                {
                    return false;
                }

                // Check type compatibility
                if (baseObj.GetType() != updateObj.GetType())
                {
                    return false;
                }

                // If there's a custom condition, apply it
                if (ShouldUpdate != null)
                {
                    return ShouldUpdate(baseObj, updateObj);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private object? GetPrimitiveValue(JsonValue value)
        {
            // Try each primitive type in order
            if (value.TryGetValue<string>(out var stringValue))
            {
                return stringValue;
            }
            if (value.TryGetValue<long>(out var longValue))
            {
                return longValue;
            }
            if (value.TryGetValue<double>(out var doubleValue))
            {
                return doubleValue;
            }
            if (value.TryGetValue<bool>(out var boolValue))
            {
                return boolValue;
            }

            return null;
        }

        private static JsonNode? CloneNode(JsonNode? node)
        {
            if (node == null) return null;
            return JsonNode.Parse(node.ToJsonString());
        }

        public class Options
        {
            public Func<object, object, bool>? ShouldUpdate { get; set; }
        }
    }
}