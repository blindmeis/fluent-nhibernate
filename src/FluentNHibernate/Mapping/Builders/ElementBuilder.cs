using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping.Builders
{
    public class ElementBuilder
    {
        readonly ElementMapping mapping;

        public ElementBuilder(ElementMapping mapping)
        {
            this.mapping = mapping;
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <typeparam name="TElementType">Value type</typeparam>
        /// <returns>Builder</returns>
        public void Type<TElementType>()
        {
            mapping.Type = new TypeReference(typeof(TElementType));
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, while using
        /// the type inferred from the dictionary signature.
        /// </summary>
        /// <param name="elementColumnName">Value column name</param>
        /// <returns>Builder</returns>
        public void Name(string elementColumnName)
        {
            mapping.AddColumn(new ColumnMapping { Name = elementColumnName });
        }
    }
}