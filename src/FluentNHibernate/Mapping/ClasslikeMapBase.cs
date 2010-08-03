using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentNHibernate.Mapping.Builders;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.Utils;

namespace FluentNHibernate.Mapping
{
    public abstract class ClasslikeMapBase<T>
    {
        protected readonly IList<IPropertyMappingProvider> properties = new List<IPropertyMappingProvider>();
        protected readonly IList<IComponentMappingProvider> components = new List<IComponentMappingProvider>();
        protected readonly IList<IOneToOneMappingProvider> oneToOnes = new List<IOneToOneMappingProvider>();
        protected readonly Dictionary<Type, ISubclassMappingProvider> subclasses = new Dictionary<Type, ISubclassMappingProvider>();
        protected readonly IList<ICollectionMappingProvider> collections = new List<ICollectionMappingProvider>();
        protected readonly IList<IManyToOneMappingProvider> references = new List<IManyToOneMappingProvider>();
        protected readonly IList<IAnyMappingProvider> anys = new List<IAnyMappingProvider>();
        protected readonly IList<IFilterMappingProvider> filters = new List<IFilterMappingProvider>();
        protected readonly IList<IStoredProcedureMappingProvider> storedProcedures = new List<IStoredProcedureMappingProvider>();

        /// <summary>
        /// Create a property mapping.
        /// </summary>
        /// <param name="memberExpression">Property to map</param>
        /// <example>
        /// Map(x => x.Name);
        /// </example>
        public PropertyBuilder Map(Expression<Func<T, object>> memberExpression)
        {
            return Map(memberExpression, null);
        }

        /// <summary>
        /// Create a property mapping.
        /// </summary>
        /// <param name="memberExpression">Property to map</param>
        /// <param name="columnName">Property column name</param>
        /// <example>
        /// Map(x => x.Name, "person_name");
        /// </example>
        public PropertyBuilder Map(Expression<Func<T, object>> memberExpression, string columnName)
        {
            return Map(memberExpression.ToMember(), columnName);
        }

        protected virtual PropertyBuilder Map(Member property, string columnName)
        {
            var propertyMapping = new PropertyMapping();
            var builder = new PropertyBuilder(propertyMapping, typeof(T), property);

            if (!string.IsNullOrEmpty(columnName))
                builder.Column(columnName);

            properties.Add(new PassThroughMappingProvider(propertyMapping));

            return builder;
        }

        /// <summary>
        /// Create a reference to another entity. In database terms, this is a many-to-one
        /// relationship.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property on the current entity</param>
        /// <example>
        /// References(x => x.Company);
        /// </example>
        public ManyToOneBuilder<TOther> References<TOther>(Expression<Func<T, TOther>> memberExpression)
        {
            return References(memberExpression, null);
        }

        /// <summary>
        /// Create a reference to another entity. In database terms, this is a many-to-one
        /// relationship.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property on the current entity</param>
        /// <param name="columnName">Column name</param>
        /// <example>
        /// References(x => x.Company, "company_id");
        /// </example>
        public ManyToOneBuilder<TOther> References<TOther>(Expression<Func<T, TOther>> memberExpression, string columnName)
        {
            return References<TOther>(memberExpression.ToMember(), columnName);
        }

        /// <summary>
        /// Create a reference to another entity. In database terms, this is a many-to-one
        /// relationship.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property on the current entity</param>
        /// <example>
        /// References(x => x.Company, "company_id");
        /// </example>
        public ManyToOneBuilder<TOther> References<TOther>(Expression<Func<T, object>> memberExpression)
        {
            return References<TOther>(memberExpression, null);
        }

        /// <summary>
        /// Create a reference to another entity. In database terms, this is a many-to-one
        /// relationship.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property on the current entity</param>
        /// <param name="columnName">Column name</param>
        /// <example>
        /// References(x => x.Company, "company_id");
        /// </example>
        public ManyToOneBuilder<TOther> References<TOther>(Expression<Func<T, object>> memberExpression, string columnName)
        {
            return References<TOther>(memberExpression.ToMember(), columnName);
        }

