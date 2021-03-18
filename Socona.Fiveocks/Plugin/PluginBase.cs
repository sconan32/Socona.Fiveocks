using System;

namespace Socona.Fiveocks.Plugin
{
    public abstract class PluginBase : IPlugin
    {
        public virtual bool Enabled { get; set; }
        public abstract string Name { get; set; }

        private Guid? _id;
        public Guid Id
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid();
                }
                return _id.Value;
            }
            set
            {
                _id = value;
            }
        }
    }
}
