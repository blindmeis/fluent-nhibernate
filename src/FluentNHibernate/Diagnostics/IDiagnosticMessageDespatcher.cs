namespace FluentNHibernate.Diagnostics
{
    public interface IDiagnosticMessageDespatcher
    {
        void RegisterListener(IDiagnosticListener listener);
    }
}