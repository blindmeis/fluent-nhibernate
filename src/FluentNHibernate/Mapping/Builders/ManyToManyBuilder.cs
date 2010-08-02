using System;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping.Builders
{
    public class ManyToManyBuilder
    {
        readonly ManyToManyMapping mapping;

        public ManyToManyBuilder(ManyToManyMapping mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <typeparam name="TChild">Child type</typeparam>
        public void Type<TChild>()
        {
            mapping.Class = new TypeReference(typeof(TChild));
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <param name="type">Type</param>
        public void Type(Type type)
        {
            mapping.Class = new TypeReference(type);
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <param name="type">Type name</param>
        public void Type(string type)
        {
            mapping.Class = new TypeReference(type);
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, while using
        /// the type inferred from the dictionary signature.
        /// </summary>
        /// <param name="relationshipColumn">Value column name</param>
        public void Column(string relationshipColumn)
        {
            mapping.AddColumn(new ColumnMapping { Name = relationshipColumn });
        }

        /// <summary>
        /// Modify the columns for this element
        /// </summary>
        public ColumnMappingCollection Columns
        {
            get { return new ColumnMappingCollection(mapping); }
        }
    }
}