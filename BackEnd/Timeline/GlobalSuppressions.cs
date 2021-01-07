// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "This is not a UI application.")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is not bad.")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Generated error response identifiers.", Scope = "type", Target = "~T:Timeline.Models.Http.ErrorResponse")]
[assembly: SuppressMessage("Naming", "CA1724:Type names should not match namespaces", Justification = "Generated error response identifiers.", Scope = "type", Target = "~T:Timeline.Models.Http.ErrorResponse")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Generated error response.", Scope = "type", Target = "~T:Timeline.Models.Http.ErrorResponse")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "That's unnecessary.")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Redundant")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No localization demand.")]
[assembly: SuppressMessage("Design", "CA1054:URI-like parameters should not be strings")]
