## 2026-04-04 - WPF Accessibility and ARIA equivalents
**Learning:** In WPF, when text inputs and grids aren't bound to their labels using LabeledBy, screen readers announce them as empty elements. The equivalent of ARIA labels in WPF is setting the `AutomationProperties.Name` property. This is crucial for making the app accessible to visually impaired users.
**Action:** Always add `AutomationProperties.Name` to input elements and data grids in WPF applications to ensure screen readers can announce them correctly.
