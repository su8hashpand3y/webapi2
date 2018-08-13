using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.ViewModels
{
    public class MessageCard
    {
        public long LastId { get; set; }
        public int UnreadCount { get; set; }
        public string LastMessage { get; set; }
        public string UserName { get; set; }
        public bool IsFav { get; set; }

        public string MessageGroupUniqueGuid { get; set; }

    }
}
