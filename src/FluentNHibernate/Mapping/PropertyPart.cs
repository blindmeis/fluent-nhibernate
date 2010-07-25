using System;
using System.Diagnostics;
using System.Linq;
using FluentNHibernate.Mapping.Providers;
using FluentNHibernate.MappingModel;
using FluentNHibernate.Utils;
using NHibernate.UserTypes;

namespace FluentNHibernate.Mapping
{
    public class PropertyPart : IPropertyMappingProvider
    {
        private readonly Member property;
        private readonly Type parentType;
        private readonly AccessStrategyBuilder<PropertyPart> access;
        private readonly PropertyGeneratedBuilder generated;
        private readonly AttributeStore<ColumnMapping> columnAttributes = new AttributeStore<ColumnMapping>();

        private bool nextBool = true;
        readonly PropertyMapping mapping;

        public PropertyPart(Member property, Type parentType)
        {
            access = new AccessStrategyBuilder<PropertyPart>(this, value => mapping.Access = value);
            generated = new PropertyGeneratedBuilder(this, value => mapping.Generated = value);

            this.property = property;
            this.parentType = parentType;

            mapping = new PropertyMapping
            {
                ContainingEntityType = parentType,
                Member = property
            };
            mapping.AddDefaultColumn(new ColumnMapping { Name = property.Name });
        }

        /// <summary>
        /// Specify if this property is database generated
        /// </summary>
        /// <example>
        /// Generated.Insert();
        /// </example>
        public PropertyGeneratedBuilder Generated
        {
            get { return generated; }
        }

        /// <summary>
        /// Specify the property column name
        /// </summary>
        /// <param name="columnName">Column name</param>
        public PropertyPart Column(string columnName)
        {
            Columns.Clear();
            Columns.Add(columnName);
            return this;
        }

        /// <summary>
        /// Modify the columns collection
        /// </summary>
        public ColumnMappingCollection<PropertyPart> Columns
        {
            get { return new ColumnMappingCollection<PropertyPart>(this, mapping); }
        }

        /// <summary>
        /// Set the access and naming strategy for this property.
        /// </summary>
        public AccessStrategyBuilder<PropertyPart> Access
        {
            get { return access; }
        }

        /// <summary>
        /// Specify that this property is insertable
        /// </summary>
        public PropertyPart Insert()
        {
            mapping.Insert = nextBool;
            nextBool = true;

            return this;
        }

        /// <summary>
        /// Specify that this property is updatable
        /// </summary>
        public PropertyPart Update()
        {
            mapping.Update = nextBool;
            nextBool = true;

            return this;
        }

        /// <summary>
        /// Specify the column length
        /// </summary>
        /// <param name="length">Column length</param>
        public PropertyPart Length(int length)
        {
            columnAttributes.Set(x => x.Length, length);
            return this;
        }

