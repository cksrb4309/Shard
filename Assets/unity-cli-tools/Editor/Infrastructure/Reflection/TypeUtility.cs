using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Infrastructure.Reflection
{

public static class TypeUtility
{
    private static readonly Dictionary<string, Type> s_typeCache = new(StringComparer.OrdinalIgnoreCase);

    public static bool TryResolveType(string typeName, out Type type)
    {
        type = null;
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return false;
        }

        if (s_typeCache.TryGetValue(typeName, out type))
        {
            return type != null;
        }

        type = FindType(typeName);
        s_typeCache[typeName] = type;
        return type != null;
    }

    public static bool TryResolveComponentType(string componentTypeName, out Type type)
    {
        if (!TryResolveType(componentTypeName, out type))
        {
            return false;
        }

        return typeof(UnityComponent).IsAssignableFrom(type);
    }

    public static bool TryResolveUnityObjectType(string typeName, out Type type)
    {
        if (!TryResolveType(typeName, out type))
        {
            return false;
        }

        return typeof(UnityEngine.Object).IsAssignableFrom(type);
    }

    private static Type FindType(string typeName)
    {
        var direct = Type.GetType(typeName, false);
        if (direct != null)
        {
            return direct;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        Type fallback = null;
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }

            foreach (var candidate in types)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (string.Equals(candidate.FullName, typeName, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }

                if (string.Equals(candidate.Name, typeName, StringComparison.OrdinalIgnoreCase)
                    && fallback == null)
                {
                    fallback = candidate;
                }
            }
        }

        return fallback;
    }
}
}
