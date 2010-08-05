using System;

namespace FluentNHibernate.Diagnostics
{
    public class StringLambdaOutputListener : IDiagnosticListener
    {
        readonly Action<string> receiveMessage;

        public StringLambdaOutputListener(Action<string> receiveMessage)
        {
            this.receiveMessage = receiveMessage;
        }

        public void Receive(DiagnosticResults results)
        {
            
        }
    }
}