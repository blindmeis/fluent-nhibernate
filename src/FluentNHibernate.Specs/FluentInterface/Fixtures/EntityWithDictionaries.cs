using System;
using System.Collections.Generic;

namespace FluentNHibernate.Specs.FluentInterface.Fixtures
{
    class EntityWithDictionaries : EntityBase
    {
        public IDictionary<string, string> ValueTypeKeyValue { get; set; }
        public IDictionary<string, Target> ValueTypeKeyEntityValue { get; set; }
    }

    class Target : EntityBase
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}