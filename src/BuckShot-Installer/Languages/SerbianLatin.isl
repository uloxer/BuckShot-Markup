; *** Inno Setup version 6.1.0+ Serbian (Latin) messages ***
;
; To download user-contributed translations of this file, go to:
;   http://www.jrsoftware.org/files/istrans/
;
; Translated by Rancher (theranchcowboy@gmail.com) and Davor (davornik@yahoo.com).
;
; Note: When translating this text, do not add periods (.) to the end of
; messages that didn't have them already, because on those messages Inno
; Setup adds the periods automatically (appending a period would result in
; two periods being displayed).

[LangOptions]
; The following three entries are very important. Be sure to read and
; understand the '[LangOptions] section' topic in the help file.
LanguageName=Srpski
LanguageID=$081a
LanguageCodePage=1250
; If the language you are translating to requires special font faces or
; sizes, uncomment any of the following entries and change them accordingly.
;DialogFontName=
;DialogFontSize=8
;WelcomeFontName=Verdana
;WelcomeFontSize=12
;TitleFontName=Arial
;TitleFontSize=29
;CopyrightFontName=Arial
;CopyrightFontSize=8

[Messages]

; *** Application titles
SetupAppTitle=Instalacija
SetupWindowTitle=Instalacija � %1
UninstallAppTitle=Deinstalacija
UninstallAppFullTitle=Deinstalacija programa %1

; *** Misc. common
InformationTitle=Podaci
ConfirmTitle=Potvrda
ErrorTitle=Gre�ka

; *** SetupLdr messages
SetupLdrStartupMessage=Instalira�ete %1. �elite li da nastavite?
LdrCannotCreateTemp=Ne mogu da napravim privremenu datoteku. Instalacija je prekinuta.
LdrCannotExecTemp=Ne mogu da pokrenem datoteku u privremenoj fascikli. Instalacija je prekinuta.
HelpTextNote=

; *** Startup error messages
LastErrorMessage=%1.%n%nGre�ka %2: %3
SetupFileMissing=Datoteka %1 nedostaje u instalacionoj fascikli. Ispravite problem ili nabavite novi primerak programa.
SetupFileCorrupt=Instalacione datoteke su o�te�ene. Nabavite novi primerak programa.
SetupFileCorruptOrWrongVer=Instalacione datoteke su o�te�ene ili nisu saglasne s ovom verzijom instalacije. Ispravite problem ili nabavite novi primerak programa.
InvalidParameter=Neispravan parametar je prenet na komandnu liniju: %n%n%1
SetupAlreadyRunning=Instalacija je ve� pokrenuta.
WindowsVersionNotSupported=Program ne podr�ava izdanje Windows-a koje koristite.
WindowsServicePackRequired=Program zahteva %1 servisni paket %2 ili noviji.
NotOnThisPlatform=Program ne�e raditi na %1.
OnlyOnThisPlatform=Program �e raditi na %1.
OnlyOnTheseArchitectures=Program se mo�e instalirati samo na izdanjima Windows-a koji rade na slede�im arhitekturama procesora:%n%n%1
WinVersionTooLowError=Program zahteva %1, izdanje %2 ili novije.
WinVersionTooHighError=Program ne mo�ete instalirati na %1 izdanju %2 ili novijem.
AdminPrivilegesRequired=Morate biti prijavljeni kao administrator da biste instalirali program.
PowerUserPrivilegesRequired=Morate biti prijavljeni kao administrator ili ovla��eni korisnik da biste instalirali program.
SetupAppRunningError=Program %1 je trenutno pokrenut.%n%nZatvorite ga i kliknite na dugme �U redu� da nastavite ili �Otka�i� da napustite instalaciju.
UninstallAppRunningError=Program %1 je trenutno pokrenut.%n%nZatvorite ga i kliknite na dugme �U redu� da nastavite ili �Otka�i� da napustite instalaciju.

; *** Startup questions
PrivilegesRequiredOverrideTitle=Odaberite na�in instalacije
PrivilegesRequiredOverrideInstruction=Odaberite na�in instalacije
PrivilegesRequiredOverrideText1=%1 mo�e biti instaliran za sve korisnike (zahteva administrativne privilegije) ili samo za vas.
PrivilegesRequiredOverrideText2=%1 mo�e da se instalira samo za vas ili za sve korisnike (zahteva administrativne privilegije).
PrivilegesRequiredOverrideAllUsers=Instaliraj za &sve korisnike
PrivilegesRequiredOverrideAllUsersRecommended=Instaliraj za &sve korisnike (preporu�eno)
PrivilegesRequiredOverrideCurrentUser=Instaliraj samo za &mene
PrivilegesRequiredOverrideCurrentUserRecommended=Instaliraj samo za &mene (preporu�eno)