        protected virtual ManyToOneBuilder<TOther> References<TOther>(Member property, string columnName)
        {
            var manyToOneMapping = new ManyToOneMapping();
            var part = new ManyToOneBuilder<TOther>(manyToOneMapping, EntityType, property);

            if (columnName != null)
                part.Column(columnName);

            references.Add(new PassThroughMappingProvider(manyToOneMapping));

            return part;
        }

        /// <summary>
        /// Create a reference to any other entity. This is an "any" polymorphic relationship.
        /// </summary>
        /// <typeparam name="TOther">Other entity to reference</typeparam>
        /// <param name="memberExpression">Property</param>
        public AnyPart<TOther> ReferencesAny<TOther>(Expression<Func<T, TOther>> memberExpression)
        {
            return ReferencesAny<TOther>(memberExpression.ToMember());
        }

        protected virtual AnyPart<TOther> ReferencesAny<TOther>(Member property)
        {
            var part = new AnyPart<TOther>(typeof(T), property);

            anys.Add(part);

            return part;
        }

        /// <summary>
        /// Create a reference to another entity based exclusively on the primary-key values.
        /// This is sometimes called a one-to-one relationship, in database terms. Generally
        /// you should use <see cref="References{TOther}(System.Linq.Expressions.Expression{System.Func{T,object}})"/>
        /// whenever possible.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property</param>
        /// <example>
        /// HasOne(x => x.ExtendedInfo);
        /// </example>
        public OneToOnePart<TOther> HasOne<TOther>(Expression<Func<T, Object>> memberExpression)
        {
            return HasOne<TOther>(memberExpression.ToMember());
        }

        /// <summary>
        /// Create a reference to another entity based exclusively on the primary-key values.
        /// This is sometimes called a one-to-one relationship, in database terms. Generally
        /// you should use <see cref="References{TOther}(System.Linq.Expressions.Expression{System.Func{T,object}})"/>
        /// whenever possible.
        /// </summary>
        /// <typeparam name="TOther">Other entity</typeparam>
        /// <param name="memberExpression">Property</param>
        /// <example>
        /// HasOne(x => x.ExtendedInfo);
        /// </example>
        public OneToOnePart<TOther> HasOne<TOther>(Expression<Func<T, TOther>> memberExpression)
        {
            return HasOne<TOther>(memberExpression.ToMember());
        }

        protected virtual OneToOnePart<TOther> HasOne<TOther>(Member property)
        {
            var part = new OneToOnePart<TOther>(EntityType, property);

            oneToOnes.Add(part);

            return part;
        }

        /// <summary>
        /// Create a dynamic component mapping. This is a dictionary that represents
        /// a limited number of columns in the database.
        /// </summary>
        /// <param name="memberExpression">Property containing component</param>
        /// <param name="dynamicComponentAction">Component setup action</param>
        /// <example>
        /// DynamicComponent(x => x.Data, comp =>
        /// {
        ///   comp.Map(x => (int)x["age"]);
        /// });
        /// </example>
        public DynamicComponentPart<IDictionary> DynamicComponent(Expression<Func<T, IDictionary>> memberExpression, Action<DynamicComponentPart<IDictionary>> dynamicComponentAction)
        {
            return DynamicComponent(memberExpression.ToMember(), dynamicComponentAction);
        }

        protected DynamicComponentPart<IDictionary> DynamicComponent(Member property, Action<DynamicComponentPart<IDictionary>> dynamicComponentAction)
        {
            var part = new DynamicComponentPart<IDictionary>(typeof(T), property);
            
            dynamicComponentAction(part);

            components.Add(part);

            return part;
        }

