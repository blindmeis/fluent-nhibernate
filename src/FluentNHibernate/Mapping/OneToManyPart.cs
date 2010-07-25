using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Mapping.Builders;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.Mapping
{
    public class OneToManyPart<TChild> : ToManyBase<OneToManyPart<TChild>, TChild, OneToManyMapping>
    {
        private readonly Type entity;
        private readonly CollectionCascadeExpression<OneToManyPart<TChild>> cascade;
        private readonly NotFoundExpression<OneToManyPart<TChild>> notFound;
        private IndexManyToManyPart manyToManyIndex;
        private readonly Type childType;
        private Type valueType;
        private bool isTernary;

        public OneToManyPart(Type entity, Member property)
            : this(entity, property, property.PropertyType)
        {
        }

        protected OneToManyPart(Type entity, Member member, Type collectionType)
            : base(entity, member, collectionType)
        {
            this.entity = entity;
            childType = collectionType;

            cascade = new CollectionCascadeExpression<OneToManyPart<TChild>>(this, value => collectionAttributes.Set(x => x.Cascade, value));
            notFound = new NotFoundExpression<OneToManyPart<TChild>>(this, value => relationshipMapping.NotFound = value);

            collectionAttributes.SetDefault(x => x.Name, member.Name);

            relationshipMapping = new OneToManyMapping
            {
                ContainingEntityType = entity
            };
        }

        /// <summary>
        /// Specifies the behaviour for if this collection is not found
        /// </summary>
        public NotFoundExpression<OneToManyPart<TChild>> NotFound
        {
            get { return notFound; }
        }

        /// <summary>
        /// Specify the cascade behaviour
        /// </summary>
        public new CollectionCascadeExpression<OneToManyPart<TChild>> Cascade
        {
            get { return cascade; }
        }

        /// <summary>
        /// Specify that this is a ternary association
        /// </summary>
        public OneToManyPart<TChild> AsTernaryAssociation()
        {
            var keyType = childType.GetGenericArguments()[0];
            return AsTernaryAssociation(keyType.Name + "_id");
        }

        /// <summary>
        /// Specify that this is a ternary association
        /// </summary>
        /// <param name="indexColumnName">Index column</param>
        public OneToManyPart<TChild> AsTernaryAssociation(string indexColumnName)
        {
            EnsureGenericDictionary();

            var keyType = childType.GetGenericArguments()[0];
            var valType = childType.GetGenericArguments()[1];

            manyToManyIndex = new IndexManyToManyPart(typeof(ManyToManyPart<TChild>));
            manyToManyIndex.Column(indexColumnName);
            manyToManyIndex.Type(keyType);

            valueType = valType;
            isTernary = true;

            return this;
        }

        /// <summary>
        /// Specify this as an entity map
        /// </summary>
        public OneToManyPart<TChild> AsEntityMap()
        {
            // The argument to AsMap will be ignored as the ternary association will overwrite the index mapping for the map.
            // Therefore just pass null.
            return AsMap(null).AsTernaryAssociation();
        }

        /// <summary>
        /// Specify this as an entity map
        /// </summary>
        /// <param name="indexColumnName">Index column</param>
        public OneToManyPart<TChild> AsEntityMap(string indexColumnName)
        {
            return AsMap(null).AsTernaryAssociation(indexColumnName);
        }

        /// <summary>
        /// Specify the key column name
        /// </summary>
        /// <param name="columnName">Column name</param>
        public OneToManyPart<TChild> KeyColumn(string columnName)
        {
            Key(ke =>
            {
                ke.Columns.Clear();
                ke.Columns.Add(columnName);
            });
            return this;
        }

        /// <summary>
        /// Modify the key columns collection
        /// </summary>
        [Obsolete("Deprecated in favour of Key(ke => ke.Columns...)")]
        public ColumnMappingCollection<OneToManyPart<TChild>> KeyColumns
        {
            get { return new ColumnMappingCollection<OneToManyPart<TChild>>(this, new KeyBuilder(keyMapping).Columns); }
        }

        /// <summary>
        /// Specify a foreign key constraint
        /// </summary>
        /// <param name="foreignKeyName">Constraint name</param>
        public OneToManyPart<TChild> ForeignKeyConstraintName(string foreignKeyName)
        {
            return Key(ke => ke.ForeignKey(foreignKeyName));
        }

        /// <summary>
        /// Sets the order-by clause for this one-to-many relationship.
        /// </summary>
        public OneToManyPart<TChild> OrderBy(string orderBy)
        {
            collectionAttributes.Set(x => x.OrderBy, orderBy);
            return this;
        }

        /// <summary>
        /// Specify that this collection is read-only
        /// </summary>
        public OneToManyPart<TChild> ReadOnly()
        {
            collectionAttributes.Set(x => x.Mutable, !nextBool);
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify a sub-select query for fetching this collection
        /// </summary>
        /// <param name="subselect">Query</param>
        public OneToManyPart<TChild> Subselect(string subselect)
        {
            collectionAttributes.Set(x => x.Subselect, subselect);
            return this;
        }

        /// <summary>
        /// Specify that the key is updatable
        /// </summary>
        [Obsolete("Deprecated in favour of Key(ke => ke.Update())")]
        public OneToManyPart<TChild> KeyUpdate()
        {
            Key(ke =>
            {
                if (nextBool)
                    ke.Update();
                else
                    ke.Not.Update();
            });
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify that the key is nullable
        /// </summary>
        [Obsolete("Deprecated in favour of Key(ke => ke.Nullable())")]
        public OneToManyPart<TChild> KeyNullable()
        {
            Key(ke =>
            {
                if (nextBool)
                    ke.Nullable();
                else
                    ke.Not.Nullable();
            });
            nextBool = true;
            return this;
        }

        protected override ICollectionMapping GetCollectionMapping()
        {
            var collection = base.GetCollectionMapping();

            // HACK: shouldn't have to do this!
            if (manyToManyIndex != null && collection is MapMapping)
                ((MapMapping)collection).Index = manyToManyIndex.GetIndexMapping();

            return collection;
        }

        protected override ICollectionRelationshipMapping GetRelationship()
        {
            if (isTernary && valueType != null)
                relationshipMapping.Class = new TypeReference(valueType);

            return relationshipMapping;
        }

        void EnsureGenericDictionary()
        {
            if (!(childType.IsGenericType && childType.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                throw new ArgumentException(member.Name + " must be of type IDictionary<> to be used in a ternary assocation. Type was: " + childType);
        }
    }
}
