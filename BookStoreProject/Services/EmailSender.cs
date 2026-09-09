using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookStoreProject.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(
            string email,
            string subject,
            string htmlMessage)
        {
            // Email sending is disabled for this local project.
            // Registration does not require email confirmation.

            return Task.CompletedTask;
        }
    }
}

