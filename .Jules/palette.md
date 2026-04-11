## 2024-05-24 - Screen Reader Labels in WPF
**Learning:** In WPF XAML, visual labels (like `TextBlock`) are not implicitly associated with input elements for screen readers. Unlike HTML `<label for="...">`, WPF requires explicit association to ensure screen readers can announce the inputs properly.
**Action:** Always use `AutomationProperties.Name` directly on input elements (`TextBox`, `PasswordBox`), `DataGrid`, and context-dependent buttons to serve as an explicit ARIA equivalent.
