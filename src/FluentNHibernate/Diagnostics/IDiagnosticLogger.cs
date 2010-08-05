using System;

namespace FluentNHibernate.Diagnostics
{
    public interface IDiagnosticLogger
    {
        void Flush();
        void ClassMapDiscovered(Type type);
    }
}