﻿using System;
using Commander.Bootstrapping;
using Commander.Commands;
using Commander.Registration;
using Commander.Registration.Nodes;
using FubuCore.Binding;

namespace Commander.Runtime
{
    public class CommandCompiler : ICommandCompiler
    {
        private readonly ICommanderContainer _container;
        private readonly IEntityBuilderRegistry _builderRegistry;
        public CommandCompiler(ICommanderContainer container, IEntityBuilderRegistry builderRegistry)
        {
            _container = container;
            _builderRegistry = builderRegistry;
        }

        public ICompilationContext CompileNew<TEntity>(CommandGraph graph, Action<ICommandContext> configure, CommandCall commandCall)
            where TEntity : class
        {
            return Compile(graph.ChainForNew<TEntity>(), configure, commandCall);
        }

        public ICompilationContext CompileExisting<TEntity>(CommandGraph graph, Action<ICommandContext> configure, CommandCall commandCall)
            where TEntity : class
        {
            var chain = graph.ChainForExisting<TEntity>();
            return Compile(chain, configure, commandCall);
        }

        // Keep this public for testing
        public ICompilationContext Compile(CommandChain chain, Action<ICommandContext> configure, CommandCall commandCall)
        {
            var context = new CommandContext(_builderRegistry);
            configure(context);

            chain
                .Placeholder()
                .ReplaceWith(commandCall);

            _container.Register(typeof (ICommand), chain.ToObjectDef());

            return _container
                .BuildCompiler()
                .Compile(context, new ServiceArguments(), chain.UniqueId);
        }
    }
}