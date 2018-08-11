using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UserUniqueId { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
