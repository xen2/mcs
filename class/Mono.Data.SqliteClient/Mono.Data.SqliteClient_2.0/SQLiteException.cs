//
// Mono.Data.SqliteClient.SQLiteException.cs
//
// Author(s):
//   Robert Simpson (robert@blackcastlesoft.com)
//
// Adapted and modified for the Mono Project by
//   Marek Habersack (grendello@gmail.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

/********************************************************
 * ADO.NET 2.0 Data Provider for Sqlite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
namespace Mono.Data.SqliteClient
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Data.Common;

#if !PLATFORM_COMPACTFRAMEWORK
  using System.Runtime.Serialization;
#endif

  /// <summary>
  /// Sqlite exception class.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [Serializable]
  public sealed class SqliteException : DbException
#else
  public sealed class SqliteException : Exception
#endif
  {
    private SqliteErrorCode _errorCode;

#if !PLATFORM_COMPACTFRAMEWORK
    private SqliteException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
#endif

    /// <summary>
    /// Public constructor for generating a Sqlite error given the base error code
    /// </summary>
    /// <param name="errorCode">The Sqlite error code to report</param>
    /// <param name="extendedInformation">Extra text to go along with the error message text</param>
    public SqliteException(int errorCode, string extendedInformation)
      : base(GetStockErrorMessage(errorCode, extendedInformation))
    {
      _errorCode = (SqliteErrorCode)errorCode;
    }

    /// <summary>
    /// Various public constructors that just pass along to the base Exception
    /// </summary>
    /// <param name="message">Passed verbatim to Exception</param>
    public SqliteException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// Various public constructors that just pass along to the base Exception
    /// </summary>
    public SqliteException()
    {
    }

    /// <summary>
    /// Various public constructors that just pass along to the base Exception
    /// <param name="message">Passed to Exception</param>
    /// <param name="innerException">Passed to Exception</param>
    /// </summary>
    public SqliteException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Retrieves the underlying Sqlite error code for this exception
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    public new SqliteErrorCode ErrorCode
#else
    public SqliteErrorCode ErrorCode
#endif
    {
      get { return _errorCode; }
    }

    /// <summary>
    /// Initializes the exception class with the Sqlite error code.
    /// </summary>
    /// <param name="errorCode">The Sqlite error code</param>
    /// <param name="errorMessage">A detailed error message</param>
    /// <returns>An error message string</returns>
    private static string GetStockErrorMessage(int errorCode, string errorMessage)
    {
      if (errorMessage == null) errorMessage = "";

      if (errorMessage.Length > 0)
        errorMessage = "\r\n" + errorMessage;

      if (errorCode < 0 || errorCode >= _errorMessages.Length)
        errorCode = 1;

      return _errorMessages[errorCode] + errorMessage;
    }

    private static string[] _errorMessages = {
      "Sqlite OK",
      "Sqlite error",
      "An internal logic error in Sqlite",
      "Access permission denied",
      "Callback routine requested an abort",
      "The database file is locked",
      "A table in the database is locked",
      "malloc() failed",
      "Attempt to write a read-only database",
      "Operation terminated by sqlite3_interrupt()",
      "Some kind of disk I/O error occurred",
      "The database disk image is malformed",
      "Table or record not found",
      "Insertion failed because the database is full",
      "Unable to open the database file",
      "Database lock protocol error",
      "Database is empty",
      "The database schema changed",
      "Too much data for one row of a table",
      "Abort due to constraint violation",
      "Data type mismatch",
      "Library used incorrectly",
      "Uses OS features not supported on host",
      "Authorization denied",
      "Auxiliary database format error",
      "2nd parameter to sqlite3_bind() out of range",
      "File opened that is not a database file",
    };
  }

  /// <summary>
  /// Sqlite error codes
  /// </summary>
  public enum SqliteErrorCode
  {
    /// <summary>
    /// Success
    /// </summary>
    Ok = 0,
    /// <summary>
    /// SQL error or missing database
    /// </summary>
    Error,
    /// <summary>
    /// Internal logic error in Sqlite
    /// </summary>
    Internal,
    /// <summary>
    /// Access permission denied
    /// </summary>
    Perm,
    /// <summary>
    /// Callback routine requested an abort
    /// </summary>
    Abort,
    /// <summary>
    /// The database file is locked
    /// </summary>
    Busy,
    /// <summary>
    /// A table in the database is locked
    /// </summary>
    Locked,
    /// <summary>
    /// malloc() failed
    /// </summary>
    NoMem,
    /// <summary>
    /// Attempt to write a read-only database
    /// </summary>
    ReadOnly,
    /// <summary>
    /// Operation terminated by sqlite3_interrupt()
    /// </summary>
    Interrupt,
    /// <summary>
    /// Some kind of disk I/O error occurred
    /// </summary>
    IOErr,
    /// <summary>
    /// The database disk image is malformed
    /// </summary>
    Corrupt,
    /// <summary>
    /// Table or record not found
    /// </summary>
    NotFound,
    /// <summary>
    /// Insertion failed because database is full
    /// </summary>
    Full,
    /// <summary>
    /// Unable to open the database file
    /// </summary>
    CantOpen,
    /// <summary>
    /// Database lock protocol error
    /// </summary>
    Protocol,
    /// <summary>
    /// Database is empty
    /// </summary>
    Empty,
    /// <summary>
    /// The database schema changed
    /// </summary>
    Schema,
    /// <summary>
    /// Too much data for one row of a table
    /// </summary>
    TooBig,
    /// <summary>
    /// Abort due to constraint violation
    /// </summary>
    Constraint,
    /// <summary>
    /// Data type mismatch
    /// </summary>
    Mismatch,
    /// <summary>
    /// Library used incorrectly
    /// </summary>
    Misuse,
    /// <summary>
    /// Uses OS features not supported on host
    /// </summary>
    NOLFS,
    /// <summary>
    /// Authorization denied
    /// </summary>
    Auth,
    /// <summary>
    /// Auxiliary database format error
    /// </summary>
    Format,
    /// <summary>
    /// 2nd parameter to sqlite3_bind out of range
    /// </summary>
    Range,
    /// <summary>
    /// File opened that is not a database file
    /// </summary>
    NotADatabase,
    /// <summary>
    /// sqlite3_step() has another row ready
    /// </summary>
    Row = 100,
    /// <summary>
    /// sqlite3_step() has finished executing
    /// </summary>
    Done = 101,
  }
}
#endif
