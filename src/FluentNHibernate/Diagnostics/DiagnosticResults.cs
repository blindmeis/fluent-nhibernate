using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentNHibernate.Diagnostics
{
    public class DiagnosticResults
    {
        public DiagnosticResults(IEnumerable<Type> classMaps)
        {
            FluentMappings = classMaps.ToArray();
        }

        public IEnumerable<Type> FluentMappings { get; private set; }
    }
}