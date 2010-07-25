using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using FluentNHibernate.Mapping.Builders;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Utils;
using NHibernate.Persister.Entity;

namespace FluentNHibernate.Mapping
{
    public class ManyToManyPart<TChild> : ToManyBase<ManyToManyPart<TChild>, TChild, ManyToManyMapping>
    {
        private readonly IList<FilterPart> childFilters = new List<FilterPart>();
        private readonly Type entity;
        private readonly FetchTypeExpression<ManyToManyPart<TChild>> fetch;
        private readonly NotFoundExpression<ManyToManyPart<TChild>> notFound;
        private IndexManyToManyPart manyToManyIndex;
        private readonly Type childType;
        private Type valueType;
        private bool isTernary;

        public ManyToManyPart(Type entity, Member property)
            : this(entity, property, property.PropertyType)
        {
            childType = property.PropertyType;
        }

        protected ManyToManyPart(Type entity, Member member, Type collectionType)
            : base(entity, member, collectionType)
        {
            this.entity = entity;
            childType = collectionType;

            fetch = new FetchTypeExpression<ManyToManyPart<TChild>>(this, value => collectionAttributes.Set(x => x.Fetch, value));
            notFound = new NotFoundExpression<ManyToManyPart<TChild>>(this, value => relationshipMapping.NotFound = value);

            relationshipMapping = new ManyToManyMapping
            {
                ContainingEntityType = entity
            };
            relationshipMapping.As<ManyToManyMapping>(x =>
                x.AddDefaultColumn(new ColumnMapping { Name = typeof(TChild).Name + "_id"}));
        }

        /// <summary>
        /// Sets a single child key column. If there are multiple columns, use ChildKeyColumns.Add
        /// </summary>
        public ManyToManyPart<TChild> ChildKeyColumn(string childKeyColumn)
        {
            relationshipMapping.As<ManyToManyMapping>(x =>
            {
                x.ClearColumns();
                x.AddColumn(new ColumnMapping {Name = childKeyColumn});
            });
            return this;
        }

        /// <summary>
        /// Sets a single parent key column. If there are multiple columns, use ParentKeyColumns.Add
        /// </summary>
        public ManyToManyPart<TChild> ParentKeyColumn(string parentKeyColumn)
        {
            Key(ke => ke.Column(parentKeyColumn));
            return this;
        }

        public ColumnMappingCollection<ManyToManyPart<TChild>> ChildKeyColumns
        {
            get { return new ColumnMappingCollection<ManyToManyPart<TChild>>(this, relationshipMapping as ManyToManyMapping); }
        }

        public ColumnMappingCollection<ManyToManyPart<TChild>> ParentKeyColumns
        {
            get { return new ColumnMappingCollection<ManyToManyPart<TChild>>(this, keyMapping); }
        }

        public ManyToManyPart<TChild> ForeignKeyConstraintNames(string parentForeignKeyName, string childForeignKeyName)
        {
            Key(ke => ke.ForeignKey(parentForeignKeyName));
            relationshipMapping.As<ManyToManyMapping>(x => x.ForeignKey = childForeignKeyName);
            return this;
        }

        public ManyToManyPart<TChild> ChildPropertyRef(string childPropertyRef)
        {
            relationshipMapping.As<ManyToManyMapping>(x => x.ChildPropertyRef = childPropertyRef);
            return this;
        }

        public FetchTypeExpression<ManyToManyPart<TChild>> FetchType
        {
            get { return fetch; }
        }

        private void EnsureDictionary()
        {
            if (!typeof(IDictionary).IsAssignableFrom(childType))
                throw new ArgumentException(member.Name + " must be of type IDictionary to be used in a non-generic ternary association. Type was: " + childType);
        }

