namespace LemonMarkets
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the <see cref="JsonObjectExtensions" />.
    /// </summary>
    internal static class JsonObjectExtensions
    {
        /// <summary>
        /// Gets an JObject from a json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public static JToken Get(
            this JObject json,
            string propertyName,
            bool throwOnError = true,
            StringComparison options = StringComparison.InvariantCultureIgnoreCase)
        {
            try
            {
                if (json.TryGetValue(propertyName, options, out JToken jToken))
                {
                    return jToken;
                }
                else
                {
                    throw new Exception("Property not found inside json object: " + propertyName);
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw ex;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an JObject from a json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public static List<JToken> GetAll(
            this JObject json,
            string propertyName,
            bool startsWith = false,
            StringComparison options = StringComparison.InvariantCultureIgnoreCase)
        {
            var result = new List<JToken>();
            try
            {
                foreach (var property in json.Properties())
                {
                    if ((startsWith && property.Name.StartsWith(propertyName, options))
                        || property.Name.Equals(propertyName, options))
                    {
                        result.Add(property.Value);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        /// <summary>
        /// Gets the value from a json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public static T GetValue<T>(
            this JObject json,
            string propertyName,
            StringComparison options = StringComparison.InvariantCultureIgnoreCase)
            where T : IComparable
        {
            return json.GetValue<T>(propertyName, true, options);
        }

        /// <summary>
        /// Gets the value from a json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public static T GetValue<T>(
            this JObject json,
            string propertyName,
            bool throwOnError,
            StringComparison options = StringComparison.InvariantCultureIgnoreCase)
                    where T : IComparable
        {
            try
            {
                if (json.TryGetValue(propertyName, options, out JToken jToken))
                {
                    return jToken.Value<T>();
                }
                else
                {
                    throw new Exception(
                        "Property not found inside json object: " + propertyName);
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                {
                    throw ex;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Gets the value from a json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="propertyNames">The propertyNames<see cref="string[]"/>.</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <param name="options">The options.</param>
        /// <returns>The <see cref="T"/>.</returns>
        public static T GetValue<T>(
            this JObject json,
            string[] propertyNames,
            bool throwOnError,
            StringComparison options = StringComparison.InvariantCultureIgnoreCase)
                    where T : IComparable
        {
            JObject lastProp = null;
            try
            {
                lastProp = JObject.Parse(json.GetValue(propertyNames[0], options).ToString());
                for (int i = 1; i < propertyNames.Length; i++)
                {
                    if (i == propertyNames.Length - 1)
                    {
                        return lastProp.GetValue<T>(propertyNames[i], options);
                    }
                    else
                    {
                        var jsonValue = lastProp.GetValue(propertyNames[i], options).ToString();
                        lastProp = JObject.Parse(jsonValue);
                    }
                }
            }
            catch
            {
                if (throwOnError)
                {
                    throw new Exception(
                        "Property not found inside json object: " + string.Join(".", propertyNames));
                }
            }

            try
            {
                return lastProp.Value<T>();
            }
            catch
            {
                if (throwOnError)
                {
                    throw new Exception(
                      "Failed to parse json value to type: " + typeof(T).Name);
                }
            }

            return default(T);
        }

        /// <summary>
        /// Adds the specified value to the json object.
        /// </summary>
        /// <param name="jObject">The j object.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFunc">The valueFunc<see cref="Func{object}"/>.</param>
        /// <returns>The <see cref="JObject"/>.</returns>
        public static JObject Add(this JObject jObject, string key, Func<object> valueFunc)
        {
            var value = valueFunc.Invoke();
            if (value != null)
            {
                jObject.Add(key, JToken.FromObject(value));
            }

            return jObject;
        }

        /// <summary>
        /// Adds the specified value to the json object.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="jObject">The j object.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFunc">The value function.</param>
        /// <param name="ignoreIfDefault">if set to <c>true</c> [ignore if default].</param>
        /// <returns>The <see cref="JObject"/>.</returns>
        public static JObject Add<T>(this JObject jObject, string key, Func<T> valueFunc, bool ignoreIfDefault = false) where T : IComparable
        {
            T value = valueFunc.Invoke();
            if (value != null)
            {
                if (ignoreIfDefault && value.Equals(default(T)))
                {
                    return jObject;
                }

                if (typeof(T) == typeof(string) && (value as string) == string.Empty)
                {
                    return jObject;
                }

                jObject.Add(key, JToken.FromObject(value));
            }

            return jObject;
        }
    }
}
