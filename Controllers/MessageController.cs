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
                bool isReply = false;
                var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                if (!string.IsNullOrEmpty(messageGroupUniqueId) && String.IsNullOrEmpty(userUniqueId))
                {
                    var userMessage = context.UserMessage.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
                    if (userMessage == null)
                    {
                        return new ServiceResponse<string> { Status = "bad", Message = "No Such Message Group Exists!" };
                    }
                    else
                    {
                        userUniqueId = userMessage.UserUniqueId;
                        isReply = true;
                    }
                }


                var user = context.User.FirstOrDefault(x => x.UserUniqueId == userUniqueId);
                if(user == null)
                {
                    return new ServiceResponse<string> { Status="bad",Message="No Such User Exists,Probably Wrong link to send message!"};
                }

                if(String.IsNullOrEmpty(HttpContext.GetUserUniqueID())  && user.AnonymousNotAllowed)
                {
                    return new ServiceResponse<string> { Status = "bad", Message = $"Login first, anonymous user cant message to {userUniqueId} account" };
                }

                Inbox msg = new Inbox(); 
                if (string.IsNullOrEmpty(messageGroupUniqueId))
                {
                    msg.MessageGroupUniqueGuid = Guid.NewGuid();
                    UserMessage um = new UserMessage { UserUniqueId = userUniqueId, MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid };
                    context.UserMessage.Add(um);
                }
                else
                {
                    msg.MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId);
                }

                msg.UserUniqueId = userUniqueId;
                msg.Message = message;
                msg.IsMyMessage = isReply;
                Random rand = new Random();
                if (string.IsNullOrEmpty(yourName) && !isReply)
                {
                    msg.UserIdentifier = yourName ?? $"Anonyms {rand.Next(100, 500)}";
                }

                context.Inbox.Add(msg);


                if (!isReply)
                {
                    Reply rep = new Reply
                    {
                        UserUniqueId = HttpContext.GetUserUniqueID(),
                        MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid,
                        Message = message,
                        IsMyMessage = true
                    };
                    var ur = new UserReply { UserUniqueId = HttpContext.GetUserUniqueID(), MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid };

                    context.UserReply.Add(ur);
                    context.Reply.Add(rep);
                }
                else
                {
                    var userReply = context.UserReply.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
                    if (userReply == null)
                    {
                        return new ServiceResponse<string> { Status = "bad", Message = "No Such Message Group Exists!" };
                    }

                    Reply rep = new Reply
                    {
                        UserUniqueId = userReply.UserUniqueId,
                        MessageGroupUniqueGuid = msg.MessageGroupUniqueGuid,
                        Message = message,
                    };
                    context.Reply.Add(rep);
                }

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

            var userReply = context.UserReply.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId);
            if (userReply == null)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "No Such Message Group Exists!" };
            }

            if (userMessage.IsDeleted)
            {
                return new ServiceResponse<string> { Status = "bad", Message = "Message Group deleted No More Replies Allowed!" };
            }


            // reply Message
            Inbox msg = new Inbox
            {
                MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId),
                Message = message,
                UserUniqueId = userMessage.UserUniqueId
            };

            context.Inbox.Add(msg);

            Reply rep = new Reply
            {
                MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId),
                Message = message,
                IsMyMessage = true,
                UserUniqueId = HttpContext.GetUserUniqueID()
            };

            context.Reply.Add(rep);

            context.SaveChanges();

            return new ServiceResponse<string>() { Status="good",Data= messageGroupUniqueId };
        }

        public ServiceResponse<List<MessageCard>> GetReplyMessageCard(long lastId)
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


                var msgs = context.Reply.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted && x.Id > lastId);

                foreach (var msg in msgs.GroupBy(x => x.MessageGroupUniqueGuid))
                {
                    var userMessage = context.UserMessage.FirstOrDefault(x => x.MessageGroupUniqueGuid.ToString() == msg.Key.ToString());
                    bool isFav = context.UserReply.First(x => x.UserUniqueId == userUniqueId && x.MessageGroupUniqueGuid == msg.Key).IsFav;
                    replyCards.Add(new MessageCard { LastId = msgs.Last().Id,UserName = userMessage?.UserUniqueId, MessageGroupUniqueGuid = msg.Key.ToString(), UnreadCount = msg.Count(y => y.IsRead == false),
                        LastMessage = msg.Any(x => x.IsRead = false) ? msg.First(x=>x.IsRead = false).Message : msg.Last().Message, IsFav = isFav });
                }
            }
            catch(Exception e)
            {
            }

            return new ServiceResponse<List<MessageCard>> { Status = "good", Data = replyCards.OrderBy(x=> !x.IsFav).ThenBy(x => x.UnreadCount).ToList() };

        }

        public ServiceResponse<List<MessageCard>> GetInboxMessagesCard(long lastId)
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


            var msgs = context.Inbox.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted && x.Id > lastId);
          
            foreach (var msg in msgs.GroupBy(x => x.MessageGroupUniqueGuid))
            {
                bool isFav = context.UserMessage.First(x => x.UserUniqueId == userUniqueId && x.MessageGroupUniqueGuid == msg.Key).IsFav;
                inboxCards.Add(new MessageCard { LastId = msgs.Last().Id, UserName = msg.First().UserIdentifier , MessageGroupUniqueGuid = msg.Key.ToString(),  UnreadCount = msg.Count(y => y.IsRead == false),
                    LastMessage = msg.Any(x => x.IsRead = false) ? msg.First(x => x.IsRead = false).Message : msg.Last().Message,
                    IsFav =  isFav} );
            }

            return new ServiceResponse<List<MessageCard>> { Status = "good", Data = inboxCards.OrderBy(x => !x.IsFav).ThenBy(x=>x.UnreadCount).ToList() };
        }

        public ServiceResponse<List<InboxViewModel>> GetInboxMessage(string messageGroupUniqueId,long lastId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<List<InboxViewModel>> { Status = "bad", Message = "You Are not logged in!" };
            }

            var msgs = context.Inbox.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted && x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId && x.Id > lastId).OrderBy(x=>x.Id);
            var inboxMessage = new List<InboxViewModel>();
            foreach (var msg in msgs)
            {
                msg.IsRead = true;
                inboxMessage.Add(new InboxViewModel { DateTime = msg.DateTime, IsMyMessage = msg.IsMyMessage, Message = msg.Message,LastId = msgs.Last().Id});
            }

            context.SaveChanges();
            return new ServiceResponse<List<InboxViewModel>> { Status = "good", Data = inboxMessage };
        }

        public ServiceResponse<List<ReplyViewModel>> GetReplyMessage(string messageGroupUniqueId,long lastId)
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userUniqueId = HttpContext.GetUserUniqueID();
            if (String.IsNullOrEmpty(userUniqueId))
            {
                return new ServiceResponse<List<ReplyViewModel>> { Status = "bad", Message = "You Are not logged in!" };
            }

            var msgs = context.Reply.Where(x => x.UserUniqueId == userUniqueId && !x.IsDeleted && x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId && x.Id > lastId).OrderBy(x => x.Id);
            var replyMessages = new List<ReplyViewModel>();
            foreach (var msg in msgs)
            {
                msg.IsRead = true;
                replyMessages.Add(new ReplyViewModel { DateTime = msg.DateTime, IsMyMessage = msg.IsMyMessage, Message = msg.Message, LastId = msgs.Last().Id });

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
                msg.IsFav = !msg.IsFav;
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
            var msg = context.UserReply.SingleOrDefault(x => x.MessageGroupUniqueGuid.ToString() == messageGroupUniqueId && x.UserUniqueId == userUniqueId);
            if (msg != null)
            {
                msg.IsFav = !msg.IsFav;
                context.SaveChanges();
            }
            return new ServiceResponse<string> { Status = "good" };
        }

        //public ServiceResponse<string> AddToReplyInfo(string messageGroupUniqueId,bool isFav)
        //{
        //    string userUniqueId = HttpContext.GetUserUniqueID();
        //    if (String.IsNullOrEmpty(userUniqueId))
        //    {
        //        return new ServiceResponse<string> { Status = "bad", Message = "User not logged in" };

        //    }

        //    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
        //    ReplyMessageInfo info = new ReplyMessageInfo { MessageGroupUniqueGuid = Guid.Parse(messageGroupUniqueId), UserUniqueId = userUniqueId, IsFav = isFav };
        //    context.ReplyMessageInfo.Add(info);
        //    context.SaveChanges();
        //    return new ServiceResponse<string> { Status = "good" };
        //}


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

        public ServiceResponse<List<Tuple<string, int>>> GetInboxMessageCount()
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userId = HttpContext.GetUserUniqueID();
            var data = context.Inbox.Where(x => x.UserUniqueId == userId && x.IsMyMessage == false && x.IsRead == false).GroupBy(x=>x.MessageGroupUniqueGuid).ToList();
            List<Tuple<string, int>> result = new List<Tuple<string, int>>();
            data.ForEach(x => result.Add(new Tuple<string, int>(x.Key.ToString(), x.Count())));
            return new ServiceResponse<List<Tuple<string, int>>> { Status = "good", Data = result };
        }

        public ServiceResponse<List<Tuple<string, int>>> GetReplyMessageCount()
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userId = HttpContext.GetUserUniqueID();
            var data = context.Reply.Where(x => x.UserUniqueId == userId && x.IsMyMessage == false && x.IsRead == false).GroupBy(x => x.MessageGroupUniqueGuid).ToList();
            List<Tuple<string, int>> result = new List<Tuple<string, int>>();
            data.ForEach(x => result.Add(new Tuple<string, int>(x.Key.ToString(), x.Count())));
            return new ServiceResponse<List<Tuple<string, int>>> { Status = "good", Data = result };
        }


        public ServiceResponse<Tuple<bool,bool>> IsUnreadMessageThere()
        {
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var userId = HttpContext.GetUserUniqueID();
            bool unreadInbox = context.Inbox.Any(x => x.UserUniqueId == userId && x.IsMyMessage == false && x.IsRead == false);
            bool unreadReply = context.Reply.Any(x => x.UserUniqueId == userId &&  x.IsMyMessage == false && x.IsRead == false);
            return new ServiceResponse<Tuple<bool, bool>> { Status = "good"  , Data = new Tuple<bool, bool>(unreadInbox,unreadReply)};
        }
    }
}
