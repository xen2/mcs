/*
 * Generated automatically: do not edit this file.
 */

using System;
using System.Runtime.InteropServices;

namespace System.Private {

[CLSCompliant(false)]
public struct stat {
	public uint st_dev;
	public uint st_mode;
	public uint st_nlink;
	public uint st_uid;
	public uint st_gid;
	public long st_size;
	public uint st_atime;
	public uint st_mtime;
	public uint st_ctime;
};

public class Wrapper {

	public const int SEEK_SET             = 0;
	public const int SEEK_CUR             = 1;
	public const int SEEK_END             = 2;
	public const int O_RDONLY             = 0x00000000;
	public const int O_WRONLY             = 0x00000001;
	public const int O_RDWR               = 0x00000002;
	public const int O_CREAT              = 0x00000040;
	public const int O_EXCL               = 0x00000080;
	public const int O_NOCTTY             = 0x00000100;
	public const int O_TRUNC              = 0x00000200;
	public const int O_SYNC               = 0x00001000;
	public const int O_APPEND             = 0x00000400;
	public const int STDIN_FILENO         = 0x00000000;
	public const int STDOUT_FILENO        = 0x00000001;
	public const int STDERR_FILENO        = 0x00000002;
	public const int S_IFMT               = 0x0000f000;
	public const int S_IFSOCK             = 0x0000c000;
	public const int S_IFLNK              = 0x0000a000;
	public const int S_IFREG              = 0x00008000;
	public const int S_IFBLK              = 0x00006000;
	public const int S_IFDIR              = 0x00004000;
	public const int S_IFCHR              = 0x00002000;
	public const int S_IFIFO              = 0x00001000;
	public const int S_ISUID              = 0x00000800;
	public const int S_ISGID              = 0x00000400;
	public const int S_ISVTX              = 0x00000200;
	public const int S_IRWXU              = 0x000001c0;
	public const int S_IRUSR              = 0x00000100;
	public const int S_IWUSR              = 0x00000080;
	public const int S_IXUSR              = 0x00000040;
	public const int S_IRWXG              = 0x00000038;
	public const int S_IRGRP              = 0x00000020;
	public const int S_IWGRP              = 0x00000010;
	public const int S_IXGRP              = 0x00000008;
	public const int S_IRWXO              = 0x00000007;
	public const int S_IROTH              = 0x00000004;
	public const int S_IWOTH              = 0x00000002;
	public const int S_IXOTH              = 0x00000001;
	public const int EPERM                = 1;
	public const int ENOENT               = 2;
	public const int ESRCH                = 3;
	public const int EINTR                = 4;
	public const int EIO                  = 5;
	public const int ENXIO                = 6;
	public const int E2BIG                = 7;
	public const int ENOEXEC              = 8;
	public const int EBADF                = 9;
	public const int ECHILD               = 10;
	public const int EAGAIN               = 11;
	public const int ENOMEM               = 12;
	public const int EACCES               = 13;
	public const int EFAULT               = 14;
	public const int ENOTBLK              = 15;
	public const int EBUSY                = 16;
	public const int EEXIST               = 17;
	public const int EXDEV                = 18;
	public const int ENODEV               = 19;
	public const int EISDIR               = 21;
	public const int EINVAL               = 22;
	public const int ENFILE               = 23;
	public const int EMFILE               = 24;
	public const int ENOTTY               = 25;
	public const int ETXTBSY              = 26;
	public const int EFBIG                = 27;
	public const int ENOSPC               = 28;
	public const int ESPIPE               = 29;
	public const int EROFS                = 30;
	public const int EMLINK               = 31;
	public const int EPIPE                = 32;
	public const int EDOM                 = 33;
	public const int ERANGE               = 34;
	public const int EDEADLK              = 35;
	public const int ENAMETOOLONG         = 36;
	public const int ENOLCK               = 37;
	public const int ENOSYS               = 38;
	public const int ENOTEMPTY            = 39;
	public const int ELOOP                = 40;
	public const int EWOULDBLOCK          = 11;
	public const int ENOMSG               = 42;
	public const int EIDRM                = 43;
	public const int ECHRNG               = 44;
	public const int EL2NSYNC             = 45;
	public const int EL3HLT               = 46;
	public const int EL3RST               = 47;
	public const int ELNRNG               = 48;
	public const int EUNATCH              = 49;
	public const int ENOCSI               = 50;
	public const int EL2HLT               = 51;
	public const int EBADE                = 52;
	public const int EBADR                = 53;
	public const int EXFULL               = 54;
	public const int ENOANO               = 55;
	public const int EBADRQC              = 56;
	public const int EBADSLT              = 57;
	public const int EDEADLOCK            = 35;
	public const int EBFONT               = 59;
	public const int ENOSTR               = 60;
	public const int ENODATA              = 61;
	public const int ETIME                = 62;
	public const int ENOSR                = 63;
	public const int ENONET               = 64;
	public const int ENOPKG               = 65;
	public const int EREMOTE              = 66;
	public const int ENOLINK              = 67;
	public const int EADV                 = 68;
	public const int ESRMNT               = 69;
	public const int ECOMM                = 70;
	public const int EPROTO               = 71;
	public const int EMULTIHOP            = 72;
	public const int EDOTDOT              = 73;
	public const int EBADMSG              = 74;
	public const int ENOTUNIQ             = 76;
	public const int EBADFD               = 77;
	public const int EREMCHG              = 78;
	public const int ELIBACC              = 79;
	public const int ELIBBAD              = 80;
	public const int ELIBSCN              = 81;
	public const int ELIBMAX              = 82;
	public const int ELIBEXEC             = 83;
	public const int EUSERS               = 87;
	public const int ENOTSOCK             = 88;
	public const int EDESTADDRREQ         = 89;
	public const int EMSGSIZE             = 90;
	public const int EPROTOTYPE           = 91;
	public const int ENOPROTOOPT          = 92;
	public const int EPROTONOSUPPORT      = 93;
	public const int ESOCKTNOSUPPORT      = 94;
	public const int EOPNOTSUPP           = 95;
	public const int EPFNOSUPPORT         = 96;
	public const int EAFNOSUPPORT         = 97;
	public const int EADDRINUSE           = 98;
	public const int EADDRNOTAVAIL        = 99;
	public const int ENETDOWN             = 100;
	public const int ENETUNREACH          = 101;
	public const int ENETRESET            = 102;
	public const int ECONNABORTED         = 103;
	public const int ECONNRESET           = 104;
	public const int ENOBUFS              = 105;
	public const int EISCONN              = 106;
	public const int ENOTCONN             = 107;
	public const int ESHUTDOWN            = 108;
	public const int ETOOMANYREFS         = 109;
	public const int ETIMEDOUT            = 110;
	public const int ECONNREFUSED         = 111;
	public const int EHOSTDOWN            = 112;
	public const int EHOSTUNREACH         = 113;
	public const int EALREADY             = 114;
	public const int EINPROGRESS          = 115;
	public const int ESTALE               = 116;
	public const int EDQUOT               = 122;
	public const int ENOMEDIUM            = 123;
	public const int ENOTDIR              = 20;


