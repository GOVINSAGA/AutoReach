
## 2026-04-05 - Add AutomationProperties.Name for Screen Readers
**Learning:** In WPF applications, input elements (TextBox, PasswordBox) and complex controls (DataGrid) don't automatically associate with their visual labels (TextBlock) for screen readers unless explicitly linked.
**Action:** Use `AutomationProperties.Name` on WPF inputs and DataGrids to provide accessible names, acting as an ARIA-equivalent for screen reader users.
