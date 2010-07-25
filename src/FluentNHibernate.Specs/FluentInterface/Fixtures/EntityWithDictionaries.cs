using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentNHibernate.Specs.FluentInterface.Fixtures
{
    class EntityWithDictionaries : EntityBase
    {
        public IDictionary<string, string> ValueTypeKeyValue { get; set; }
    }
}