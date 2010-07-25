using System.Collections.Generic;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping
{
    public class PassThroughMappingProvider : IMappingProvider, ICollectionMappingProvider
    {
        private readonly object mapping;

        public PassThroughMappingProvider(object mapping)
        {
            this.mapping = mapping;
        }

        public ClassMapping GetClassMapping()
        {
            return (ClassMapping)mapping;
        }

        public ICollectionMapping GetCollectionMapping()
        {
            return (ICollectionMapping)mapping;
        }

        public HibernateMapping GetHibernateMapping()
        {
            return new HibernateMapping();
        }

        public IEnumerable<Member> GetIgnoredProperties()
        {
            return new Member[0];
        }
    }
}