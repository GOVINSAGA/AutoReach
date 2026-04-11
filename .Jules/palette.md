## 2024-05-16 - Add AutomationProperties.Name to WPF Inputs
**Learning:** Visual labels like TextBlocks are not implicitly associated with input elements in WPF for screen readers, unlike HTML labels with `for` attributes.
**Action:** Use `AutomationProperties.Name` on WPF inputs (TextBox, PasswordBox, DataGrid) and context-dependent buttons to ensure screen readers can properly announce their purpose.
