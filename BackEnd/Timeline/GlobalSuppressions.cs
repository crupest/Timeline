﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "This is not a UI application.")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is not bad.")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Redundant")]
