## 2024-05-18 - Added AutomationProperties.Name to inputs in WPF
**Learning:** In WPF XAML, visual labels like `TextBlock` are not implicitly associated with their corresponding inputs for screen readers. This differs from HTML where `<label for="id">` implicitly links the two.
**Action:** Always use `AutomationProperties.Name` directly on input elements (`TextBox`, `PasswordBox`), `DataGrid`, and context-dependent buttons as an explicit ARIA equivalent to ensure screen readers can properly announce them.
