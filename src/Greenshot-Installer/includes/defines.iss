#define ExeName "BuckShot"
; Basic build version determined by nerdbank gitversioning, e.g. 1.2.345
#define Version GetEnv('BuildVersionSimple')
; Build version with optional suffix depending on branch, e.g. 1.2.345-g1tc033174ef
#define VersionEnhanced GetEnv('BuildVersionEnhanced')
#define SolutionDir ".."
#define GreenshotProjectDir "..\Greenshot"
#define LanguagesDir "..\..\installer\payload\Languages"
#define BinDir "bin\Release\net480"
; Greenshot Light (setup-light.iss) installs its own build of BuckShot.exe, see Greenshot.csproj
#ifndef ReleaseDir
  #define ReleaseDir "..\..\installer\payload"
#endif
#define PluginDir "..\..\installer\payload\Plugins"
#define CertumThumbprint GetEnv('CertumThumbprint')
#define DefaultInstallFlags "overwritereadonly ignoreversion"

#ifndef AppDisplayName
  #define AppDisplayName "BuckShot Markup"
#endif

#ifndef OutputSuffix
  #define OutputSuffix ""
#endif
