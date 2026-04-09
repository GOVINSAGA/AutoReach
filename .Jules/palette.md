## 2026-04-09 - Adding AutomationProperties.Name for screen readers
**Learning:** In WPF apps, text inputs (`TextBox`, `PasswordBox`) and `DataGrid` elements often lack proper contextual labels when bound to views, making them opaque to screen readers despite having visual text block labels nearby. Using `AutomationProperties.Name` acts as an ARIA equivalent.
**Action:** When creating form layouts with visually detached labels in WPF, always pair them with `AutomationProperties.Name` directly on the input elements.
