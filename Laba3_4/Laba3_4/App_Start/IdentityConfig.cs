using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Laba3_4.Models;
using System.Net.Mail;
using System.Net;

namespace Laba3_4
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {

            return Task.FromResult(0);
        }
    }

    public interface IEmailSender
    {
        void SendEmail(string fromName, string fromEmail, string toName, string toEmail, string topic, string text);
    }

    public class EmailSender : IEmailSender
    {

        private SmtpClient _smtpClient;

        public EmailSender(string serverName, int port, string login, string password)
        {
            _smtpClient = new SmtpClient(serverName, port)
            {
                Credentials = new NetworkCredential(login, password),
                EnableSsl = true
            };
        }

        public void SendEmail(string fromName, string fromEmail, string toName, string toEmail, string topic, string text)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromEmail, fromName);
            message.To.Add(new MailAddress(toEmail, toName));
            message.Body = text;
            message.Subject = topic;
            message.IsBodyHtml = true;

            _smtpClient.Send(message);
        }
    }


    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Подключите здесь службу SMS, чтобы отправить текстовое сообщение.
            return Task.FromResult(0);
        }
    }

    // Настройка диспетчера пользователей приложения. UserManager определяется в ASP.NET Identity и используется приложением.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {

        private const string ServerName = "smtp.gmail.com";
        private const int ServerPort = 587;
        private const string SenderEmail = "sanya25031997@gmail.com";
        private const string SenderName = "Laba3_4";
        private const string SenderPassword = "shura2015";

        private UserStore<ApplicationUser> _store;
        private IEmailSender _emailSender;


        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
            _store = store as UserStore<ApplicationUser>;
            _emailSender = new EmailSender(ServerName, ServerPort, SenderEmail, SenderPassword);
        }

        public void UpdateUser(ApplicationUser user)
        {
            Task<IdentityResult> updateTask = base.UpdateAsync(user);
            updateTask.Wait();
            _store.Context.SaveChanges();
        }

        public void DeleteUser(ApplicationUser user)
        {
            Task<IdentityResult> deleteTask = base.DeleteAsync(user);
            deleteTask.Wait();
            _store.Context.SaveChanges();
        }

        public void SendEmailToUser(ApplicationUser user, string topic, string text)
        {
            string email = user.Email;
            string firstName = user.FirstName;
            string lastName = user.LastName;
            string userToName = user.FirstName + " " + user.LastName;
            _emailSender.SendEmail(SenderName, SenderEmail, userToName, user.Email, topic, text);
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Настройка логики проверки имен пользователей
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Настройка логики проверки паролей
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            // Настройка параметров блокировки по умолчанию
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Регистрация поставщиков двухфакторной проверки подлинности. Для получения кода проверки пользователя в данном приложении используется телефон и сообщения электронной почты
            // Здесь можно указать собственный поставщик и подключить его.
            manager.RegisterTwoFactorProvider("Код, полученный по телефону", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Ваш код безопасности: {0}"
            });
            manager.RegisterTwoFactorProvider("Код из сообщения", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Код безопасности",
                BodyFormat = "Ваш код безопасности: {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Настройка диспетчера входа для приложения.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
