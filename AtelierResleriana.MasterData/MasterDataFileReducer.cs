using System.Text.Json.Nodes;

namespace AtelierResleriana.MasterData
{
    public class MasterDataFileReducer
    {
        public JsonNode? Reduce(string masterDataFileJson)
        {
            JsonNode? rootNode = JsonNode.Parse(masterDataFileJson);

            return Reduce(rootNode);
        }

        public JsonNode? Reduce(JsonNode jsonNode)
        {
            if (jsonNode is JsonArray rootArray)
            {
                return Reduce(rootArray);
            }

            return null;
        }

        public JsonNode? Reduce(JsonArray jsonArray)
        {
            var processedArray = new JsonArray();
            foreach (JsonNode? item in jsonArray)
            {
                if (item is JsonObject obj)
                {
                    // Check if object has an "id" property that's a number
                    if (obj["id"] is JsonValue idValue && idValue.TryGetValue<int>(out _))
                    {
                        var processedObj = ProcessObject(obj);
                        if (processedObj.Count > 1) // More than just the id field
                        {
                            processedArray.Add(processedObj);
                        }
                    }
                }
            }

            return processedArray.Count > 0 ? processedArray : null;
        }

        private JsonObject ProcessObject(JsonObject obj)
        {
            var result = new JsonObject();
            bool hasStringProperty = false;

            // Always include the id if it exists
            if (obj["id"] != null)
            {
                result["id"] = CloneNode(obj["id"]);
            }

            foreach (var property in obj)
            {
                if (property.Key == "id") continue; // Already handled

                if (property.Value == null) continue;

                // Handle each type of JsonNode
                if (property.Value is JsonValue jsonValue)
                {
                    if (jsonValue.TryGetValue<string>(out _))
                    {
                        result[property.Key] = CloneNode(property.Value);
                        hasStringProperty = true;
                    }
                }
                else if (property.Value is JsonObject nestedObj)
                {
                    var processedNestedObj = ProcessObject(nestedObj);
                    if (processedNestedObj.Count > 0)
                    {
                        result[property.Key] = processedNestedObj;
                        hasStringProperty = true;
                    }
                }
                else if (property.Value is JsonArray array)
                {
                    var processedArray = ProcessArray(array);
                    if (processedArray.Count > 0)
                    {
                        result[property.Key] = processedArray;
                        hasStringProperty = true;
                    }
                }
            }

            // If no string properties were found and this isn't a root object (with id),
            // return an empty object
            if (!hasStringProperty && result.Count == 1 && result["id"] != null)
            {
                return new JsonObject();
            }

            return result;
        }

        private JsonArray ProcessArray(JsonArray array)
        {
            var result = new JsonArray();
            bool hasStringProperty = false;

            foreach (JsonNode? item in array)
            {
                if (item == null) continue;

                if (item is JsonValue jsonValue)
                {
                    if (jsonValue.TryGetValue<string>(out _))
                    {
                        result.Add(CloneNode(item));
                        hasStringProperty = true;
                    }
                }
                else if (item is JsonObject obj)
                {
                    var processedObj = ProcessObject(obj);
                    if (processedObj.Count > 0)
                    {
                        result.Add(processedObj);
                        hasStringProperty = true;
                    }
                }
                else if (item is JsonArray nestedArray)
                {
                    var processedNestedArray = ProcessArray(nestedArray);
                    if (processedNestedArray.Count > 0)
                    {
                        result.Add(processedNestedArray);
                        hasStringProperty = true;
                    }
                }
            }

            return hasStringProperty ? result : new JsonArray();
        }

        private static JsonNode? CloneNode(JsonNode? node)
        {
            if (node == null) return null;
            return JsonNode.Parse(node.ToJsonString());
        }
    }
}