# BuckShot Self-Service

Self-Service is the window where BuckShot checks itself: is everything set up so BuckShot works as expected, where are its
files, who blocks the clipboard, who takes the Print key, and is the installation complete. Most checks come with a fix.

This document has three parts:

- [For users](#for-users): how to open Self-Service and what every section shows and does.
- [For developers](#for-developers): the Debug-only Integration section, and how to run the browser extension from a
  Debug build.
- [Building a Self-Service section](#building-a-self-service-section): how a section (a "card" in the sidebar) is made,
  step by step.

---

## For users

- [Opening Self-Service](#opening-self-service)
- [The window](#the-window)
- [System & Hardware](#system--hardware)
- [Your setup](#your-setup)
- [Files & Configuration](#files--configuration)
- [The log viewer](#the-log-viewer)
- [Clipboard Diagnostics](#clipboard-diagnostics)
- [Hotkeys & Conflicts](#hotkeys--conflicts)
- [Installation Integrity](#installation-integrity)
- [Delete old Imgur uploads](#delete-old-imgur-uploads)
- [Keys](#keys)
- [Light edition and portable version](#light-edition-and-portable-version)

### Opening Self-Service

| From | How |
|---|---|
| The About window | *About BuckShot* in the menu of the BuckShot icon, then the *Self-Service* button, or press `S` |
| The error window | When BuckShot hits an unexpected error, the error window has a *Self-Service* button |
| A link | [buckshot://self-service](buckshot://self-service), with `?section=` to open one section, e.g. [buckshot://self-service?section=hotkeys](buckshot://self-service?section=hotkeys). The section names are `system`, `setup`, `files`, `clipboard`, `hotkeys`, `checksum` and `imgur`. An unknown name opens *System & Hardware*. Links only work with the full installer, see [buckshot-proxy-and-integration.md](buckshot-proxy-and-integration.md#23-url-protocol-scheme-buckshot). |

There is only one Self-Service window: opening it again (or a link to another section) brings the open window to the front
and switches to that section.

### The window

On the left are the sections. A section with something to fix shows a badge: the number of problems (*Your setup* and
*Hotkeys & Conflicts*), or ⚠️ / ❌ when screenshots probably don't end up where you expect them (*Files & Configuration*). On the right is the selected section. At the bottom is a status line, which tells you what the last action
did (e.g. *Copied report to clipboard!*). The moon or sun button in the title bar switches between the dark and the light
theme.

A section checks again when you open it, and with *Refresh* (or `F5`).

### System & Hardware

What a supporter asks for first when you report a problem.

- **Process memory**, **GC managed heap** and **system physical RAM**: how much memory BuckShot uses, and how much your PC
  has free.
- **Uptime**, process id, threads, handles and GDI / USER objects of BuckShot. A GDI or USER count which keeps growing
  points to a leak.
- **Full Environment Diagnostic Report**: the BuckShot version and edition (and *Portable*), 32 or 64 bit, the .NET
  version, the Windows version, and the loaded plugins, plus the numbers above.

*Copy System Info* copies the report to the clipboard, so you can paste it into a bug report.

### Your setup

The same checks as the *Your setup* step of the welcome window, each with ✓ or with a fix:

| Check | When something is off | Fix |
|---|---|---|
| Latest version | A newer BuckShot is available | *Download it* opens the downloads page. When BuckShot can't reach the update feed it says so. |
| Icon next to the clock | Windows 11 hides the BuckShot icon behind the arrow (^) | *Show it next to the clock* tells Windows to show it right away. When Windows has no entry for the icon yet, the taskbar settings open instead, where you can switch it on under *Other system tray icons*. Windows 10 doesn't hide new icons, there this check is always ✓. |
| Print key | The Windows Snipping Tool or OneDrive takes the Print key | *Let BuckShot have the Print key* switches that off, the same as in [Hotkeys & Conflicts](#hotkeys--conflicts). |
| Start with Windows | | The check box turns starting BuckShot with Windows on or off for you (your own user), like the setting in the settings window. See [autostart.md](autostart.md). |

The badge counts the problems: an outdated version, a hidden icon, the Snipping Tool and OneDrive.

### Files & Configuration

- **Screenshot Folder (where BuckShot saves)**: the folder from the settings, the folder that is really used (with
  variables like `${YYYY}` filled in), and the name the next file gets, e.g. with the filename pattern. Below that the
  checks, each with ✓, ⚠️ or ❌:

  | Check | What it means |
  |---|---|
  | The folder exists and BuckShot can write to it | All is well. |
  | No folder configured | BuckShot saves to the default folder (the Desktop). |
  | The folder contains variables | BuckShot fills them in when it saves, and creates the folder. |
  | The folder doesn't exist yet | BuckShot creates it at the next save. |
  | The drive or network share isn't available | Until it is, BuckShot saves to the Desktop and shows a notification. Your setting stays as it is. |
  | BuckShot can't write to the folder | Check the permissions, or whether *Controlled folder access* of Windows Security blocks BuckShot. |
  | A network folder | When it can't be reached (e.g. without VPN), BuckShot saves to the Desktop for that time and shows a notification. |
  | OneDrive synchronizes the folder | Files can be online only, or show sync conflicts. This also applies to the Desktop when OneDrive backs it up. |
  | The full path is too long | The path of a file is longer than Windows allows by default. |
  | The filename pattern doesn't give a valid filename | Fix the pattern in the settings (Output tab), or reset it. |
  | The filename pattern creates subfolders | Just so you know where the files go. |

  *Open Folder* opens it in Explorer, *Copy Path* copies it. *Reset Folder to Default* sets the folder back to the default
  (the Desktop, or the folder of the portable version), *Reset Filename Pattern* sets the pattern back to the default. The
  result of every check is written to the log, so a supporter sees it there too. When a save had to go to the Desktop,
  its notification points here.
- **BuckShot Log File (buckshot.log)**: where the log is, and its size. *Copy Path* copies the path.
- **BuckShot Configuration File (buckshot.ini)**: where your settings are, and the size. *Copy Path* copies the path.
- **Quick Glance**: the end of the log (the last 16 KB) right in the window. *Open Log Viewer* opens the whole log in its
  own window.

### The log viewer

The log viewer is its own window, so you can keep it open next to BuckShot. It opens from *Files & Configuration* and from
a [buckshot://log](buckshot://log) link.

- It shows the end of the log first (the last 64 KB). *Load Earlier (+64 KB)* and *Load Entire Log* load more.
- New log entries appear while the window is open, without refreshing.
- **Search** (`Ctrl`+`F`) shows only the log entries which contain the text, from the whole log file, ignoring case. A log
  entry is its first line with the date and every line after it, e.g. the stack trace of an error. The line above the log
  says how many entries match. A link can open the viewer with a search, e.g.
  [buckshot://log?search=HDR](buckshot://log?search=HDR).
- **Log level** sets how much BuckShot writes to the log: *ERROR*, *WARN*, *INFO* or *DEBUG* (the most). It applies right
  away, until BuckShot restarts, so you can reproduce a problem with *DEBUG* without changing a file. A link can't change
  it.
- *Wrap text*, *Copy Log* (copies what is shown), and *Refresh Tail* (`F5`).

### Clipboard Diagnostics

For "BuckShot couldn't copy to the clipboard": another program holds the clipboard open, and nobody else can use it
meanwhile.

- **The status** at the top: the clipboard is free, or *BLOCKED* by a program, with its name, process id, window title and
  path. Below it, which program put the current data on the clipboard.
- **Test Clipboard Now** checks once more, and notes the result in the monitor log.
- **Live Clipboard Blocker Monitor Loop**: *Start Monitor Loop* checks every 600 ms and logs every lock, so you can also
  catch a program which blocks the clipboard only for a moment. *Clear Log* empties the list.
- **Active Formats Currently on Clipboard**: every format of the current clipboard content (e.g. *DIB*, *PNG*, *HTML*), with
  a description. It updates by itself whenever the clipboard changes.

### Hotkeys & Conflicts

For "Print does nothing" or "Print opens something else".

- **BuckShot Configured Shortcuts**: the hotkeys of all recipes which are on (the capture hotkeys of the settings first,
  then the hotkeys of other recipes) and their status: *Registered* when BuckShot listens for the hotkey, *Not Registered / Conflict* when it
  doesn't (e.g. a key name in `buckshot.ini` it can't read), *Disabled* when no hotkey is set. *Re-register BuckShot
  Hotkeys* registers them again, e.g. after you closed a program which had taken the keys.
- **Microsoft OneDrive**: OneDrive can save screenshots itself (*Automatically save screenshots I capture to OneDrive*) and
  takes the Print key for that. *Disable OneDrive Screenshot Interception* switches it off in the OneDrive settings of your
  user.
- **Windows Snipping Tool**: newer Windows 11 versions open the Snipping Tool with the Print key (*Use the Print screen key to
  open Snipping Tool*). *Disable Snipping Tool Takeover* switches it off for your user; *Open Windows Keyboard Settings*
  opens the page where Windows has this setting.
- **Dropbox**: whether Dropbox is running. Dropbox can take Print and Alt+Print (*Share screenshots using Dropbox*);
  BuckShot can't switch that off, the section says where it is in Dropbox.
- **Other Known Screen Capture Applications**: whether ShareX, Snagit, Lightshot, PicPick, the Snipping Tool or the Snip &
  Sketch overlay are running, with their process id. Each of them can take the hotkeys BuckShot uses.

The badge counts the problems: OneDrive, the Snipping Tool, and a hotkey which isn't registered.

### Installation Integrity

For "BuckShot crashes on start" or "a plugin is missing" after an update or a virus scanner: are the files of the
installation the ones BuckShot released?

The installer and the portable version contain `checksum.SHA256`, the SHA-256 checksum of every file. This section checks
every file against it:

- **Valid**: the file is the released one.
- **Mismatch (Corrupt)**: the file was changed, e.g. a broken update. Reinstalling BuckShot fixes it.
- **Missing**: the file is listed, but not there.
- **Unlisted**: a file which isn't in the list, e.g. one left over from an older version.

At the top are the totals and the result. Filter the list (*All*, *Issues Only*, *Mismatched*, *Missing*, *Unlisted*,
*Valid*) or search it by name, path or checksum. Selecting a file shows its full path and both checksums, and *Copy
Details* copies them. *Copy Integrity Report* copies the result of all files, *Re-check All Files* checks again, and the
link to `checksum.SHA256` opens the list itself.

### Delete old Imgur uploads

BuckShot no longer uploads to Imgur. This section is only there when you uploaded images anonymously with an earlier
BuckShot: BuckShot still has the delete keys of those uploads, and only with these keys can an anonymous upload be
deleted.

- The list shows each upload with its thumbnail, title and date (loaded from Imgur when you open the section). Uploads
  which Imgur no longer has are removed from the list.
- *Delete from Imgur* deletes the selected images on Imgur, after asking. This can't be undone.
- *Open* opens the image in the browser.
- *Save delete links* saves a web page with every image and its delete link, and opens it, so you can delete them later
  yourself.
- *Forget the delete keys* removes the uploads from BuckShot, the images stay on Imgur. Save the delete links first if you
  want to keep them.

When the last upload is deleted or forgotten, the section is gone the next time the window opens. The welcome window shows
the same list as a step when there are uploads.

### Keys

| Key | Does |
|---|---|
| `Esc` | Closes the window |
| `F5` | Checks the selected section again |
| `T` | Switches between the dark and the light theme |
| `1` to `9` (also on the number pad) | The section at that place in the sidebar: `1` System & Hardware, `2` Your setup, `3` Files & Configuration, `4` Clipboard Diagnostics, `5` Hotkeys & Conflicts, `6` Installation Integrity, then *Delete old Imgur uploads* when it is there |
| `E` | System & Hardware |
| `F`, `L` or `I` | Files & Configuration |
| `C` | Clipboard Diagnostics |
| `H` | Hotkeys & Conflicts |
| `K` or `V` | Installation Integrity |

The keys don't work while you type in a text box (e.g. the search of *Installation Integrity*), except `Esc`.

### Light edition and portable version

BuckShot Light and the portable version have the same Self-Service. The difference: they don't register `buckshot:`
links, so [buckshot://self-service](buckshot://self-service) and [buckshot://log](buckshot://log) don't open
anything there. Open Self-Service from the About window instead.

---

## For developers

- [What a Debug build adds](#what-a-debug-build-adds)
- [The Integration (Debug) section](#the-integration-debug-section)
- [Running the browser extension from a Debug build](#running-the-browser-extension-from-a-debug-build)
- [Troubleshooting the extension](#troubleshooting-the-extension)
- [Good to know](#good-to-know)

### What a Debug build adds

A Debug build (`Debug` configuration, `#if DEBUG`) adds the **Integration (Debug)** section 🛠️ to the sidebar
(`debug_integration`, also [buckshot://self-service?section=debug_integration](buckshot://self-service?section=debug_integration)).
It registers what the installer would register, but for your user only (HKCU), so you need no administrator rights and
no installer. A Release build doesn't have the section at all.

### The Integration (Debug) section

It works with the files in the output folder of the Debug build, `src\BuckShot\bin\Debug\net480\` (the folder of the
running `BuckShot.exe`):

- **Registry & Binary Status**: whether `buckshot-proxy.exe` is in the output folder, and where the native messaging host
  is registered for Chrome, Edge and Firefox, and the `buckshot://` protocol.
- **Browser Extension ID (Unpacked Developer Mode)**: the ID of your unpacked extension, see below. *Save ID to Manifest*
  writes it into `org.buckshot.proxy.json`.
- **Register Debug Host & Protocol (HKCU)** writes these keys, each pointing to the output folder:

  | Key (under `HKEY_CURRENT_USER`) | Value |
  |---|---|
  | `Software\Google\Chrome\NativeMessagingHosts\org.greenshot.proxy` | `org.buckshot.proxy.json` |
  | `Software\Microsoft\Edge\NativeMessagingHosts\org.greenshot.proxy` | `org.buckshot.proxy.json` |
  | `Software\Mozilla\NativeMessagingHosts\org.greenshot.proxy` | `org.buckshot.proxy-firefox.json` |
  | `Software\Classes\buckshot` | the `buckshot:` protocol, started with `buckshot-proxy.exe "%1"` |

  It saves the extension ID too, when one is filled in.
- **Unregister from HKCU** removes all four keys again. Do this before you install a released BuckShot, or links and the
  extension keep starting your Debug build.
- **Test buckshot:// URL Scheme** opens [buckshot://about](buckshot://about): when the About window opens, the round
  trip browser → `buckshot-proxy.exe` → BuckShot works.
- **Extension Folder** opens `src\BuckShot.BrowserExtension` in Explorer, the folder you load into the browser.

When the section opens and the output folder has no host manifests yet, it copies `org.buckshot.proxy.json` and
`org.buckshot.proxy-firefox.json` from `src\BuckShot-Installer\additional_files`.

### Running the browser extension from a Debug build

The extension talks to BuckShot with
[native messaging](https://developer.chrome.com/docs/extensions/develop/concepts/native-messaging): the browser starts
`buckshot-proxy.exe` (the "host"), which passes the messages to BuckShot over its named pipe. The browser only starts a
host which is registered and lists the extension; BuckShot checks the extension once more (`ExtensionOriginPolicy`). See
[buckshot-proxy-and-integration.md](buckshot-proxy-and-integration.md) for the protocol, and ADR 001 / ADR 003 for why
it is done this way.

1. **Build** the solution in the `Debug` configuration. Build the whole solution, not only `BuckShot.csproj`:
   `buckshot-proxy.exe` is a C project (`src\buckshot-proxy\buckshot-proxy.vcxproj`, x64) which builds into the same
   `src\BuckShot\bin\Debug\net480\` folder. The extension doesn't work with BuckShot Light (`Debug Light`): Light has no
   browser extension code and no proxy.
2. **Start** the Debug BuckShot, open Self-Service (About window, `S`) and select *Integration (Debug)*. *Proxy
   Executable* must say *Found*.
3. **Load the extension unpacked**:
   - **Chrome**: `chrome://extensions`, switch on *Developer mode*, *Load unpacked*, select `src\BuckShot.BrowserExtension`.
   - **Edge**: `edge://extensions`, switch on *Developer mode*, *Load unpacked*, select the same folder.
   - **Firefox** (121 or newer): `about:debugging#/runtime/this-firefox`, *Load Temporary Add-on…*, select
     `src\BuckShot.BrowserExtension\manifest.json`. Firefox removes a temporary add-on when it closes, load it again
     after a restart.
4. **Chrome and Edge only: save the extension ID.** An unpacked extension gets an ID from its folder path (the manifest has
   no `key`), shown on its card in the extensions page, e.g. `abcdefghijklmnopabcdefghijklmnop`. Paste it into *Browser
   Extension ID* and click *Save ID to Manifest*. The output folder's `org.buckshot.proxy.json` then allows only
   `chrome-extension://<your id>/`; BuckShot still accepts the published extension as well. Firefox needs nothing here:
   the manifest sets the ID `buckshot@getbuckshot.org`, which `org.buckshot.proxy-firefox.json` and BuckShot already
   allow.
5. **Register**: click *Register Debug Host & Protocol (HKCU)*. All lines of the status turn to *Registered (HKCU)*.
6. **Try it**: open the extension's popup. It says *Ready* when it reached BuckShot (it reconnects by itself, after a few
   seconds up to a minute; reload the extension in the extensions page to connect right away). *Capture Visible Page*
   sends the visible part of the page to BuckShot.

The ID only needs saving again when you load the extension from another folder. BuckShot reads the host manifests on
every connection, so a new ID works without restarting BuckShot. A rebuild doesn't remove the registration, but a clean
of the output folder removes the manifests: open the section again (it copies them back), save the ID and register again.

### Troubleshooting the extension

| Symptom | Cause and fix |
|---|---|
| The popup stays *Offline* or *Connecting…*, the extension's service worker console says *Specified native messaging host not found* | The host isn't registered for this browser, or the manifest file it points to is gone. Register again in the section. |
| *Access to the specified native messaging host is forbidden* | `org.buckshot.proxy.json` doesn't list this extension's ID. Save the ID (step 4). |
| The popup connects, but BuckShot doesn't answer | BuckShot rejected the extension. Search the log for it: [buckshot://log?search=origin](buckshot://log?search=origin) shows *browser extension origin '…' is not allowed*. Check that the saved ID is the one on the extension's card. |
| *Proxy Executable* says *Missing* | `buckshot-proxy.exe` wasn't built. Build the whole solution (it needs the C++ build tools of Visual Studio). |
| Links open the installed BuckShot instead of the Debug build, or the other way around | The HKCU registration wins over the installer's registration. *Unregister from HKCU* to use the installed one again. |

The service worker console is on the extension's card in the extensions page (*Inspect views: service worker*, in Firefox
*Inspect*).

### Good to know

- The Debug build has no `checksum.SHA256`, so *Installation Integrity* says the checksum manifest isn't found. To try the
  section, use the output of a Release build of `BuckShot-Installer`, or the portable version.
- *Delete old Imgur uploads* is only shown when `buckshot.ini` has entries in `ImgurUploadHistory` (section `[Imgur]`).
- The log viewer's level changes `LogHelper.RootLevel` in memory only; the `log4net-*.xml` files stay as they are.

---

## Building a Self-Service section

A section is one entry in the sidebar with its panel on the right. Everything lives in `src\BuckShot\SelfService`
(see [code-structure.md](code-structure.md): WPF views in `Views`, view models in `ViewModels`):

| File | Holds |
|---|---|
| `ViewModels\SelfServiceSectionViewModel.cs` | The base class of every section |
| `ViewModels\SelfServiceViewModel.cs` | The list of sections, which one is selected, the theme brushes |
| `ViewModels\<Name>SectionViewModel.cs` | One section: its checks, its fixes, its texts |
| `Views\SelfServiceWindow.xaml` | The sidebar, and one panel per section |
| `Views\SelfServiceWindow.xaml.cs` | Shows the selected panel, the keys, the button handlers |
| `src\BuckShot.Base\Languages\ISelfServiceLanguage.cs` and `src\BuckShot\Languages\buckshot.en-US.ini` | The texts |
| `src\BuckShot.Tests\Forms\SelfServiceTests.cs` | The tests of the sections |

### 1. The view model

Derive from `SelfServiceSectionViewModel` and fill in the four abstract properties:

```csharp
public class PrinterSectionViewModel : SelfServiceSectionViewModel
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(PrinterSectionViewModel));

    // The id for the panel, ?section= of buckshot://self-service links, and the tests. Lower case, never changed later.
    public override string Id => "printer";
    public override string Title => Texts.SelfService.CategoryPrinter;
    public override string Subtitle => Texts.SelfService.CategoryPrinterSub;
    public override string Icon => "🖨️";

    public override void OnNavigatedTo()
    {
        // Check again each time the user opens the section
        Refresh();
    }

    public override void Refresh()
    {
        // The checks, then the badge: a number of problems, or nothing
        int issues = ...;
        BadgeText = issues > 0 ? issues.ToString() : null;
        BadgeBrush = issues > 0 ? WpfThemeHelper.WarningBackground : null;
        OnPropertyChanged(nameof(...));
    }
}
```

What the base class gives you:

- `BadgeText` / `BadgeBrush`: the badge in the sidebar, shown while `BadgeText` isn't empty. Use
  `WpfThemeHelper.WarningBackground` for problems, so the badge follows the theme.
- `OnNavigatedTo()` / `OnNavigatedFrom()`: called when the user selects the section or leaves it. Start expensive work
  (network, scanning files) here or later, not in the constructor: every section is created when the window opens.
  `ImgurSectionViewModel` loads its thumbnails only on the first `OnNavigatedTo()`.
- `Refresh()`: called by `F5` and the *Refresh* buttons.
- `OnLanguageChanged()`: raises `Title` and `Subtitle`; override it (and call the base) when the section builds texts itself.
- `OnPropertyChanged()` for your own properties.

Rules the existing sections follow:

- **Read only until the user clicks.** A check never changes anything. A fix is a button, changes only what the user's own
  account can change (HKCU, their own files), and logs what it did and why it failed (`log4net`, `Log.Warn` for a fix
  which didn't work). Ask first before anything which can't be undone, like *Delete from Imgur*.
- **Never throw.** The window calls `Refresh()` from key presses and buttons: catch, log, and show the problem in the
  section (most sections set a `StatusMessage`, which the status line shows).
- **UI thread.** The view model is used on the UI thread. Work which takes long (network, hashing) runs `async` with
  `ConfigureAwait(false)` and comes back with `UiDispatcher.Current.InvokeAsync(...)`; start it with
  `FireAndLog("what it does", Log)`. `Task.Run` is only allowed with a `// PARALLEL:` comment (see `BannedSymbols.txt`):
  `ChecksumSectionViewModel` hashes the files on the thread pool with `ThreadPoolSwitch.SwitchToThreadPoolAsync()`.
- **One check, one place.** When another window needs the same check, use the same view model.
  `SetupSectionViewModel` is both the *Your setup* section and the *Your setup* step of the welcome window, and it reuses
  the OneDrive and Snipping Tool checks of `HotkeySectionViewModel` instead of repeating them.

### 2. Add it to the list

In `SelfServiceViewModel`: a property for the section, create it in the constructor, and add it to `Sections`. The order of
`Sections` is the order in the sidebar.

```csharp
public PrinterSectionViewModel PrinterSection { get; }
...
PrinterSection = new PrinterSectionViewModel();
Sections.Add(PrinterSection);
```

A section which only makes sense sometimes is only added then, like `ImgurSection` (`ImgurSectionViewModel.HasUploads`).
Its property stays `null` otherwise, and bindings to a `null` property resolve silently. A Debug-only section goes inside
`#if DEBUG`, see `IntegrationDebugSection`: in other builds the property is a `null` `object`, so the XAML still binds
without errors. Code which BuckShot Light doesn't have (plugins, AI tools, browser extension, toasts) stays out of a
section with `#if !BUCKSHOT_LIGHT`, or the Light build fails its check of forbidden types.

When the section needs cleanup (timers, subscriptions), call it from `SelfServiceViewModel.Cleanup()`, like
`ClipboardSection.StopMonitoring()`.

### 3. The panel

In `SelfServiceWindow.xaml`, add a panel next to the others in the right column: copy the `SetupPanel` element (the
shortest one, it keeps the padding of the others), name the copy `PrinterPanel` and keep it `Visibility="Collapsed"`.
Inside it, a header and one card per check:

```xml
<StackPanel>
    <StackPanel Margin="0,0,0,16">
        <TextBlock Text="{wpf:Text SelfService.PrinterTitle}" FontSize="18" FontWeight="SemiBold" Foreground="{Binding TextPrimaryBrush}"/>
        <TextBlock Text="{wpf:Text SelfService.PrinterSub}" FontSize="12" Foreground="{Binding TextSecondaryBrush}" Margin="0,3,0,0"/>
    </StackPanel>
    <Border Style="{StaticResource CardBorderStyle}">
        <!-- One card per check -->
    </Border>
</StackPanel>
```

- The `DataContext` of the window is the `SelfServiceViewModel`, so bind with `PrinterSection.Property`.
- Use the styles of the window: `CardBorderStyle` for a card, `PrimaryButtonStyle`, `SecondaryButtonStyle`, and
  `WarningButtonStyle` for the button of a fix. Colors come from the brushes of the view model (`TextPrimaryBrush`,
  `TextSecondaryBrush`, `CardBackgroundBrush`, `AccentBrush`, `WarningTextBrush`, ...), never fixed colors, so dark and
  light theme both work.
- Texts with `{wpf:Text SelfService.Key}`: they change with the language without restarting.
- A panel which another window shows too goes in its own `UserControl` in `Views`, like `ImgurUploadsView`
  (`<views:ImgurUploadsView x:Name="ImgurPanel" DataContext="{Binding ImgurSection}" .../>`), which the welcome window
  shows as well.

Then in `SelfServiceWindow.xaml.cs`:

- `UpdateVisiblePanel()`: `PrinterPanel.Visibility = sectionId == "printer" ? Visibility.Visible : Visibility.Collapsed;`
- `UpdateStatusText()`: a `case "printer":` when the section has a `StatusMessage` for the status line.
- One click handler per button, which calls a method of the view model and shows its `StatusMessage`:

  ```csharp
  private void OnFixPrinterClicked(object sender, RoutedEventArgs e)
  {
      ViewModel?.PrinterSection.Fix();
      UpdateStatusText(ViewModel?.PrinterSection.StatusMessage);
  }
  ```

  A Debug-only section needs its handlers in the `#else` branch too (empty), because the XAML is the same in every build.
- `OnWindowKeyDown()`: the number keys already select the section by its place in the sidebar. Add a letter only for a
  common section, and add it to the [key table](#keys).

### 4. Texts

Every text a user reads is translatable (developer-only sections like *Integration (Debug)* aren't):

1. Add the key to the `[SelfService]` section of `src\BuckShot\Languages\buckshot.en-US.ini`, e.g.
   `category_printer=Printer` and `category_printer_sub=...`.
2. Add the property to `ISelfServiceLanguage` with the English text as its comment: `category_printer` becomes
   `string CategoryPrinter { get; }`. The implementation is generated.
3. Use it as `Texts.SelfService.CategoryPrinter` in code and `{wpf:Text SelfService.CategoryPrinter}` in XAML.

Other languages fall back to English until they're translated, see [translation](translation/README.md). Texts which
belong to another window and are shared (the *Your setup* texts are in `[Welcome]`) stay where they are.

### 5. Links

`buckshot://self-service?section=<id>` works for the new section without any change: `SelfServiceViewModel.SelectSection`
matches the `Id`, ignoring case. Add the section name to the `buckshot:self-service` row and a test link to the table in
[buckshot-proxy-and-integration.md](buckshot-proxy-and-integration.md#23-url-protocol-scheme-buckshot), and to
[Opening Self-Service](#opening-self-service) here.

### 6. Tests

Add to `SelfServiceTests`: the section is in `Sections` (or only under its condition), `SelectSection("<id>")` selects it,
and its checks work on a clean machine without throwing. Put the logic you want to test in `internal static` methods with
their inputs as parameters, like `SetupSectionViewModel.IsHiddenInOverflow(RegistryKey, string, int)`, which the tests
call with a registry key of their own instead of the user's. The tests run in random order and without a synchronization
context, so don't depend on another test or on the UI thread unless the test creates it (see the `STAThread` tests of
the window).

### 7. This document

Describe the section in [For users](#for-users): what it checks, what each button does, and when the badge counts.
