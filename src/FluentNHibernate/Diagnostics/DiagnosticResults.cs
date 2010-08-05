using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentNHibernate.Diagnostics
{
    public class DiagnosticResults
    {
        public DiagnosticResults(IEnumerable<Type> classMaps)
        {
            ClassMaps = classMaps.ToArray();
        }

        public IEnumerable<Type> ClassMaps { get; private set; }
    }
}