using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping.Builders;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Utils;

namespace FluentNHibernate.Mapping
{
    public abstract class ToManyBase<T, TChild, TRelationshipAttributes> : ICollectionMappingProvider
        where T : ToManyBase<T, TChild, TRelationshipAttributes>, ICollectionMappingProvider
        where TRelationshipAttributes : ICollectionRelationshipMapping
    {
        private readonly AccessStrategyBuilder<T> access;
        private readonly FetchTypeExpression<T> fetch;
        private readonly OptimisticLockBuilder<T> optimisticLock;
        private readonly CollectionCascadeExpression<T> cascade;
        protected ICompositeElementMappingProvider componentMapping;
        protected bool nextBool = true;

        protected readonly AttributeStore<ICollectionMapping> collectionAttributes = new AttributeStore<ICollectionMapping>();
        protected readonly KeyMapping keyMapping;
        protected ICollectionRelationshipMapping relationshipMapping;
        private readonly IList<FilterMapping> filters = new List<FilterMapping>();
        private Func<AttributeStore, ICollectionMapping> collectionBuilder;
        protected IndexMapping indexMapping;
        protected Member member;
        private Type entity;
        ElementMapping elementMapping;
        CacheMapping cache;

        protected ToManyBase(Type entity, Member member, Type type)
        {
            this.entity = entity;
            this.member = member;
            AsBag();
            access = new AccessStrategyBuilder<T>((T)this, value => collectionAttributes.Set(x => x.Access, value));
            fetch = new FetchTypeExpression<T>((T)this, value => collectionAttributes.Set(x => x.Fetch, value));
            optimisticLock = new OptimisticLockBuilder<T>((T)this, value => collectionAttributes.Set(x => x.OptimisticLock, value));
            cascade = new CollectionCascadeExpression<T>((T)this, value => collectionAttributes.Set(x => x.Cascade, value));

            SetDefaultCollectionType(type);
            SetCustomCollectionType(type);

            collectionAttributes.SetDefault(x => x.Name, member.Name);

            keyMapping = new KeyMapping();
            keyMapping.AddDefaultColumn(new ColumnMapping { Name = entity.Name + "_id" });
        }

        /// <summary>
        /// Specify how the foreign key is configured.
        /// </summary>
        /// <param name="keyConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public T Key(Action<KeyBuilder> keyConfiguration)
        {
            keyConfiguration(new KeyBuilder(keyMapping));
            return (T)this;
        }

        /// <summary>
        /// This method is used to set a different key column in this table to be used for joins.
        /// The output is set as the property-ref attribute in the "key" subelement of the collection
        /// </summary>
        /// <param name="propertyRef">The name of the column in this table which is linked to the foreign key</param>
        /// <returns>OneToManyPart</returns>
        public T PropertyRef(string propertyRef)
        {
            return Key(ke => ke.PropertyRef(propertyRef));
        }

        /// <summary>
        /// Specify caching for this entity.
        /// </summary>
        public CacheBuilder Cache
        {
            get
            {
                cache = cache ?? new CacheMapping();
                return new CacheBuilder(cache, entity);
            }
        }

        /// <summary>
        /// Specify the lazy-load behaviour
        /// </summary>
        public T LazyLoad()
        {
            collectionAttributes.Set(x => x.Lazy, nextBool ? Lazy.True : Lazy.False);
            nextBool = true;
            return (T)this;
        }

        /// <summary>
        /// Specify extra lazy loading
        /// </summary>
        public T ExtraLazyLoad()
        {
            collectionAttributes.Set(x => x.Lazy, nextBool ? Lazy.Extra : Lazy.True);
            nextBool = true;
            return (T)this;
        }

        /// <summary>
        /// Inverse the ownership of this entity. Make the other side of the relationship
        /// responsible for saving.
        /// </summary>
        public T Inverse()
        {
            collectionAttributes.Set(x => x.Inverse, nextBool);
            nextBool = true;
            return (T)this;
        }

        /// <summary>
        /// Specify the cascade behaviour
        /// </summary>
        public CollectionCascadeExpression<T> Cascade
        {
            get { return cascade; }
        }

        /// <summary>
        /// Use a set collection
        /// </summary>
        public T AsSet()
        {
            collectionBuilder = attrs => new SetMapping(attrs);
            return (T)this;
        }

        /// <summary>
        /// Use a set collection
        /// </summary>
        /// <param name="sort">Sorting</param>
        public T AsSet(SortType sort)
        {
            collectionBuilder = attrs => new SetMapping(attrs) { Sort = sort.ToString().ToLowerInvariant() };
            return (T)this;
        }

        /// <summary>
        /// Use a set collection
        /// </summary>
        /// <typeparam name="TComparer">Item comparer</typeparam>
        public T AsSet<TComparer>() where TComparer : IComparer<TChild>
        {
            collectionBuilder = attrs => new SetMapping(attrs) { Sort = typeof(TComparer).AssemblyQualifiedName };
            return (T)this;
        }

        /// <summary>
        /// Use a bag collection
        /// </summary>
        public T AsBag()
        {
            collectionBuilder = attrs => new BagMapping(attrs);
            return (T)this;
        }

        /// <summary>
        /// Use a list collection
        /// </summary>
        public T AsList()
        {
            collectionBuilder = attrs => new ListMapping(attrs);
            CreateIndexMapping(null);

            if (indexMapping.Columns.IsEmpty())
                indexMapping.AddDefaultColumn(new ColumnMapping { Name = "Index" });

            return (T)this;
        }

        /// <summary>
        /// Use a list collection with an index
        /// </summary>
        /// <param name="customIndexMapping">Index mapping</param>
        public T AsList(Action<IndexBuilder> customIndexMapping)
        {
            collectionBuilder = attrs => new ListMapping(attrs);
            CreateIndexMapping(customIndexMapping);

            if (indexMapping.Columns.IsEmpty())
                indexMapping.AddDefaultColumn(new ColumnMapping { Name = "Index" });

            return (T)this;
        }

        /// <summary>
        /// Use an array
        /// </summary>
        /// <typeparam name="TIndex">Index type</typeparam>
        /// <param name="indexSelector">Index property</param>
        public T AsArray<TIndex>(Expression<Func<TChild, TIndex>> indexSelector)
        {
            return AsArray(indexSelector, null);
        }

        /// <summary>
        /// Use an array
        /// </summary>
        /// <typeparam name="TIndex">Index type</typeparam>
        /// <param name="indexSelector">Index property</param>
        /// <param name="customIndexMapping">Index mapping</param>
        public T AsArray<TIndex>(Expression<Func<TChild, TIndex>> indexSelector, Action<IndexBuilder> customIndexMapping)
        {
            collectionBuilder = attrs => new ArrayMapping(attrs);
            return AsIndexedCollection(indexSelector, customIndexMapping);
        }

        /// <summary>
        /// Make this collection indexed
        /// </summary>
        /// <typeparam name="TIndex">Index type</typeparam>
        /// <param name="indexSelector">Index property</param>
        /// <param name="customIndexMapping">Index mapping</param>
        public T AsIndexedCollection<TIndex>(Expression<Func<TChild, TIndex>> indexSelector, Action<IndexBuilder> customIndexMapping)
        {
            var indexMember = indexSelector.ToMember();
            return AsIndexedCollection<TIndex>(indexMember.Name, customIndexMapping);
        }

        /// <summary>
        /// Make this collection index
        /// </summary>
        /// <typeparam name="TIndex">Index type</typeparam>
        /// <param name="indexColumn">Index column</param>
        /// <param name="customIndexMapping">Index mapping</param>
        public T AsIndexedCollection<TIndex>(string indexColumn, Action<IndexBuilder> customIndexMapping)
        {
            CreateIndexMapping(customIndexMapping);

            if (!indexMapping.IsSpecified("Type"))
                indexMapping.SetDefaultValue(x => x.Type, new TypeReference(typeof(TIndex)));

            if (indexMapping.Columns.IsEmpty())
                indexMapping.AddDefaultColumn(new ColumnMapping { Name = indexColumn });

            return (T)this;
        }

        private void CreateIndexMapping(Action<IndexBuilder> customIndex)
        {
            indexMapping = new IndexMapping();
            var builder = new IndexBuilder(indexMapping);

            if (customIndex != null)
                customIndex(builder);
        }

        /// <summary>
        /// Map an element/value type
        /// </summary>
        /// <param name="columnName">Column name</param>
        public T Element(string columnName)
        {
            elementMapping = new ElementMapping { ContainingEntityType = typeof(T) };
            
            var builder = new ElementBuilder(elementMapping);
            builder.Type<TChild>();

            if (!string.IsNullOrEmpty(columnName))
                builder.Column(columnName);

            return (T)this;
        }

        /// <summary>
        /// Map an element/value type
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="customElementMapping">Custom mapping</param>
        public T Element(string columnName, Action<ElementBuilder> customElementMapping)
        {
            Element(columnName);
            if (customElementMapping != null) customElementMapping(new ElementBuilder(elementMapping));
            return (T)this;
        }

        /// <summary>
        /// Maps this collection as a collection of components.
        /// </summary>
        /// <param name="action">Component mapping</param>
        public T Component(Action<CompositeElementBuilder<TChild>> action)
        {
            var compositeElementMapping = new CompositeElementMapping();
            var part = new CompositeElementBuilder<TChild>(compositeElementMapping, typeof(T));

            action(part);

            componentMapping = new PassThroughMappingProvider(compositeElementMapping);

            return (T)this;
        }

        /// <summary>
        /// Sets the table name for this one-to-many.
        /// </summary>
        /// <param name="name">Table name</param>
        public T Table(string name)
        {
            collectionAttributes.Set(x => x.TableName, name);
            return (T)this;
        }

        /// <summary>
        /// Specify that the deletes should be cascaded
        /// </summary>
        public T ForeignKeyCascadeOnDelete()
        {
            return Key(ke => ke.CascadeOnDelete());
        }

        /// <summary>
        /// Specify the fetching behaviour
        /// </summary>
        public FetchTypeExpression<T> Fetch
        {
            get { return fetch; }
        }

        /// <summary>
        /// Set the access and naming strategy for this one-to-many.
        /// </summary>
        public AccessStrategyBuilder<T> Access
        {
            get { return access; }
        }

        /// <summary>
        /// Specify the optimistic locking behaviour
        /// </summary>
        public OptimisticLockBuilder<T> OptimisticLock
        {
            get { return optimisticLock; }
        }

        /// <summary>
        /// Specify a custom persister
        /// </summary>
        /// <typeparam name="TPersister">Persister</typeparam>
        public T Persister<TPersister>()
        {
            collectionAttributes.Set(x => x.Persister, new TypeReference(typeof(TPersister)));
            return (T)this;
        }

        /// <summary>
        /// Specify a check constraint
        /// </summary>
        /// <param name="constraintName">Constraint name</param>
        public T Check(string constraintName)
        {
            collectionAttributes.Set(x => x.Check, constraintName);
            return (T)this;
        }

        /// <summary>
        /// Specify that this collection is generic (optional)
        /// </summary>
        public T Generic()
        {
            collectionAttributes.Set(x => x.Generic, nextBool);
            nextBool = true;
            return (T)this;
        }

        /// <summary>
        /// Sets the where clause for this one-to-many relationship.
        /// Note: This only supports simple cases, use the string overload for more complex clauses.
        /// </summary>
        public T Where(Expression<Func<TChild, bool>> where)
        {
            var sql = ExpressionToSql.Convert(where);

            return Where(sql);
        }

        /// <summary>
        /// Sets the where clause for this one-to-many relationship.
        /// </summary>
        public T Where(string where)
        {
            collectionAttributes.Set(x => x.Where, where);
            return (T)this;
        }

        /// <summary>
        /// Specify the select batch size
        /// </summary>
        /// <param name="size">Batch size</param>
        public T BatchSize(int size)
        {
            collectionAttributes.Set(x => x.BatchSize, size);
            return (T)this;
        }

        /// <summary>
        /// Inverts the next boolean operation
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Not
        {
            get
            {
                nextBool = !nextBool;
                return (T)this;
            }
        }

        /// <summary>
        /// Sets a custom collection type
        /// </summary>
        public T CollectionType<TCollection>()
        {
            return CollectionType(typeof(TCollection));
        }

        /// <summary>
        /// Sets a custom collection type
        /// </summary>
        public T CollectionType(Type type)
        {
            return CollectionType(new TypeReference(type));
        }

        /// <summary>
        /// Sets a custom collection type
        /// </summary>
        public T CollectionType(string type)
        {
            return CollectionType(new TypeReference(type));
        }

        /// <summary>
        /// Sets a custom collection type
        /// </summary>
        public T CollectionType(TypeReference type)
        {
            collectionAttributes.Set(x => x.CollectionType, type);
            return (T)this;
        }

        /// <summary>
        /// Specify the table schema
        /// </summary>
        /// <param name="schema">Schema name</param>
        public T Schema(string schema)
        {
            collectionAttributes.Set(x => x.Schema, schema);
            return (T)this;
        }

        /// <summary>
        /// Specifies an entity-name.
        /// </summary>
        /// <remarks>See http://nhforge.org/blogs/nhibernate/archive/2008/10/21/entity-name-in-action-a-strongly-typed-entity.aspx</remarks>
        public T EntityName(string entityName)
        {
            relationshipMapping.EntityName = entityName;
            return (T)this;
        }

        /// <overloads>
        /// Applies a filter to this entity given it's name.
        /// </overloads>
        /// <summary>
        /// Applies a filter to this entity given it's name.
        /// </summary>
        /// <param name="name">The filter's name</param>
        /// <param name="condition">The condition to apply</param>
        public T ApplyFilter(string name, string condition)
        {
            var filterMapping = new FilterMapping();
            var builder = new FilterBuilder(filterMapping);
            
            builder.Name(name);
            builder.Condition(condition);
            
            filters.Add(filterMapping);
            return (T)this;
        }

        /// <overloads>
        /// Applies a filter to this entity given it's name.
        /// </overloads>
        /// <summary>
        /// Applies a filter to this entity given it's name.
        /// </summary>
        /// <param name="name">The filter's name</param>
        public T ApplyFilter(string name)
        {
            return (T)this.ApplyFilter(name, null);
        }

        /// <overloads>
        /// Applies a named filter to this one-to-many.
        /// </overloads>
        /// <summary>
        /// Applies a named filter to this one-to-many.
        /// </summary>
        /// <param name="condition">The condition to apply</param>
        /// <typeparam name="TFilter">
        /// The type of a <see cref="FilterDefinition"/> implementation
        /// defining the filter to apply.
        /// </typeparam>
        public T ApplyFilter<TFilter>(string condition) where TFilter : FilterDefinition, new()
        {
            return this.ApplyFilter(new TFilter().Name, condition);
        }

        /// <summary>
        /// Applies a named filter to this one-to-many.
        /// </summary>
        /// <typeparam name="TFilter">
        /// The type of a <see cref="FilterDefinition"/> implementation
        /// defining the filter to apply.
        /// </typeparam>
        public T ApplyFilter<TFilter>() where TFilter : FilterDefinition, new()
        {
            return this.ApplyFilter<TFilter>(null);
        }

        void SetDefaultCollectionType(Type type)
        {
            if (type.Namespace == "Iesi.Collections.Generic" || type.Closes(typeof(HashSet<>)))
                AsSet();
        }

        void SetCustomCollectionType(Type type)
        {
            if (type.Namespace.StartsWith("Iesi") || type.Namespace.StartsWith("System") || type.IsArray)
                return;

            collectionAttributes.Set(x => x.CollectionType, new TypeReference(type));
        }

        ICollectionMapping ICollectionMappingProvider.GetCollectionMapping()
        {
            return GetCollectionMapping();
        }

        protected virtual ICollectionMapping GetCollectionMapping()
        {
            var mapping = collectionBuilder(collectionAttributes.CloneInner());

            if (!mapping.IsSpecified("Name"))
                mapping.SetDefaultValue(x => x.Name, GetDefaultName());

            mapping.ContainingEntityType = entity;
            mapping.ChildType = typeof(TChild);
            mapping.Member = member;
            mapping.Key = keyMapping;
            mapping.Key.ContainingEntityType = entity;
            mapping.Relationship = GetRelationship();
            mapping.Relationship.SetDefaultClass(new TypeReference(typeof(TChild)));

            if (cache != null)
                mapping.Cache = cache;

            if (componentMapping != null)
            {
                mapping.CompositeElement = componentMapping.GetCompositeElementMapping();
                mapping.Relationship = null; // HACK: bad design
            }

            // HACK: Index only on list and map - shouldn't have to do this!
            if (indexMapping != null && mapping is IIndexedCollectionMapping)
                ((IIndexedCollectionMapping)mapping).Index = indexMapping;

            if (elementMapping != null)
            {
                mapping.Element = elementMapping;
                mapping.Relationship = null;
            }

            foreach (var filterMapping in filters)
                mapping.Filters.Add(filterMapping);

            return mapping;
        }

        private string GetDefaultName()
        {
            if (member.IsMethod)
            {
                // try to guess the backing field name (GetSomething -> something)
                if (member.Name.StartsWith("Get"))
                {
                    var name = member.Name.Substring(3);

                    if (char.IsUpper(name[0]))
                        name = char.ToLower(name[0]) + name.Substring(1);

                    return name;
                }
            }

            return member.Name;
        }

        protected abstract ICollectionRelationshipMapping GetRelationship();
    }
}
