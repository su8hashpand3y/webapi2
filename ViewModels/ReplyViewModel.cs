﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.ViewModels
{
    public class ReplyViewModel
    {
        //public string MessageGroupUniqueGuid { get; set; }
        public long Id { get; set; }
        public string Message { get; set; }
        public bool IsMyMessage { get; set; }
        public DateTime DateTime { get; set; }

        public long LastId { get; set; }
    }
}
