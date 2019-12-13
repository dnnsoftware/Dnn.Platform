#region Usings

using System;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.ComponentModel
{
    internal class TransientComponentBuilder : IComponentBuilder
    {
        private readonly string _Name;
        private readonly Type _Type;

        /// <summary>
        /// Initializes a new instance of the TransientComponentBuilder class.
        /// </summary>
        /// <param name="name">The name of the component</param>
        /// <param name="type">The type of the component</param>
        public TransientComponentBuilder(string name, Type type)
        {
            _Name = name;
            _Type = type;
        }

        #region IComponentBuilder Members

        public object BuildComponent()
        {
            return Reflection.CreateObject(_Type);
        }

        public string Name
        {
            get
            {
                return _Name;
            }
        }

        #endregion
    }
}
