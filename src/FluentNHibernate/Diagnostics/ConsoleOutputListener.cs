using System;

namespace FluentNHibernate.Diagnostics
{
    public class ConsoleOutputListener : StringLambdaOutputListener
    {
        public ConsoleOutputListener()
            : base(Console.WriteLine)
        {}
    }
}