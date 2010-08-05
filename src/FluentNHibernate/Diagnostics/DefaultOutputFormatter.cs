using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentNHibernate.Diagnostics
{
    public class DefaultOutputFormatter : IDiagnosticResultsFormatter
    {
        public string Format(DiagnosticResults results)
        {
            var sb = new StringBuilder();

            Title(sb, "Fluent Mappings");
            sb.AppendLine();
            sb.AppendLine("Types discovered:");
            sb.AppendLine();

            var fluentMappings = results.FluentMappings
                .OrderBy(x => x.Name)
                .ToArray();

            Table(sb,
                fluentMappings.Select(x => x.Name),
                fluentMappings.Select(x => x.AssemblyQualifiedName));

            return sb.ToString();
        }

        void Table(StringBuilder sb, params IEnumerable<string>[] columns)
        {
            var columnWidths = columns
                .Select(x => x.Max(val => val.Length))
                .ToArray();
            var rowCount = columns.First().Count();

            for (var row = 0; row < rowCount; row++)
            {
                sb.Append("  ");
                for (var i = 0; i < columns.Length; i++)
                {
                    var column = columns[i];
                    var width = columnWidths[i];
                    var value = column.ElementAt(row);

                    sb.Append(value.PadRight(width));
                    sb.Append(" | ");
                }

                sb.Length -= 3; // remove last separator
                sb.AppendLine();
            }
        }

        void Title(StringBuilder sb, string title)
        {
            sb.AppendLine(title);
            sb.AppendLine("".PadLeft(title.Length, '-'));
        }
    }
}