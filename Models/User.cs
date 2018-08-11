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
        public bool AnonymousNotAllowed { get; set; }
        public bool PublicSearchNotAvailable { get; set; }
        public string Ans1 { get; set; }
        public string Ans2 { get; set; }
        public string Ans3 { get; set; }
    }
}
