namespace ControlService.Application.Common;

/// <summary>The response of a command that has nothing to return, since handlers always return a
/// <c>Result&lt;TResponse&gt;</c>.</summary>
public readonly record struct Unit;
