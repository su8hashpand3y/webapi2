using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MinLength(4,ErrorMessage ="Hey,UserId must have alteast 4 letters")]
        public string UserUniqueId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        public string Photo { get; set; }
        [Required]
        public string FavNumber { get; set; }
        [Required]
        public string FavColor { get; set; }
        [Required]
        public string FavMonth { get; set; }
        public string Ext { get; set; }
    }
}
