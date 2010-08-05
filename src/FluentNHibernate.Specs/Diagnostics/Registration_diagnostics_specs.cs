using System;
using FluentNHibernate.Diagnostics;
using FluentNHibernate.Mapping;
using FluentNHibernate.Specs.Automapping.Fixtures;
using Machine.Specifications;

namespace FluentNHibernate.Specs.Diagnostics
{
    public class when_registering_types_with_diagnostics_enabled
    {
        Establish context = () =>
        {
            model = new FluentNHibernate.PersistenceModel();
            model.Diagnostics
                .Enable()
                .RegisterListener(new StubListener(x => results = x));
        };

        Because of = () =>
        {
            model.AddMappingsFromSource(new StubTypeSource(typeof(First), typeof(FirstMap), typeof(SecondMap), typeof(ThirdMap), typeof(CompMap)));
            model.BuildMappings();
        };

        It should_produce_results_when_enabled = () =>
            results.ShouldNotBeNull();

        It should_register_each_ClassMap_type_and_return_them_in_the_results = () =>
            results.FluentMappings.ShouldContain(typeof(FirstMap), typeof(SecondMap));

        It should_register_each_SubclassMap_type_and_return_them_in_the_results = () =>
            results.FluentMappings.ShouldContain(typeof(ThirdMap));

        It should_register_each_ComponentMap_type_and_return_them_in_the_results = () =>
            results.FluentMappings.ShouldContain(typeof(CompMap));

        It should_not_register_non_fluent_mapping_types = () =>
            results.FluentMappings.ShouldNotContain(typeof(First));
        
        static FluentNHibernate.PersistenceModel model;
        static DiagnosticResults results;
    }

    class FirstMap : ClassMap<First>
    {
        public FirstMap()
        {
            Id(x => x.Id);
        }
    }
    class First
    {
        public int Id { get; set; }
    }

    class SecondMap : ClassMap<Second>
    {
        public SecondMap()
        {
            Id(x => x.Id);
        }
    }

    class Second
    {
        public int Id { get; set; }
    }

    class ThirdMap : SubclassMap<Third> {}

    class Third : Second {}

    class CompMap : ComponentMap<Comp> {}
    class Comp {}

    class StubListener : IDiagnosticListener
    {
        readonly Action<DiagnosticResults> receiver;

        public StubListener(Action<DiagnosticResults> receiver)
        {
            this.receiver = receiver;
        }

        public void Receive(DiagnosticResults results)
        {
            receiver(results);
        }
    }
}
