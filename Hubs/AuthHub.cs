using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAuthAPI.Hubs
{
    public class User
    {
        public string Name { get; set; }
        public HashSet<string> ConnectionIds { get; set; }
    }

    [Authorize]
    public class AuthHub : Hub<IAuth>
    {
        private static readonly ConcurrentDictionary<string, List<User>> ActiveUsersDic = new ConcurrentDictionary<string, List<User>>(StringComparer.InvariantCultureIgnoreCase);
        public IEnumerable<string> GetConnectedUsers(string dbID)
        {
            if (!ActiveUsersDic.ContainsKey(dbID))
                return Enumerable.Empty<string>();

            return ActiveUsersDic[dbID].Where(x => {
                lock (x.ConnectionIds)
                {
                    return !x.ConnectionIds.Contains
                            (Context.ConnectionId, StringComparer.InvariantCultureIgnoreCase);
                }

            }).Select(x => x.Name);
        }

        public override Task OnConnectedAsync()
        {
            string dbID = Context.User.Identities.First().Claims.FirstOrDefault(x => x.Type == "dbID").Value;
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            var userList = ActiveUsersDic.GetOrAdd(dbID, _ => new List<User>()
            {
                new User()
                {
                    Name = userName,
                    ConnectionIds = new HashSet<string>()
                }
            });
            var user = userList.Find(obj => obj.Name == userName);
            if (user != null)
            {
                lock (user.ConnectionIds)
                {
                    user.ConnectionIds.Add(connectionId);
                }
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string dbID = Context.User.Identities.First().Claims.FirstOrDefault(x => x.Type == "dbID").Value;
            string userName = Context.User.Identity.Name;
            string connectionId = Context.ConnectionId;

            ActiveUsersDic.TryGetValue(dbID, out List<User> ActiveUsers);
            if (ActiveUsers != null)
            {
                var user = ActiveUsers.Find(user => user.Name == userName);
                if (user != null)
                {
                    lock (user.ConnectionIds)
                    {
                        user.ConnectionIds.RemoveWhere(cid => cid.Equals(connectionId));

                        if (!user.ConnectionIds.Any())
                        {
                            ActiveUsersDic[dbID].Remove(user);
                        }
                    }
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public void ForceLogout(string dbID, string email)
        {
            string connectionID = this.forceLogout(dbID, email);
            if (connectionID != null)
            {
                Clients.Client(connectionID).ForceLogoutEx();
            }
        }

        private string forceLogout(string dbID, string email)
        {
            if (ActiveUsersDic.ContainsKey(dbID))
            {
                User receiver = this.getUser(dbID, email);
                if (receiver != null)
                {
                    if (receiver.ConnectionIds.Count > 1)
                    {
                        string connectionId = Context.ConnectionId;
                        if (receiver.ConnectionIds.Contains(connectionId))
                        {
                            if (receiver.ConnectionIds.First() != connectionId)
                            {
                                return receiver.ConnectionIds.First();
                            }
                        }
                    }
                }
            }
            return null;
        }

        private User getUser(string dbID, string userName)
        {
            User user = null;
            if (ActiveUsersDic.ContainsKey(dbID))
                user = ActiveUsersDic[dbID].Find(obj => obj.Name == userName);
            return user;
        }
    }
}
