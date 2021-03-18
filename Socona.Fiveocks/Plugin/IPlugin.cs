using System;

namespace Socona.Fiveocks.Plugin
{
    public interface IPlugin
    {
        bool Enabled { get; set; }

        string Name { get; set; }

        Guid Id { get; set; }
    }
}
