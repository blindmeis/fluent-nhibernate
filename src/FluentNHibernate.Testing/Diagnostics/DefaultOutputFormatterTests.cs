using FluentNHibernate.Diagnostics;
using NUnit.Framework;

namespace FluentNHibernate.Testing.Diagnostics
{
    [TestFixture]
    public class DefaultOutputFormatterTests
    {
        [Test]
        public void should_produce_simple_format()
        {
            var formatter = new DefaultOutputFormatter();
            var output = formatter.Format(new DiagnosticResults(new[] {typeof(Two),typeof(One)}));

            output.ShouldEqual(
                "Fluent Mappings\r\n" +
                "---------------\r\n" +
                "Types discovered:\r\n" +
                typeof(One).Name + " | " + typeof(One).AssemblyQualifiedName + "\r\n" +
                typeof(Two).Name + " | " + typeof(Two).AssemblyQualifiedName + "\r\n");
        }

        class One { }
        class Two { }
    }
}