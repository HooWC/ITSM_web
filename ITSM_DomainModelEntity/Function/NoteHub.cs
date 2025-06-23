using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Function
{
    public class NoteHub : Hub
    {
        public async Task SendNote(string incidentId, object noteData)
        {
            await Clients.All.SendAsync("ReceiveNote", incidentId, noteData);
        }

        public async Task JoinIncidentGroup(string incidentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, incidentId);
        }

        public async Task LeaveIncidentGroup(string incidentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, incidentId);
        }
    }
}
