using CSharpFunctionalExtensions;
using HospitalManagementSystem.Application.Models.User;
using HospitalManagementSystem.Application.Models;
using HospitalManagementSystem.Application.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Text.Encodings.Web;
using HospitalManagementSystem.Application.Models.Account;
using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.DataAccess;


namespace HospitalManagementSystem.Application.Services
{
    public class AccountService : IAccountService
    {

        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration config;
        private readonly IEmailSender emailSender;
        private readonly LinkGenerator linkGenerator;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UrlEncoder urlEncoder;


        public AccountService(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           IUnitOfWork unitOfWork,
                           IConfiguration config,
                           IEmailSender emailSender,
                           LinkGenerator linkGenerator,
                           UrlEncoder urlEncoder,
                           IHttpContextAccessor httpContextAccessor)
        {
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.unitOfWork = unitOfWork;
            this.emailSender = emailSender;
            this.linkGenerator = linkGenerator;
            this.httpContextAccessor = httpContextAccessor;
            this.urlEncoder = urlEncoder;
        }


        public async Task<Result<RegisterResponseModel, DomainError>> RegisterAsync(RegisterModel model)
        {

            ApplicationUser newUser = new()
            {
                DateOfbirth = model.DateOfbirth,
                UserName = model.UserName,
                Email = model.Email,
                PasswordHash = model.Password,
                Gender = Gender.Male,
            };
        

            var result = await userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
                return Result.Failure<RegisterResponseModel, DomainError>(new DomainError(result.Errors.Select(e => e.Description)));
            result = await userManager.AddToRoleAsync(newUser, SD.Patient);
            if (!result.Succeeded)
                return Result.Failure<RegisterResponseModel, DomainError>(new DomainError(result.Errors.Select(e => e.Description)));

            await unitOfWork.Patients.AddAsync(new()
            {
                User = newUser,
              
                Name = model.FirstName + " " + model.LastName,
            });
            await ConfirmEmailAysnc(newUser.Email);
            await unitOfWork.CompleteAsync();
           
            await signInManager.SignInAsync(newUser, isPersistent: false);
           
            return new RegisterResponseModel
            {
                UserName = model.UserName,
                Email = model.Email,
            };
        }
         


        public async Task<Result<LoginResponseModel, DomainError>> LoginAsync(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return Result.Failure<LoginResponseModel, DomainError>(new DomainError("Invalid Password or Email"));

            var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe,
                                                                 lockoutOnFailure: true);
            if(!result.Succeeded)
                return Result.Failure<LoginResponseModel, DomainError>(new DomainError("Invalid Password or Email"));

            return new LoginResponseModel
            {
                Succeeded = result.Succeeded,
                IsLockedOut = result.IsLockedOut,
                RequiresTwoFactor = result.RequiresTwoFactor,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
            };
        }

        public async Task<Result<string>> ForgetPasswordAsync(ForgotPasswordModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Result.Success("Please,Check your Email.....");

            string token = await userManager.GeneratePasswordResetTokenAsync(user);

            var callBackUrl = GetLink("ResetPassword", new { Token = token });

            // Send email with reset link 
            await emailSender.SendEmailAsync(user.Email, "Reset Password",
                                            $"Click the link to reset your password: <a href ={callBackUrl}>click<a>");

            return Result.Success("Please,Check your Email.....");
        }

        public async Task<Result<string, DomainError>> ResetPasswordAsync(ResetPasswordModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Result.Failure<string, DomainError>(new DomainError("Something wrong has happened"));
            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            return result.Succeeded ? "Password has been reset" : Result.Failure<string, DomainError>(new DomainError("Something wrong has happened"));

        }

        public async Task<Result<string, DomainError>> ConfirmEmailAysnc(ConfirmEmailModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Result.Failure<string, DomainError>(new DomainError("Error"));
            var result = await userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded) return Result.Failure<string, DomainError>(new DomainError("Something wrong has happened"));

            return "Confirmed";
        }

        public async Task ConfirmEmailAysnc(string Email)
        {
            var user = await userManager.FindByEmailAsync(Email);
            var ConfirmEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = GetLink("ConfirmPassword", new { Email = Email, token = ConfirmEmailToken });
            await emailSender.SendEmailAsync(Email, "Confirm Your Email",
                                           $"To Confirm your Email <a href = {callBackUrl}>here</a>");

        }

        public async Task<Result<GetAuthenticatorKeyResponseModel, DomainError>> GetAuthenticatorTokenAysnc(GetAuthenticatorTokenModel model)
        {
            string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            var user = await userManager.FindByNameAsync(model.UserName ?? "");
            if (user == null)
                return Result.Failure<GetAuthenticatorKeyResponseModel, DomainError>(new DomainError("Something wrong has happened "));
            await userManager.ResetAuthenticatorKeyAsync(user);
            var token = await userManager.GetAuthenticatorKeyAsync(user);

            string AuthUri = string.Format(AuthenticatorUriFormat, urlEncoder.Encode("Hospital"),
                urlEncoder.Encode(user.UserName), token);

            return new GetAuthenticatorKeyResponseModel
            {
                Token = token,
                QRCodeUrl = AuthUri

            };
        }

        public async Task<Result<TwoFactorAuthenticationResponseModel, DomainError>> EnableTwoFactorAuthenticationAsync(TwoFactorAuthenticationModel model)
        {

            var user = await userManager.FindByNameAsync(model.UserName ?? "");
            if (user == null)
                return Result.Failure<TwoFactorAuthenticationResponseModel, DomainError>(new DomainError("Something wrong has happened "));

            bool succeeded = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, model.Code);

            if (!succeeded)
                return Result.Failure<TwoFactorAuthenticationResponseModel, DomainError>(new DomainError("Verify ,Your two factor auth code could not be validated."));
            await userManager.SetTwoFactorEnabledAsync(user, true);
            return new TwoFactorAuthenticationResponseModel
            {
                Msg = "succeeded"

            };

        }

        public async Task SignOutAsync()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<Result> GetTwoFactorAuthenticationUserAsync(GetTwoFactorAuthenticationUserModel model)
        {
            var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) Result.Failure("error");

            return Result.Success();
        }

        public async Task<Result<VerifyAuthenticatorResponseModel>> TwoFactorAuthenticatorSignInAsync(VerifyAuthenticatorModel model)
        {
            var result = await signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, model.RememberMe,
                   rememberClient: false);
            if (!result.Succeeded)
                return Result.Failure<VerifyAuthenticatorResponseModel>("error");
            return new VerifyAuthenticatorResponseModel
            {
                IsLockedOut = result.IsLockedOut,
                Succeeded = result.Succeeded,

            };
        }

        public async Task<Result<ProfileModel>> GetProfileDataAsync(string? UserName)
        {
            var user = await userManager.FindByNameAsync(UserName);
            if (user == null) Result.Failure("error");
            return new ProfileModel
            {
                Name = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfbirth,
                IsEmailConfirmed = user.EmailConfirmed,
                IsTwoFactorEnabled = user.TwoFactorEnabled,
                NationalIdOrPassport = user.SSN,

            };
        }
        
        private string GetLink(string ActionName, object? prams)
        {
            var context = httpContextAccessor.HttpContext;
            var callbackurl = $"{context.Request.Scheme}://{context.Request.Host}" +
                linkGenerator.GetPathByAction(httpContext: context, ActionName, "Account", prams);
            return callbackurl;
        }
    }
}