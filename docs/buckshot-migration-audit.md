# BuckShot Markup migration audit

Base: BuckShotPlus 0.8.666, commit `6157fb40e1b790886cce3251a8faa4e5966ad1bf`. Work is confined to the new BuckShot-Markup checkout. The original checkout, installation and configuration are not migrated or modified.

## Identity and version

`src/version.json` is the version authority (`0.8.666`). Nerdbank.GitVersioning generates assembly/file metadata from it. `ProductIdentity` exposes the public product name, source URL and full three-part generated version; About and CLI version responses use it. Installer builds read the same generated file version and reject a conflicting manual override. Internal assembly and resource names remain unchanged for compatibility.

Public About content contains the product/version, labelled source link, GPL and upstream credits. Upstream donation/support/help marketing and welcome onboarding are removed from the tray/About/startup paths. Artwork is a separate integration stage.

Localized INI display values and language fallback attributes are rebranded without renaming keys or resource filenames. Copyright-bearing translations are retained as attribution. The audit searched tracked Greenshot/GreenShotPlus references across source, installer, translations, metadata, scripts and documentation; retained categories are listed below rather than mechanically renaming their identifiers.

## Coexistence

BuckShot uses `buckshot-markup.ini`, separate defaults/fixed files, `BuckShot-Markup` roaming/local/temp directories, a new application mutex, distinct managed/native/MCP pipe prefixes, startup value, AppUserModelID and toast COM identifier. Installer AppId, installation directory, Start-menu links and project ProgID are independent.

Installer legacy cleanup and uninstall migration logic that touched BuckShot's registration is removed. `.greenshot` receives an additional BuckShot Open-with ProgID; existing file defaults and the upstream `buckshot:` protocol are not replaced. Executables retain their internal names inside the separate installation directory. The GNU GPL licence file remains intact; its installer page adds a labelled `TNewLinkLabel` source hyperlink.

## Recipes boundary

Public authoring/settings/tray quick options, external recipe-file loading, RecipeEditor plugin distribution, CLI recipe commands and MCP authoring/catalog/validation commands are disabled. The settings approvals/proposal controls are hidden. Incoming recipe commands are rejected independently of source.

The private `RecipeManager`, built-in flows, trigger manager, capture runner, pipeline steps and required schemas/resources remain. Ordinary region/window/full-screen/file capture depends on these implementations. Deleting their types or resources would break normal capture. Internal names and historic recipe tests/docs remain as implementation/history, not an advertised product feature. RecipeEditor source remains for upstream history and existing test compilation, but its plugin is neither distributed nor loaded.

## Deliberately retained upstream references

- GPL/copyright notices, `LICENSE`, third-party licences, credits and source-file legal headers.
- C# namespaces, assembly names, resource names, language keys and language-pack filenames.
- `.greenshot` project/clipboard serialization markers and type allowlists, native helper executable names and internal IPC source identifiers.
- Upstream history, historical release notes, migration provenance, development documentation and unreachable authoring implementation resources.
- The original remote is retained as `buckshotplus` and official upstream as `upstream`; publication targets the new repository.

## Validation scope

## Integrated product changes

- A cyan crop/blueprint/dimension/divider identity replaces shipped application, tray, helper, portable and installer logos. About includes the new drafting artwork.
- Original editor vectors render at the current toolbar/DPI size in light and dark themes. Context menus and numbered steps use the same artwork; emoji insertion is retained.
- Settings > Measurements persists fresh-document precision, font, text/background colour, transparency and snapping. Project metadata continues to override new-document defaults; calibration remains document-specific.
- Fresh capture destinations are Clipboard and FileDefault; existing configured destinations are not migrated or overwritten.
- A selected individual object has a draggable rotation handle, with one undo record, Shift angle steps and rollback on interruption. The exact-angle dialog remains available.
- Update checks use this repository's `update-feed.json` and releases, not BuckShot's upstream service. The feed records published release availability; `src/version.json` remains the authority for the built product version.
- Recipe catalog/page publishing workflows and MCP's dynamic public recipe discovery are removed.
- Upstream BuckShot deployment, Chocolatey, Pages and Cloudflare publishing workflows are retired in this fork. The BuckShot Windows-installer workflow is the supported packaging path.

## Integration validation

Resolved in 0.8.667: discarded add/geometry/field history records no longer dispose live pasted images. Delete records relinquish ownership when restored and still dispose detached images when their history is discarded. Targeted pixel, undo/redo, persistence and disposal regressions cover the correction.

Stage 1 validates a serial managed build, product/storage/IPC boundary regressions and built-in capture pipeline tests. Installer native/package validation and final visual branding are integration-stage gates; no installer or release is published at this stage. Public UI/source-reference review must be repeated after later stages integrate their new icons and defaults.
