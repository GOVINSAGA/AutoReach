## 2024-05-18 - Missing ARIA Equivalent for WPF Inputs and DataGrids
**Learning:** In WPF applications, standard text inputs (`TextBox`, `PasswordBox`) and `DataGrid` elements don't inherently have screen reader accessible names unless explicitly provided. They act similarly to HTML inputs missing labels or ARIA tags.
**Action:** Always add `AutomationProperties.Name` to these input elements and grids to serve as an ARIA label equivalent, ensuring screen readers can announce their purpose correctly.
