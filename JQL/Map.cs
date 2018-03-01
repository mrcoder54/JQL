using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JQL
{
    public class Map
    {
        public Map(string propertyName, PropertyType type, string query)
        {
            this.DestinationProperty = propertyName;
            this.SourceType = type;
            this.SourceQuery = query;
        }

        //Property Name of the return type (T)
        public string DestinationProperty { get; set; }
        //The type of the property.  ie. DateTime
        [JsonConverter(typeof(StringEnumConverter))]
        public PropertyType SourceType { get; set; }
        //The JSON query string that will be value of the propery 
        public string SourceQuery { get; set; }
    }
}
