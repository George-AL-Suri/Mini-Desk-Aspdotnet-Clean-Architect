namespace MiniDesk.Application.Common.Exceptions;

public class ForbiddenException(string message = "You are not allowed to do this.") : Exception(message);