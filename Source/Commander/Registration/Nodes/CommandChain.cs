﻿using System;
using System.Collections.Generic;
using System.Linq;
using Commander.Registration.Graph;

namespace Commander.Registration.Nodes
{
    public class CommandChain : CommandNode
    {
        public CommandChain(Type entityType)
        {
            EntityType = entityType;
            Top = new Placeholder();
        }

        public Type EntityType { get; private set; }

        public CommandNode Top { get { return Next; } private set { Next = value; } }

        public override CommandCategory Category { get { return CommandCategory.Chain; } }

        public IEnumerable<CommandCall> Calls { get { return this.OfType<CommandCall>(); } }

        public string FirstCallDescription
        {
            get
            {
                CommandCall call = FirstCall();
                return call == null ? string.Empty : call.Description;
            }
        }

        public string InputTypeName
        {
            get
            {
                CommandCall call = FirstCall();
                return call == null || call.InputType() == null ? string.Empty : call.InputType().Name;
            }
        }


        public override void AddToEnd(CommandNode node)
        {
            if (Top == null)
            {
                Top = node;
                return;
            }

            CommandNode last = this.OfType<CommandNode>().LastOrDefault();
            if (last != null)
            {
                last.AddAfter(node);
            }
        }

        public override ObjectDef ToObjectDef()
        {
            ObjectDef def = Top.ToObjectDef();
            def.Name = UniqueId.ToString();
            return def;
        }

        public void Prepend(CommandNode node)
        {
            if (Top == null)
            {
                Top = node;
            }
            else
            {
                Top.AddBefore(node);
                Top = node;
            }
        }

        protected override ObjectDef buildObjectDef()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<CommandNode> GetEnumerator()
        {
            if (Top != null)
            {
                yield return Top;
                foreach (CommandNode node in Top)
                {
                    yield return node;
                }
            }
        }

        public Placeholder Placeholder()
        {
            return (Placeholder)this.FirstOrDefault(c => c.Category == CommandCategory.Placeholder);
        }

        public CommandCall FirstCall()
        {
            return Calls.FirstOrDefault();
        }

        public bool ContainsNode(Func<CommandNode, bool> filter)
        {
            return this.Any(filter);
        }

        public Type CommandInputType()
        {
            CommandCall call = FirstCall();
            return call == null ? null : call.InputType();
        }


        public bool HasInput()
        {
            CommandCall call = FirstCall();
            return call == null ? false : call.HasInput;
        }

        public static CommandChain For<T>()
        {
            return new CommandChain(typeof(T));
        }

        public override CommandNode Copy()
        {
            return new CommandChain(EntityType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(CommandChain)) return false;
            return Equals((CommandChain)obj);
        }

        public bool Equals(CommandChain other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.EntityType, EntityType);
        }

        public override int GetHashCode()
        {
            return (EntityType != null ? EntityType.GetHashCode() : 0);
        }
    }
}