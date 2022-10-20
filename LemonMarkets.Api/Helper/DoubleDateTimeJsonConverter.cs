using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LemonMarkets.Helper
{
    public class DoubleDateTimeJsonConverter: DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //this should likely be null, but since the provider json returned empty string, it was unavoidable... (i'm not sure what we will read using reader, if data is actually null on the json side, feel free to experiment 
            try
            {
                if (reader.Value == null)
                    return DateTime.MinValue;
                var unixTs = Convert.ToInt64(reader.Value);

                var dtOffset = DateTimeOffset.FromUnixTimeSeconds(unixTs);
                return dtOffset.DateTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
    }
}
