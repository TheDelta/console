using System;
using UnityEngine;

namespace Popcron.Console
{
    public class Vector4Converter : Vector3Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(Vector4);
            }
        }

        public Vector4Converter() { }

        public override object Convert(string value)
        {
            Vector4 result = Vector4.zero;
            const int length = 4;

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