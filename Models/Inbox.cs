using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.Models
{
    public class Inbox
    {
        public Inbox()
        {
            DateTime = DateTime.Now;
        }

        public long Id { get; set; }
        public Guid MessageGroupUniqueGuid { get; set; }

        public string Message { get; set; }
        public string UserUniqueId { get; set; }
        public string UserIdentifier { get; set; } 
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
        public bool IsMyMessage { get; set; }

        public DateTime DateTime { get; set; }
    }
}
