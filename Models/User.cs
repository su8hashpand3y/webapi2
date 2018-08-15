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
        public string FavColor { get; set; }
        public string FavMonth { get; set; }
        public string FavNumber { get; set; }
    }
}