; *** Misc. errors
ErrorCreatingDir=Ne mogu da napravim fasciklu �%1�.
ErrorTooManyFilesInDir=Ne mogu da napravim datoteku u fascikli �%1� jer sadr�i previ�e datoteka.

; *** Setup common messages
ExitSetupTitle=Napu�tanje instalacije
ExitSetupMessage=Instalacija nije zavr�ena. Ako sada iza�ete, program ne�e biti instaliran.%n%nInstalaciju mo�ete pokrenuti i dovr�iti nekom dugom prilikom.%n%n�elite li da je zatvorite?
AboutSetupMenuItem=&O programu
AboutSetupTitle=Podaci o programu
AboutSetupMessage=%1 verzija %2%n%3%n%n%1 internet stranica:%n%4
AboutSetupNote=
TranslatorNote=

; *** Buttons
ButtonBack=< &Nazad
ButtonNext=&Dalje >
ButtonInstall=&Instaliraj
ButtonOK=&U redu
ButtonCancel=&Otka�i
ButtonYes=&Da
ButtonYesToAll=D&a za sve
ButtonNo=&Ne
ButtonNoToAll=N&e za sve
ButtonFinish=&Zavr�i
ButtonBrowse=&Pretra�i�
ButtonWizardBrowse=&Pretra�i�
ButtonNewFolder=&Napravi fasciklu

; *** "Select Language" dialog messages
SelectLanguageTitle=Odabir jezika
SelectLanguageLabel=Izaberite jezik za kori��enje tokom instalacije:

; *** Common wizard text
ClickNext=Kliknite na �Dalje� da nastavite ili �Otka�i� da napustite instalaciju.
BeveledLabel=
BrowseDialogTitle=Odabir fascikle
BrowseDialogLabel=Izaberite fasciklu sa spiska i kliknite na �U redu�.
NewFolderName=Nova fascikla

; *** "Welcome" wizard page
WelcomeLabel1=Dobro do�li na instalaciju programa [name]
WelcomeLabel2=Instalira�ete [name/ver] na ra�unar.%n%nPre nego �to nastavite, preporu�ujemo vam da zatvorite sve druge programe.

; *** "Password" wizard page
WizardPassword=Lozinka
PasswordLabel1=Instalacija je za�ti�ena lozinkom.
PasswordLabel3=Unesite lozinku i kliknite na �Dalje� da nastavite. Imajte na umu da je lozinka osetljiva na mala i velika slova.
PasswordEditLabel=&Lozinka:
IncorrectPassword=Navedena lozinka nije ispravna. Poku�ajte ponovo.

; *** "License Agreement" wizard
WizardLicense=Ugovor o licenci
LicenseLabel=Pa�ljivo pro�itajte slede�e pre nego �to nastavite.
LicenseLabel3=Pro�itajte Ugovor o licenci koji se nalazi ispod. Morate prihvatiti uslove ovog ugovora pre nego �to nastavite.
LicenseAccepted=&Prihvatam ugovor
LicenseNotAccepted=&Ne prihvatam ugovor

; *** "Information" wizard pages
WizardInfoBefore=Informacije
InfoBeforeLabel=Pa�ljivo pro�itajte slede�e pre nego �to nastavite.
InfoBeforeClickLabel=Kada budete spremni da nastavite instalaciju, kliknite na �Dalje�.
WizardInfoAfter=Informacije
InfoAfterLabel=Pa�ljivo pro�itajte slede�e pre nego �to nastavite.
InfoAfterClickLabel=Kada budete spremni da nastavite instalaciju, kliknite na �Dalje�.

; *** "User Information" wizard page
WizardUserInfo=Korisni�ki podaci
UserInfoDesc=Unesite svoje podatke.
UserInfoName=&Korisnik:
UserInfoOrg=&Organizacija:
UserInfoSerial=&Serijski broj:
UserInfoNameRequired=Morate navesti ime.

