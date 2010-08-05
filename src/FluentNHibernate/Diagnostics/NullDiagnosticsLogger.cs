using System;

namespace FluentNHibernate.Diagnostics
{
    public class NullDiagnosticsLogger : IDiagnosticLogger
    {
        public void Flush()
        {}

        public void FluentMappingDiscovered(Type type)
        {}
    }
}