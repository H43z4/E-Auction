using eauction.Data;
using eauction.Helpers;
using eauction.Infrastructure;
using iText.Layout.Element;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Models.Domain.Identity;
using Models.Views.Auction;
using Models.Views.Input;
using NotificationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace eauction.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration config;
        SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private IEmailSender _emailSender;
        private readonly INotificationManager notificationManager;
        private readonly ApplicationDbContext _db;

        public AuthController(
            IConfiguration config, 
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            INotificationManager notificationManager,
            ApplicationDbContext db)
        {
            this.config = config;
            _userManager = userManager;
            _signInManager = signInManager;
            this.notificationManager = notificationManager;
            _db = db;
        }

        public JsonResult Error()
        {
            return new JsonResult(new
            {
                status = false,
                msg = "We are not able to process your request at the moment. Sorry, for inconvenience."
            });
        }

        [HttpPost("Token")]
        [AllowAnonymous]
        public async Task<JsonResult> Token([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                var user = this._db.Users.SingleOrDefault(x => x.CNIC == genericParamtersList.CNIC);

                if (user == null)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Invalid credentials."
                    });
                }

                var userRoles = this._db.UserRoles.Any(x => x.UserId == user.Id && x.RoleId == 4);    // special user

                if (userRoles)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Access denied."
                    });
                }

                var signInResult = await _signInManager.PasswordSignInAsync(user, genericParamtersList.password, false, lockoutOnFailure: false);

                if (signInResult.Succeeded)
                {
                    var token = new JwtService(config).GenerateSecurityToken(user);

                    return new JsonResult(new
                    {
                        status = true,
                        token,
                        expiry = DateTime.Now.AddMinutes(480),
                        user.CNIC,
                        Name = user.FullName,
                        user.NTN,
                        user.PhoneNumber,
                        user.Email
                    });
                }
                else if (signInResult.IsLockedOut)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Account locked out."
                    });
                }

                else if (!user.EmailConfirmed)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Account not confirmed."
                    });
                }

                else
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Invalid credentials."
                    });
                }
            }
            catch
            {
                return new JsonResult(new
                {
                    status = false,
                    msg = "System error."
                });
            }
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<JsonResult> Register([FromBody]SignupModel signupModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                    return new JsonResult(new
                    {
                        status = false,
                        errors
                    });
                }

                signupModel.CNIC = RegexUtilities.Clean(signupModel.CNIC);  //  remove dashes
                signupModel.PhoneNumber = RegexUtilities.Clean(signupModel.PhoneNumber);  //  remove dashes

                if (Infrastructure.RegexUtilities.IsValidCNIC(signupModel.CNIC) == false)
                {
                    ModelState.AddModelError(string.Empty, "CNIC should consists of valid 13 digits number.");
                }

                var parameters = new SqlParameter[3]
                {
                    new SqlParameter("@CNIC", signupModel.CNIC),
                    new SqlParameter("@Email", signupModel.Email),
                    new SqlParameter("@PhoneNumber", signupModel.PhoneNumber),
                };

                var existingUsers = this._db.Users.FromSqlRaw("EXEC [Identity].GetExistingUsers @CNIC, @Email, @PhoneNumber", parameters).ToList();

                if (existingUsers.Count() > 0)
                {
                    var error = "This CNIC/ Email/ Phone Number has already been taken.";

                    return new JsonResult(new
                    {
                        status = false,
                        errors = new string[] { error }
                    });
                }

                var user = new Models.Domain.Identity.User
                {
                    UserName = signupModel.CNIC,
                    FullName = signupModel.Name,
                    CNIC = signupModel.CNIC,
                    NTN = signupModel.NTN,
                    Company = signupModel.Company,
                    PhoneNumber = signupModel.PhoneNumber,
                    Email = signupModel.Email,
                    UserTypeId = 1
                };

                var result = await _userManager.CreateAsync(user, signupModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(user, new Claim("FullName", signupModel.Name));

                    var otp = this.GetOTP(user);

                    try
                    {
                        await this.SendMessages(user, otp);
                    }
                    catch
                    {
                    }

                    return new JsonResult(new
                    {
                        status = true,
                        userId = user.Id
                    });
                }

                return new JsonResult(new
                {
                    status = false,
                    errors = result.Errors.Select(x => x.Description)
                });

            }
            catch (Exception ex)
            {
                return this.Error();
            }
        }


        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<JsonResult> ForgotPassword([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                if (Infrastructure.RegexUtilities.IsValidCNIC(genericParamtersList.CNIC) == false)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "CNIC should consists of valid 13 digits number."
                    });
                }

                var existingUser = this._db.Users.SingleOrDefault(x => x.CNIC == genericParamtersList.CNIC);

                if (existingUser == null)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Invalid CNIC."
                    });
                }

                if (existingUser.EmailConfirmed == false)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Account not confirmed."
                    });
                }

                /////   USER CREATED BUT OTP IS NOT CONFIRMED

                var otp = this.GetOTP(existingUser);

                try
                {
                    await this.SendMessages(existingUser, otp);
                }
                catch
                {
                }

                return new JsonResult(new
                {
                    status = true,
                    userId = existingUser.Id
                });
            }
            catch (Exception ex)
            {
                return this.Error();
            }
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
            {
                var modelStateerrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                return new JsonResult(new
                {
                    status = false,
                    errors = modelStateerrors
                });
            }

            var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == resetPasswordModel.UserId);

            if (userIdentity != null)
            {
                if (userIdentity.OTP == resetPasswordModel.OTP && userIdentity.OtpExpiryOn > DateTime.Now)
                {
                    var user = this._db.Users.SingleOrDefault(x => x.Id == resetPasswordModel.UserId);

                    if (user != null)
                    {
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _signInManager.UserManager.ResetPasswordAsync(user, code, resetPasswordModel.Password);

                        if (result.Succeeded)
                        {
                            return new JsonResult(new
                            {
                                status = true,
                                msg = "Your password has been reset."
                            });
                        }
                        
                        return new JsonResult(new
                        {
                            status = false,
                            errors = result.Errors.Select(x => x.Description)
                        });
                    }
                    else
                    {
                        var errors1 = new string[] { "Invalid user account." };

                        return new JsonResult(new
                        {
                            status = false,
                            errors = errors1
                        });
                    }
                }
                else
                {
                    var errors2 = new string[] { "Invalid Pin code." };

                    return new JsonResult(new
                    {
                        status = false,
                        errors = errors2
                    });
                }
            }

            var errors = new string[] { "Invalid Pin code." };

            return new JsonResult(new
            {
                status = false,
                errors
            });
        }


        [HttpPost("ChangePassword")]
        public async Task<JsonResult> ChangePassword([FromBody]ChangePassword input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                    return new JsonResult(new
                    {
                        status = false,
                        errors
                    });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    var errors = new string[] { "Invalid user." };

                    return new JsonResult(new
                    {
                        status = false,
                        errors
                    });
                    //return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        errors = changePasswordResult.Errors.Select(x => x.Description)
                    });
                }

                await _signInManager.RefreshSignInAsync(user);
                //_logger.LogInformation("User changed their password successfully.");

                return new JsonResult(new
                {
                    status = true,
                    msg = "Your password has been changed."
                });
            }
            catch
            {
                return this.Error();
            }
        }


        [HttpPost("GetProfile")]
        public Task<JsonResult> GetProfile()
        {
            try
            {
                var userId = System.Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = this._db.Users.SingleOrDefault(x => x.Id == userId);

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    profile = new
                    {
                        user.Address,
                        user.CNIC,
                        user.FatherHusbandName,
                        Name = user.UserName,
                        user.NTN,
                        user.PhoneNumber
                    }
                }));
            }
            catch (Exception ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    msg = "System error."
                }));
            }
        }

        [HttpPost("SaveProfile")]
        public async Task<JsonResult> SaveProfile([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                if (string.IsNullOrEmpty(genericParamtersList.CNIC) && string.IsNullOrEmpty(genericParamtersList.NTN))
                {
                    //return Task.FromResult(new JsonResult(new
                    //{
                    //    status = false,
                    //    msg = "Anyone of CNIC or NTN is also required."
                    //}));
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Anyone of CNIC or NTN is also required."
                    });
                }

                if (!string.IsNullOrEmpty(genericParamtersList.CNIC))
                {
                    genericParamtersList.CNIC = Infrastructure.RegexUtilities.Clean(genericParamtersList.CNIC);
                }

                if (!string.IsNullOrEmpty(genericParamtersList.phoneNumber))
                {
                    genericParamtersList.phoneNumber = Infrastructure.RegexUtilities.Clean(genericParamtersList.phoneNumber);
                }

                var userId = System.Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = this._db.Users.SingleOrDefault(x => x.Id == userId);

                this._db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                user.Address = genericParamtersList.address;
                user.CNIC = genericParamtersList.CNIC;
                user.FatherHusbandName = genericParamtersList.fatherHusbandName;
                user.FullName = genericParamtersList.name;
                user.NTN = genericParamtersList.NTN;
                user.PhoneNumber = genericParamtersList.phoneNumber;

                var rowsAffected = this._db.SaveChanges();

                //await _signInManager.RefreshSignInAsync(user).ConfigureAwait(false);

                if (rowsAffected == 1)
                {
                    //return Task.FromResult(new JsonResult(new
                    //{
                    //    status = true,
                    //}));
                    return new JsonResult(new
                    {
                        status = true,
                    });
                }

                // If we got this far, something failed, redisplay form
                //return Task.FromResult(new JsonResult(new
                //{
                //    status = false,
                //    msg = "Invalid data"
                //}));
                return new JsonResult(new
                {
                    status = false,
                    msg = "Invalid data"
                });
            }
            catch (Exception ex)
            {
                //return Task.FromResult(new JsonResult(new
                //{
                //    status = false,
                //    msg = "System error."
                //}));
                return new JsonResult(new
                {
                    status = false,
                    msg = "System error."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody]GenericParamtersList genericParamtersList)
        {
            var user = this._db.Users.SingleOrDefault(x => x.Id == genericParamtersList.userId);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == genericParamtersList.userId);

                    if (userIdentity != null)
                    {
                        if (userIdentity.OTP == genericParamtersList.OTP && userIdentity.OtpExpiryOn >= DateTime.Now)
                        {
                            this._db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                            user.EmailConfirmed = true;
                            user.PhoneNumberConfirmed = true;

                            await this._db.SaveChangesAsync();

                            return new JsonResult(new 
                            { 
                                status = true,
                                msg = "Thank you, your account has been confirmed."
                            });
                        }
                    }
                }
            }

            return new JsonResult(new 
            { 
                status = false,
                msg = "Incorrect PIN code."
            });
        }

        [AllowAnonymous]
        [HttpPost("ResendOTP")]
        public async Task<IActionResult> ResendOTP([FromBody]GenericParamtersList genericParamtersList)
        {
            var existingUser = this._db.Users.SingleOrDefault(x => x.Id == genericParamtersList.userId);

            if (existingUser != null)
            {
                if (!existingUser.EmailConfirmed)
                {
                    var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == genericParamtersList.userId);

                    if (userIdentity != null)
                    {
                        this._db.Entry<UserIdentity>(userIdentity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                        string otp = DateTime.Now.ToString("ffff");

                        userIdentity.OTP = otp;
                        userIdentity.OtpExpiryOn = DateTime.Now.AddHours(1);

                        this._db.SaveChanges();

                        var msg = new List<string>();

                        try
                        {
                            await this.SendMessages(existingUser, otp);
                        }
                        catch
                        {
                        }

                        foreach (var item in this.notificationManager.NotificationSentOn)
                        {
                            if (item == Models.enums.NotificationType.EMAIL)
                            {
                                msg.Add("PIN code resent on given email address.");
                            }
                            else if (item == Models.enums.NotificationType.SMS)
                            {
                                msg.Add("PIN code resent on given phone number.");
                            }
                        }


                        //try
                        //{
                        //    var smsSent = await this.SendSMS(existingUser, otp);

                        //    if (smsSent == true)
                        //    {
                        //        msg.Add("PIN code resent on given phone number.");
                        //    }
                        //}
                        //catch
                        //{
                        //}

                        //try
                        //{
                        //    await this.SendEmail(existingUser, otp);

                        //    msg.Add("PIN code resent on given email address.");
                        //}
                        //catch
                        //{
                        //}

                        if (msg.Count == 0)
                        {
                            return new JsonResult(new
                            {
                                status = false,
                                msg = "Could not send PIN code on the given phone number / email address."
                            });
                        }

                        return new JsonResult(new 
                        { 
                            status = true,
                            msg
                        });
                    }
                }
            }

            return new JsonResult(new { status = false });
        }

        [AllowAnonymous]
        [HttpPost("IsLatestAppVersion")]
        public Task<JsonResult> IsLatestAppVersion([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                var apk = this._db.APK.FirstOrDefault();

                if (apk != null)
                {
                    if (apk.Version == genericParamtersList.apkVersion)
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = true,
                            updateRequired = false,
                            message = ""
                        }));
                    }
                    else
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = true,
                            updateRequired = true,
                            message = ""
                        }));
                    }
                }
            }
            catch
            {
            }

            return Task.FromResult(new JsonResult(new
            {
                status = false,
                updateRequired = false,
                message = ""
            }));
        }



        private async Task SendMessages(User user, string OTP)
        {
            var emailSetting = this._db.EmailSetting.Where(x => x.MessageTypeId == 1).FirstOrDefault();

            emailSetting.Receiver = user.Email;
            emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine).Replace("#otp", OTP);

            await this.notificationManager.SendMessage(user.PhoneNumber, emailSetting);
        }

        private string GetOTP(User user)
        {
            string otp = DateTime.Now.ToString("ffff");

            var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == user.Id);

            if (userIdentity != null)
            {
                this._db.Entry<UserIdentity>(userIdentity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                userIdentity.OTP = otp;
                userIdentity.OtpExpiryOn = DateTime.Now.AddHours(1);

                this._db.SaveChanges();
            }
            else
            {
                this._db.UserIdentity.Add(new UserIdentity()
                {
                    UserId = user.Id,
                    OTP = otp,
                    OtpExpiryOn = DateTime.Now.AddHours(1)
                });

                this._db.SaveChanges();
            }

            return otp;
        }



        #region EPayment

        [HttpPost("GetToken")]
        [AllowAnonymous]
        public async Task<JsonResult> GetToken([FromBody]ePayAuth ePayAuth)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                return new JsonResult(new
                {
                    status = false,
                    msg = "Data validation failed.",
                    errors = modelStateErrors
                });
            }

            try
            {
                var epayServerIP = HttpContext.Connection.RemoteIpAddress.ToString();

                var allowedIP = this.config.GetSection("EPayment:AllowedHost").Value;
                var secret = this.config.GetSection("EPayment:secret").Value;
                var apikey = this.config.GetSection("EPayment:apikey").Value;

                if (string.Equals(ePayAuth.secret, secret) == false || string.Equals(ePayAuth.apikey, apikey) == false)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        msg = "Invalid credentials."
                    });
                }

                //if (config.)
                //{
                //    return new JsonResult(new
                //    {
                //        status = false,
                //        msg = "Invalid credentials."
                //    });
                //}

                var token = new JwtService(config).GenerateSecurityTokenForEPay(secret, apikey);

                return new JsonResult(new
                {
                    status = true,
                    token,
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    status = false,
                    msg = "Problem generating token."
                });
            }
        }

        #endregion
    }
}