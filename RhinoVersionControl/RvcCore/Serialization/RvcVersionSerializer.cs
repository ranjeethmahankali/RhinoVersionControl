using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RvcCore.VersionManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RvcCore.Serialization
{
    public class RvcVersionSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(RvcVersion).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //incomplete
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            RvcVersion version = (RvcVersion)value;

            writer.WriteStartObject();
            //writing the id of the version
            writer.WritePropertyName(Entity.ID_PROP_NAME);
            writer.WriteValue(version.Id);

            writer.WritePropertyName(RvcVersion.DOWNSTREAM_PROP_NAME);
            writer.WriteStartObject();
            foreach(var changeSet in version.DownstreamChangeSets.Keys)
            {
                
            }
            writer.WriteEndObject();
            //incomplete
            throw new NotImplementedException();
        }
    }
}
