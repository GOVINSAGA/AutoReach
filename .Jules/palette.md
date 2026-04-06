## 2026-04-06 - WPF Loading State UX DataTrigger Fix
**Learning:** In WPF, when using a DataTrigger in a Style to dynamically change a property (like Button Content), the default value must be set via a Style Setter rather than directly on the element tag. Setting it directly on the tag overrides the trigger due to dependency property precedence rules.
**Action:** Use <Setter Property="Content" Value="Default"/> inside the Style block when implementing loading states or similar dynamic content swaps on WPF buttons.
