## 2024-04-04 - Dynamic Button Text in WPF
**Learning:** When using a DataTrigger in a WPF Style to change a button's Content dynamically, setting the default Content property directly on the Button tag (e.g., `<Button Content="Start Batch">`) will override the trigger due to dependency property precedence rules.
**Action:** Always move the default Content setting into a Style Setter (e.g., `<Setter Property="Content" Value="Start Batch" />`) alongside the DataTrigger so that state changes can successfully override it.
## 2024-04-22 - Explicit ARIA Equivalents in WPF
**Learning:** WPF visual labels (like `TextBlock`) aren't automatically associated with input fields for screen readers. Using `AutomationProperties.Name` on standard WPF inputs (`TextBox`, `PasswordBox`) and buttons is the exact equivalent of `aria-label` in web dev. It's particularly useful for contextualizing identical generic buttons (like multiple "Browse" buttons) without changing the visual layout.
**Action:** Always check `AutomationProperties.Name` when adding or reviewing interactive controls in XAML that lack implicit context or rely solely on nearby visual TextBlocks for labeling.
