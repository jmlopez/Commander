using System;
using Commander.Registration;
using Commander.Registration.Nodes;
using Commander.Runtime;

namespace Commander
{
    public class CommandInvoker : ICommandInvoker
    {
        private readonly CommandGraph _graph;
        private readonly ICommandCompiler _compiler;
        public CommandInvoker(CommandGraph graph, ICommandCompiler compiler)
        {
            _graph = graph;
            _compiler = compiler;
        }

        public InvocationResult<TEntity> ForNew<TEntity>(IDomainCommand<TEntity> command)
            where TEntity : class
        {
            return ForNew(ctx => { }, command);
        }

        public InvocationResult<TEntity> ForNew<TEntity, TCommand>()
            where TEntity : class
            where TCommand : IDomainCommand<TEntity>
        {
            return ForNew<TEntity, TCommand>(ctx => { });
        }

        public InvocationResult<TEntity> ForNew<TEntity, TCommand>(Action<ICommandContext> configure)
            where TEntity : class
            where TCommand : IDomainCommand<TEntity>
        {
            // Ignore the null - it's C# ceremony
            var command = CommandCall.For<TCommand>(t => t.Execute(null));
            return Invoke<TEntity>(configure, command, _compiler.CompileNew<TEntity>);
        }

        public InvocationResult<TEntity> ForNew<TEntity>(Action<ICommandContext> configure) 
            where TEntity : class
        {
            return ForNew(configure, new EmptyDomainCommand<TEntity>());
        }

        public InvocationResult<TEntity> ForNew<TEntity>(Action<ICommandContext> configure, IDomainCommand<TEntity> command)
            where TEntity : class
        {
            return Invoke<TEntity>(configure, command.ToCommandCall(), _compiler.CompileNew<TEntity>);
        }

        public InvocationResult<TEntity> ForExisting<TEntity>(IDomainCommand<TEntity> command)
            where TEntity : class
        {
            return ForExisting(ctx => { }, command);
        }

        public InvocationResult<TEntity> ForExisting<TEntity>(Action<ICommandContext> configure, IDomainCommand<TEntity> command)
            where TEntity : class
        {
            return Invoke<TEntity>(configure, command.ToCommandCall(), _compiler.CompileExisting<TEntity>);
        }

        public InvocationResult<TEntity> ForExisting<TEntity, TCommand>()
            where TEntity : class
            where TCommand : IDomainCommand<TEntity>
        {
            return ForExisting<TEntity, TCommand>(ctx => { });
        }

        public InvocationResult<TEntity> ForExisting<TEntity>(Action<ICommandContext> configure) where TEntity : class
        {
            return ForExisting(configure, new EmptyDomainCommand<TEntity>());
        }

        public InvocationResult<TEntity> ForExisting<TEntity, TCommand>(Action<ICommandContext> configure)
            where TEntity : class
            where TCommand : IDomainCommand<TEntity>
        {
            // Ignore the null - it's C# ceremony
            var command = CommandCall.For<TCommand>(t => t.Execute(null));
            return Invoke<TEntity>(configure, command, _compiler.CompileExisting<TEntity>);
        }

        // Keep this public for testing
        public InvocationResult<TEntity> Invoke<TEntity>(Action<ICommandContext> configure, CommandCall command, Compiler compiler)
            where TEntity : class
        {
            using (var compilationContext = compiler(_graph, configure, command))
            {
                compilationContext
                    .Command
                    .Execute();

                return compilationContext
                        .Context
                        .Get<InvocationResult<TEntity>>();
            }
        }

        #region Nested Type: EmptyDomainCommand<TEntity>
        public class EmptyDomainCommand<TEntity> : IDomainCommand<TEntity>
            where TEntity : class
        {
            public void Execute(TEntity entity)
            {
            }
        }
        #endregion
    }
}