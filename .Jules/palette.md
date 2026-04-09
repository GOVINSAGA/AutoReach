## 2024-04-09 - WPF Accessibility Discovery
**Learning:** In WPF applications, unlike HTML where labels can implicitly describe nearby inputs, visual TextBlock elements used as labels are not automatically associated with their adjacent TextBox or PasswordBox inputs for screen readers.
**Action:** Use AutomationProperties.Name directly on all interactive elements (inputs, DataGrids, and ambiguous buttons like 'Browse') to provide explicit, screen-reader-accessible ARIA equivalents.
