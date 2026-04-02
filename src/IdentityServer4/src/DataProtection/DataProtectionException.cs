#nullable enable
using System;

namespace IdentityServer4.DataProtection;

/// <summary>
/// Exception thrown when an error occurs with data protection
/// </summary>
/// <param name="ex"></param>
/// <param name="msg"></param>
public class DataProtectionException(Exception ex, string msg): Exception(msg, ex);