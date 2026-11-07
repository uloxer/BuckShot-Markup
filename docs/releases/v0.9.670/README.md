# BuckShot Markup 0.9.670

- Retained six recent-colour slots, with stable swatch refresh and toolbar overflow behavior.
- Improved selection, snapping, rotation and dimension-rendering efficiency; reused unchanged layouts and stationary snap targets.
- Added a small snap release margin to reduce target switching near boundaries while preserving modifier constraints.
- Fixed high-zoom rotation-handle hit testing, undo repainting and graphics-state inconsistencies between full and partial redraws.

The focused Windows regression suite and per-user installer checks run before packaging. Includes the EXE installer and SHA-256 checksum only; MSIX remains deferred.
