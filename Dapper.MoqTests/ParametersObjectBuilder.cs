using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using PropertyAttributes = System.Reflection.PropertyAttributes;

namespace Dapper.MoqTests
{
    /// <summary>
    /// An anonymous object builder from a MockDbParameterCollection
    /// </summary>
    internal class ParametersObjectBuilder : ISqlParametersBuilder
    {
        private static readonly Lazy<ModuleBuilder> ModuleBuilder = new Lazy<ModuleBuilder>(GetModuleBuilder);

        public object FromParameters(IReadOnlyCollection<DbParameter> parameters)
        {
            if (parameters.Count == 0)
                return null;

            var builder = GetTypeBuilder();

            parameters = ReGroupSerialisedParameters(parameters).ToArray();
            foreach (var parameter in parameters)
                AddProperty(builder, parameter);

            AddToStringOverride(
                builder, 
                string.Join(", ", parameters.Select(ParameterToString)));

            var type = builder.CreateType();
            var instance = Activator.CreateInstance(type);

            foreach (var parameter in parameters)
            {
                var propertyInfo = type.GetProperty(parameter.ParameterName);
                if (propertyInfo == null)
                    throw new InvalidOperationException($"Could not find property: '{parameter.ParameterName}'");
                propertyInfo.SetValue(instance, parameter.Value);
            }

            return instance;
        }

        private IEnumerable<DbParameter> ReGroupSerialisedParameters(IReadOnlyCollection<DbParameter> parameters)
        {
            var serialisedParameters = parameters.Where(IsSerialisedParameter).ToArray();
            return parameters.Except(serialisedParameters).Concat(ReconstituteParameters(serialisedParameters));
        }

        private IEnumerable<DbParameter> ReconstituteParameters(IEnumerable<DbParameter> serialisedParameters)
        {
            var orderedParameters = serialisedParameters.OrderBy(p => p.ParameterName);
            var buffer = new List<DbParameter>();
            string parameterName = null;

            foreach (var parameter in orderedParameters)
            {
                if (!buffer.Any())
                {
                    buffer.Add(parameter);
                    parameterName = GetParameterName(parameter.ParameterName);
                    continue;
                }

                if (parameter.ParameterName == parameterName + (buffer.Count + 1))
                {
                    buffer.Add(parameter);
                    continue;
                }

                yield return new MockDbParameter
                {
                    ParameterName = parameterName,
                    Value = GetReconstitutedParameterValue(buffer.Select(p => p.Value).ToArray())
                };
                parameterName = null;
                buffer.Clear();
                buffer.Add(parameter);
                parameterName = GetParameterName(parameter.ParameterName);
            }

            if (buffer.Any())
            {
                yield return new MockDbParameter
                {
                    ParameterName = parameterName,
                    Value = GetReconstitutedParameterValue(buffer.Select(p => p.Value).ToArray())
                };
            }
        }

        private static object GetReconstitutedParameterValue(IReadOnlyCollection<object> values)
        {
            var firstValueType = values.First(v => v != null)?.GetType();
            if (firstValueType == null)
                return values; //all items are null

            if (values.All(v => v != null && v.GetType().Equals(firstValueType)))
            {
                //assumes that the first item is of the 'widest' type
                //the the value should be an array of <firstValueType> rather than an array of <object>
                var typedArray = Array.CreateInstance(firstValueType, values.Count);
                for (var itemIndex = 0; itemIndex < values.Count; itemIndex++)
                {
                    typedArray.SetValue(values.ElementAt(itemIndex), itemIndex);
                }

                return typedArray;
            }

            if (values.Any(v => v == null))
            {
                //value types cannot be null, so array must be of type object...
                return values;
            }

            //no values are null, some are of different types, but the first item is of a value type
            //TODO: cover more cases here...
            return values;
        }

        private static string GetParameterName(string parameterName)
        {
            return parameterName.Substring(0, parameterName.Length - 1); //lop off the last character which should be '1'
        }

        private bool IsSerialisedParameter(DbParameter parameter)
        {
            return Regex.IsMatch(parameter.ParameterName, @"^.+?\d+$");
        }

        private static string ParameterToString(DbParameter parameter)
        {
            return $"{parameter.ParameterName} = {parameter.Value}";
        }

        private static void AddToStringOverride(TypeBuilder builder, string stringRepresentation)
        {
            var methodBuilder = builder.DefineMethod(
                nameof(ToString),
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final, CallingConventions.HasThis,
                typeof(string),
                Type.EmptyTypes);
            methodBuilder.SetReturnType(typeof(string));

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "{ " + stringRepresentation + " }");
            il.Emit(OpCodes.Ret);

            var toStringMethod = typeof(object).GetMethod(nameof(ToString));
            if (toStringMethod != null)
                builder.DefineMethodOverride(methodBuilder, toStringMethod);

            AddDebuggerDisplayAttribute(builder, stringRepresentation);
        }

        private static void AddDebuggerDisplayAttribute(TypeBuilder builder, string stringRepresentation)
        {
            var attributeType = typeof(DebuggerDisplayAttribute);
            var constructor = attributeType.GetConstructors().SingleOrDefault();

            if (constructor == null)
                return;

            var attributeBuilder = new CustomAttributeBuilder(constructor, new object[] { stringRepresentation });
            builder.SetCustomAttribute(attributeBuilder);
        }

        private static void AddProperty(TypeBuilder builder, IDataParameter parameter)
        {
            var propertyType = parameter.Value.GetType();
            var propertyName = parameter.ParameterName;

            var fieldBuilder = builder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMethodBuilder = builder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMethodBuilder.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setPropMethodBuilder =
                builder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            var setIl = setPropMethodBuilder.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMethodBuilder);
            propertyBuilder.SetSetMethod(setPropMethodBuilder);
        }

        private static AssemblyBuilder GetAssemblyBuilder()
        {
            var assembly = new AssemblyName("Dapper.MoqTests.AnonymousParameterTypes");
#if DOTNETCORE
            return AssemblyBuilder.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
#endif
#if DOTNETFRAMEWORK
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
#endif
        }

        private static ModuleBuilder GetModuleBuilder()
        {
            var assemblyBuilder = GetAssemblyBuilder();
            return assemblyBuilder.DefineDynamicModule("MainModule");
        }

        private static TypeBuilder GetTypeBuilder()
        {
            return ModuleBuilder.Value.DefineType(
                "Dapper.MoqTests.AnonymousParameterType" + Guid.NewGuid(),
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                typeof(object));
        }
    }
}