; *** "Select Destination Location" wizard page
WizardSelectDir=Odabir odredi�ne fascikle
SelectDirDesc=Izaberite mesto na kom �elite da instalirate [name].
SelectDirLabel3=Program �e instalirati [name] u slede�u fasciklu.
SelectDirBrowseLabel=Kliknite na �Dalje� da nastavite. Ako �elite da izaberete drugu fasciklu, kliknite na �Potra�i��.
DiskSpaceGBLabel=Potrebno je najmanje [gb] GB slobodnog prostora na disku.
DiskSpaceMBLabel=Potrebno je najmanje [mb] MB slobodnog prostora na disku.
CannotInstallToNetworkDrive=Ne mogu da instaliram na mre�nu jedinicu.
CannotInstallToUNCPath=Ne mogu da instaliram na UNC putanju.
InvalidPath=Morate navesti punu putanju s obele�jem diska (npr.%n%nC:\APP%n%nili putanja u obliku%n%n\\server\share)
InvalidDrive=Disk koji ste izabrali ne postoji ili nije dostupan. Izaberite neki drugi.
DiskSpaceWarningTitle=Nedovoljno prostora na disku
DiskSpaceWarning=Program zahteva najmanje %1 kB slobodnog prostora, a izabrani disk na raspolaganju ima samo %2 kB.%n%n�elite li ipak da nastavite?
DirNameTooLong=Naziv fascikle ili putanja je preduga�ka.
InvalidDirName=Naziv fascikle nije ispravan.
BadDirName32=Naziv fascikle ne sme sadr�ati ni�ta od slede�eg:%n%n%1
DirExistsTitle=Fascikla ve� postoji
DirExists=Fascikla:%n%n%1%n%nve� postoji. �elite li ipak da instalirate program u nju?
DirDoesntExistTitle=Fascikla ne postoji
DirDoesntExist=Fascikla:%n%n%1%n%nne postoji. �elite li da je napravite?

; *** "Select Components" wizard page
WizardSelectComponents=Odabir komponenata
SelectComponentsDesc=Koje komponente �elite da instalirate?
SelectComponentsLabel2=Izaberite komponente koje �elite da instalirate, a o�istite one koje ne �elite. Kliknite na �Dalje� da nastavite.
FullInstallation=Puna instalacija
; if possible don't translate 'Compact' as 'Minimal' (I mean 'Minimal' in your language)
CompactInstallation=Podrazumevana instalacija
CustomInstallation=Prilago�ena instalacija
NoUninstallWarningTitle=Komponente ve� postoje
NoUninstallWarning=Slede�e komponente ve� postoje na ra�unaru:%n%n%1%n%nDe�trikliranje ovih komponenti ih ne�e ukloniti.%n%n�elite li da nastavite?
ComponentSize1=%1 kB
ComponentSize2=%1 MB
ComponentsDiskSpaceGBLabel=Izabrane stavke zahtevaju najmanje [gb] GB slobodnog prostora.
ComponentsDiskSpaceMBLabel=Izabrane stavke zahtevaju najmanje [mb] MB slobodnog prostora.

; *** "Select Additional Tasks" wizard page
WizardSelectTasks=Odabir dodatnih zadataka
SelectTasksDesc=Izaberite neke dodatne zadatke.
SelectTasksLabel2=Izaberite dodatne zadatke koje �elite da izvr�ite pri instaliranju programa [name] i kliknite na �Dalje�.

; *** "Select Start Menu Folder" wizard page
WizardSelectProgramGroup=Odabir fascikle u meniju �Start�
SelectStartMenuFolderDesc=Izaberite mesto na kom �elite da postavite pre�ice.
SelectStartMenuFolderLabel3=Instalacija �e postaviti pre�ice programa u slede�oj fascikli u meniju �Start�.
SelectStartMenuFolderBrowseLabel=Kliknite na �Dalje� da nastavite. Ako �elite da izaberete drugu fasciklu, kliknite na �Potra�i��.
MustEnterGroupName=Morate navesti naziv fascikle.
GroupNameTooLong=Naziv fascikle ili putanja je preduga�ka.
InvalidGroupName=Naziv fascikle nije ispravan.
BadGroupName=Naziv fascikle ne sme sadr�ati ni�ta od slede�eg:%n%n%1
NoProgramGroupCheck2=N&e pravi fasciklu u meniju �Start�

