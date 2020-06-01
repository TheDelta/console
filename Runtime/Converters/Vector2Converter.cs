using System;
using UnityEngine;

namespace Popcron.Console
{
    public class Vector2Converter : Vector3Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(Vector2);
            }
        }

        public Vector2Converter() { }

        public override object Convert(string value)
        {
            Vector3 result = Vector3.zero;
            const int length = 3;

            var splitted = PrepareString(value, length);
            if (splitted != null)
            {
                for (int i = 0; i < length; i++)
                {
                    float val = 0;
                    if (!float.TryParse(splitted[i], out val))
                    {
                        Debug.LogWarning($"Could not parse {result.GetType().ToString()} value at index {i}");
                        return null;
                    }
                    result[i] = val;
                }
                return result;
            }

            return null;
        }
    }
}