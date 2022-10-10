using LazyCache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRExampleRazorPages.Service;

namespace SignalRExampleRazorPages.Hub
{
    public class CamaraHub:Microsoft.AspNetCore.SignalR.Hub
    {
        private  readonly IConnectionService _connectionService;
        public CamaraHub(IConnectionService connectionService)
        {
            _connectionService = connectionService;
        }

        public async Task SendCamaras(List<CamaraModel> camaras)
        {
            _connectionService.AddCamaras(Context.ConnectionId,camaras);
            await Clients.All.SendAsync("ReceiveCamaras", _connectionService.GetCamaras());
        }
        [HubMethodName("PostData")]
        public async Task PostData(CaramaraStream stream)
        {
            await Clients.All.SendAsync("CamaraRecived", stream);
            //await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

      
        public override Task OnConnectedAsync()
        {
            _connectionService.StartConnection(Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionService.RemoveConnection(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
