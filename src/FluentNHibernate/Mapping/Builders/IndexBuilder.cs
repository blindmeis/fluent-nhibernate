using System;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping.Builders
{
    public class IndexBuilder
    {
        readonly IndexMapping mapping;

        public IndexBuilder(IndexMapping mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        /// Specifies that this index is a many-to-many index.
        /// </summary>
        public void AsManyToMany()
        {
            mapping.IsManyToMany = true;
        }

        /// <summary>
        /// Specifies that this index is a one-to-many index. Note: some methods aren't available
        /// for one-to-many indexes.
        /// </summary>
        public void AsOneToMany()
        {
            mapping.IsManyToMany = false;
        }

        /// <summary>
        /// Specifies the column name for the index or key of the dictionary.
        /// </summary>
        /// <param name="indexColumnName">Column name</param>
        public void Column(string indexColumnName)
        {
            mapping.AddColumn(new ColumnMapping { Name = indexColumnName });
        }

        /// <summary>
        /// Modify the columns for this element
        /// </summary>
        public ColumnMappingCollection Columns
        {
            get { return new ColumnMappingCollection(mapping); }
        }

        /// <summary>
        /// Specifies the type of the index/key column
        /// </summary>
        /// <typeparam name="TIndex">Index type</typeparam>
        public void Type<TIndex>()
        {
            mapping.Type = new TypeReference(typeof(TIndex));
        }

        /// <summary>
        /// Specifies the type of the index/key column
        /// </summary>
        /// <param name="type">Type</param>
        public void Type(Type type)
        {
            mapping.Type = new TypeReference(type);
        }

        /// <summary>
        /// Specifies the type of the index/key column
        /// </summary>
        /// <param name="type">Type</param>
        public void Type(string type)
        {
            mapping.Type = new TypeReference(type);
        }
    }
}