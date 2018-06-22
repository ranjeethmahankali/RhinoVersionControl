﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RvcCore.VersionManagement;
using Newtonsoft.Json;

namespace RvcCore.Serialization
{
    class ChangeSetSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ChangeSet).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //incomplete
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //incomplete
            throw new NotImplementedException();
        }
    }
}
