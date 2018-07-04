using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi1.Models
{
    public class WebApiDBContext : DbContext
    {
        public WebApiDBContext(DbContextOptions<WebApiDBContext> options) : base(options) { }

        public DbSet<Inbox> Inbox { get; set; }
        public DbSet<Reply> Reply { get; set; }
        public DbSet<UserMessage> UserMessage { get; set; }
        public DbSet<User> User { get; set; }
    }
}
