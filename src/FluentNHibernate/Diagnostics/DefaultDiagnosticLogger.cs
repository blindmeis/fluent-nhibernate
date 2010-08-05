using System;
using System.Collections.Generic;

namespace FluentNHibernate.Diagnostics
{
    public class DefaultDiagnosticLogger : IDiagnosticLogger
    {
        readonly IDiagnosticMessageDespatcher despatcher;
        readonly List<Type> classMaps = new List<Type>();
        bool isDirty;

        public DefaultDiagnosticLogger(IDiagnosticMessageDespatcher despatcher)
        {
            this.despatcher = despatcher;
        }

        public void Flush()
        {
            if (!isDirty) return;

            var results = BuildResults();

            despatcher.Publish(results);
        }

        DiagnosticResults BuildResults()
        {
            return new DiagnosticResults(classMaps);
        }

        public void ClassMapDiscovered(Type type)
        {
            isDirty = true;
            classMaps.Add(type);
        }
    }
}