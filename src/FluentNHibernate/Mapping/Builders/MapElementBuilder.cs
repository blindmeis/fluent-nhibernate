using System;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping.Builders
{
    public class MapElementBuilder<TKey, TValue>
    {
        readonly MapMapping mapping;
        readonly ElementMapping element;
        readonly KeyMapping key;

        public MapElementBuilder(MapMapping mapping, Member member)
        {
            this.mapping = mapping;
            this.mapping.Element = element = new ElementMapping();
            this.mapping.Key = key = new KeyMapping();

            SetDefaultsFromMember(member);
        }

        void SetDefaultsFromMember(Member member)
        {
            mapping.Member = member;
            mapping.SetDefaultValue(x => x.Name, member.Name);
            mapping.SetDefaultValue(x => x.TableName, mapping.ContainingEntityType.Name + member.Name);

            element.AddDefaultColumn(new ColumnMapping { Name = "Value" });
            element.SetDefaultValue(x => x.Type, new TypeReference(typeof(TValue)));

            key.AddDefaultColumn(new ColumnMapping { Name = mapping.ContainingEntityType.Name + "_id" });
        }

        /// <summary>
        /// Sets the map name. Optional.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Name(string mapName)
        {
            mapping.Name = mapName;
            return this;
        }

        /// <summary>
        /// Sets the name of the table containing the dictionary values. Optional.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Table(string tableName)
        {
            mapping.TableName = tableName;
            return this;
        }

        /// <summary>
        /// Specify how the element (or value) is configured.
        /// </summary>
        /// <param name="elementConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Element(Action<ElementBuilder> elementConfiguration)
        {
            elementConfiguration(new ElementBuilder(element));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, while using
        /// the type inferred from the dictionary signature.
        /// </summary>
        /// <param name="elementColumnName">Value column name</param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Element(string elementColumnName)
        {
            return Element(el => el.Name(elementColumnName));
        }

        /// <summary>
        /// Specifies the name of the column used for the values of the dictionary, and the
        /// type of the values.
        /// </summary>
        /// <param name="elementColumnName">Value column name</param>
        /// <typeparam name="TElementType">Value type</typeparam>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Element<TElementType>(string elementColumnName)
        {
            return Element(el =>
            {
                el.Name(elementColumnName);
                el.Type<TElementType>();
            });
        }

        /// <summary>
        /// Specifies the type of the values contained in the dictionary, while using the
        /// default column name.
        /// </summary>
        /// <typeparam name="TElementType">Value type</typeparam>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Element<TElementType>()
        {
            return Element(el => el.Type<TElementType>());
        }

        /// <summary>
        /// Specify how the foreign key is configured.
        /// </summary>
        /// <param name="keyConfiguration">Configuration <see cref="Action"/></param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Key(Action<KeyBuilder> keyConfiguration)
        {
            keyConfiguration(new KeyBuilder(key));
            return this;
        }

        /// <summary>
        /// Specifies the name of the column used for the foreign key.
        /// </summary>
        /// <param name="keyColumnName">Key column name</param>
        /// <returns>Builder</returns>
        public MapElementBuilder<TKey, TValue> Key(string keyColumnName)
        {
            return Key(ke => ke.Column(keyColumnName));
        }
    }
}