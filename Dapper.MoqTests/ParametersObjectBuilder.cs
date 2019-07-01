using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Dapper.MoqTests
{
    internal class ParametersObjectBuilder
    {
        private static readonly Lazy<ParametersObjectBuilder> helper = new Lazy<ParametersObjectBuilder>(() => new ParametersObjectBuilder());
        private static readonly Lazy<ModuleBuilder> moduleBuilder = new Lazy<ModuleBuilder>(() => helper.Value.ModuleBuilder());

        public static object FromParameters(MockDbParameterCollection parameters)
        {
            var builder = TypeBuilder();

            foreach (MockDbParameter parameter in parameters)
                AddProperty(builder, parameter);

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
                "AnonymousParameterType" + Guid.NewGuid(),
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);
        }
    }
}
