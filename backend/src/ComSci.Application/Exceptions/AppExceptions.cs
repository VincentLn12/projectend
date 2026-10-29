namespace ComSci.Application.Exceptions;

/// <summary>The requested record does not exist (or is soft-deleted). Mapped to HTTP 404.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>The request is well-formed but breaks a business rule. Mapped to HTTP 400.</summary>
public class BadRequestException(string message) : Exception(message);
