using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserUniqueId { get; set; }
        public string PhotoUrl { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool AnonymousNotAllowed { get; set; }
        public bool PublicSearchNotAvailable { get; set; }
        public string SecurityQue1 { get; set; }
        public string SecurityAns1 { get; set; }
        public string SecurityQue2 { get; set; }
        public string SecurityAns2 { get; set; }
        public string SecurityQue3 { get; set; }
        public string SecurityAns3 { get; set; }
    }
}
