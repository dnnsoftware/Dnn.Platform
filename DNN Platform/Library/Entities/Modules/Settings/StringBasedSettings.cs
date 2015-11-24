using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DotNetNuke.Common;

namespace DotNetNuke.Entities.Modules.Settings
{
    public class StringBasedSettings : ISettingsStore
    {
        Dictionary<string, string> _cache;
        Dictionary<string, Action> _dirty;
        Func<string, string> _get;
        Action<string, string> _set;

        /// <param name="get">Function to get a value out of your setting store</param>
        /// <param name="set">Action to save a value back to the setting store</param>
        public StringBasedSettings(Func<string, string> get, Action<string, string> set)
        {
            Requires.NotNull("Get", get);
            Requires.NotNull("Set", set);

            _get = get;
            _set = set;
            _cache = new Dictionary<string, string>();
            _dirty = new Dictionary<string, Action>();
        }

        // All changes to the settings are recorded and get only executed on demand 
        public void Save()
        {
            foreach (var save in _dirty.Values) save();
            _dirty.Clear();
        }

        // CallerMemberName is used for the name of the setting. No more magic strings
        public string Get(string @default = default(string), [CallerMemberName] string name = null)
        {
            Requires.NotNull("Name", name);

            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }
            else
            {
                var value = _get(name) ?? @default;
                _cache[name] = value;
                return value;
            }
        }

        public T Get<T>(T @default = default(T), [CallerMemberName] string name = null)
        {
            //required to behave Get and Get<string> the same
            if (typeof(T) == typeof(string)) return (T)(object)Get((string)(object)@default, name);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            Requires.NotNull("Converter for T", converter);

            try
            {
                var defaultValueAsString = converter.ConvertToInvariantString(@default);
                string value = Get(defaultValueAsString, name);
                return (T)converter.ConvertFromInvariantString(value);
            }
            catch
            {
                return @default;
            }
        }

        public void Set(string value, [CallerMemberName] string name = null)
        {
            Requires.NotNull("Name", name);
            var modified = !(_cache.ContainsKey(name) && _cache[name] == value);

            if (modified)
            {
                _cache[name] = value;
                _dirty[name] = () => _set(name, value);
            };
        }

        public void Set<T>(T value, [CallerMemberName] string name = null)
        {
            //required to behave Set and Set<string> the same
            if (typeof(T) == typeof(string))
                Set((string)(object)value, name);
            else
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                Requires.NotNull("Converter for T", converter);

                Set(converter.ConvertToInvariantString(value), name);
            }
        }
    }
}
