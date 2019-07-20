using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Dapper.MoqTests
{
    internal static class ParametersObjectBuilder
    {
        private static readonly object NoParameters = new EmptyParameters();
        private static readonly Lazy<ModuleBuilder> ModuleBuilder = new Lazy<ModuleBuilder>(GetModuleBuilder);

        public static object FromParameters(MockDbParameterCollection parameters)
        {
            if (parameters.Count == 0)
                return NoParameters;

            var builder = GetTypeBuilder();

            foreach (MockDbParameter parameter in parameters)
                AddProperty(builder, parameter);

            AddToStringOverride(builder, parameters.ToString());

            var type = builder.CreateType();
            var instance = Activator.CreateInstance(type);

            foreach (MockDbParameter parameter in parameters)
            {
                var propertyInfo = type.GetProperty(parameter.ParameterName);
                if (propertyInfo == null)
                    throw new InvalidOperationException($"Could not find property: '{parameter.ParameterName}'");
                propertyInfo.SetValue(instance, parameter.Value);
            }

            return instance;
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
            il.Emit(OpCodes.Ldstr, $"{{ {stringRepresentation} }}");
            il.Emit(OpCodes.Ret);

            var toStringMethod = typeof(object).GetMethod(nameof(ToString));
            if (toStringMethod != null)
                builder.DefineMethodOverride(methodBuilder, toStringMethod);
        }

        private static void AddProperty(TypeBuilder builder, MockDbParameter parameter)
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
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
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

        private class EmptyParameters
        {
            public override string ToString()
            {
                return "<No command parameters>";
            }

            public override bool Equals(object obj)
            {
                return obj is EmptyParameters;
            }

            public override int GetHashCode()
            {
                return typeof(EmptyParameters).GetHashCode();
            }
        }
    }
}
