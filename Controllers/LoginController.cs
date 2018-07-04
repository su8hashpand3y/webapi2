﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using HashLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApi1.Models;
using WebApi1.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi1.Controllers
{
    public class LoginController : Controller
    {
        private const string bucketName = "hgtdata";
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

       

        private async Task<string> UploadFileAsync(IFormFile file, string keyName)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(this.s3Client);

                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    InputStream = file.OpenReadStream(),
                    StorageClass = S3StorageClass.Standard,
                    Key = keyName,
                    CannedACL = S3CannedACL.PublicRead
                };


                await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
                // append Full address here 
                return $"https://s3.ap-south-1.amazonaws.com/{bucketName}/keyName";
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
                        var hasher = new Hasher();
                        var hashedPassword = hasher.HashPassword(model.Password);


                        User newUser = new User
                        {
                            Name = model.Name,
                            Password = hashedPassword.Hash,
                            Salt = hashedPassword.Salt,
                            UserUniqueId = model.UserUniqueId,
                        };

                        if (model.Photo != null)
                        {
                            try
                            {
                                String ext = System.IO.Path.GetExtension(model.Photo.FileName);
                                newUser.PhotoUrl = await this.UploadFileAsync(model.Photo, $"{model.UserUniqueId}{ext}");
                            }
                            catch { }
                        }


                        context.User.Add(newUser);
                        context.SaveChanges();
                        return this.LoginUser(new RegisterViewModel { UserUniqueId = model.UserUniqueId, Password = model.Password });
                    }
                    else
                    {
                        return Ok(new ServiceResponse<string> { Status = "bad", Message = $"{model.UserUniqueId} is already taken!" });
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
        public IActionResult Login([FromBody]RegisterViewModel user)
        {
            return LoginUser(user);
        }

        private IActionResult LoginUser(RegisterViewModel user)
        {
            bool succeeded = false;
            var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
            var foundUser = context.User.FirstOrDefault(x => x.UserUniqueId == user.UserUniqueId);
            if (foundUser == null)
                return Ok(new ServiceResponse<string> { Status = "bad", Message = "User not found!" });

            var hash = new HashedPassword(foundUser.Password, foundUser.Salt);
            var hasher = new Hasher();
            if (hasher.Check(user.Password, hash))
                succeeded = true;
            else return Ok(new ServiceResponse<string> { Status = "error", Message = "Wrong Password!" });

            if (succeeded)
            {
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
                return Ok(new ServiceResponse<string> { Status = "error", Message = "Could not verify username and password!" });
            }
        }

        [HttpGet()]
        public bool IsUpdateMandatory()
        {
            return false;
        }


       
        public ServiceResponse<List<UserDataViewModel>> Search(string searchTerm,int skip)
        {
            try
            {
                var context = this.services.GetService(typeof(WebApiDBContext)) as WebApiDBContext;
                var matchingUser  = context.User.Where(x => x.Name.Contains(searchTerm) || x.UserUniqueId.Contains(searchTerm)).Skip(skip);
                return new ServiceResponse<List<UserDataViewModel>> { Status = "good", Data = matchingUser.Select(x => new UserDataViewModel { Name = $"{x.Name}({x.UserUniqueId})", PhotoUrl = x.PhotoUrl }).ToList() };
            }
            catch (Exception e)
            {
            }

            return null;
        }
    }
}
