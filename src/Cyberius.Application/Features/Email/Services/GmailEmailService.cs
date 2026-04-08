using System.Net;
using System.Net.Mail;
using Cyberius.Application.Features.Email.Interfaces;
using Cyberius.Domain.Options;
using Microsoft.Extensions.Options;

namespace Cyberius.Application.Features.Email.Services;

public sealed class GmailEmailService(IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendPasswordResetAsync(
        string toEmail, string toName, string resetLink, CancellationToken ct = default)
    {
        var subject = "Сброс пароля — Cyberius";
        var body = BuildResetEmailHtml(toName, resetLink);

        await SendAsync(toEmail, toName, subject, body, ct);
    }

    private async Task SendAsync(
        string toEmail, string toName, string subject, string body, CancellationToken ct)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
        };

        using var message = new MailMessage();
        message.From = new MailAddress(_settings.From, _settings.DisplayName);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;
        message.To.Add(new MailAddress(toEmail, toName));

        await client.SendMailAsync(message, ct);
    }
    
    public async Task SendEmailConfirmationAsync(
      string toEmail, string toName, string confirmLink, CancellationToken ct = default)
    {
      var subject = "Подтверждение email — DevBlog";
      var body    = BuildConfirmEmailHtml(toName, confirmLink);
      await SendAsync(toEmail, toName, subject, body, ct);
    }

    private static string BuildConfirmEmailHtml(string name, string confirmLink) => $"""
    <!DOCTYPE html>
    <html lang="ru">
    <head><meta charset="UTF-8"/></head>
    <body style="margin:0;padding:0;background:#0a0f1e;font-family:'Segoe UI',Arial,sans-serif;">
      <table width="100%" cellpadding="0" cellspacing="0">
        <tr>
          <td align="center" style="padding:40px 20px;">
            <table width="600" cellpadding="0" cellspacing="0"
              style="background:#0d1b2e;border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,0.08);">
              <tr>
                <td style="background:linear-gradient(135deg,#0ca2e7,#818cf8);padding:32px;text-align:center;">
                  <h1 style="margin:0;color:#fff;font-size:24px;font-weight:900;">Cyberius</h1>
                  <p style="margin:4px 0 0;color:rgba(255,255,255,0.8);font-size:13px;">.NET & Angular</p>
                </td>
              </tr>
              <tr>
                <td style="padding:40px 32px;">
                  <p style="margin:0 0 16px;color:#e2e8f0;font-size:16px;">
                    Привет, <strong>{name}</strong>!
                  </p>
                  <p style="margin:0 0 24px;color:#94a3b8;font-size:14px;line-height:1.6;">
                    Спасибо за регистрацию на Cyberius. Подтвердите ваш email чтобы начать пользоваться сайтом.
                    Ссылка действительна <strong style="color:#e2e8f0;">24 часа</strong>.
                  </p>
                  <div style="text-align:center;margin:32px 0;">
                    <a href="{confirmLink}"
                      style="display:inline-block;background:linear-gradient(135deg,#0ca2e7,#818cf8);
                             color:#fff;text-decoration:none;padding:14px 32px;
                             border-radius:12px;font-size:15px;font-weight:700;">
                      Подтвердить email
                    </a>
                  </div>
                  <p style="margin:0;color:#64748b;font-size:12px;line-height:1.6;">
                    Если вы не регистрировались — просто проигнорируйте это письмо.<br/><br/>
                    Или скопируйте ссылку:<br/>
                    <a href="{confirmLink}" style="color:#0ca2e7;word-break:break-all;">{confirmLink}</a>
                  </p>
                </td>
              </tr>
              <tr>
                <td style="padding:20px 32px;border-top:1px solid rgba(255,255,255,0.06);
                            text-align:center;color:#475569;font-size:11px;">
                  © {DateTime.UtcNow.Year} Cyberius.
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </table>
    </body>
    </html>
    """;
    
    private static string BuildResetEmailHtml(string name, string resetLink) => $"""
         <!DOCTYPE html>
         <html lang="ru">
         <head><meta charset="UTF-8"/></head>
         <body style="margin:0;padding:0;background:#0a0f1e;font-family:'Segoe UI',Arial,sans-serif;">
           <table width="100%" cellpadding="0" cellspacing="0">
             <tr>
               <td align="center" style="padding:40px 20px;">
                 <table width="600" cellpadding="0" cellspacing="0"
                   style="background:#0d1b2e;border-radius:16px;overflow:hidden;border:1px solid rgba(255,255,255,0.08);">

                   <!-- Header -->
                   <tr>
                     <td style="background:linear-gradient(135deg,#0ca2e7,#818cf8);padding:32px;text-align:center;">
                       <h1 style="margin:0;color:#fff;font-size:24px;font-weight:900;">Cyberius</h1>
                       <p style="margin:4px 0 0;color:rgba(255,255,255,0.8);font-size:13px;">.NET & Angular</p>
                     </td>
                   </tr>

                   <!-- Body -->
                   <tr>
                     <td style="padding:40px 32px;">
                       <p style="margin:0 0 16px;color:#e2e8f0;font-size:16px;">
                         Привет, <strong>{name}</strong>!
                       </p>
                       <p style="margin:0 0 24px;color:#94a3b8;font-size:14px;line-height:1.6;">
                         Мы получили запрос на сброс пароля для вашего аккаунта.
                         Нажмите кнопку ниже чтобы создать новый пароль.
                         Ссылка действительна <strong style="color:#e2e8f0;">15 минут</strong>.
                       </p>

                       <!-- Button -->
                       <div style="text-align:center;margin:32px 0;">
                         <a href="{resetLink}"
                           style="display:inline-block;background:linear-gradient(135deg,#0ca2e7,#818cf8);
                                  color:#fff;text-decoration:none;padding:14px 32px;
                                  border-radius:12px;font-size:15px;font-weight:700;">
                           Сбросить пароль
                         </a>
                       </div>

                       <p style="margin:0;color:#64748b;font-size:12px;line-height:1.6;">
                         Если вы не запрашивали сброс пароля — просто проигнорируйте это письмо.
                         Ваш пароль останется прежним.<br/><br/>
                         Или скопируйте ссылку в браузер:<br/>
                         <a href="{resetLink}" style="color:#0ca2e7;word-break:break-all;">{resetLink}</a>
                       </p>
                     </td>
                   </tr>

                   <!-- Footer -->
                   <tr>
                     <td style="padding:20px 32px;border-top:1px solid rgba(255,255,255,0.06);
                                 text-align:center;color:#475569;font-size:11px;">
                       © {DateTime.UtcNow.Year} Cyberius. Это автоматическое письмо, не отвечайте на него.
                     </td>
                   </tr>

                 </table>
               </td>
             </tr>
           </table>
         </body>
         </html>
         """;
}