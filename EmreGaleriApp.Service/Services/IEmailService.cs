namespace EmreGaleriApp.Service.Services
{
    public interface IEmailService
    {

        Task SendResetPasswordEmail(string resetEmailLink, string ToEmail);

        Task SendOrderApprovedEmail(string toEmail, string userName, byte[]? invoicePdf = null);
        Task SendOrderRejectedEmail(string toEmail, string userName);

        Task SendEmailAsync(string toEmail, string subject, string body);


    }

}

