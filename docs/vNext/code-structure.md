# Code structure

Where code goes in BuckShot, and the names it gets. Folders are grouped by feature first (Capturing, Recipes, Settings, Ipc, Ai, Plugins),
and inside a feature by kind (Views, ViewModels, Tools, Steps, ...). The namespace follows the folder.

## Rules

- **`Forms` is only for WinForms.** A WPF window, page or control goes into a `Views` folder and is named `…Window`, `…Page` or `…View`.
- **A class which only exists to be bound by a view is a view model**: it is named `…ViewModel` and lives in a `ViewModels` folder next to the `Views` folder,
  one class per file.
- **Not everything with a UI is a view.** Capture tools and overlays draw into the capture window but are no XAML; they live in `Capturing/Tools` and `Capturing/Overlays`.
- **Recipes are the umbrella for the capture pipeline.** Execution, steps, triggers, approval and the recipe windows are under `BuckShot/Recipes`,
  the shared model under `BuckShot.Base/Recipes`. A plugin's recipe steps live in its own `Recipes` folder (e.g. `BuckShot.Plugin.Atlassian/Jira/Recipes/JiraStep.cs`).
- **Optional parts live in their own folder and register themselves.** The browser extension (`BuckShot/Ipc/BrowserExtension`), the AI tools (`BuckShot/Ai`)
  and plugin loading (`BuckShot/Plugins`) are left out of BuckShot Light by excluding their folders in `BuckShot.csproj`.
  They plug in through `[assembly: BuckShotModule(typeof(...))]` in their folder, implementing `IIpcCommandExtension` (IPC commands) or `IBuiltInRecipeProvider` (built-in recipes).
  `CheckLightAssembly` fails the Light build when one of their type names is still in BuckShot.exe.
- **Notifications go through `INotificationService`, created by an `INotificationServiceProvider`** (both in `BuckShot.Base/Interfaces`).
  The provider checks whether its platform is available and initializes it (the toasts register BuckShot with Windows),
  `NotificationServiceFactory` (`BuckShot/Shell`) takes the first available one: the toasts, else the balloons of the tray icon.
  See [notifications.md](notifications.md).
- **HTTP goes through `HttpService.Current`** (`BuckShot.Base/Http`): one HttpClient with the proxy setting, the certificate exceptions
  of the configuration and the user agent. It logs method, host and status only; headers, path, query and content can hold tokens and are not logged.
  Upload content with progress comes from `HttpContents`. Jira and Confluence have their own small REST clients on it (`Api/JiraClient.cs`, `Api/ConfluenceClient.cs`).
  See [http.md](http.md).
- **BuckShot.exe of the full version is x64, BuckShot Light is AnyCPU.** COM doesn't give a toast click to a BuckShot it starts for it
  when the exe is AnyCPU (its PE header says 32-bit, although it runs as a 64-bit process), see [notifications.md](notifications.md).
  Light has no toasts (the tray icon shows its notifications), no `buckshot-cli.exe` and no `buckshot-proxy.exe` (its installer
  opens files with `BuckShot.exe --file`), so it also runs on 32-bit Windows and natively on ARM64. The libraries and plugins stay
  AnyCPU, Full and Light load them; the solution platform stays "Any CPU" (the output paths are `bin\<Configuration>\net480`).
- A namespace must not shadow a type it uses: the capture folder is `Capturing`, because a `BuckShot.Capture` namespace would hide `BuckShot.Base.Core.Capture`.

## Frozen: do not move or rename

The `.greenshot` file stores the drawing surface with the BinaryFormatter, which writes full type names (see `BinaryFormatterHelper`).
Moving these types breaks opening existing files:

- `BuckShot.Editor/Drawing/**` (containers, fields, filters, adorners, emoji, `Surface`) and `EmojiPicker`
- `BuckShot.Base/Interfaces/Drawing/**`, `ISurface` and the surface event types
- `BuckShot.Editor/Helpers/BinaryFormatterHelper.cs` and the editor file format handlers
- the assembly names

The Zxing `BarcodeContainer` is not in the `BinaryFormatterHelper` allow-list, so barcodes are not part of `.greenshot` files today;
once it is added there, its type names (`BuckShot.Plugin.Zxing.Drawing`) are frozen too.

Also kept as they are: the plugin classes (`…Plugin`, recipe requirements match their full type name, embedded resources are named after them),
`BuckShot.Base.Interfaces.Capture` (capture tool plugin API) and the pack URIs of `BuckShot.Base/Wpf/Styles`.

## Layout

| Folder | What is in it |
|---|---|
| `BuckShot/Capturing` | Capture window (`Views`), tools, overlays, info panel view model, key registry, placement |
| `BuckShot/Recipes` | `RecipeManager`, `Pipeline` (execution), `Steps`, `Triggers`, `Approval`, `Views`, `ViewModels` |
| `BuckShot/Settings` | Settings window (`Views`) and its view models |
| `BuckShot/Ipc` | Named pipe server and client, dispatcher, `Cli`, `BrowserExtension` |
| `BuckShot/Ai` | Everything for AI tools (buckshot-mcp), with its settings page |
| `BuckShot/Plugins` | Plugin loading and the plugins settings page |
| `BuckShot/SelfService`, `BuckShot/Views`, `BuckShot/ViewModels` | Self-service, about, bug report and the other app windows |
| `BuckShot/Shell` | Start (`BuckShotApplication`), the shell which composes the services and owns the shutdown (`BuckShotShell`), the tray icon and its WPF menu |
| `BuckShot.Base/Recipes` | `Pipeline`, `Contracts`, `Sources`, `Triggers`, `Expressions` |
| `BuckShot.Base/Wpf` | Shared WPF helpers, styles, `Views`, `ViewModels`, the themed menus (`ThemedMenu`) and the destination picker |
| `BuckShot.Editor/Views` | WPF windows of the editor (the editor itself is still WinForms in `Forms`) |
| `BuckShot.Editor/Tabs` | The tabs of the editor window: `EditorTab` (one capture, the `IImageEditor`), the tab strip and dragging tabs between windows |
| `BuckShot.Plugin.*` | Root: the plugin class, its configuration and language interfaces. `Destinations`, `Recipes` (steps), `Views`, `Api` (the web service client and its entities), `Images`; Jira also `Monitoring`, Zxing `Drawing` (barcode container) and `Processing` (detection, generation), Stamps `Library` (the stamps folder) and `Editor` (its editor menu entry) |
