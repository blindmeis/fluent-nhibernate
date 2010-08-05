using System;

namespace FluentNHibernate.Diagnostics
{
    public class DiagnosticsConfiguration
    {
        readonly IDiagnosticMessageDespatcher despatcher;
        readonly Action<IDiagnosticLogger> setLogger;

        public DiagnosticsConfiguration(IDiagnosticMessageDespatcher despatcher, Action<IDiagnosticLogger> setLogger)
        {
            this.despatcher = despatcher;
            this.setLogger = setLogger;
        }

        public DiagnosticsConfiguration Enable()
        {
            setLogger(new DefaultDiagnosticsLogger());
            return this;
        }

        public DiagnosticsConfiguration Disable()
        {
            setLogger(new NullDiagnosticsLogger());
            return this;
        }

        public DiagnosticsConfiguration RegisterListener(IDiagnosticListener listener)
        {
            despatcher.RegisterListener(listener);
            return this;
        }

        public DiagnosticsConfiguration OutputToConsole()
        {
            RegisterListener(new ConsoleOutputListener());
            return this;
        }
    }
}