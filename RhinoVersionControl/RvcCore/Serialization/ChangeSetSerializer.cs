using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using RvcCore.VersionManagement;
using Newtonsoft.Json;

namespace RvcCore.Serialization
{
    public class ChangeSetSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ChangeSet).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            ChangeSet set = new ChangeSet();
            JObject obj = JObject.Load(reader);

            List<JToken> children = obj.Children().ToList();
            foreach(var child in children)
            {
                JProperty prop = child as JProperty;
                if(prop is null) { continue; }
                if(prop.Name == Entity.ID_PROP_NAME)
                {
                    Guid id;
                    if (!Guid.TryParse(prop.Value.ToString(), out id))
                    {
                        throw new InvalidOperationException("Cannot parse guid");
                    }
                    set.Id = id;
                }
                else if(prop.Name == ChangeSet.CHANGES_PROP)
                {
                    
                }
            }
            //incomplete
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            ChangeSet set = (ChangeSet)value;
            writer.WriteStartObject();

            //id of the changeset
            writer.WritePropertyName(Entity.ID_PROP_NAME);
            writer.WriteValue(set.Id.ToString());

            writer.WritePropertyName(ChangeSet.CHANGES_PROP);
            writer.WriteStartObject();
            foreach(var key in set.ChangesMap.Keys)
            {
                //writer.WritePropertyName(key.ToString());
                serializer.Serialize(writer, set.ChangesMap[key]);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }
    }
}
