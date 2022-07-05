using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eauction.SignalR
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(int highestBidPrice, string time, string bidder)
        {
            await Clients.All.SendAsync("ReceiveMessage", highestBidPrice, time, bidder);
        }
    }
}
