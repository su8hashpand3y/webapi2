using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.Models
{
    public class Reply
    {
        public Reply()
        {
            DateTime = DateTime.Now;
        }
        public long Id { get; set; }
        public Guid MessageGroupUniqueGuid { get; set; }

        public string Message { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
        public bool IsMyMessage { get; set; }
        public DateTime DateTime { get; set; }

    }
}
