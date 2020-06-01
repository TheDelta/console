using System;
using UnityEngine;

namespace Popcron.Console
{
    public class Vector3Converter : Converter
    {
        public override Type Type
        {
            get
            {
                return typeof(Vector3);
            }
        }

        public Vector3Converter() { }

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

        protected virtual string[] PrepareString(string value, int length)
        {
            Debug.Log(value);

            if (value.Length > 0 && (value[0] == '(' || value[0] == '[')) value = value.Substring(1);
            if (value.Length > 0 && (value[value.Length - 1] == ')' || value[value.Length - 1] == ']')) value = value.Substring(0, value.Length - 1);
            value = value.Trim();

            Debug.Log(value);

            var splitted = value.Split(new char[] { ',' }, length, StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length != length) splitted = value.Split(new char[] { ' ' }, length, StringSplitOptions.RemoveEmptyEntries);

            Debug.Log(splitted);

            return splitted.Length == length ? splitted : null;
        }
    }
}