using System;
using System.Collections.Generic;
using System.Globalization;

namespace WebApplication.Helpers
{
    public class EventArgumentParser
    {
        private readonly Dictionary<string, string> _data;

        public EventArgumentParser(string eventArgument)
        {
            _data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(eventArgument))
                return;

            var partes = eventArgument.Split('|');

            foreach (var parte in partes)
            {
                var kv = parte.Split('=');
                if (kv.Length == 2)
                {
                    _data[kv[0].Trim()] = kv[1].Trim();
                }
            }
        }

        public string GetString(string key)
        {
            return _data.ContainsKey(key) ? _data[key] : string.Empty;
        }

        public int GetInt(string key)
        {
            if (_data.ContainsKey(key) && int.TryParse(_data[key], out int val))
                return val;

            return 0;
        }

        public decimal GetDecimal(string key)
        {
            if (_data.ContainsKey(key) &&
                decimal.TryParse(_data[key], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return val;

            return 0;
        }

        public bool GetBool(string key)
        {
            if (_data.ContainsKey(key) && bool.TryParse(_data[key], out bool val))
                return val;

            return false;
        }

        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }
    }
}