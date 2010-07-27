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
    }
}