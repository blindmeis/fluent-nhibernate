using System;
using FluentNHibernate.MappingModel;

namespace FluentNHibernate.Mapping.Builders
{
    public class KeyBuilder
    {
        readonly KeyMapping mapping;

        public KeyBuilder(KeyMapping mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        /// Sets the column name
        /// </summary>
        /// <param name="keyColumnName"></param>
        public void Column(string keyColumnName)
        {
            mapping.AddColumn(new ColumnMapping { Name = keyColumnName });
        }
    }
}