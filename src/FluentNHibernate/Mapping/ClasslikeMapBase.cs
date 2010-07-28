using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentNHibernate.Mapping.Builders;
using FluentNHibernate.Mapping.Providers;
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
        public PropertyPart Map(Expression<Func<T, object>> memberExpression)
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
        public PropertyPart Map(Expression<Func<T, object>> memberExpression, string columnName)
        {
            return Map(memberExpression.ToMember(), columnName);
        }

        protected virtual PropertyPart Map(Member property, string columnName)
        {
            var propertyMap = new PropertyPart(property, typeof(T));

            if (!string.IsNullOrEmpty(columnName))
                propertyMap.Column(columnName);

            properties.Add(propertyMap);

            return propertyMap;
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
        public ManyToOnePart<TOther> References<TOther>(Expression<Func<T, TOther>> memberExpression)
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
        public ManyToOnePart<TOther> References<TOther>(Expression<Func<T, TOther>> memberExpression, string columnName)
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
        public ManyToOnePart<TOther> References<TOther>(Expression<Func<T, object>> memberExpression)
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
        public ManyToOnePart<TOther> References<TOther>(Expression<Func<T, object>> memberExpression, string columnName)
        {
            return References<TOther>(memberExpression.ToMember(), columnName);
        }

        protected virtual ManyToOnePart<TOther> References<TOther>(Member property, string columnName)
        {
            var part = new ManyToOnePart<TOther>(EntityType, property);

            if (columnName != null)
                part.Column(columnName);

            references.Add(part);

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

        public MapBuilder<TKey, TChild> HasMany<TKey, TChild>(Expression<Func<T, IDictionary<TKey, TChild>>> memberExpression)        {
            return HasManyElement(memberExpression);
        }

        #region Element Maps

        public MapBuilder<TKey, char> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, string> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, int> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, uint> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, long> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, ulong> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, double> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, decimal> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, short> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, ushort> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, float> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, byte> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<TKey, bool> HasMany<TKey>(Expression<Func<T, IDictionary<TKey, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, char> HasMany(Expression<Func<T, IDictionary<char, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, string> HasMany(Expression<Func<T, IDictionary<char, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, int> HasMany(Expression<Func<T, IDictionary<char, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, uint> HasMany(Expression<Func<T, IDictionary<char, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, long> HasMany(Expression<Func<T, IDictionary<char, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, ulong> HasMany(Expression<Func<T, IDictionary<char, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, double> HasMany(Expression<Func<T, IDictionary<char, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, decimal> HasMany(Expression<Func<T, IDictionary<char, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, short> HasMany(Expression<Func<T, IDictionary<char, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, ushort> HasMany(Expression<Func<T, IDictionary<char, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, float> HasMany(Expression<Func<T, IDictionary<char, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, byte> HasMany(Expression<Func<T, IDictionary<char, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<char, bool> HasMany(Expression<Func<T, IDictionary<char, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, char> HasMany(Expression<Func<T, IDictionary<string, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, string> HasMany(Expression<Func<T, IDictionary<string, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, int> HasMany(Expression<Func<T, IDictionary<string, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, uint> HasMany(Expression<Func<T, IDictionary<string, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, long> HasMany(Expression<Func<T, IDictionary<string, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, ulong> HasMany(Expression<Func<T, IDictionary<string, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, double> HasMany(Expression<Func<T, IDictionary<string, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, decimal> HasMany(Expression<Func<T, IDictionary<string, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, short> HasMany(Expression<Func<T, IDictionary<string, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, ushort> HasMany(Expression<Func<T, IDictionary<string, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, float> HasMany(Expression<Func<T, IDictionary<string, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, byte> HasMany(Expression<Func<T, IDictionary<string, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<string, bool> HasMany(Expression<Func<T, IDictionary<string, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, char> HasMany(Expression<Func<T, IDictionary<int, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, string> HasMany(Expression<Func<T, IDictionary<int, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, int> HasMany(Expression<Func<T, IDictionary<int, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, uint> HasMany(Expression<Func<T, IDictionary<int, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, long> HasMany(Expression<Func<T, IDictionary<int, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, ulong> HasMany(Expression<Func<T, IDictionary<int, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, double> HasMany(Expression<Func<T, IDictionary<int, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, decimal> HasMany(Expression<Func<T, IDictionary<int, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, short> HasMany(Expression<Func<T, IDictionary<int, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, ushort> HasMany(Expression<Func<T, IDictionary<int, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, float> HasMany(Expression<Func<T, IDictionary<int, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, byte> HasMany(Expression<Func<T, IDictionary<int, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<int, bool> HasMany(Expression<Func<T, IDictionary<int, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, char> HasMany(Expression<Func<T, IDictionary<uint, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, string> HasMany(Expression<Func<T, IDictionary<uint, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, int> HasMany(Expression<Func<T, IDictionary<uint, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, uint> HasMany(Expression<Func<T, IDictionary<uint, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, long> HasMany(Expression<Func<T, IDictionary<uint, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, ulong> HasMany(Expression<Func<T, IDictionary<uint, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, double> HasMany(Expression<Func<T, IDictionary<uint, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, decimal> HasMany(Expression<Func<T, IDictionary<uint, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, short> HasMany(Expression<Func<T, IDictionary<uint, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, ushort> HasMany(Expression<Func<T, IDictionary<uint, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, float> HasMany(Expression<Func<T, IDictionary<uint, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, byte> HasMany(Expression<Func<T, IDictionary<uint, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<uint, bool> HasMany(Expression<Func<T, IDictionary<uint, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, char> HasMany(Expression<Func<T, IDictionary<long, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, string> HasMany(Expression<Func<T, IDictionary<long, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, int> HasMany(Expression<Func<T, IDictionary<long, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, uint> HasMany(Expression<Func<T, IDictionary<long, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, long> HasMany(Expression<Func<T, IDictionary<long, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, ulong> HasMany(Expression<Func<T, IDictionary<long, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, double> HasMany(Expression<Func<T, IDictionary<long, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, decimal> HasMany(Expression<Func<T, IDictionary<long, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, short> HasMany(Expression<Func<T, IDictionary<long, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, ushort> HasMany(Expression<Func<T, IDictionary<long, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, float> HasMany(Expression<Func<T, IDictionary<long, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, byte> HasMany(Expression<Func<T, IDictionary<long, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<long, bool> HasMany(Expression<Func<T, IDictionary<long, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, char> HasMany(Expression<Func<T, IDictionary<ulong, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, string> HasMany(Expression<Func<T, IDictionary<ulong, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, int> HasMany(Expression<Func<T, IDictionary<ulong, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, uint> HasMany(Expression<Func<T, IDictionary<ulong, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, long> HasMany(Expression<Func<T, IDictionary<ulong, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, ulong> HasMany(Expression<Func<T, IDictionary<ulong, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, double> HasMany(Expression<Func<T, IDictionary<ulong, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, decimal> HasMany(Expression<Func<T, IDictionary<ulong, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, short> HasMany(Expression<Func<T, IDictionary<ulong, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, ushort> HasMany(Expression<Func<T, IDictionary<ulong, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, float> HasMany(Expression<Func<T, IDictionary<ulong, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, byte> HasMany(Expression<Func<T, IDictionary<ulong, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ulong, bool> HasMany(Expression<Func<T, IDictionary<ulong, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, char> HasMany(Expression<Func<T, IDictionary<double, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, string> HasMany(Expression<Func<T, IDictionary<double, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, int> HasMany(Expression<Func<T, IDictionary<double, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, uint> HasMany(Expression<Func<T, IDictionary<double, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, long> HasMany(Expression<Func<T, IDictionary<double, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, ulong> HasMany(Expression<Func<T, IDictionary<double, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, double> HasMany(Expression<Func<T, IDictionary<double, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, decimal> HasMany(Expression<Func<T, IDictionary<double, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, short> HasMany(Expression<Func<T, IDictionary<double, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, ushort> HasMany(Expression<Func<T, IDictionary<double, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, float> HasMany(Expression<Func<T, IDictionary<double, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, byte> HasMany(Expression<Func<T, IDictionary<double, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<double, bool> HasMany(Expression<Func<T, IDictionary<double, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, char> HasMany(Expression<Func<T, IDictionary<decimal, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, string> HasMany(Expression<Func<T, IDictionary<decimal, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, int> HasMany(Expression<Func<T, IDictionary<decimal, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, uint> HasMany(Expression<Func<T, IDictionary<decimal, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, long> HasMany(Expression<Func<T, IDictionary<decimal, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, ulong> HasMany(Expression<Func<T, IDictionary<decimal, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, double> HasMany(Expression<Func<T, IDictionary<decimal, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, decimal> HasMany(Expression<Func<T, IDictionary<decimal, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, short> HasMany(Expression<Func<T, IDictionary<decimal, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, ushort> HasMany(Expression<Func<T, IDictionary<decimal, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, float> HasMany(Expression<Func<T, IDictionary<decimal, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, byte> HasMany(Expression<Func<T, IDictionary<decimal, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<decimal, bool> HasMany(Expression<Func<T, IDictionary<decimal, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, char> HasMany(Expression<Func<T, IDictionary<short, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, string> HasMany(Expression<Func<T, IDictionary<short, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, int> HasMany(Expression<Func<T, IDictionary<short, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, uint> HasMany(Expression<Func<T, IDictionary<short, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, long> HasMany(Expression<Func<T, IDictionary<short, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, ulong> HasMany(Expression<Func<T, IDictionary<short, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, double> HasMany(Expression<Func<T, IDictionary<short, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, decimal> HasMany(Expression<Func<T, IDictionary<short, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, short> HasMany(Expression<Func<T, IDictionary<short, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, ushort> HasMany(Expression<Func<T, IDictionary<short, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, float> HasMany(Expression<Func<T, IDictionary<short, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, byte> HasMany(Expression<Func<T, IDictionary<short, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<short, bool> HasMany(Expression<Func<T, IDictionary<short, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, char> HasMany(Expression<Func<T, IDictionary<ushort, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, string> HasMany(Expression<Func<T, IDictionary<ushort, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, int> HasMany(Expression<Func<T, IDictionary<ushort, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, uint> HasMany(Expression<Func<T, IDictionary<ushort, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, long> HasMany(Expression<Func<T, IDictionary<ushort, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, ulong> HasMany(Expression<Func<T, IDictionary<ushort, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, double> HasMany(Expression<Func<T, IDictionary<ushort, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, decimal> HasMany(Expression<Func<T, IDictionary<ushort, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, short> HasMany(Expression<Func<T, IDictionary<ushort, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, ushort> HasMany(Expression<Func<T, IDictionary<ushort, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, float> HasMany(Expression<Func<T, IDictionary<ushort, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, byte> HasMany(Expression<Func<T, IDictionary<ushort, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<ushort, bool> HasMany(Expression<Func<T, IDictionary<ushort, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, char> HasMany(Expression<Func<T, IDictionary<float, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, string> HasMany(Expression<Func<T, IDictionary<float, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, int> HasMany(Expression<Func<T, IDictionary<float, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, uint> HasMany(Expression<Func<T, IDictionary<float, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, long> HasMany(Expression<Func<T, IDictionary<float, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, ulong> HasMany(Expression<Func<T, IDictionary<float, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, double> HasMany(Expression<Func<T, IDictionary<float, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, decimal> HasMany(Expression<Func<T, IDictionary<float, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, short> HasMany(Expression<Func<T, IDictionary<float, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, ushort> HasMany(Expression<Func<T, IDictionary<float, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, float> HasMany(Expression<Func<T, IDictionary<float, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, byte> HasMany(Expression<Func<T, IDictionary<float, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<float, bool> HasMany(Expression<Func<T, IDictionary<float, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, char> HasMany(Expression<Func<T, IDictionary<byte, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, string> HasMany(Expression<Func<T, IDictionary<byte, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, int> HasMany(Expression<Func<T, IDictionary<byte, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, uint> HasMany(Expression<Func<T, IDictionary<byte, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, long> HasMany(Expression<Func<T, IDictionary<byte, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, ulong> HasMany(Expression<Func<T, IDictionary<byte, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, double> HasMany(Expression<Func<T, IDictionary<byte, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, decimal> HasMany(Expression<Func<T, IDictionary<byte, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, short> HasMany(Expression<Func<T, IDictionary<byte, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, ushort> HasMany(Expression<Func<T, IDictionary<byte, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, float> HasMany(Expression<Func<T, IDictionary<byte, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, byte> HasMany(Expression<Func<T, IDictionary<byte, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<byte, bool> HasMany(Expression<Func<T, IDictionary<byte, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, char> HasMany(Expression<Func<T, IDictionary<bool, char>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, string> HasMany(Expression<Func<T, IDictionary<bool, string>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, int> HasMany(Expression<Func<T, IDictionary<bool, int>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, uint> HasMany(Expression<Func<T, IDictionary<bool, uint>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, long> HasMany(Expression<Func<T, IDictionary<bool, long>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, ulong> HasMany(Expression<Func<T, IDictionary<bool, ulong>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, double> HasMany(Expression<Func<T, IDictionary<bool, double>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, decimal> HasMany(Expression<Func<T, IDictionary<bool, decimal>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, short> HasMany(Expression<Func<T, IDictionary<bool, short>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, ushort> HasMany(Expression<Func<T, IDictionary<bool, ushort>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, float> HasMany(Expression<Func<T, IDictionary<bool, float>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, byte> HasMany(Expression<Func<T, IDictionary<bool, byte>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        public MapBuilder<bool, bool> HasMany(Expression<Func<T, IDictionary<bool, bool>>> memberExpression)
        {
            return HasManyElement(memberExpression);
        }

        MapBuilder<TKey, TValue> HasManyElement<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> memberExpression)
        {
            return HasManyElement<TKey, TValue>(memberExpression.ToMember());
        }

        MapBuilder<TKey, TValue> HasManyElement<TKey, TValue>(Member member)
        {
            var mapping = new MapMapping
            {
                ContainingEntityType = typeof(T)
            };

            collections.Add(new PassThroughMappingProvider(mapping));

            return new MapBuilder<TKey, TValue>(mapping, member);
        }

        #endregion

        public MapBuilder<char, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<char, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<string, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<string, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<int, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<int, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<uint, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<uint, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<long, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<long, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<ulong, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<ulong, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<double, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<double, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<decimal, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<decimal, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<short, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<short, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<ushort, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<ushort, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<float, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<float, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<byte, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<byte, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        public MapBuilder<bool, TValue> HasMany<TValue>(Expression<Func<T, IDictionary<bool, TValue>>> memberExpression)
        {
            return HasMapManyToMany(memberExpression);
        }

        MapBuilder<TKey, TValue> HasMapManyToMany<TKey, TValue>(Expression<Func<T, IDictionary<TKey, TValue>>> memberExpression)
        {
            return HasMapManyToMany<TKey, TValue>(memberExpression.ToMember());
        }

        MapBuilder<TKey, TValue> HasMapManyToMany<TKey, TValue>(Member member)
        {
            var mapping = new MapMapping
            {
                ContainingEntityType = typeof(T)
            };

            collections.Add(new PassThroughMappingProvider(mapping));

            return new MapBuilder<TKey, TValue>(mapping, member);
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

        #endregion

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
