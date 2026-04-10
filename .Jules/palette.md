## 2024-04-10 - WPF TextBlock Labels Lack Implicit Association
**Learning:** In WPF, visual `TextBlock` labels are not implicitly associated with input controls (like `TextBox`, `PasswordBox`, or `DataGrid`) for screen readers, unlike `<label for="id">` in HTML.
**Action:** Always explicitly use `AutomationProperties.Name` on input elements, DataGrids, and context-dependent buttons in WPF to serve as an explicit ARIA-equivalent label so screen readers can announce them correctly.
