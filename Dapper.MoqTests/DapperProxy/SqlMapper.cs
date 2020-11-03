using System.Reflection;

namespace Dapper.MoqTests
{
#pragma warning disable IDE0060 // Remove unused parameter
    /// <summary>
    /// A strongly typed reference/copy of the otherwise private definitions of the methods in the Dapper\SqlMapper type
    /// </summary>
    internal abstract partial class SqlMapper : MockDatabase
    {
        public static MethodInfo Execute(CommandDefinition a) =>
            Method("Execute", CommandDefinition);

        public static MethodInfo ExecuteScalar(CommandDefinition a) =>
            Method("ExecuteScalar", CommandDefinition);

        public static MethodInfo ExecuteScalar<T>(CommandDefinition a) =>
            Method<T>("ExecuteScalar", CommandDefinition);

        public static MethodInfo ExecuteImpl<T>(ref CommandDefinition a) =>
            Method("ExecuteImpl", CommandDefinition.MakeByRefType());

        public static MethodInfo ExecuteScalarImpl<T>(ref CommandDefinition command) =>
            Method<T>("ExecuteScalarImpl", CommandDefinition.MakeByRefType());
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
