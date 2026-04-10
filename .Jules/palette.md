## 2026-04-10 - Explicit ARIA Labels Needed in WPF
**Learning:** In WPF XAML, visual labels (e.g., `TextBlock`) are not implicitly associated with inputs. Because of this, screen readers may not accurately announce the purpose of an input field.
**Action:** Use `AutomationProperties.Name` directly on input elements, DataGrids, and context-dependent buttons as an explicit ARIA equivalent to ensure screen readers can properly announce them.
