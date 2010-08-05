using FluentNHibernate.Diagnostics;
using FluentNHibernate.Mapping;
using FluentNHibernate.Testing.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace FluentNHibernate.Testing.Diagnostics
{
    [TestFixture]
    public class DiagnosticLoggerTests
    {
        [Test]
        public void should_not_flush_messages_to_despatcher_when_no_messages_logged()
        {
            var despatcher = Mock<IDiagnosticMessageDespatcher>.Create();
            var logger = new DefaultDiagnosticLogger(despatcher);

            logger.Flush();

            despatcher.AssertWasNotCalled(x => x.Publish(Arg<DiagnosticResults>.Is.Anything));
        }

        [Test]
        public void should_flush_messages_to_despatcher()
        {
            var despatcher = Mock<IDiagnosticMessageDespatcher>.Create();
            var logger = new DefaultDiagnosticLogger(despatcher);

            logger.FluentMappingDiscovered(typeof(SomeClassMap));
            logger.Flush();

            despatcher.AssertWasCalled(x => x.Publish(Arg<DiagnosticResults>.Is.Anything));
        }

        [Test]
        public void should_add_class_maps_to_result()
        {
            var despatcher = Mock<IDiagnosticMessageDespatcher>.Create();
            var logger = new DefaultDiagnosticLogger(despatcher);

            logger.FluentMappingDiscovered(typeof(SomeClassMap));
            logger.Flush();

            DiagnosticResults result = null;
            despatcher.AssertWasCalled(x => x.Publish(Arg<DiagnosticResults>.Is.Anything),
                c => c.Callback<DiagnosticResults>(x =>
                {
                    result = x;
                    return true;
                }));

            result.FluentMappings.ShouldContain(typeof(SomeClassMap));
        }

        class SomeClassMap : ClassMap<SomeClass> { }
        class SomeClass {}
    }
}
