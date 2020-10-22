using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
