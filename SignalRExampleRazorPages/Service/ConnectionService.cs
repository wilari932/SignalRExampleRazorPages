using System.Collections.Concurrent;


namespace SignalRExampleRazorPages.Service
{
    public class ConnectionService: IConnectionService
    {
       private readonly ConcurrentDictionary<string, Connection> connections = new ConcurrentDictionary<string, Connection>();


       public void AddCamaras(string connectionId,List<CamaraModel> camaras)
       {
           connections.AddOrUpdate(connectionId,(key)=>
           {
               var connection = AddConnectionFactory(connectionId)(connectionId);
               connection.CamaraModels = camaras ?? new List<CamaraModel>();
               return connection;
           }, (key,connection)=>
           {
                UpdateConnectionFactory(connectionId)(connectionId,connection).CamaraModels = camaras ?? new List<CamaraModel>();
                return connection;
           });
        }

       public IReadOnlyList<CamaraModel> GetCamaras()
       {
          return connections.SelectMany(x => x.Value.CamaraModels).ToList();
       }
       public void StartConnection(string connectionId)
       {
           connections.AddOrUpdate(connectionId, AddConnectionFactory(connectionId), UpdateConnectionFactory(connectionId));
       }
       public void RemoveConnection(string connectionId)
       {
           connections.TryRemove(connectionId, out _);
       }

        private static Func<string, Connection,Connection> UpdateConnectionFactory(string connectionId)
       {
           return (key, f) =>
           {
               f ??= new Connection
               {
                   ConnectionId = connectionId,
                   CamaraModels = new List<CamaraModel>()
               };
               return f;
           };
       }
       private static Func<string, Connection> AddConnectionFactory(string connectionId)
       {
           return (connectionId) => UpdateConnectionFactory(connectionId)(connectionId,null);
       }

    }
}
