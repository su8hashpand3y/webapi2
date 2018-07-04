using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.Models
{
    public class ReplyMessageInfo
    {
        public long Id { get; set; }
        public Guid MessageGroupUniqueGuid { get; set; }
        public string UserUniqueId { get; set; }
        public bool IsFav { get; set; }
        public bool IsDeleted { get; set; }
    }
}
