using System;
using System.Linq.Expressions;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;
using FluentNHibernate.MappingModel.Identity;
using FluentNHibernate.Utils;

namespace FluentNHibernate.Mapping.Builders
{
    public class MapBuilder<TKey, TValue>
    {
        readonly MapMapping mapping;
        KeyMapping key;
        IndexMapping index;
        ManyToManyMapping relationship;
        ElementMapping element;

        public MapBuilder(MapMapping mapping, Member member)
        {
            this.mapping = mapping;

            InitialiseDefaults(member);
        }

        void InitialiseDefaults(Member member)
        {
            mapping.Member = member;
            mapping.SetDefaultValue(x => x.Name, member.Name);
            mapping.SetDefaultValue(x => x.TableName, mapping.ContainingEntityType.Name + member.Name);

            mapping.Key = key = new KeyMapping();
            key.AddDefaultColumn(new ColumnMapping { Name = mapping.ContainingEntityType.Name + "_id" });

            mapping.Index = index = new IndexMapping();
            index.AddDefaultColumn(new ColumnMapping { Name = "Key" });
            index.SetDefaultValue(x => x.Type, new TypeReference(KeyType));

            if (ValueType.IsSimpleType())
            {
                // element mapping
                mapping.Element = element = new ElementMapping();
                element.AddDefaultColumn(new ColumnMapping { Name = "Value" });
                element.SetDefaultValue(x => x.Type, new TypeReference(typeof(TValue)));
            }
            else if (KeyType.IsSimpleType())
            {
                // value/entity mapping
                mapping.Relationship = relationship = new ManyToManyMapping();
                relationship.Class = new TypeReference(ValueType);
                relationship.AddDefaultColumn(new ColumnMapping { Name = ValueType.Name + "_id" });
                relationship.ParentType = mapping.ContainingEntityType;
                relationship.ChildType = ValueType;
            }
            else
            {
                // entity/entity or ambiguous
            }
        }

        /// <summary>
        /// Sets the map name. Optional.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Name(string mapName)
        {
            mapping.Name = mapName;
            return this;
        }

        /// <summary>
        /// Sets the name of the table containing the dictionary values. Optional.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Table(string tableName)
        {
            mapping.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Specify how the index (or key) is configured.
        /// </summary>
        /// <param name="indexConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Index(Action<IndexBuilder> indexConfiguration)
        {
            mapping.Index = index = index ?? new IndexMapping();
            indexConfiguration(new IndexBuilder(index));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the keys of the dictionary, and the
        /// type of the key.
        /// </summary>
        /// <param name="indexColumnName">Key column name</param>
        /// <typeparam name="TIndexType">Key type</typeparam>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Index<TIndexType>(string indexColumnName)
        {
            return Index(ix =>
            {
                ix.Column(indexColumnName);
                ix.Type<TIndexType>();
            });
        }

        /// <summary>
        /// Specifies the name of the column used for the keys of the dictionary, while using
        /// the type inferred from the dictionary signature.
        /// </summary>
        /// <param name="indexColumnName">Key column name</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Index(string indexColumnName)
        {
            return Index(ix => ix.Column(indexColumnName));
        }

        /// <summary>
        /// Specifies the type of the keys used for the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <typeparam name="TIndexType">Key type</typeparam>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Index<TIndexType>()
        {
            return Index(ix => ix.Type<TIndexType>());
        }

        /// <summary>
        /// Specify how the element (or value) is configured.
        /// </summary>
        /// <param name="elementConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Element(Action<ElementBuilder> elementConfiguration)
        {
            mapping.Element = element = element ?? new ElementMapping();
            elementConfiguration(new ElementBuilder(element));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, and the
        /// type of the values.
        /// </summary>
        /// <param name="elementColumnName">Value column name</param>
        /// <typeparam name="TElementType">Value type</typeparam>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Element<TElementType>(string elementColumnName)
        {
            return Element(el =>
            {
                el.Column(elementColumnName);
                el.Type<TElementType>();
            });
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, while using
        /// the type inferred from the dictionary signature.
        /// </summary>
        /// <param name="elementColumnName">Value column name</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Element(string elementColumnName)
        {
            return Element(el => el.Column(elementColumnName));
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <typeparam name="TElementType">Value type</typeparam>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Element<TElementType>()
        {
            return Element(el => el.Type<TElementType>());
        }

        /// <summary>
        /// Specify how the foreign key is configured.
        /// </summary>
        /// <param name="keyConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Key(Action<KeyBuilder> keyConfiguration)
        {
            mapping.Key = key = key ?? new KeyMapping();
            keyConfiguration(new KeyBuilder(key));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the foreign key.
        /// </summary>
        /// <param name="keyColumnName">Key column name</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Key(string keyColumnName)
        {
            return Key(ke => ke.Column(keyColumnName));
        }

        /// <summary>
        /// Define a component (composite-element) to be used as the value of the dictionary.
        /// </summary>
        /// <param name="componentBuilder">Builder action</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> Component(Action<CompositeElementPart<TValue>> componentBuilder)
        {
            var builder = new CompositeElementPart<TValue>(mapping.ContainingEntityType);

            componentBuilder(builder);

            mapping.CompositeElement = ((ICompositeElementMappingProvider)builder).GetCompositeElementMapping();
            mapping.Relationship = null;
                
            return this;
        }

        /// <summary>
        /// Define a component for use as the index (composite-index).or dictionary key.
        /// </summary>
        /// <param name="componentBuilder">Builder action</param>
        /// <returns>Builder</returns>
        public MapBuilder<TKey, TValue> ComponentIndex(Action<CompositeIndexBuilder<TKey>> componentBuilder)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Specify the sorting for the dictionary.
        /// </summary>
        public SortBuilder<MapBuilder<TKey, TValue>> Sort
        {
            get { return new SortBuilder<MapBuilder<TKey,TValue>>(this, value => mapping.Sort = value); }
        }

        static Type KeyType
        {
            get { return typeof(TKey); }
        }

        static Type ValueType
        {
            get { return typeof(TValue); }
        }
    }

    public class SortBuilder<T>
    {
        readonly T parent;
        readonly Action<string> setter;

        public SortBuilder(T parent, Action<string> setter)
        {
            this.parent = parent;
            this.setter = setter;
        }

        public T Natural()
        {
            setter("natural");
            return parent;
        }

        public T Unsorted()
        {
            setter("unsorted");
            return parent;
        }

        public T WithComparer<TComparer>()
        {
            return WithComparer(typeof(TComparer));
        }

        public T WithComparer(Type comparer)
        {
            return WithComparer(comparer.AssemblyQualifiedName);
        }

        public T WithComparer(string comparer)
        {
            setter(comparer);
            return parent;
        }
    }

    public class CompositeIndexBuilder<T>
    {
        public KeyPropertyPart Map(Expression<Func<T, object>> memberExpression)
        {
            return new KeyPropertyPart(new KeyPropertyMapping());
        }

        public KeyManyToOnePart References(Expression<Func<T, object>> memberExpression)
        {
            return new KeyManyToOnePart(new KeyManyToOneMapping());
        }
    }
}