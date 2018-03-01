using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Xml;
using Newtonsoft.Json.Linq;
using System.IO;

namespace JQL
{
    /// <summary>
    /// This will allow transformation of data from one type of object to another type.
    /// The return object must be a class.
    /// </summary>
    /// <typeparam name="T">The return object after data is parsed from XML, JSON or an object.</typeparam>
    public class DataConvert<T>
    {
        private readonly PropertyInfo[] properties;
        private Map[] mapping;
        /// <summary>
        /// Initialize an instance of DataConvert<T> where T is the return type
        /// </summary>
        /// <param name="map">Absolute path to the map file or the string value of the map file.  This is instructions to convert data to the return type.</param>
        /// <param name="mapIsFile">Set to true if map parameter is the aboslute path of map file</param>
        public DataConvert(string map, bool mapIsFile = true)
        {
            //Reflection to get properties of T
            properties = typeof(T).GetProperties();
            if (properties.FirstOrDefault(prop => prop.Name.Equals("Count") || prop.Name.Equals("Length")) != null)
                throw new ArgumentOutOfRangeException("Return type, T, must a class object.");

            if(mapIsFile)
                map = File.ReadAllText(map);

            mapping = JsonConvert.DeserializeObject<Map[]>(map);
            VerifyMap();
        }

        /// <summary>
        ///  Converts the data to the specified return type
        /// </summary>
        /// <param name="data">XML, JSON formated string, or a class object</param>
        /// <returns></returns>
        public T Parse( object data)
        {
            T t = (T)Activator.CreateInstance(typeof(T));
            var sourceData = string.Empty;
            var destinationData = string.Empty;

            //Check if data is XML, JSON or object
            //if object, it will be converted to JSON
            sourceData = GetSource(data);

            //Use map to parse sourceData into destinationData
            //destinationData is dynamically JSON formatted data
            foreach(var map in mapping)
            {
                var value = GetValue(sourceData, map.SourceType, map.SourceQuery);
                //var propA = properties.First(p => p.Name == map.PropertyName);
                //propA.SetValue(t, value);

                SetProperty(map.DestinationProperty, t, value);

            }

            return t;
        }

        private void SetProperty(string compoundProperty, object target, object value)
        {
            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                //target = propertyToGet.GetValue(target, null);
                if (propertyToGet.GetValue(target, null) == null)
                {
                    var newProp = Activator.CreateInstance(propertyToGet.PropertyType);
                    propertyToGet.SetValue(target, newProp, null);
                }

                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }

        //Determines the format of the source data. XML, JSON or a class
        private string GetSource(object data)
        {
            var source = string.Empty;

            if (data.GetType().Equals(typeof(string)))
            {
                source = (string)data;

                if (source.StartsWith("<"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(source);
                    source = JsonConvert.SerializeXmlNode(doc);
                }
                else if (!source.StartsWith("{") && !source.StartsWith("["))
                    throw new ArgumentException("Unsupported Data");

            }
            else
                source = JsonConvert.SerializeObject(data);

            return source;
        }

        //Ensure that the properties in the mapping exists in the return type
        private void VerifyMap()
        {
            foreach(var map in mapping)
            {
                PropertyInfo prop = null;
                var props = map.DestinationProperty.Split('.');
                if (props.Length == 1)
                {
                    prop = properties.FirstOrDefault(p => p.Name.Equals(map.DestinationProperty));
                    ThrowErrorIfPropNull(prop, map.DestinationProperty);
                }
                else
                {
                    var tempPropCollection = (PropertyInfo[])properties.Clone();
                    for (int current = 0; current < props.Length; current++)
                    {
                        prop = tempPropCollection.FirstOrDefault(p => p.Name.Equals(props[current]));
                        if (current != props.Length - 1)
                            tempPropCollection = prop.PropertyType.GetProperties();
                        else
                            ThrowErrorIfPropNull(prop, map.DestinationProperty);
                    }

                    tempPropCollection = null;
                }
            }
        }

        private void ThrowErrorIfPropNull(PropertyInfo prop, string propertyName)
        {
            if (prop == null)
                throw new ArgumentException(string.Format("Property doesn't exists in return type: {0}", propertyName));
        }

        private object GetValue(string source, PropertyType type, string query)
        {
            JToken val = null;
            var mapType = GetMapType(type);
            if (source.StartsWith("["))
            {
                JArray array = JArray.Parse(source);
                val = array.SelectToken(query);
            }
            else
            {
                JObject response = JObject.Parse(source);
                val = response.SelectToken(query);
            }

            return val.ToObject(mapType);
        }

        private Type GetMapType(PropertyType type)
        {
            switch (type)
            {
                case PropertyType.BoolType:
                    return typeof(bool);
                case PropertyType.BoolTypeArray:
                    return typeof(bool[]);
                case PropertyType.BoolTypeList:
                    return typeof(List<bool>);
                case PropertyType.DateTimeType:
                    return typeof(DateTime);
                case PropertyType.IntType:
                    return typeof(int);
                case PropertyType.IntTypeArray:
                    return typeof(int[]);
                case PropertyType.IntTypeList:
                    return typeof(List<int>);
                case PropertyType.StringType:
                    return typeof(string);
                case PropertyType.StringTypeArray:
                    return typeof(string[]);
                case PropertyType.StringTypeList:
                    return typeof(List<string>);
                default:
                    throw new ArgumentException(string.Format("Unsupported Type, {0}.", type.ToString()));
            }
        }
    }
}