        /// <summary>
        /// Specify the nullability of this property
        /// </summary>
        public PropertyPart Nullable()
        {
            columnAttributes.Set(x => x.NotNull, !nextBool);
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify that this property is read-only
        /// </summary>
        public PropertyPart ReadOnly()
        {
            mapping.Insert = !nextBool;
            mapping.Update = !nextBool;
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify the property formula
        /// </summary>
        /// <param name="formula">Formula</param>
        public PropertyPart Formula(string formula)
        {
            mapping.Formula = formula;
            return this;
        }

        /// <summary>
        /// Specify the lazy-loading behaviour
        /// </summary>
        public PropertyPart LazyLoad()
        {
            mapping.Lazy = nextBool;
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify an index name
        /// </summary>
        /// <param name="index">Index name</param>
        public PropertyPart Index(string index)
        {
            columnAttributes.Set(x => x.Index, index);
            return this;
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <typeparam name="TCustomtype">A type which implements <see cref="IUserType"/>.</typeparam>
        /// <returns>This property mapping to continue the method chain</returns>
        public PropertyPart CustomType<TCustomtype>()
        {
            return CustomType(typeof(TCustomtype));
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <param name="type">A type which implements <see cref="IUserType"/>.</param>
        /// <returns>This property mapping to continue the method chain</returns>
        public PropertyPart CustomType(Type type)
        {
            if (typeof(ICompositeUserType).IsAssignableFrom(type))
                AddColumnsFromCompositeUserType(type);

            return CustomType(TypeMapping.GetTypeString(type));
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <param name="type">A type which implements <see cref="IUserType"/>.</param>
        /// <returns>This property mapping to continue the method chain</returns>
        public PropertyPart CustomType(string type)
        {
            mapping.Type = new TypeReference(type);
            return this;
        }

        /// <summary>
        /// Specifies that a custom type (an implementation of <see cref="IUserType"/>) should be used for this property for mapping it to/from one or more database columns whose format or type doesn't match this .NET property.
        /// </summary>
        /// <param name="typeFunc">A function which returns a type which implements <see cref="IUserType"/>. The argument of the function is the mapped property type</param>
        /// <returns>This property mapping to continue the method chain</returns>
        public PropertyPart CustomType(Func<Type, Type> typeFunc)
        {
            var type = typeFunc.Invoke(this.property.PropertyType);

            if (typeof(ICompositeUserType).IsAssignableFrom(type))
                AddColumnsFromCompositeUserType(type);

            return CustomType(TypeMapping.GetTypeString(type));
        }

        private void AddColumnsFromCompositeUserType(Type compositeUserType)
        {
            var inst = (ICompositeUserType)Activator.CreateInstance(compositeUserType);

            foreach (var name in inst.PropertyNames)
            {
                Columns.Add(name);
            }
        }

        /// <summary>
        /// Specify a custom SQL type
        /// </summary>
        /// <param name="sqlType">SQL type</param>
        public PropertyPart CustomSqlType(string sqlType)
        {
            columnAttributes.Set(x => x.SqlType, sqlType);
            return this;
        }

        /// <summary>
        /// Specify that this property has a unique constranit
        /// </summary>
        public PropertyPart Unique()
        {
            columnAttributes.Set(x => x.Unique, nextBool);
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Specify decimal precision
        /// </summary>
        /// <param name="precision">Decimal precision</param>
        public PropertyPart Precision(int precision)
        {
            columnAttributes.Set(x => x.Precision, precision);
            return this;
        }

        /// <summary>
        /// Specify decimal scale
        /// </summary>
        /// <param name="scale">Decimal scale</param>
        public PropertyPart Scale(int scale)
        {
            columnAttributes.Set(x => x.Scale, scale);
            return this;
        }

        /// <summary>
        /// Specify a default value
        /// </summary>
        /// <param name="value">Default value</param>
        public PropertyPart Default(string value)
        {
            columnAttributes.Set(x => x.Default, value);
            return this;
        }

        /// <summary>
        /// Specifies the name of a multi-column unique constraint.
        /// </summary>
        /// <param name="keyName">Name of constraint</param>
        public PropertyPart UniqueKey(string keyName)
        {
            columnAttributes.Set(x => x.UniqueKey, keyName);
            return this;
        }

        /// <summary>
        /// Specify that this property is optimistically locked
        /// </summary>
        public PropertyPart OptimisticLock()
        {
            mapping.OptimisticLock = nextBool;
            nextBool = true;
            return this;
        }

        /// <summary>
        /// Inverts the next boolean
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public PropertyPart Not
        {
            get
            {
                nextBool = !nextBool;
                return this;
            }
        }

        /// <summary>
        /// Specify a check constraint
        /// </summary>
        /// <param name="constraint">Constraint name</param>
        public PropertyPart Check(string constraint)
        {
            columnAttributes.Set(x => x.Check, constraint);
            return this;
        }

        PropertyMapping IPropertyMappingProvider.GetPropertyMapping()
        {
            if (mapping.IsSpecified("Formula"))
                mapping.ClearColumns();

            foreach (var column in mapping.Columns)
            {
                if (!column.IsSpecified("NotNull") && property.PropertyType.IsNullable() && property.PropertyType.IsEnum())
                    column.SetDefaultValue(x => x.NotNull, false);

                column.MergeAttributes(columnAttributes);
            }

            if (!mapping.IsSpecified("Name"))
                mapping.Name = mapping.Member.Name;

            if (!mapping.IsSpecified("Type"))
                mapping.SetDefaultValue("Type", GetDefaultType());

            return mapping;
        }

        TypeReference GetDefaultType()
        {
            var type = new TypeReference(property.PropertyType);

            if (property.PropertyType.IsEnum())
                type = new TypeReference(typeof(GenericEnumMapper<>).MakeGenericType(property.PropertyType));

            if (property.PropertyType.IsNullable() && property.PropertyType.IsEnum())
                type = new TypeReference(typeof(GenericEnumMapper<>).MakeGenericType(property.PropertyType.GetGenericArguments()[0]));

            return type;
        }
    }
}
