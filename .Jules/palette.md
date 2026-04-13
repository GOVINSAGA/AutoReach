## 2024-05-18 - Missing ARIA Equivalent Labels on WPF Input Elements

**Learning:** In WPF XAML, visual labels (e.g., `TextBlock`) are not implicitly associated with their corresponding input elements for accessibility tools. A screen reader may fail to announce the purpose of input boxes like `TextBox` or `PasswordBox`, contextual buttons, and `DataGrid`s without explicit setup.

**Action:** Directly use `AutomationProperties.Name` on input elements, `DataGrid`s, and context-dependent buttons as an explicit ARIA equivalent to ensure screen readers can properly announce their function and state to users.
