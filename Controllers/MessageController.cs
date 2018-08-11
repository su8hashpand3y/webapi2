using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApi1.Models;
using WebApi1.ViewModels;
using WebApi1.Helpers;

namespace WebApi1.Controllers
{
    public class MessageController : Controller
    {
        private IConfiguration Configuration { get; }
        private IServiceProvider services { get; }

        public MessageController(IServiceProvider services,IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.services = services;
        }

        public ServiceResponse<string> SendMessage(string userUniqueId,string message,string messageGroupUniqueId,string yourName)
        {
            try
            {
                var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                var user = context.User.FirstOrDefault(x => x.UserUniqueId == userUniqueId);
                if(user == null)
                {
                    return new ServiceResponse<string> { Status="bad",Message="No Such User Exists,Probably Wrong link to send message!"};
                }

                if(String.IsNullOrEmpty(HttpContext.GetUserUniqueID()) && user.AnonymousNotAllowed)
                {
                    return new ServiceResponse<string> { Status = "bad", Message = $"Login first, anonymous user cant message to {userUniqueId} account" };
                }

                Inbox msg = new Inbox(); 
                if (string.IsNullOrEmpty(messageGroupUniqueId))
                {
                    msg.MessageGroupUniqueGuid = Guid.NewGuid();
                    UserMessage um = new UserMessage { UserUniqueId = userUniqueId, MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid };
                    context.Add(um);
                }
                else
                {
                    msg.MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId);
                }

                msg.UserUniqueId = userUniqueId;
                msg.Message = message;
                Random rand = new Random();
                if (string.IsNullOrEmpty(messageGroupUniqueId))
                {
                    msg.UserIdentifier = yourName ?? $"Anonyms {rand.Next(100, 500)}";
                }

                context.Inbox.Add(msg);



                Reply rep = new Reply
                {
                    MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid,
                    Message = message,
                    IsMyMessage = true
                };

                context.Reply.Add(rep);

                context.SaveChanges();

                return new ServiceResponse<string> { Status = "good", Data = msg.MessageGroupUniqueGuid.ToString() };

            }
            catch (Exception e)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "Something went wrong" };
            }
        }

        public ServiceResponse<string> ReplyMessage(string message, string messageGroupUniqueId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userMessage = context.UserMessage.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
            if (userMessage == null)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "No Such Message Group Exists!" };
            }
            if(userMessage.IsDeleted)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "Message Group deleted No More Replies Allowed!" };
            }


            // reply Message
            Inbox msg = new Inbox
            {
                MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId),
                Message = message,
                IsMyMessage = true,
                UserUniqueId = HttpContext.GetUserUniqueID()
            };

            context.Inbox.Add(msg);

            Reply rep = new Reply
            {
                MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId),
                Message = message,
            };

            context.Reply.Add(rep);

            context.SaveChanges();

            return new ServiceResponse<string>() { Status="good",Data= messageGroupUniqueId };
        }

        public ServiceResponse<List<MessageCard>> GetReplyMessageCard(int skip)
        {
            List<MessageCard> replyCards = new List<MessageCard>();

            try
            {
                var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                var userUniqueId = HttpContext.GetUserUniqueID();
                if (String.IsNullOrEmpty(userUniqueId))
                {
                    return new ServiceResponse<List<MessageCard>> { Status = "bad", Message = "You Are not logged in!" };
                }

                var user = context.User.SingleOrDefault(x => x.UserUniqueId == userUniqueId);
                if (user == null)
                {
                    return new ServiceResponse<List<MessageCard>> { Status = "bad", Message = "The logged in user is not a valid user login again Please!" };
                }



                var msgs = context.Inbox.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted).Skip(skip).GroupBy(x => x.MessageGroupUniqueGuid);

                foreach (var msg in msgs)
                {
                    bool isFav = context.UserMessage.First(x => x.UserUniqueId == userUniqueId && x.MessageGroupUniqueGuid == msg.Key).IsFav;
                    replyCards.Add(new MessageCard { UserName = "Anonymous", MessageGroupUniqueGuid = msg.Key.ToString(), UnreadCount = msg.Count(y => y.IsRead == false), LastMessage = msg.Last().Message, IsFav = isFav });
                }
            }
            catch  { }

            return new ServiceResponse<List<MessageCard>> { Status = "good", Data = replyCards };

        }

        public ServiceResponse<List<MessageCard>> GetInboxMessagesCard(int skip)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<List<MessageCard>> { Status = "bad", Message = "You Are not logged in!" };
            }

            var user = context.User.SingleOrDefault(x => x.UserUniqueId == userUniqueId);
            if (user == null)
            {
                return new ServiceResponse<List<MessageCard>> { Status = "bad", Message = "The logged in user is not a valid user login again Please!" };
            }

            List<MessageCard> inboxCards = new List<MessageCard>();


            var msgs = context.Inbox.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted).Skip(skip).GroupBy(x => x.MessageGroupUniqueGuid);
          
            foreach (var msg in msgs)
            {
                bool isFav = context.UserMessage.First(x => x.UserUniqueId == userUniqueId && x.MessageGroupUniqueGuid == msg.Key).IsFav;
                inboxCards.Add(new MessageCard { UserName = "Anonymous", MessageGroupUniqueGuid = msg.Key.ToString(),  UnreadCount = msg.Count(y => y.IsRead == false), LastMessage = msg.Last().Message ,IsFav =  isFav} );
            }

            return new ServiceResponse<List<MessageCard>> { Status = "good", Data = inboxCards };
        }

        public ServiceResponse<List<InboxViewModel>> GetInboxMessage(string messageGroupUniqueId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<List<InboxViewModel>> { Status = "bad", Message = "You Are not logged in!" };
            }

            var msgs = context.Inbox.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted && x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
            var inboxMessage = new List<InboxViewModel>();
            foreach (var msg in msgs)
            {
                msg.IsRead = true;
                inboxMessage.Add(new InboxViewModel { DateTime = msg.DateTime, IsMyMessage = msg.IsMyMessage, Message = msg.Message });
            }

            context.SaveChanges();
            return new ServiceResponse<List<InboxViewModel>> { Status = "good", Data = inboxMessage };
        }

        public ServiceResponse<List<ReplyViewModel>> GetReplyMessage(string messageGroupUniqueId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var msgs = context.Reply.Where(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
            var replyMessages = new List<ReplyViewModel>();
            foreach (var msg in msgs)
            {
                msg.IsRead = true;
                replyMessages.Add(new ReplyViewModel { DateTime = msg.DateTime, IsMyMessage = msg.IsMyMessage, Message = msg.Message });

            }

            context.SaveChanges();
            return new ServiceResponse<List<ReplyViewModel>> { Status = "good", Data = replyMessages };
        }



        public ServiceResponse<string> Delete(string messageGroupUniqueId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userMessage = context.UserMessage.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
            if (userMessage == null)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "No Such Message Group Exists!" };
            }

            if(HttpContext.GetUserUniqueID()== userMessage.UserUniqueId)
            {
                userMessage.IsDeleted = true;
                context.SaveChanges();
                return new ServiceResponse<string> { Status = "good", Data = messageGroupUniqueId };
            }
            else
            {
                return new ServiceResponse<string> { Status = "bad", Message = "You Cant perform this action." };
            }
        }


        public ServiceResponse<string> MarkInboxAsFav(string messageGroupUniqueId)
        {
            string userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<string> { Status = "bad", Message = "User not logged in" };

            }

            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var msg = context.UserMessage.SingleOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId && x.UserUniqueId == userUniqueId);
            if (msg != null)
            {
                msg.IsFav = true;
                context.SaveChanges();
            }
            return new ServiceResponse<string> { Status = "good" };
        }

        public ServiceResponse<string> MarkReplyAsFav(string messageGroupUniqueId)
        {
            string userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<string> { Status = "bad",Message="User not logged in" };

            }

            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var msg = context.ReplyMessageInfo.SingleOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId && x.UserUniqueId == userUniqueId);
            if (msg != null)
            {
                msg.IsFav = true;
                context.SaveChanges();
            }
            return new ServiceResponse<string> { Status = "good" };
        }

        public ServiceResponse<string> AddToReplyInfo(string messageGroupUniqueId,bool isFav)
        {
            string userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<string> { Status = "bad", Message = "User not logged in" };

            }

            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            ReplyMessageInfo info = new ReplyMessageInfo { MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId), UserUniqueId = userUniqueId, IsFav = isFav };
            context.ReplyMessageInfo.Add(info);
            context.SaveChanges();
            return new ServiceResponse<string> { Status = "good" };
        }


        //public ServiceResponse<string> MarkInboxAsRead(long Id)
        //{
        //    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
        //    var msg = context.Inbox.SingleOrDefault(x => x.Id == Id);
        //    if(msg != null)
        //    {
        //        msg.IsRead = true;
        //        context.SaveChanges();
        //    }
        //    return new ServiceResponse<string> { Status = "good" };
        //}

        //public ServiceResponse<string> MarkReplyAsRead(long Id)
        //{
        //    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
        //    var msg = context.Reply.SingleOrDefault(x => x.Id == Id);
        //    if (msg != null)
        //    {
        //        msg.IsRead = true;
        //        context.SaveChanges();
        //    }
        //    return new ServiceResponse<string> { Status = "good" };
        //}
    }
}
