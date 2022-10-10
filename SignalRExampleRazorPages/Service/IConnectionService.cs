

namespace SignalRExampleRazorPages.Service
{
    public interface IConnectionService
    {
        void StartConnection(string connectionId);
        void RemoveConnection(string connectionId);
        IReadOnlyList<CamaraModel> GetCamaras();
        void AddCamaras(string connectionId, List<CamaraModel> camaras);
    }
}
