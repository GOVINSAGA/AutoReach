## 2026-04-07 - Missing ARIA equivalents in WPF forms
**Learning:** In WPF applications, input fields (TextBox, PasswordBox) and interactive elements like DataGrids do not inherently announce their purpose or labels to screen readers by default.
**Action:** Always apply `AutomationProperties.Name` as an ARIA equivalent to input fields, buttons, and DataGrids in WPF applications to ensure screen readers announce them properly.
