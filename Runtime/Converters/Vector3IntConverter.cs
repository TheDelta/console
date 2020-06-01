using System;
using UnityEngine;

namespace Popcron.Console
{
    public class Vector3IntConverter : Vector3Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(Vector3Int);
            }
        }

        public Vector3IntConverter() { }

        public override object Convert(string value)
        {
            Vector3Int result = Vector3Int.zero;
            const int length = 3;

            var splitted = PrepareString(value, length);
            if (splitted != null)
            {
                for (int i = 0; i < length; i++)
                {
                    int val = 0;
                    if (!int.TryParse(splitted[i], out val))
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