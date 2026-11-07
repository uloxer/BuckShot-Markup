# BuckShot Markup 0.9.673

Installed application files, native helpers, plugin folders/DLLs, and language packs now use BuckShot names. Shortcuts, file associations, autostart, IPC verification and plugin discovery use the renamed files.

Managed assembly identities and namespaces remain unchanged for editable project serialization and WPF resource compatibility. The packaged application config maps those identities to the BuckShot filenames using CLR codeBase entries. Existing .greenshot and .buckshot projects remain supported; upstream copyright and GPL notices are retained.

The installer removes obsolete bundled Greenshot payload only within its BuckShot installation directory. Existing settings and upstream Greenshot installations remain independent.

Windows CI validates regression tests, the recursive installed filename inventory, shortcut target, CLR/plugin/language loading, actual application launch and CLI shutdown, fresh install/uninstall, and upgrade from 0.9.672. This release distributes the EXE installer and SHA-256 file only.
