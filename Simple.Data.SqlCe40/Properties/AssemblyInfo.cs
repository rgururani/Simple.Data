﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Simple.Data.SqlCe40;

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2aa66263-94ee-46d5-bb0f-bae22bcec3e8")]

[assembly: InternalsVisibleTo("Simple.Data.SqlCe40Test")]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
[assembly: AllowPartiallyTrustedCallers]

[assembly: SqlCe40Provider]