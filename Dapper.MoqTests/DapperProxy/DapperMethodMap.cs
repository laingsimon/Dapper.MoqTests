using System;

namespace Dapper.MoqTests
{
    internal class DapperMethodMap
    {
        private static CommandDefinition CommandDefinition;
        private readonly Settings settings;

        public DapperMethodMap(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            this.settings = settings;
        }

        public DapperMethodChain ExecuteImpl<T>()
        {
            if (settings.PreferCommandDefinitions)
            {
                return DapperMethodChain
                .ForCallInStack(SqlMapper.ExecuteImpl<T>(ref CommandDefinition))
                .WithUserCall(SqlMapper.Execute(CommandDefinition));
            }

            return DapperMethodChain
            .ForCallInStack(SqlMapper.ExecuteImpl<T>(ref CommandDefinition))
            .WithUserCall(m => m.Execute("", null, null, null, null));
        }

        public DapperMethodChain ExecuteImpl()
        {
            if (settings.PreferCommandDefinitions)
            {
                return DapperMethodChain
                .ForCallInStack(SqlMapper.ExecuteImpl<object>(ref CommandDefinition))
                .WithUserCall(SqlMapper.Execute(CommandDefinition));
            }

            return DapperMethodChain
                .ForCallInStack(SqlMapper.ExecuteImpl<object>(ref CommandDefinition))
                .WithUserCall(m => m.Execute("", null, null, null, null));
        }

        public DapperMethodChain Execute()
        {
            if (settings.PreferCommandDefinitions)
            {
                return DapperMethodChain
                .ForCallInStack(SqlMapper.Execute(CommandDefinition))
                .WithUserCall(SqlMapper.Execute(CommandDefinition));
            }

            return DapperMethodChain
                .ForCallInStack(SqlMapper.Execute(CommandDefinition))
                .WithUserCall(m => m.Execute("", null, null, null, null));
        }

        public DapperMethodChain ExecuteScalarImpl<T>()
        {
            if (settings.PreferCommandDefinitions)
            {
                return DapperMethodChain
                .ForCallInStack(SqlMapper.ExecuteScalarImpl<T>(ref CommandDefinition))
                .WithUserCall(SqlMapper.ExecuteScalar(CommandDefinition));
            }

            return DapperMethodChain
                .ForCallInStack(SqlMapper.ExecuteScalarImpl<T>(ref CommandDefinition))
                .WithUserCall(m => m.ExecuteScalar<T>("", null, null, null, null));
        }

        public DapperMethodChain QueryAsync<T>()
        {
            if (settings.PreferCommandDefinitions)
            {
                return DapperMethodChain
                .ForCallInStack(m => m.QueryAsync<T>(typeof(T), CommandDefinition))
                .WithUserCall(m => m.QueryAsync<T>(CommandDefinition));
            }

            return DapperMethodChain
                .ForCallInStack(m => m.QueryAsync<T>(typeof(T), CommandDefinition))
                .WithUserCall(m => m.QueryAsync<T>("", null, null, null, null));
        }
    }
}
