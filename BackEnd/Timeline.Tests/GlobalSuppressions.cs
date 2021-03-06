﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "This is not a UI application.")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", Justification = "Tests name have underscores.")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Test may catch all exceptions.")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Test classes can be nested.")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "This is redundant.")]
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Test classes do not need to implement it that way.")]
[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Test classes do not need to implement it that way.")]
[assembly: SuppressMessage("Usage", "CA2234:Pass system uri objects instead of strings", Justification = "I really don't understand this rule.")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Tests do not need make strings resources.")]
[assembly: SuppressMessage("Design", "CA1054:Uri parameters should not be strings", Justification = "That's unnecessary.")]
[assembly: SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "That's unnecessary.")]
[assembly: SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "We use another contract like IAsyncLifetime.")]