        /// <summary>
        /// Creates a component reference. This is a place-holder for a component that is defined externally with a
        /// <see cref="ComponentMap{T}"/>; the mapping defined in said <see cref="ComponentMap{T}"/> will be merged
        /// with any options you specify from this call.
        /// </summary>
        /// <typeparam name="TComponent">Component type</typeparam>
        /// <param name="member">Property exposing the component</param>
        /// <returns>Component reference builder</returns>
        public virtual ReferenceComponentPart<TComponent> Component<TComponent>(Expression<Func<T, TComponent>> member)
        {
            var part = new ReferenceComponentPart<TComponent>(member.ToMember(), typeof(T));

            components.Add(part);

            return part;
        }

        /// <summary>
        /// Maps a component
        /// </summary>
        /// <typeparam name="TComponent">Type of component</typeparam>
        /// <param name="expression">Component property</param>
        /// <param name="action">Component mapping</param>
        /// <example>
        /// Component(x => x.Address, comp =>
        /// {
        ///   comp.Map(x => x.Street);
        ///   comp.Map(x => x.City);
        /// });
        /// </example>
        public ComponentPart<TComponent> Component<TComponent>(Expression<Func<T, TComponent>> expression, Action<ComponentPart<TComponent>> action)
        {
            return Component(expression.ToMember(), action);
        }

        /// <summary>
        /// Maps a component
        /// </summary>
        /// <typeparam name="TComponent">Type of component</typeparam>
        /// <param name="expression">Component property</param>
        /// <param name="action">Component mapping</param>
        /// <example>
        /// Component(x => x.Address, comp =>
        /// {
        ///   comp.Map(x => x.Street);
        ///   comp.Map(x => x.City);
        /// });
        /// </example>
        public ComponentPart<TComponent> Component<TComponent>(Expression<Func<T, object>> expression, Action<ComponentPart<TComponent>> action)
        {
            return Component(expression.ToMember(), action);
        }
        
        protected virtual ComponentPart<TComponent> Component<TComponent>(Member property, Action<ComponentPart<TComponent>> action)
        {
            var part = new ComponentPart<TComponent>(typeof(T), property);

            action(part);

            components.Add(part);

            return part;
        }

        #region HasMany

        private OneToManyPart<TChild> MapHasMany<TChild, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            return HasMany<TChild>(expression.ToMember());
        }

        protected virtual OneToManyPart<TChild> HasMany<TChild>(Member member)
        {
            var part = new OneToManyPart<TChild>(EntityType, member);

            collections.Add(part);

            return part;
        }

        /// <summary>
        /// Maps a collection of entities as a one-to-many
        /// </summary>
        /// <typeparam name="TChild">Child entity type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasMany(x => x.Locations);
        /// </example>
        public OneToManyPart<TChild> HasMany<TChild>(Expression<Func<T, IEnumerable<TChild>>> memberExpression)
        {
            return MapHasMany<TChild, IEnumerable<TChild>>(memberExpression);
        }

        /// <summary>
        /// Maps a collection of entities as a one-to-many
        /// </summary>
        /// <typeparam name="TChild">Child entity type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasMany(x => x.Locations);
        /// </example>
        public OneToManyPart<TChild> HasMany<TChild>(Expression<Func<T, object>> memberExpression)
        {
            return MapHasMany<TChild, object>(memberExpression);
        }

