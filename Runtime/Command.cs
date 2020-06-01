﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Popcron.Console
{
    [Serializable]
    public class Command
    {
        private MethodInfo method;
        private PropertyInfo property;
        private FieldInfo field;

        private MethodInfo get;
        private MethodInfo set;

        private CommandAttribute attribute;
        private Type owner;
        private List<string> names = new List<string>();
        private List<object> parameters = new List<object>();

        public Category Category { get; set; } = null;

        public bool Enabled { get; set; } = true;
        public bool Visible { get; set; } = true;

        public string Name
        {
            get
            {
                return attribute.name;
            }
        }

        public List<string> Names
        {
            get
            {
                return names;
            }
        }

        public Type Owner
        {
            get
            {
                return owner;
            }
        }

        public string Description
        {
            get
            {
                return attribute.description;
            }
        }

        public List<string> Parameters
        {
            get
            {
                List<string> names = new List<string>();
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i] is FieldInfo field)
                    {
                        names.Add(field.Name);
                    }
                    else if (parameters[i] is ParameterInfo param)
                    {
                        names.Add(param.Name);
                    }
                }
                return names;
            }
        }

        public bool IsStatic
        {
            get
            {
                if (method != null) return method.IsStatic;
                if (field != null) return field.IsStatic;
                if (property != null)
                {
                    if (get != null) return get.IsStatic;
                    if (set != null) return get.IsStatic;
                }

                return false;
            }
        }

        public MemberInfo Member
        {
            get
            {
                if (method != null) return method;
                if (property != null) return property;
                if (field != null) return field;

                return null;
            }
        }

        private Command(MethodInfo method, CommandAttribute attribute, Type owner)
        {
            this.method = method;
            Initialize(attribute, owner);
        }

        private Command(PropertyInfo property, CommandAttribute attribute, Type owner)
        {
            this.property = property;
            Initialize(attribute, owner);
        }

        private Command(FieldInfo field, CommandAttribute attribute, Type owner)
        {
            this.field = field;
            Initialize(attribute, owner);
        }

        private void Initialize(CommandAttribute attribute, Type owner)
        {
            this.attribute = attribute;
            this.owner = owner;

            //find alias attributes
            AliasAttribute[] aliases = Member.GetAliases();
            foreach (AliasAttribute alias in aliases)
            {
                names.Add(alias.name);
            }

            names.Add(Name);

            if (method != null)
            {
                //set parameters
                parameters.Clear();
                ParameterInfo[] ps = method.GetParameters();
                for (int i = 0; i < ps.Length; i++)
                {
                    parameters.Add(ps[i]);
                }
            }
            else if (property != null)
            {
                ParameterInfo parameter = null;
                get = property.GetGetMethod();
                set = property.GetSetMethod();

                if (get != null)
                {
                    parameter = get.ReturnParameter;
                }

                if (parameter != null)
                {
                    parameters.Add(parameter);
                }
            }
            else if (field != null)
            {
                parameters.Add(field);
            }
        }

        public object Invoke(object owner, params object[] parameters)
        {
            if (method != null)
            {
                return method.Invoke(owner, parameters);
            }
            else if (property != null)
            {
                if (parameters == null || parameters.Length == 0)
                {
                    //if no parameters were passed, then get
                    if (get != null)
                    {
                        return get.Invoke(owner, parameters);
                    }
                }
                else if (parameters != null && parameters.Length == 1)
                {
                    //if 1 parameter was passed, then set
                    if (set != null)
                    {
                        property.SetValue(owner, parameters[0]);
                    }
                }
            }
            else if (field != null)
            {
                if (parameters == null || parameters.Length == 0)
                {
                    return field.GetValue(owner);
                }
                else if (parameters != null && parameters.Length == 1)
                {
                    field.SetValue(owner, parameters[0]);
                }
            }

            return null;
        }

        public static Command Create(MethodInfo method, Type type)
        {
            CommandAttribute attribute = method.GetCommand();

            if (attribute == null) return null;

            Command command = new Command(method, attribute, type);
            return command;
        }

        public static Command Create(PropertyInfo property, Type type)
        {
            CommandAttribute attribute = property.GetCommand();

            if (attribute == null) return null;

            Command command = new Command(property, attribute, type);
            return command;
        }

        public static Command Create(FieldInfo field, Type type)
        {
            CommandAttribute attribute = field.GetCommand();

            if (attribute == null) return null;

            Command command = new Command(field, attribute, type);
            return command;
        }

        public bool Matches(List<string> parametersGiven, out object[] convertedParameters)
        {
            convertedParameters = null;

            bool isProperty = property != null && parametersGiven.Count <= 2;
            bool isField = field != null && parametersGiven.Count <= 2;

            //get the total amount of params required
            int paramsRequired = 0;
            int optionalParams = 0;
            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] is FieldInfo field)
                {
                    paramsRequired++;
                }
                else if (parameters[i] is ParameterInfo param)
                {
                    if (!param.IsOptional)
                    {
                        paramsRequired++;
                    }
                    else
                    {
                        optionalParams++;
                    }
                }
            }

            //parameter amount mismatch
            if (method != null)
            {
                if (parametersGiven.Count < paramsRequired || parametersGiven.Count > paramsRequired + optionalParams)
                {
                    return false;
                }
            }

            if (isProperty || isField)
            {
                if (parametersGiven.Count == 0)
                {
                    //get the value
                    return true;
                }
            }

            //try to infer the type from input parameters
            convertedParameters = new object[parameters.Count];
            for (int i = 0; i < parameters.Count; i++)
            {
                string parameter = null;
                ParameterInfo param = parameters[i] as ParameterInfo;
                FieldInfo field = parameters[i] as FieldInfo;
                if (i >= parametersGiven.Count)
                {
                    //out of bounds, optional parameter
                    param = parameters[i] as ParameterInfo;
                    parameter = param.DefaultValue.ToString();
                }
                else
                {
                    parameter = parametersGiven[i];
                }

                object propValue = null;
                Type parameterType = param?.ParameterType ?? field.FieldType;
                if (!parameterType.IsEnum)
                {
                    Converter converter = Converter.GetConverter(parameterType);
                    if (converter == null)
                    {
                        throw new ConverterNotFoundException("No converter for type " + parameterType.Name + " was found");
                    }

                    propValue = converter.Convert(parameter);

                    //couldnt get a value
                    if (propValue == null)
                    {
                        throw new FailedToConvertException("Failed to convert " + parameter + " to type " + parameterType.Name + " using " + converter.GetType().Name);
                    }
                }
                else
                {
                    //manually convert here if its an enum
                    propValue = Enum.Parse(parameterType, parameter);

                    //couldnt get a value
                    if (propValue == null)
                    {
                        throw new FailedToConvertException("Failed to parse " + parameter + " to " + parameterType.Name);
                    }
                }

                convertedParameters[i] = propValue;
            }

            return true;
        }

        public string ToStringWithCategory(Category category)
        {

            StringBuilder textBuilder = new StringBuilder();

            //not static, so show that it needs an @id
            if (!this.IsStatic)
            {
                textBuilder.Append(string.Format("@{0} ", category.ID.Length > 0 ? category.ID : "<id>"));
            }

            textBuilder.Append(this.ToString());

            return textBuilder.ToString();
        }

        public override string ToString()
        {
            StringBuilder textBuilder = new StringBuilder();

            if (!this.IsStatic)
            {
                textBuilder.Append(string.Format("@{0} ", Category != null && Category.ID.Length > 0 ? Category.ID : "<id>"));
            }

            textBuilder.Append(string.Join("/", this.Names));
            if (this.Member is MethodInfo)
            {
                foreach (ParameterInfo parameter in (this.Member as MethodInfo).GetParameters())
                {
                    if (parameter.HasDefaultValue)
                        textBuilder.Append($" <<i><color=yellow>{parameter.Name}</color></i>:<color=cyan>{parameter.ParameterType.Name}</color>=<color=lightblue>{parameter.DefaultValue}</color>>");
                    else
                        textBuilder.Append($" <<color=yellow>{parameter.Name}</color>:<color=cyan>{parameter.ParameterType.Name}</color>>");
                }
            }
            else if (this.Member is PropertyInfo property)
            {
                MethodInfo set = property.GetSetMethod();
                if (set != null)
                {
                    textBuilder.Append($" <<color=yellow>{property.Name}</color>:<color=cyan>{property.PropertyType.Name}</color>>");
                }
            }
            else if (this.Member is FieldInfo)
            {
                var fieldInfo = this.Member as FieldInfo;

                textBuilder.Append($" <<color=yellow>{fieldInfo.Name}</color>:<color=cyan>{fieldInfo.FieldType.Name}</color>>");
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                textBuilder.Append(" = " + this.Description);
            }

            return textBuilder.ToString();
        }
    }
}