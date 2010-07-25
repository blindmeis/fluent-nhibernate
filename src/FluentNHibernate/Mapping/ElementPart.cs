using System;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping
{
    public class ElementPart : IElementMappingProvider
    {
        private readonly Type entity;
        private readonly ColumnMappingCollection<ElementPart> columns;
        readonly ElementMapping mapping;

        public ElementPart(Type entity)
        {
            this.entity = entity;
            mapping = new ElementMapping();
            columns = new ColumnMappingCollection<ElementPart>(this, mapping);          
        }

        /// <summary>
        /// Specify the element column name
        /// </summary>
        /// <param name="elementColumnName">Column name</param>
        public ElementPart Column(string elementColumnName)
        {
            columns.Add(elementColumnName);
            return this;
        }

        /// <summary>
        /// Modify the columns for this element
        /// </summary>
        public ColumnMappingCollection<ElementPart> Columns
        {
            get { return columns; }
        }

        /// <summary>
        /// Specify the element type
        /// </summary>
        /// <typeparam name="TElement">Element type</typeparam>
        public ElementPart Type<TElement>()
        {
            mapping.Type = new TypeReference(typeof(TElement));
            return this;
        }

        /// <summary>
        /// Specify the element column length
        /// </summary>
        /// <param name="length">Column length</param>
        public ElementPart Length(int length)
        {
            mapping.Length = length;
            return this;
        }

        /// <summary>
        /// Specify the element column formula
        /// </summary>
        /// <param name="formula">Formula</param>
        public ElementPart Formula(string formula)
        {
            mapping.Formula = formula;
            return this;
        }

        ElementMapping IElementMappingProvider.GetElementMapping()
        {
            mapping.ContainingEntityType = entity;

            return mapping;
        }
    }
}