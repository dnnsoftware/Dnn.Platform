﻿using System;

namespace ClientDependency.Core
{

    /// <summary>
    /// Defines all information relating to a pre-defined bundle
    /// </summary>
    internal class BundleDefinition
    {
        public BundleDefinition(
            ClientDependencyType type,
            string name, 
            int priority = Constants.DefaultPriority, int group = Constants.DefaultGroup)
        {
            if (name == null) throw new ArgumentNullException("name");
            Type = type;
            Name = name;
            Group = group;
            Priority = priority;
        }

        public ClientDependencyType Type { get; private set; }
        public string Name { get; private set; }
        public int Group { get; set; }
        public int Priority { get; set; }

        protected bool Equals(BundleDefinition other)
        {
            return Type == other.Type && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BundleDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type*397) ^ Name.GetHashCode();
            }
        }
    }
}