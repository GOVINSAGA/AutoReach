## 2025-04-08 - Visual Labels vs. Screen Reader Announcements in WPF
**Learning:** In WPF applications, standard inputs like `TextBox`, `PasswordBox`, and components like `DataGrid` are not automatically associated with visually adjacent `TextBlock` elements for accessibility. A screen reader will only announce the control type but leave out the context/label.
**Action:** Always manually apply `AutomationProperties.Name="..."` to inputs (and DataGrids) using the same text as their visible label to ensure complete screen reader coverage.
