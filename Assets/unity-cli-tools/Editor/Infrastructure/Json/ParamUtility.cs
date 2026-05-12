using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityCliTools.Infrastructure.Json
{

public static class ParamUtility
{
    public static IReadOnlyList<string> ReadStringList(JObject parameters, string key)
    {
        if (parameters == null || string.IsNullOrWhiteSpace(key))
        {
            return Array.Empty<string>();
        }

        var token = parameters[key];
        if (token == null)
        {
            return Array.Empty<string>();
        }

        if (token is JArray array)
        {
            var values = new List<string>(array.Count);
            foreach (var item in array)
            {
                var value = item?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        var raw = token.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        var parts = raw.Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var list = new List<string>(parts.Length);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                list.Add(trimmed);
            }
        }

        return list;
    }

    public static int ClampOrDefault(int? value, int defaultValue, int min, int max)
    {
        if (!value.HasValue)
        {
            return defaultValue;
        }

        if (value.Value < min)
        {
            return min;
        }

        if (value.Value > max)
        {
            return max;
        }

        return value.Value;
    }

    public static bool TryReadVector3(JObject parameters, string key, out Vector3 value)
    {
        value = default;
        if (parameters == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var token = parameters[key];
        if (token == null)
        {
            return false;
        }

        var parts = ReadFloatList(token);
        if (parts.Count < 3)
        {
            return false;
        }

        value = new Vector3(parts[0], parts[1], parts[2]);
        return true;
    }

    public static bool TryReadVector2(JObject parameters, string key, out Vector2 value)
    {
        value = default;
        if (parameters == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var token = parameters[key];
        if (token == null)
        {
            return false;
        }

        var parts = ReadFloatList(token);
        if (parts.Count < 2)
        {
            return false;
        }

        value = new Vector2(parts[0], parts[1]);
        return true;
    }

    public static bool TryReadVector4(JObject parameters, string key, out Vector4 value)
    {
        value = default;
        if (parameters == null || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var token = parameters[key];
        if (token == null)
        {
            return false;
        }

        var parts = ReadFloatList(token);
        if (parts.Count < 4)
        {
            return false;
        }

        value = new Vector4(parts[0], parts[1], parts[2], parts[3]);
        return true;
    }

    public static bool TryReadColor(JObject parameters, string key, out Color value)
    {
        value = default;
        if (!TryReadVector4(parameters, key, out var vector))
        {
            return false;
        }

        value = new Color(vector.x, vector.y, vector.z, vector.w);
        return true;
    }

    public static bool TryReadQuaternion(JObject parameters, string key, out Quaternion value)
    {
        value = default;
        if (!TryReadVector4(parameters, key, out var vector))
        {
            return false;
        }

        value = new Quaternion(vector.x, vector.y, vector.z, vector.w);
        return true;
    }

    public static List<float> ReadFloatList(JToken token)
    {
        var values = new List<float>();
        if (token == null)
        {
            return values;
        }

        if (token is JArray array)
        {
            foreach (var item in array)
            {
                if (float.TryParse(item?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                {
                    values.Add(parsed);
                }
            }

            return values;
        }

        var raw = token.ToString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return values;
        }

        var parts = raw.Split(new[] { ',', ';', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (float.TryParse(part.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                values.Add(parsed);
            }
        }

        return values;
    }
}
}