        /// <summary>
        /// Maps a dictionary
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasMany(x => x.Locations);
        /// </example>
        public MapBuilder<TKey, TValue> HasMany<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> memberExpression)        {
            return HasManyMap(memberExpression);
        }

        /// <summary>
        /// Maps a dictionary
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasMany(x => x.Locations);
        /// </example>
        public MapBuilder<TKey, TValue> HasMany<TKey, TValue>(Expression<Func<T, IDictionary>> memberExpression)
        {
            return HasManyMap<TKey, TValue>(memberExpression.ToMember());
        }

        #endregion

        #region HasManyToMany

        private ManyToManyPart<TChild> MapHasManyToMany<TChild, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            return HasManyToMany<TChild>(expression.ToMember());
        }

        protected virtual ManyToManyPart<TChild> HasManyToMany<TChild>(Member property)
        {
            var part = new ManyToManyPart<TChild>(EntityType, property);

            collections.Add(part);

            return part;
        }

        /// <summary>
        /// Maps a collection of entities as a many-to-many
        /// </summary>
        /// <typeparam name="TChild">Child entity type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasManyToMany(x => x.Locations);
        /// </example>
        public ManyToManyPart<TChild> HasManyToMany<TChild>(Expression<Func<T, IEnumerable<TChild>>> memberExpression)
        {
            return MapHasManyToMany<TChild, IEnumerable<TChild>>(memberExpression);
        }

        /// <summary>
        /// Maps a collection of entities as a many-to-many
        /// </summary>
        /// <typeparam name="TChild">Child entity type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasManyToMany(x => x.Locations);
        /// </example>
        public ManyToManyPart<TChild> HasManyToMany<TChild>(Expression<Func<T, object>> memberExpression)
        {
            return MapHasManyToMany<TChild, object>(memberExpression);
        }

        /// <summary>
        /// Maps a generic dictionary
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasManyToMany(x => x.Locations);
        /// </example>
        public MapBuilder<TKey, TValue> HasManyToMany<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> memberExpression)
        {
            return HasManyMap(memberExpression);
        }

        /// <summary>
        /// Maps a dictionary
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="memberExpression">Collection property</param>
        /// <example>
        /// HasManyToMany(x => x.Locations);
        /// </example>
        public MapBuilder<TKey, TValue> HasManyToMany<TKey, TValue>(Expression<Func<T, IDictionary>> memberExpression)
        {
            return HasManyMap<TKey, TValue>(memberExpression.ToMember());
        }

        #endregion

        MapBuilder<TKey, TValue> HasManyMap<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> memberExpression)
        {
            return HasManyMap<TKey, TValue>(memberExpression.ToMember());
        }

        MapBuilder<TKey, TValue> HasManyMap<TKey, TValue>(Member member)
        {
            var mapping = new MapMapping
            {
                ContainingEntityType = typeof(T)
            };

            collections.Add(new PassThroughMappingProvider(mapping));

            return new MapBuilder<TKey, TValue>(mapping, member);
        }

        /// <summary>
        /// Specify an insert stored procedure
        /// </summary>
        /// <param name="innerText">Stored procedure call</param>
        public StoredProcedurePart SqlInsert(string innerText)
        {
            return StoredProcedure("sql-insert", innerText);
        }

        /// <summary>
        /// Specify an update stored procedure
        /// </summary>
        /// <param name="innerText">Stored procedure call</param>
        public StoredProcedurePart SqlUpdate(string innerText)
        {
            return StoredProcedure("sql-update", innerText);
        }

        /// <summary>
        /// Specify an delete stored procedure
        /// </summary>
        /// <param name="innerText">Stored procedure call</param>
        public StoredProcedurePart SqlDelete(string innerText)
        {
            return StoredProcedure("sql-delete", innerText);
        }

        /// <summary>
        /// Specify an delete all stored procedure
        /// </summary>
        /// <param name="innerText">Stored procedure call</param>
        public StoredProcedurePart SqlDeleteAll(string innerText)
        {
            return StoredProcedure("sql-delete-all", innerText);
        }

        protected StoredProcedurePart StoredProcedure(string element, string innerText)
        {
            var part = new StoredProcedurePart(element, innerText);
            storedProcedures.Add(part);
            return part;
        }

        protected virtual IEnumerable<IPropertyMappingProvider> Properties
		{
			get { return properties; }
		}

        protected virtual IEnumerable<IComponentMappingProvider> Components
		{
			get { return components; }
		}

        public Type EntityType
        {
            get { return typeof(T); }
        }
    }
}
