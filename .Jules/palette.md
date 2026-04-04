## 2025-02-12 - [WPF Button State UX]
**Learning:** WPF buttons using `Border` with `CornerRadius` inside their `Resources` inline (instead of proper styles/templates) break standard disabled state visualizations. Relying on hardcoded background colors obscures visual feedback when buttons are disabled or busy.
**Action:** Move inline UI element definitions into dedicated styles in `Themes/Styles.xaml`, and explicitly handle `IsEnabled=False` with an Opacity trigger to ensure consistent and accessible interaction feedback across the application.