; *** "Ready to Install" wizard page
WizardReady=Instalacija je spremna
ReadyLabel1=Program je spreman da instalira [name] na ra�unar.
ReadyLabel2a=Kliknite na �Instaliraj� da zapo�nete instalaciju ili �Nazad� da ponovo pregledate i promenite pojedine postavke.
ReadyLabel2b=Kliknite na �Instaliraj� da zapo�nete instalaciju.
ReadyMemoUserInfo=Korisni�ki podaci:
ReadyMemoDir=Odredi�na fascikla:
ReadyMemoType=Vrsta instalacije:
ReadyMemoComponents=Izabrane komponente:
ReadyMemoGroup=Fascikla u meniju �Start�:
ReadyMemoTasks=Dodatni zadaci:

; *** TDownloadWizardPage wizard page and DownloadTemporaryFile
DownloadingLabel=Preuzimanje dodatnih datoteka...
ButtonStopDownload=&Zaustavi preuzimanje
StopDownload=Da li ste sigurni da �elite da zaustavite preuzimanje?
ErrorDownloadAborted=Preuzimanje je prekinuto
ErrorDownloadFailed=Preuzimanje nije uspelo: %1 %2
ErrorDownloadSizeFailed=Dobijanje veli�ine nije uspelo: %1 %2
ErrorFileHash1=He� datoteke nije uspeo: %1
ErrorFileHash2=Neispravan he� datoteke: o�ekivan %1, prona�en %2
ErrorProgress=Neispravan napredak: %1 od %2
ErrorFileSize=Neispravna veli�ina datoteke: o�ekivan %1, prona�en %2

; *** "Preparing to Install" wizard page
WizardPreparing=Priprema za instalaciju
PreparingDesc=Program se priprema da instalira [name] na ra�unar.
PreviousInstallNotCompleted=Instalacija ili deinstalacija prethodnog programa nije zavr�ena. Potrebno je da ponovo pokrenete ra�unar da bi se instalacija zavr�ila.%n%nNakon ponovnog pokretanja, otvorite instalaciju i instalirajte program [name].
CannotContinue=Ne mogu da nastavim instalaciju. Kliknite na �Otka�i� da iza�ete.
ApplicationsFound=Slede�i programi koriste datoteke koje treba da a�urira instalacioni program. Preporu�ujemo vam da dozvolite instalacionom programu da zatvori ove programe.
ApplicationsFound2=Slede�i programi koriste datoteke koje treba da a�urira instalacioni program. Preporu�ujemo vam da dozvolite instalacionom programu da zatvori ove programe. Nakon �to se instalacija zavr�i, instalacioni program �e poku�ati da ponovo pokrene zatvorene programe.
CloseApplications=&Zatvori programe
DontCloseApplications=&Ne zatvaraj programe
ErrorCloseApplications=Ne mogu da zatvorim sve programe. Pre nego �to nastavite, preporu�ujemo vam da zatvorite sve programe koji koriste datoteke koje treba da a�urira instalacioni program.
PrepareToInstallNeedsRestart=Instalacija mora da ponovo pokrene ra�unar. Nakon ponovnog pokretanja ra�unara, ponovo pokrenite instalaciju da biste dovr�ili instalaciju [name].%n%n�elite li da ponovo pokrenete ra�unar sada?

; *** "Installing" wizard page
WizardInstalling=Instaliranje
InstallingLabel=Sa�ekajte da se [name] instalira na ra�unar.

; *** "Setup Completed" wizard page
FinishedHeadingLabel=[name] � zavr�etak instalacije
FinishedLabelNoIcons=Instaliranje programa [name] je zavr�eno.
FinishedLabel=Instaliranje programa [name] je zavr�eno. Mo�ete ga pokrenuti preko postavljenih ikona.
ClickFinish=Kliknite na �Zavr�i� da iza�ete.
FinishedRestartLabel=Potrebno je ponovno pokretanje ra�unara da bi se instalacija zavr�ila. �elite li da ga ponovo pokrenete?
FinishedRestartMessage=Potrebno je ponovno pokretanje ra�unara da bi se instalacija zavr�ila.%n%n�elite li da ga ponovo pokrenete?
ShowReadmeCheck=Da, �elim da pogledam tekstualnu datoteku
YesRadio=&Da, ponovo pokreni ra�unar
NoRadio=&Ne, kasnije �u ga pokrenuti
; used for example as 'Run MyProg.exe'
RunEntryExec=&Pokreni %1
; used for example as 'View Readme.txt'
RunEntryShellExec=Pogledaj %1

