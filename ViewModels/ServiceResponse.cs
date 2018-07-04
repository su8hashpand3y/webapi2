using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.ViewModels
{
    public class ServiceResponse<T> 
    {
        public string Status { get; set; } = "bad";
        public string Message { get; set; } = "Something went wrong";
        public T Data { get; set; }
    }
}
