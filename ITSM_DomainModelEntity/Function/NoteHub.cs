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

        public async Task UpdateUnreadCount(string userId, int count)
        {
            await Clients.User(userId).SendAsync("ReceiveUnreadCount", count);
        }

        public async Task MarkNoteAsRead(string noteId, string userId)
        {
            await Clients.All.SendAsync("NoteMarkedAsRead", noteId, userId);
        }
    }
}
