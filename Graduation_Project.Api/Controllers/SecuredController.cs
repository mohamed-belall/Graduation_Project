using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Text;
using MimeKit;
using static System.Net.Mime.MediaTypeNames;
using MailKit.Security;
using System.Net.Mail;
using Graduation_Project.Core.IServices;

namespace Graduation_Project.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecuredController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public SecuredController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("secured")] // api/Secured/secured
        public IActionResult Secured()
        {
            return Ok("Hello From Secured");
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendEmail(string to, string subject, string body)
        {
            var success = await _emailService.SendEmailAsync(to, subject, body);

            if (success)
                return Ok(new { message = "Email sent successfully!" });

            return BadRequest(new { message = "Failed to send email." });
        }

        //[HttpPost("SendEmail")] // POST: api/Secured/SendEmail
        //public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest model)
        //{
        //    if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Subject) || string.IsNullOrEmpty(model.Message))
        //    {
        //        return BadRequest(new { message = "Email, Subject, and Message are required" });
        //    }

        //    try
        //    {
        //        await _emailService.SendEmailAsync(model.Email, model.Subject, model.Message);
        //        return Ok(new { message = "Email sent successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (not shown here for brevity)
        //        return StatusCode(500, new { message = "An error occurred while sending the email", details = ex.Message });
        //    }
        //}
    }

    public class SendEmailRequest
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    //[HttpPost("sendEmail")] // api/Secured/sendEmail
    //public async Task<IActionResult> SendEmail(string body)
    //{
    //    try
    //    {
    //        var email = new MimeMessage();
    //        email.From.Add(MailboxAddress.Parse("virgil78@ethereal.email"));
    //        email.To.Add(MailboxAddress.Parse("mohasanbder@gmail.com"));
    //        email.Subject = "Test Email Subject";
    //        email.Body = new TextPart(TextFormat.Html) { Text = body };

    //        using var smtp = new MailKit.Net.Smtp.SmtpClient();
    //        await smtp.ConnectAsync("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
    //        await smtp.AuthenticateAsync("virgil78@ethereal.email", "bYUg7kV1MyB1SEVfZ8");
    //        await smtp.SendAsync(email);
    //        await smtp.DisconnectAsync(true);

    //        return Ok(new { message = "Email sent successfully!" });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { message = "Email sending failed", error = ex.Message });
    //    }
    //}

    //// end point for sending email
    //[Authorize(Roles = nameof(UserRoleType.Doctor))]
    //[HttpPost("sendEmail")] // api/Secured/sendEmail
    //public IActionResult SendEmail(string body)
    //{
    //    var email = new MimeMessage();
    //    email.From.Add(MailboxAddress.Parse("virgil78@ethereal.email"));
    //    email.To.Add(MailboxAddress.Parse("virgil78@ethereal.email"));
    //    email.Subject = "Test Email Subject";
    //    email.Body = new TextPart(TextFormat.Html) { Text = body };

    //    using var smtp = new SmtpClient();
    //    smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
    //    smtp.Authenticate("virgil78@ethereal.email", "bYUg7kV1MyB1SEVfZ8");
    //    smtp.Send(email);
    //    smtp.Disconnect(true);
    //    return Ok();

    //}



}
    
