using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Dapper.MoqTests
{
    internal class ParametersObjectBuilder
    {
        private static readonly object NoParameters = new _NoParameters();

        private static readonly Lazy<ParametersObjectBuilder> helper = new Lazy<ParametersObjectBuilder>(() => new ParametersObjectBuilder());
        private static readonly Lazy<ModuleBuilder> moduleBuilder = new Lazy<ModuleBuilder>(() => helper.Value.ModuleBuilder());

        public static object FromParameters(MockDbParameterCollection parameters)
        {
            if (parameters.Count == 0)
                return NoParameters;

            var builder = TypeBuilder();

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

            builder.DefineMethodOverride(methodBuilder, typeof(object).GetMethod(nameof(ToString)));
        }

        private static void AddProperty(TypeBuilder builder, MockDbParameter parameter)
        {
            var propertyType = parameter.Value.GetType();
            var propertyName = parameter.ParameterName;

            var fieldBuilder = builder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMthdBldr = builder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setPropMthdBldr =
                builder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            var setIl = setPropMthdBldr.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        private AssemblyBuilder AssemblyBuilder()
        {
            var assembly = new AssemblyName("Dapper.MoqTests.AnonymousParameterTypes");
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
        }

        private ModuleBuilder ModuleBuilder()
        {
            var assemblyBuilder = AssemblyBuilder();
            return assemblyBuilder.DefineDynamicModule("MainModule");
        }

        private static TypeBuilder TypeBuilder()
        {
            return moduleBuilder.Value.DefineType(
                "Dapper.MoqTests.AnonymousParameterType" + Guid.NewGuid(),
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                typeof(object));
        }

        private class _NoParameters
        {
            public override string ToString()
            {
                return "<No command parameters>";
            }

            public override bool Equals(object obj)
            {
                return obj is _NoParameters;
            }

            public override int GetHashCode()
            {
                return typeof(_NoParameters).GetHashCode();
            }
        }
    }
}
