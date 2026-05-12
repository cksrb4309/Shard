using System;
using UnityEngine;
using UnityComponent = UnityEngine.Component;

namespace UnityCliTools.Infrastructure.Selection
{

public static class ComponentUtility
{
    public static UnityComponent ResolveComponent(GameObject gameObject, string componentType, int componentIndex)
    {
        if (gameObject == null || string.IsNullOrWhiteSpace(componentType))
        {
            return null;
        }

        var components = gameObject.GetComponents<UnityComponent>();
        var matched = 0;
        for (var i = 0; i < components.Length; i++)
        {
            var component = components[i];
            if (component == null)
            {
                continue;
            }

            var type = component.GetType();
            var isMatch = string.Equals(type.Name, componentType, StringComparison.Ordinal)
                || string.Equals(type.FullName, componentType, StringComparison.Ordinal)
                || string.Equals(type.Name, componentType, StringComparison.OrdinalIgnoreCase)
                || string.Equals(type.FullName, componentType, StringComparison.OrdinalIgnoreCase);

            if (!isMatch)
            {
                continue;
            }

            if (matched == componentIndex)
            {
                return component;
            }

            matched++;
        }

        return null;
    }
}
}
