using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityLicenseCollector.Editor
{
    /// <summary>
    /// Provides additional functionality missing from <see cref="JsonUtility"/>.
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// Deserializes the specified string as a JSON array without a root object.
        /// </summary>
        public static T[] FromJson<T>(string json)
        {
            string dummyJson = $"{{\"{SerializableArray<T>.RootName}\": {json}}}";
            var obj = JsonUtility.FromJson<SerializableArray<T>>(dummyJson);
            return obj.array;
        }

        /// <summary>
        /// Converts the specified collection to a JSON array without a root object.
        /// </summary>
        public static string ToJson<T>(IEnumerable<T> collection)
        {
            string json = JsonUtility.ToJson(new SerializableArray<T>(collection));
            int start = SerializableArray<T>.RootName.Length + 4;
            int len = json.Length - start - 1;
            return json.Substring(start, len);
        }

        /// <summary>
        /// Converts the specified collection to a formatted JSON array without a root object.
        /// </summary>
        public static string ToJsonFormatted<T>(IEnumerable<T> collection)
        {
            string json = JsonUtility.ToJson(new SerializableArray<T>(collection), true);
            var lines = json.Split('\n');
            var sb = new StringBuilder();
            var rootLine = lines[1];
            
            int colonIndex = rootLine.IndexOf(':');
            if (colonIndex >= 0)
            {
                lines[1] = rootLine[(colonIndex + 1)..].TrimStart();
            }

            for (int i = 1; i < lines.Length - 1; i++)
            {
                string line = lines[i];
                if (line.Length > 4)
                {
                    sb.AppendLine(line[4..]);
                }
                else if (line.Length > 0)
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        [Serializable]
        private struct SerializableArray<T>
        {
            public const string RootName = nameof(array);
            public T[] array;
            public SerializableArray(IEnumerable<T> collection) => this.array = collection.ToArray();
        }
    }
}
