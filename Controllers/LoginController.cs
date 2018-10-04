﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApi1.Helpers;
using WebApi1.Models;
using WebApi1.ViewModels;


namespace WebApi1.Controllers
{
    public class LoginController : Controller
    {
        private const string bucketName = "rajdoot";
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSoutheast1;
        private IAmazonS3 s3Client;
        private IConfiguration Configuration { get; }
        private IServiceProvider services { get; }
        private readonly IHostingEnvironment hostingEnvironment;

        public LoginController(IAmazonS3 s3Client, IServiceProvider services, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            this.s3Client = s3Client;
            this.services = services;
            Configuration = configuration;
            this.hostingEnvironment = hostingEnvironment;
        }

       

        private async Task<string> UploadFileAsync(byte[] file, string keyName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(this.s3Client);
                Stream stream = new MemoryStream(file);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    InputStream = stream,
                    StorageClass = S3StorageClass.Standard,
                    Key = keyName,
                    CannedACL = S3CannedACL.PublicRead
                };


                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                // append Full address here 
                return $"https://s3.ap-south-1.amazonaws.com/{bucketName}/{keyName}";
            }
            catch (Exception e)
            {
                return string.Empty;
            }

        }


        [AllowAnonymous]
        [HttpPost()]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                    if (context.User.FirstOrDefault(x => x.UserUniqueId == model.UserUniqueId) == null)
                    {



                        User newUser = new User
                        {
                            UserUniqueId = model.UserUniqueId,
                            Name = model.Name,
                            FavColor = model.FavColor,
                            FavNumber = model.FavNumber
                        };

                        var hasher = new PasswordHasher<User>();
                        var hashedPassword = hasher.HashPassword(newUser, model.Password);
                        newUser.Password = hashedPassword;
                            try
                            {
                                if (!String.IsNullOrEmpty(model.Photo))
                                {
                                    
                                   
                                    string path = $"{model.UserUniqueId}{model.Ext.Trim('\'')}";
                                    newUser.PhotoUrl = await UploadFileAsync(Convert.FromBase64String(model.Photo), path);
                                  
                                }
                            }
                            catch { }


                        context.User.Add(newUser);
                        context.SaveChanges();
                        return this.LoginUser(new LoginViewModel { UserUniqueId = model.UserUniqueId, Password = model.Password });
                    }
                    else
                    {
                        return Ok(new ServiceResponse<string> { Status = "bad", Message = $"User Id {model.UserUniqueId} is already taken!" });
                    }
                }
                catch
                {
                    return Ok(new ServiceResponse<string> { Status = "bad", Message = "Somthing doesn't seems to work" });
                }
            }
            else
            {
                var modelErrors = new StringBuilder();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        modelErrors.AppendLine(modelError.ErrorMessage);
                    }
                }

                return Ok(new ServiceResponse<string> { Status = "bad", Message = modelErrors.ToString() });
            }
        }


        [AllowAnonymous]
        [HttpPost()]
        public IActionResult Forget(RegisterViewModel model)
        {
                try
                {
                    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                    var user = context.User.FirstOrDefault(x => x.UserUniqueId == model.UserUniqueId);
                if(user == null)
                {
                    return Ok(new ServiceResponse<string> { Status = "bad", Message = "User Not Found" });
                }
                var hasher = new PasswordHasher<User>();
                        var hashedPassword = hasher.HashPassword(user, model.Password);
                        user.Password = hashedPassword;
                    if (model.FavColor.Equals(user.FavColor,StringComparison.InvariantCultureIgnoreCase) 
                    && model.FavNumber.Equals(user.FavNumber, StringComparison.InvariantCultureIgnoreCase))
                    {
                        context.SaveChanges();
                        return this.LoginUser(new LoginViewModel { UserUniqueId = model.UserUniqueId, Password = model.Password });
                    }
                    else
                    {
                        return Ok(new ServiceResponse<string> { Status = "bad", Message = "Your Answer do not match "});
                    }

                }
                catch
                {
                    return Ok(new ServiceResponse<string> { Status = "bad", Message = "Somthing doesn't seems to work" });
                }
        }


        [AllowAnonymous]
        [HttpPost()]
        public IActionResult Login([FromForm]LoginViewModel user)
        {
            return LoginUser(user);
        }

        private IActionResult LoginUser(LoginViewModel user)
        {
            bool succeeded = false;
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var foundUser = context.User.FirstOrDefault(x => x.UserUniqueId.Equals(user.UserUniqueId, StringComparison.InvariantCultureIgnoreCase));
            if (foundUser == null)
                return Ok(new ServiceResponse<string> { Status = "bad", Message = "User not found!" });

            var hash = new PasswordHasher<User>();
            if (hash.VerifyHashedPassword(foundUser, foundUser.Password, user.Password) == PasswordVerificationResult.Success)
                succeeded = true;
            else return Ok(new ServiceResponse<string> { Status = "bad", Message = "Wrong Password!" });

            if (succeeded)
            {
                try
                {
                    foundUser.DeviceId = Request.Headers["etag"];
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    Console.WriteLine($"Logged in with Device Id: {foundUser.DeviceId }");
                }

                var claims = new[]
                {
                              new Claim(ClaimTypes.NameIdentifier, foundUser.UserUniqueId)
                        };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: Configuration["ValidIssuer"],
                    audience: Configuration["ValidAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds);

                return Ok(new ServiceResponse<string> { Status = "good", Message = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            else
            {
                return Ok(new ServiceResponse<string> { Status = "bad", Message = "Could not verify username and password!" });
            }
        }

        [HttpGet()]
        public IActionResult CheckAuthorization()
        {
            var userId = HttpContext.GetUserUniqueID();
            return String.IsNullOrEmpty(userId) ? Ok(new ServiceResponse<string> { Status = "bad", Message = "App Update Needed" }): Ok(new ServiceResponse<string> { Status = "good", Message = "Already Logged in" });
        }

        [HttpGet()]
        public IActionResult IsUpdateMandatory()
        {
            return Boolean.Parse(Configuration["IsUpdateRequired"]) ? Ok(new ServiceResponse<string> { Status = "good", Message = "App Update Needed" }) : Ok(new ServiceResponse<string> { Status = "bad", Message = "App Update Needed" });
        }



        [HttpGet()]
        public ServiceResponse<List<UserDataViewModel>> Search(string searchTerm,int skip)
        {
            try
            {
                var userId = HttpContext.GetUserUniqueID();
                List<UserDataViewModel> result = new List<UserDataViewModel>();
                if (!String.IsNullOrEmpty(userId))
                {
                    var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                    //var fullMactchUser = context.User.Where(x => x.Name == searchTerm || x.UserUniqueId == searchTerm).Skip(skip);
                    //result.AddRange(fullMactchUser.Select(x => new UserDataViewModel { Name = x.Name, UserId = x.UserUniqueId, UserImage = x.PhotoUrl }).ToList());
                    var matchingUser = context.User.Where(x => x.Name.ToLower().Contains(searchTerm.ToLower()) || x.UserUniqueId.ToLower().Contains(searchTerm.ToLower())).Skip(skip);
                    result.AddRange(matchingUser.Select(x => new UserDataViewModel { Name = x.Name, UserId = x.UserUniqueId, UserImage = x.PhotoUrl }).ToList());
                }

                return new ServiceResponse<List<UserDataViewModel>> { Status = "good", Data = result.ToList() };
            }
            catch (Exception e)
            {
            }

            return null;
        }
    }
}
