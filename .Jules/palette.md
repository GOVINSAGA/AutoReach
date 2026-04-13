## 2024-04-13 - Explicit ARIA Equivalents for WPF Inputs
**Learning:** In WPF XAML, visual labels (like TextBlock) are not implicitly associated with input elements like TextBox or PasswordBox for screen readers, unlike HTML's `<label for>`.
**Action:** Use `AutomationProperties.Name` directly on input elements, DataGrids, and context-dependent buttons as an explicit ARIA equivalent to ensure screen readers can properly announce them.
