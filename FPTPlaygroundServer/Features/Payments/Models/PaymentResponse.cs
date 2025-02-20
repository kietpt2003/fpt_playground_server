namespace FPTPlaygroundServer.Features.Payments.Models;

public record PaymentResponse(
    int Error,
    string Message,
    object? Data
);
