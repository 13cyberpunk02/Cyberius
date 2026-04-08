namespace Cyberius.Application.Features.Newsletter.DTOs;

public record SendNewsletterRequest(string Subject, string HtmlBody);