	[DllImport("monowrapper", EntryPoint="mono_wrapper_seek", CharSet=CharSet.Ansi)]
	public unsafe static extern long seek (IntPtr fd, long offset, int whence);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_read", CharSet=CharSet.Ansi)]
	[CLSCompliant(false)]
	public unsafe static extern int read (IntPtr fd, void * buf, int count);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_write", CharSet=CharSet.Ansi)]
	[CLSCompliant(false)]
	public unsafe static extern int write (IntPtr fd, void * buf, int count);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_fstat", CharSet=CharSet.Ansi)]
	[CLSCompliant(false)]
	public unsafe static extern int fstat (IntPtr fd, stat * buf);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_ftruncate", CharSet=CharSet.Ansi)]
	public unsafe static extern int ftruncate (IntPtr fd, long length);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_open", CharSet=CharSet.Ansi)]
	public unsafe static extern IntPtr open (string path, int flags, int mode);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_close", CharSet=CharSet.Ansi)]
	public unsafe static extern int close (IntPtr fd);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_stat", CharSet=CharSet.Ansi)]
	[CLSCompliant(false)]
	public unsafe static extern int stat (string path, stat * buf);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_unlink", CharSet=CharSet.Ansi)]
	public unsafe static extern int unlink (string path);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_opendir", CharSet=CharSet.Ansi)]
	public unsafe static extern IntPtr opendir (string path);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_readdir", CharSet=CharSet.Ansi)]
	public unsafe static extern string readdir (IntPtr dir);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_closedir", CharSet=CharSet.Ansi)]
	public unsafe static extern int closedir (IntPtr dir);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_getenv", CharSet=CharSet.Ansi)]
	public unsafe static extern IntPtr getenv (string variable);

	[DllImport("monowrapper", EntryPoint="mono_wrapper_environ", CharSet=CharSet.Ansi)]
	public unsafe static extern IntPtr environ ();

}
}
