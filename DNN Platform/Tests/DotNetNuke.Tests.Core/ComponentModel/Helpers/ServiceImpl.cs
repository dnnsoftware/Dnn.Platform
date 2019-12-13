using System;

namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class ServiceImpl : IService
    {
        private static readonly Random rnd = new Random();
        private readonly int id = rnd.Next();

        public int Id
        {
            get { return id; }
        }
    }
}
