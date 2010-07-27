using System;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping.Builders
{
    public class MapManyToManyBuilder<TKey, TValue>
    {
        readonly MapMapping mapping;
        readonly KeyMapping key;
        readonly IndexMapping index;
        readonly ManyToManyMapping relationship;

        public MapManyToManyBuilder(MapMapping mapping, Member member)
        {
            this.mapping = mapping;
            this.mapping.Key = key = new KeyMapping();
            this.mapping.Index = index = new IndexMapping();
            this.mapping.Relationship = relationship = new ManyToManyMapping();

            SetDefaultsFromMember(member);
        }

        void SetDefaultsFromMember(Member member)
        {
            mapping.Member = member;
            mapping.SetDefaultValue(x => x.Name, member.Name);
            mapping.SetDefaultValue(x => x.TableName, mapping.ContainingEntityType.Name + member.Name);

            key.AddDefaultColumn(new ColumnMapping { Name = mapping.ContainingEntityType.Name + "_id" });

            index.AddDefaultColumn(new ColumnMapping { Name = "Key" });
            index.SetDefaultValue(x => x.Type, new TypeReference(typeof(TKey)));

            relationship.Class = new TypeReference(typeof(TValue));
            relationship.AddDefaultColumn(new ColumnMapping { Name = typeof(TValue).Name + "_id" });
            relationship.ParentType = mapping.ContainingEntityType;
            relationship.ChildType = typeof(TValue);
        }

        /// <summary>
        /// Sets the map name. Optional.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Builder</returns>
        public MapManyToManyBuilder<TKey, TValue> Name(string mapName)
        {
            mapping.Name = mapName;
            return this;
        }

        /// <summary>
        /// Sets the name of the table containing the dictionary values. Optional.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Builder</returns>
        public MapManyToManyBuilder<TKey, TValue> Table(string tableName)
        {
            mapping.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Specify how the index (or key) is configured.
        /// </summary>
        /// <param name="indexConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapManyToManyBuilder<TKey, TValue> Index(Action<IndexBuilder> indexConfiguration)
        {
            indexConfiguration(new IndexBuilder(index));
            return this;
        }

        /// <summary>
        /// Specify how the foreign key is configured.
        /// </summary>
        /// <param name="keyConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapManyToManyBuilder<TKey, TValue> Key(Action<KeyBuilder> keyConfiguration)
        {
            keyConfiguration(new KeyBuilder(key));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the foreign key.
        /// </summary>
        /// <param name="keyColumnName">Key column name</param>
        /// <returns>Builder</returns>
        public MapManyToManyBuilder<TKey, TValue> Key(string keyColumnName)
        {
            return Key(ke => ke.Column(keyColumnName));
        }
    }
}