        private void EnsureGenericDictionary()
        {
            if (!(childType.IsGenericType && childType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                throw new ArgumentException(member.Name + " must be of type IDictionary<> to be used in a ternary assocation. Type was: " + childType);
        }

        public ManyToManyPart<TChild> AsTernaryAssociation()
        {
            EnsureGenericDictionary();

            var indexType = typeof(TChild).GetGenericArguments()[0];
            var valueType = typeof(TChild).GetGenericArguments()[1];

            return AsTernaryAssociation(indexType.Name + "_id", valueType.Name + "_id");
        }

        public ManyToManyPart<TChild> AsTernaryAssociation(string indexColumn, string valueColumn)
        {
            return AsTernaryAssociation(indexColumn, valueColumn, x => {});
        }

        public ManyToManyPart<TChild> AsTernaryAssociation(string indexColumn, string valueColumn, Action<IndexManyToManyPart> indexAction)
        {
            EnsureGenericDictionary();

            var indexType = typeof(TChild).GetGenericArguments()[0];
            var valueType = typeof(TChild).GetGenericArguments()[1];

            manyToManyIndex = new IndexManyToManyPart(typeof(ManyToManyPart<TChild>));
            manyToManyIndex.Column(indexColumn);
            manyToManyIndex.Type(indexType);

            if (indexAction != null)
                indexAction(manyToManyIndex);

            ChildKeyColumn(valueColumn);
            this.valueType = valueType;

            isTernary = true;

            return this;
        }

        public ManyToManyPart<TChild> AsTernaryAssociation(Type indexType, Type valueType)
        {
            return AsTernaryAssociation(indexType, indexType.Name + "_id", valueType, valueType.Name + "_id");
        }

        public ManyToManyPart<TChild> AsTernaryAssociation(Type indexType, string indexColumn, Type valueType, string valueColumn)
        {
            return AsTernaryAssociation(indexType, indexColumn, valueType, valueColumn, x => {});
        }

        public ManyToManyPart<TChild> AsTernaryAssociation(Type indexType, string indexColumn, Type valueType, string valueColumn, Action<IndexManyToManyPart> indexAction)
        {
            EnsureDictionary();

            manyToManyIndex = new IndexManyToManyPart(typeof(ManyToManyPart<TChild>));
            manyToManyIndex.Column(indexColumn);
            manyToManyIndex.Type(indexType);

            if (indexAction != null)
                indexAction(manyToManyIndex);

            ChildKeyColumn(valueColumn);
            this.valueType = valueType;

            isTernary = true;

            return this;
        }

        public ManyToManyPart<TChild> AsSimpleAssociation()
        {
            EnsureGenericDictionary();

            var indexType = typeof(TChild).GetGenericArguments()[0];
            var valueType = typeof(TChild).GetGenericArguments()[1];

            return AsSimpleAssociation(indexType.Name + "_id", valueType.Name + "_id");
        }

        public ManyToManyPart<TChild> AsSimpleAssociation(string indexColumn, string valueColumn)
        {
            EnsureGenericDictionary();

            var indexType = typeof(TChild).GetGenericArguments()[0];
            var valueType = typeof(TChild).GetGenericArguments()[1];

            indexMapping = new IndexMapping();
            var builder = new IndexBuilder(indexMapping);
            builder.Column(indexColumn);
            builder.Type(indexType);

            ChildKeyColumn(valueColumn);
            this.valueType = valueType;

            isTernary = true;

            return this;
        }

        public ManyToManyPart<TChild> AsEntityMap()
        {
            // The argument to AsMap will be ignored as the ternary association will overwrite the index mapping for the map.
            // Therefore just pass null.
            return AsMap(null).AsTernaryAssociation();
        }

        public ManyToManyPart<TChild> AsEntityMap(string indexColumn, string valueColumn)
        {
            return AsMap(null).AsTernaryAssociation(indexColumn, valueColumn);
        }

        public Type ChildType
        {
            get { return typeof(TChild); }
        }

        public NotFoundExpression<ManyToManyPart<TChild>> NotFound
        {
            get { return notFound; }
        }

        protected override ICollectionRelationshipMapping GetRelationship()
        {
            if (isTernary && valueType != null)
                relationshipMapping.Class = new TypeReference(valueType);

            relationshipMapping.As<ManyToManyMapping>(x =>
            {
                foreach (var filterPart in childFilters)
                    x.ChildFilters.Add(filterPart.GetFilterMapping());
            });

            return relationshipMapping;
        }

        /// <summary>
        /// Sets the order-by clause on the collection element.
        /// </summary>
        public ManyToManyPart<TChild> OrderBy(string orderBy)
        {
            collectionAttributes.Set(x => x.OrderBy, orderBy);
            return this;
        }

        /// <summary>
        /// Sets the order-by clause on the many-to-many element.
        /// </summary>
        public ManyToManyPart<TChild> ChildOrderBy(string orderBy)
        {
            relationshipMapping.As<ManyToManyMapping>(x => x.OrderBy = orderBy);
            return this;
        }

        public ManyToManyPart<TChild> ReadOnly()
        {            
            collectionAttributes.Set(x => x.Mutable, !nextBool);
            nextBool = true;
            return this;
        }

        public ManyToManyPart<TChild> Subselect(string subselect)
        {
            collectionAttributes.Set(x => x.Subselect, subselect);
            return this;
        }

        /// <overloads>
        /// Applies a filter to the child element of this entity given it's name.
        /// </overloads>
        /// <summary>
        /// Applies a filter to the child element of this entity given it's name.
        /// </summary>
        /// <param name="name">The filter's name</param>
        /// <param name="condition">The condition to apply</param>
        public ManyToManyPart<TChild> ApplyChildFilter(string name, string condition)
        {
            var part = new FilterPart(name, condition);
            childFilters.Add(part);
            return this;
        }

        /// <overloads>
        /// Applies a filter to the child element of this entity given it's name.
        /// </overloads>
        /// <summary>
        /// Applies a filter to the child element of this entity given it's name.
        /// </summary>
        /// <param name="name">The filter's name</param>
        public ManyToManyPart<TChild> ApplyChildFilter(string name)
        {
            return this.ApplyChildFilter(name, null);
        }

        /// <overloads>
        /// Applies a named filter to the child element of this many-to-many.
        /// </overloads>
        /// <summary>
        /// Applies a named filter to the child element of this many-to-many.
        /// </summary>
        /// <param name="condition">The condition to apply</param>
        /// <typeparam name="TFilter">
        /// The type of a <see cref="FilterDefinition"/> implementation
        /// defining the filter to apply.
        /// </typeparam>
        public ManyToManyPart<TChild> ApplyChildFilter<TFilter>(string condition) where TFilter : FilterDefinition, new()
        {
            var part = new FilterPart(new TFilter().Name, condition);
            childFilters.Add(part);
            return this;
        }

        /// <summary>
        /// Applies a named filter to the child element of this many-to-many.
        /// </summary>
        /// <typeparam name="TFilter">
        /// The type of a <see cref="FilterDefinition"/> implementation
        /// defining the filter to apply.
        /// </typeparam>
        public ManyToManyPart<TChild> ApplyChildFilter<TFilter>() where TFilter : FilterDefinition, new()
        {
            return ApplyChildFilter<TFilter>(null);
        }

        /// <summary>
        /// Sets the where clause for this relationship, on the many-to-many element.
        /// </summary>
        public ManyToManyPart<TChild> ChildWhere(string where)
        {
            relationshipMapping.As<ManyToManyMapping>(x => x.Where = where);
            return this;
        }

        protected override ICollectionMapping GetCollectionMapping()
        {
            var collection = base.GetCollectionMapping();

            // HACK: Index only on list and map - shouldn't have to do this!
            if (indexMapping != null && collection is IIndexedCollectionMapping)
                ((IIndexedCollectionMapping)collection).Index = indexMapping;

            // HACK: shouldn't have to do this!
            if (manyToManyIndex != null && collection is MapMapping)
                ((MapMapping)collection).Index = manyToManyIndex.GetIndexMapping();

            return collection;
        }
    }
}
