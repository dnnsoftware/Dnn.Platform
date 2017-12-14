using System;

namespace ClientDependency.Core
{

    /// <summary>
    /// Defines all information relating to a pre-defined bundle
    /// </summary>
    internal class BundleDefinition
    {
        public BundleDefinition(
            ClientDependencyType type,
            string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            Type = type;
            Name = name;
        }

        public ClientDependencyType Type { get; private set; }
        public string Name { get; private set; }

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