; *** "Setup Needs the Next Disk" stuff
ChangeDiskTitle=Slede�i disk
SelectDiskLabel2=Ubacite disk %1 i kliknite na �U redu�.%n%nAko se datoteke na ovom disku mogu prona�i u nekoj drugoj fascikli, unesite odgovaraju�u putanju ili kliknite na �Potra�i��.
PathLabel=&Putanja:
FileNotInDir2=Datoteka �%1� se ne nalazi u �%2�. Ubacite pravi disk ili izaberite drugu fasciklu.
SelectDirectoryLabel=Izaberite mesto slede�eg diska.

; *** Installation phase messages
SetupAborted=Instalacija nije zavr�ena.%n%nIspravite problem i pokrenite je ponovo.
AbortRetryIgnoreSelectAction=Odaberite radnju
AbortRetryIgnoreRetry=&Poku�aj opet
AbortRetryIgnoreIgnore=&Zanemari gre�ku i nastavi
AbortRetryIgnoreCancel=Prekini instalaciju

; *** Installation status messages
StatusClosingApplications=Zatvaram programe�
StatusCreateDirs=Pravim fascikle�
StatusExtractFiles=Raspakujem datoteke�
StatusCreateIcons=Postavljam pre�ice�
StatusCreateIniEntries=Postavljam INI unose�
StatusCreateRegistryEntries=Postavljam unose u registar�
StatusRegisterFiles=Upisujem datoteke�
StatusSavingUninstall=�uvam podatke o deinstalaciji�
StatusRunProgram=Zavr�avam instalaciju�
StatusRestartingApplications=Ponovo pokre�em programe�
StatusRollback=Poni�tavam izmene�

; *** Misc. errors
ErrorInternal2=Unutra�nja gre�ka: %1
ErrorFunctionFailedNoCode=%1 neuspeh
ErrorFunctionFailed=%1 neuspeh; kod %2
ErrorFunctionFailedWithMessage=%1 neuspeh; kod %2.%n%3
ErrorExecutingProgram=Ne mogu da pokrenem datoteku:%n%1

; *** Registry errors
ErrorRegOpenKey=Gre�ka pri otvaranju unosa u registru:%n%1\%2
ErrorRegCreateKey=Gre�ka pri stvaranju unosa u registru:%n%1\%2
ErrorRegWriteKey=Gre�ka pri upisivanju unosa u registar:%n%1\%2

; *** INI errors
ErrorIniEntry=Gre�ka pri stvaranju INI unosa u datoteci �%1�.

; *** File copying errors
FileAbortRetryIgnoreSkipNotRecommended=&Presko�ite ovu datoteku (ne preporu�uje se)
FileAbortRetryIgnoreIgnoreNotRecommended=&Zanemari gre�ku i nastavi (ne preporu�uje se)
SourceIsCorrupted=Izvorna datoteka je o�te�ena
SourceDoesntExist=Izvorna datoteka �%1� ne postoji
ExistingFileReadOnly2=Postoje�a datoteka ne mo�e da se zameni jer je samo za �itanje.
ExistingFileReadOnlyRetry=&Uklonite atribut samo za �itanje i poku�ajte ponovo
ExistingFileReadOnlyKeepExisting=&Zadr�ite postoje�u datoteku
ErrorReadingExistingDest=Do�lo je do gre�ke pri poku�aju �itanja postoje�e datoteke:
FileExistsSelectAction=Odaberi akciju
FileExists2=Datoteka ve� postoji.
FileExistsOverwriteExisting=&Zameni postoje�u datoteku
FileExistsKeepExisting=&Zadr�i postoje�u datoteku
FileExistsOverwriteOrKeepAll=&Uradi ovo i ubudu�e
ExistingFileNewerSelectAction=Odaberi akciju
ExistingFileNewer2=Postoje�a datoteka je novija od one koju treba instalirati.
ExistingFileNewerOverwriteExisting=&Zameni postoje�u datoteku
ExistingFileNewerKeepExisting=&Zadr�i postoje�u datoteku (preporu�eno)
ExistingFileNewerOverwriteOrKeepAll=&Uradi ovo i ubudu�e
ErrorChangingAttr=Do�lo je do gre�ke pri izmeni osobine slede�e datoteke:
ErrorCreatingTemp=Do�lo je do gre�ke pri stvaranju datoteke u odredi�noj fascikli:
ErrorReadingSource=Do�lo je do gre�ke pri �itanju izvorne datoteke:
ErrorCopying=Do�lo je do gre�ke pri umno�avanju datoteke:
ErrorReplacingExistingFile=Do�lo je do gre�ke pri zameni postoje�e datoteke:
ErrorRestartReplace=Ne mogu da zamenim:
ErrorRenamingTemp=Do�lo je do gre�ke pri preimenovanju datoteke u odredi�noj fascikli:
ErrorRegisterServer=Ne mogu da upi�em DLL/OCX: %1
ErrorRegSvr32Failed=RegSvr32 nije uspeo. Gre�ka %1
ErrorRegisterTypeLib=Ne mogu da upi�em biblioteku tipova: %1

