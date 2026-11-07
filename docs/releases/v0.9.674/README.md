# BuckShot Markup 0.9.674

Active application namespaces, class names, managed assemblies, native helpers, projects, solution, XAML resources, build properties, installer inputs and CI paths now use BuckShot. Required upstream legal/provenance notices and historical release documentation are retained.

Legacy .greenshot projects, serialized field settings and clipboard payloads are adapted through the existing strict type allowlist. Compatibility tests use actual project and clipboard bytes captured from the published 0.9.673 assemblies, including grouped polygons, fields and embedded images. New saves use BuckShot assembly/type names. Old third-party plugin binaries are not supported; bundled plugins are rebuilt together.

Editor startup now defers repeated initial layouts until runtime tools, icons and translated labels are ready, and removes duplicate initial icon assignment. The isolated cold/warm editor benchmark records construction and first paint timings; no timing threshold is used as a hardware-independent guarantee.

Windows CI checks regression and compatibility tests, packaged plugin/language loading, application startup and CLI, fresh per-user install/uninstall and upgrade from 0.9.672. This release distributes EXE and SHA-256 only.
