namespace Pharmacy_Dashboard.MVC.Helpers
{
    public static class EmailTemplateService
    {
        public static string GetResetPasswordEmailBody(string resetLink)
        {
            return $@"
            <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
                <div style='max-width: 600px; margin: auto; background: #ffffff; border: 1px solid #ddd; border-radius: 10px; padding: 20px; text-align: center;'>
            
                    <!-- Logo -->
                    <img src='https://baltostorage.blob.core.windows.net/images/Logo/balto-logo.png' alt='Pharmacy Logo' style='max-width: 150px; margin-bottom: 20px;'>

                    <h2 style='color: #2c3e50;'>Reset Your Password</h2>
                    <p style='text-align: left;'>Hello,</p>
                    <p style='text-align: left;'>You recently requested to reset your password. Click the button below to proceed:</p>

                    <!-- Reset Button -->
                    <a href='{resetLink}'
                       style='display: inline-block; padding: 12px 24px; background-color: #3498db; color: #fff; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0;'>
                       Reset Password
                    </a>

                    <p style='text-align: left;'>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; text-align: left;'><a href='{resetLink}'>{resetLink}</a></p>

                    <p style='text-align: left;'>If you didn’t request this, please ignore this email.</p>

                    <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>

                    <p style='color: #888;'>Thanks,<br><strong>Pharmacy System Team</strong></p>
                </div>
            </div>";
        }

        public static string GetEmailConfirmationBody(string userName, string confirmationLink)
        {
            return $@"
    <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 30px;'>
        <div style='max-width: 600px; margin: auto; background: #ffffff; border: 1px solid #ddd; border-radius: 10px; padding: 20px; text-align: center;'>

            <!-- Logo -->
            <img src='https://baltostorage.blob.core.windows.net/images/Logo/balto-logo.png' alt='Pharmacy Logo' style='max-width: 150px; margin-bottom: 20px;'>

            <h2 style='color: #2c3e50;'>Confirm Your Email</h2>
            <p style='text-align: left;'>Hello {userName},</p>
            <p style='text-align: left;'>Thank you for registering. Please confirm your email address by clicking the button below:</p>

            <!-- Confirm Button -->
            <a href='{confirmationLink}'
               style='display: inline-block; padding: 12px 24px; background-color: #2ecc71; color: #fff; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0;'>
               Confirm Email
            </a>

            <p style='text-align: left;'>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; text-align: left;'><a href='{confirmationLink}'>{confirmationLink}</a></p>

            <hr style='margin: 30px 0; border: none; border-top: 1px solid #eee;'>

            <p style='color: #888;'>Thanks,<br><strong>Pharmacy System Team</strong></p>
        </div>
    </div>";
        }




    }
}
