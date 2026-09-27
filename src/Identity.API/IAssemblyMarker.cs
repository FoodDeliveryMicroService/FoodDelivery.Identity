namespace Identity.API;

/// <summary>
/// Marker type used by WebApplicationFactory&lt;T&gt; in the test projects to
/// locate the Identity.API assembly without needing to make the top-level
/// Program class public. Drop this file anywhere in Identity.API (e.g. next
/// to Program.cs) and rebuild.
/// </summary>
public interface IAssemblyMarker;