; *** Uninstall display name markings
; used for example as 'My Program (32-bit)'
UninstallDisplayNameMark=%1 (%2)
; used for example as 'My Program (32-bit, All users)'
UninstallDisplayNameMarks=%1 (%2, %3)
UninstallDisplayNameMark32Bit=32-bit
UninstallDisplayNameMark64Bit=64-bit
UninstallDisplayNameMarkAllUsers=Svi korisnici
UninstallDisplayNameMarkCurrentUser=Trenutni korisnik

; *** Post-installation errors
ErrorOpeningReadme=Do�lo je do gre�ke pri otvaranju tekstualne datoteke.
ErrorRestartingComputer=Ne mogu ponovo da pokrenem ra�unar. Uradite to sami.

; *** Uninstaller messages
UninstallNotFound=Datoteka �%1� ne postoji. Ne mogu da deinstaliram program.
UninstallOpenError=Datoteka �%1� ne mo�e da se otvori. Ne mogu da deinstaliram program.
UninstallUnsupportedVer=Izve�taj �%1� je u neprepoznatljivom formatu. Ne mogu da deinstaliram program.
UninstallUnknownEntry=Nepoznat unos (%1) se pojavio u izve�taju deinstalacije.
ConfirmUninstall=�elite li da deinstalirate %1 i sve njegove komponente?
UninstallOnlyOnWin64=Program se mo�e deinstalirati samo na 64-bitnom vindousu.
OnlyAdminCanUninstall=Program mo�e deinstalirati samo korisnik s administratorskim pravima.
UninstallStatusLabel=Sa�ekajte da se %1 deinstalira sa ra�unara.
UninstalledAll=%1 je deinstaliran sa ra�unara.
UninstalledMost=%1 je deinstaliran.%n%nNeke komponente ipak morati sami obrisati.
UninstalledAndNeedsRestart=Potrebno je ponovno pokretanje ra�unara da bi se instalacija zavr�ila.%n%n�elite li da ponovo pokrenete ra�unar?
UninstallDataCorrupted=Datoteka �%1� je o�te�ena. Ne mogu da deinstaliram program.

; *** Uninstallation phase messages
ConfirmDeleteSharedFileTitle=Brisanje deljene datoteke
ConfirmDeleteSharedFile2=Sistem je prijavio da slede�u deljenu datoteku vi�e ne koristi nijedan program. �elite li da je uklonite?%n%nAko nekim programima i dalje treba ova datoteka a ona je obrisana, ti programi mo�da ne�e ispravno raditi. Ako niste sigurni �ta da radite, kliknite na �Ne�. Ostavljanje datoteke na disku ne�e prouzrokovati nikakvu �tetu.
SharedFileNameLabel=Naziv datoteke:
SharedFileLocationLabel=Putanja:
WizardUninstalling=Stanje deinstalacije
StatusUninstalling=Deinstaliram %1�

; *** Shutdown block reasons
ShutdownBlockReasonInstallingApp=Instaliram %1.
ShutdownBlockReasonUninstallingApp=Deinstaliram %1.

; The custom messages below aren't used by Setup itself, but if you make
; use of them in your scripts, you'll want to translate them.

[CustomMessages]

NameAndVersion=%1 verzija %2
AdditionalIcons=Dodatne ikone:
CreateDesktopIcon=&Postavi ikonu na radnu povr�inu
CreateQuickLaunchIcon=P&ostavi ikonu na traku za brzo pokretanje
ProgramOnTheWeb=%1 na internetu
UninstallProgram=Deinstaliraj %1
LaunchProgram=Pokreni %1
AssocFileExtension=&Pove�i %1 sa formatom %2
AssocingFileExtension=Povezujem %1 sa formatom %2�
AutoStartProgramGroupDescription=Pokretanje:
AutoStartProgram=Automatski pokreni %1
AddonHostProgramNotFound=%1 se ne nalazi u navedenoj fascikli.%n%n�elite li ipak da nastavite?