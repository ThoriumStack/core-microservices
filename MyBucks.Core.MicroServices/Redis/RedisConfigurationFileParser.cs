using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyBucks.Core.MicroServices.Redis
{
    internal class RedisConfigurationFileParser
    {
        private readonly IDictionary<string, string> _data =
            (IDictionary<string, string>) new SortedDictionary<string, string>(
                (IComparer<string>) StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _context = new Stack<string>();
        private string _currentPath;
        private JsonTextReader _reader;

        public static IDictionary<string, string> Parse(Stream input)
        {
            return new RedisConfigurationFileParser().ParseStream(input);
        }
        
        public static IDictionary<string, string> Parse(string input)
        {
            return new RedisConfigurationFileParser().ParseStream(input);
        }

        private IDictionary<string, string> ParseStream(Stream input)
        {
            this._data.Clear();
            this._reader = new JsonTextReader((TextReader) new StreamReader(input));
            this._reader.DateParseHandling = DateParseHandling.None;
            this.VisitJObject(JObject.Load((JsonReader) this._reader));
            return this._data;
        }
        
        private IDictionary<string, string> ParseStream(string input)
        {
            this._data.Clear();
            var jsonConfig = (JObject) JsonConvert.DeserializeObject(input);
            this.VisitJObject(jsonConfig);
            return this._data;
        }

        private void VisitJObject(JObject jObject)
        {
            foreach (JProperty property in jObject.Properties())
            {
                this.EnterContext(property.Name);
                this.VisitProperty(property);
                this.ExitContext();
            }
        }

        private void VisitProperty(JProperty property)
        {
            this.VisitToken(property.Value);
        }

        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    this.VisitJObject(token.Value<JObject>());
                    break;
                case JTokenType.Array:
                    this.VisitArray(token.Value<JArray>());
                    break;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                    this.VisitPrimitive(token.Value<JValue>());
                    break;
                default:
                    throw new FormatException(/*Resources.FormatError_UnsupportedJSONToken(
                        (object) this._reader.TokenType, (object) this._reader.Path, (object) this._reader.LineNumber,
                        (object) this._reader.LinePosition)*/);
            }
        }

        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; ++index)
            {
                this.EnterContext(index.ToString());
                this.VisitToken(array[index]);
                this.ExitContext();
            }
        }

        private void VisitPrimitive(JValue data)
        {
            string currentPath = this._currentPath;
            if (this._data.ContainsKey(currentPath))
                throw new FormatException(/*Resources.FormatError_KeyIsDuplicated((object) currentPath));
            this._data[currentPath] = data.ToString((IFormatProvider) CultureInfo.InvariantCulture*/);
        }

        private void EnterContext(string context)
        {
            this._context.Push(context);
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
        }

        private void ExitContext()
        {
            this._context.Pop();
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse<string>());
        }
    }
}