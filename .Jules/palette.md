## 2024-05-16 - AutomationProperties.Name as ARIA equivalent in WPF
**Learning:** Visual labels (like TextBlock elements) in WPF XAML are not implicitly associated with their adjacent input controls for screen readers, unlike `<label for="id">` in HTML.
**Action:** Use `AutomationProperties.Name` directly on input elements, DataGrids, and context-dependent buttons. This acts as an explicit ARIA equivalent to ensure screen readers can properly announce them.
