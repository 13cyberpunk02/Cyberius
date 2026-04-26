using System.Net;
using Cyberius.Application.Features.Email.Interfaces;
using Cyberius.Domain.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Cyberius.Application.Features.Email.Services;

public sealed class GmailEmailService(IOptions<EmailSettings> options) : IEmailService
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendPasswordResetAsync(
        string toEmail, string toName, string resetLink, CancellationToken ct = default)
    {
        await SendAsync(toEmail, toName,
            "Подтверждение эл. почты — Cyberius",
            BuildConfirmEmailHtml(toName, resetLink), ct);
    }

    private async Task SendAsync(
        string toEmail, string toName, string subject, string body, CancellationToken ct)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        client.LocalEndPoint = new IPEndPoint(IPAddress.Any, 0);
        
        await client.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls,
            ct);

        await client.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    public async Task SendEmailConfirmationAsync(
        string toEmail, string toName, string confirmLink, CancellationToken ct = default)
    {
        await SendAsync(toEmail, toName,
            "Подтверждение эл. почты — Cyberius",
            BuildConfirmEmailHtml(toName, confirmLink), ct);
    }

    public async Task SendSubscriptionConfirmationAsync(
        string toEmail, string unsubToken, CancellationToken ct = default)
    {
        var unsubLink = $"/unsubscribe?token={unsubToken}";
        var body = $"""
                    <!DOCTYPE html><html lang="ru"><head><meta charset="UTF-8"/></head>
                    <body style="margin:0;padding:0;background:#0a0f1e;font-family:'Segoe UI',Arial,sans-serif;">
                      <table width="100%" cellpadding="0" cellspacing="0">
                        <tr><td align="center" style="padding:40px 20px;">
                          <table width="600" cellpadding="0" cellspacing="0"
                            style="background:#0d1b2e;border-radius:16px;border:1px solid rgba(255,255,255,0.08);">
                            <tr>
                              <td style="background:linear-gradient(135deg,#0ca2e7,#818cf8);padding:32px;text-align:center;">
                                <h1 style="margin:0;color:#fff;font-size:24px;font-weight:900;">Cyberius</h1>
                                <p style="margin:4px 0 0;color:rgba(255,255,255,0.8);font-size:13px;">.NET & Angular</p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:40px 32px;">
                                <h2 style="margin:0 0 16px;color:#e2e8f0;font-size:20px;">✅ Вы подписаны!</h2>
                                <p style="margin:0 0 24px;color:#94a3b8;font-size:14px;line-height:1.6;">
                                  Отлично! Теперь вы будете получать уведомления о новых статьях
                                  по C#, .NET и Angular. Обещаем — только полезный контент, никакого спама.
                                </p>
                                <p style="margin:0;color:#64748b;font-size:12px;">
                                  Если вы хотите отписаться: 
                                  <a href="{unsubLink}" style="color:#0ca2e7;">нажмите здесь</a>
                                </p>
                              </td>
                            </tr>
                            <tr>
                              <td style="padding:20px 32px;border-top:1px solid rgba(255,255,255,0.06);
                                          text-align:center;color:#475569;font-size:11px;">
                                © {DateTime.UtcNow.Year} Cyberius. 
                                <a href="{unsubLink}" style="color:#475569;">Отписаться</a>
                              </td>
                            </tr>
                          </table>
                        </td></tr>
                      </table>
                    </body></html>
                    """;

        await SendAsync(toEmail, toEmail, "Вы подписались на Cyberius 🎉", body, ct);
    }

    public async Task SendNewsletterAsync(
        string toEmail, string subject, string htmlBody,
        string unsubToken, CancellationToken ct = default)
    {
        var unsubLink = $"http://localhost:8080/unsubscribe?token={unsubToken}";
        var fullBody = htmlBody.Replace("{{UNSUB_LINK}}", unsubLink);

        await SendAsync(toEmail, toEmail, subject, fullBody, ct);
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
                         Спасибо за регистрацию на Cyberius. Подтвердите ваш эл. почту чтобы начать пользоваться сайтом.
                         Ссылка действительна <strong style="color:#e2e8f0;">24 часа</strong>.
                       </p>
                       <div style="text-align:center;margin:32px 0;">
                         <a href="{confirmLink}"
                           style="display:inline-block;background:linear-gradient(135deg,#0ca2e7,#818cf8);
                                  color:#fff;text-decoration:none;padding:14px 32px;
                                  border-radius:12px;font-size:15px;font-weight:700;">
                           Подтвердить эл. почту
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