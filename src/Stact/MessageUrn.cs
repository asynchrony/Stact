// Copyright 2010 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using System.Runtime.Serialization.Formatters;
using Magnum.Extensions;
using Magnum.Reflection;

namespace Stact
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;


    public static class MessageUrn<T>
    {
        public static MessageUrn Urn = typeof(T).ToMessageUrn();
        public static string UrnString = typeof(T).ToMessageUrn().ToString();
    }


    [Serializable]
    public class MessageUrn :
        Uri
    {
        [ThreadStatic]
        static IDictionary<Type, string> _cache;

        public MessageUrn(Type type)
            : base(GetUrnForType(type))
        {
        }

        public MessageUrn(string uriString)
            : base(uriString)
        {
        }

        protected MessageUrn(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        public Type GetType(bool throwOnError = true, bool ignoreCase = true)
        {
            if (Segments.Length == 0)
                return null;

            string[] names = Segments[0].Split(':');
            if (names[0] != "message")
                return null;

            string typeName;
            string assemblyName;
            SplitFullyQualifiedTypeName(LocalPath.Substring(8), out typeName, out assemblyName);

            var messageType = Type.GetType("{0},{1}".FormatWith(typeName, assemblyName), true, true);
            
            return messageType;
        }

        public static void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            if (assemblyDelimiterIndex != null)
            {
                typeName = fullyQualifiedTypeName.Substring(0, assemblyDelimiterIndex.Value).Trim();
                assemblyName = fullyQualifiedTypeName.Substring(assemblyDelimiterIndex.Value + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.Value - 1).Trim();
            }
            else
            {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }
        }

        private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
        {
            // we need to get the first comma following all surrounded in brackets because of generic types
            // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            int scope = 0;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        scope++;
                        break;
                    case ']':
                        scope--;
                        break;
                    case ',':
                        if (scope == 0)
                            return i;
                        break;
                }
            }

            return null;
        }

        static string IsInCache(Type type, Func<Type, string> provider)
        {
            if (_cache == null)
                _cache = new Dictionary<Type, string>();

            string urn;
            if (_cache.TryGetValue(type, out urn))
                return urn;

            urn = provider(type);

            _cache[type] = urn;

            return urn;
        }

        static string GetUrnForType(Type type)
        {
            return IsInCache(type, x => string.Concat("urn:message:", x.AssemblyQualifiedName));
        }
    }
}