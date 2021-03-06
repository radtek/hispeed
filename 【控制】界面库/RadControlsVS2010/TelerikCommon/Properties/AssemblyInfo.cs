using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

#if EVALUATION

[assembly: AssemblyTitle("Telerik.Common Trial Version")]
[assembly: AssemblyDescription("Trial Version")]

#else

[assembly: AssemblyTitle("Telerik.Common")]
[assembly: AssemblyDescription("")]

#endif


[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Telerik Corporation")]
[assembly: AssemblyProduct("Telerik.Common")]
[assembly: AssemblyCopyright(Telerik.WinControls.VersionNumber.CopyrightText)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: InternalsVisibleTo("UISerialTests,PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("RadControlsUITests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.UI, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.Docking, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("VisualStyleBuilder, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("VisualStyleBuilder.Design, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.GridView, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.UI.Design, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.RadDock, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("Telerik.WinControls.Scheduler, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("TelerikData, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("QuickStart, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("RadFormTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("GridControlDesignerSpike, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]
[assembly: InternalsVisibleTo("RadControlSpy, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a1d418f82d72dc0e2f407fa60bf0b04797d423db31b4e5a4a99c53908c7f9428eb9dc9a3d20533fe88893a5541de45059e3267320fb19c95bb256855a6a7019eae0538e8af2682d78fc33217dbc465cc495301cf792ab97482ebc9d32bd5be4b83de352160d05ed1be61b21ee3c602d7c507fdda4bd2d25d830660ba1300c9de")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b0c8ebc2-742e-47c8-bea6-6128037f1a12")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion(Telerik.WinControls.VersionNumber.Number)]
[assembly: AssemblyFileVersion(Telerik.WinControls.VersionNumber.Number)]

#if NANT
[assembly: AssemblyKeyFile(@"..\RadControl.snk")]
#endif

namespace Telerik.WinControls
{
    /// <summary>
    /// Defines the current version of the RadControls for WinForms
    /// </summary>
    public sealed class VersionNumber
    {
        public const string Number = "2011.2.11.0712";
        public const string CopyrightText = "Copyright ?Telerik 2011";
        public const string MarketingVersion = "Q2 2011";
    }
}
